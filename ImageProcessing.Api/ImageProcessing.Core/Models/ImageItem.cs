using ImageProcessing.Core.Enums;

namespace ImageProcessing.Core.Models;

public class ImageItem
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public int Width { get; set; }

    public int Height { get; set; }

    public long FileSizeInBytes { get; set; }

    public ImageStatus Status { get; set; }

    public List<PipelineType> PipelineHistory { get; set; } = [];
}
