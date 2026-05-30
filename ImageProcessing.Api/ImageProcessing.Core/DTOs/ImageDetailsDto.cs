using ImageProcessing.Core.Enums;

namespace ImageProcessing.Core.DTOs;

public class ImageDetailsDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public int Width { get; set; }

    public int Height { get; set; }

    public ImageStatus Status { get; set; }

    public List<PipelineType> PipelineHistory { get; set; } = [];
}
