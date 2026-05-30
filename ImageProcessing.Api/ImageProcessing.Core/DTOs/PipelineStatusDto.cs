using ImageProcessing.Core.Enums;

namespace ImageProcessing.Core.DTOs;

public class PipelineStatusDto
{
    public PipelineType PipelineType { get; set; }

    public int ActiveImagesCount { get; set; }
}
