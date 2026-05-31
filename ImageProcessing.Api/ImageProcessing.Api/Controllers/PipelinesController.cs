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

    public PipelinesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PipelineStatusDto>> GetActivePipelines()
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
}
