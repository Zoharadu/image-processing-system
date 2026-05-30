using ImageProcessing.Core.DTOs;
using ImageProcessing.Core.Enums;
using ImageProcessing.Core.Interfaces;
using ImageProcessing.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SixLabors.ImageSharp;

namespace ImageProcessing.Api.Controllers;

[ApiController]
[Route("images")]
public class ImagesController : ControllerBase
{
    private const string StorageFolderName = "ImageStorage";

    private readonly IImageService _imageService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(
        IImageService imageService,
        IServiceScopeFactory scopeFactory,
        IWebHostEnvironment environment,
        ILogger<ImagesController> logger)
    {
        _imageService = imageService;
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        var id = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var storageDirectory = Path.Combine(_environment.ContentRootPath, StorageFolderName);
        Directory.CreateDirectory(storageDirectory);
        var filePath = Path.Combine(storageDirectory, $"{id}{extension}");

        try
        {
            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Identify reads only header metadata (width/height); it does not decode
            // pixel data or modify the file on disk.
            var imageInfo = await Image.IdentifyAsync(filePath, cancellationToken);

            var image = new ImageItem
            {
                Id = id,
                FileName = file.FileName,
                Extension = extension,
                FilePath = filePath,
                Width = imageInfo.Width,
                Height = imageInfo.Height,
                FileSizeInBytes = file.Length,
                Status = ImageStatus.InProcess
            };

            _imageService.Add(image);

            StartBackgroundProcessing(id);

            return Accepted(new { id });
        }
        catch (UnknownImageFormatException)
        {
            TryDeleteFile(filePath);
            return BadRequest("The uploaded file is not a supported image.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle uploaded image {FileName}.", file.FileName);
            TryDeleteFile(filePath);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to process the uploaded image.");
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<ImageListDto>> GetCompleted()
    {
        var images = _imageService.GetCompleted()
            .Select(image => new ImageListDto
            {
                Id = image.Id,
                FileName = image.FileName,
                Width = image.Width,
                Height = image.Height,
                Status = image.Status
            })
            .ToList();

        return Ok(images);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ImageDetailsDto> GetById(Guid id)
    {
        var image = _imageService.GetById(id);
        if (image is null)
        {
            return NotFound();
        }

        return Ok(new ImageDetailsDto
        {
            Id = image.Id,
            FileName = image.FileName,
            Width = image.Width,
            Height = image.Height,
            Status = image.Status,
            PipelineHistory = image.PipelineHistory
        });
    }

    [HttpGet("{id:guid}/download")]
    public IActionResult Download(Guid id)
    {
        var image = _imageService.GetById(id);
        if (image is null)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(image.FilePath) || !System.IO.File.Exists(image.FilePath))
        {
            return NotFound("The image file could not be found on disk.");
        }

        if (!new FileExtensionContentTypeProvider().TryGetContentType(image.FilePath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileStream = System.IO.File.OpenRead(image.FilePath);
        return File(fileStream, contentType, image.FileName);
    }

    private void StartBackgroundProcessing(Guid imageId)
    {
        // Fire-and-forget on a fresh DI scope; the request scope is disposed once the
        // response returns, so scoped services must not be reused across that boundary.
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processingService = scope.ServiceProvider.GetRequiredService<IImageProcessingService>();
                await processingService.RunImagePipelineAsync(imageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background pipeline failed for image {ImageId}.", imageId);
            }
        });
    }

    private void TryDeleteFile(string filePath)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file {FilePath}.", filePath);
        }
    }
}
