using ImageProcessing.Core.DTOs;
using ImageProcessing.Core.Enums;
using ImageProcessing.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessing.Api.Controllers;

[ApiController]
[Route("pipelines")]
public class PipelinesController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<PipelinesController> _logger;

    public PipelinesController(IImageService imageService, ILogger<PipelinesController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PipelineStatusDto>> GetActivePipelines()
    {
        try
        {
            var activePipelines = _imageService.GetAll()
                .Where(image => image.Status == ImageStatus.InProcess && image.PipelineHistory.Count > 0)
                .GroupBy(image => image.PipelineHistory[^1])
                .Select(group => new PipelineStatusDto
                {
                    PipelineType = group.Key,
                    ActiveImagesCount = group.Count()
                })
                .ToList();

            return Ok(activePipelines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve active pipeline status.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve pipeline status.");
        }
    }
}
