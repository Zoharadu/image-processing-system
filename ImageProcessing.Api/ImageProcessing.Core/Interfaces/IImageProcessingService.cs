namespace ImageProcessing.Core.Interfaces;

public interface IImageProcessingService
{
    Task RunImagePipelineAsync(Guid imageId, CancellationToken cancellationToken = default);
}
