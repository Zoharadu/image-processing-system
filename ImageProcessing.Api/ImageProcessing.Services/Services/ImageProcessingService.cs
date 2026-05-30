using ImageProcessing.Core.Enums;
using ImageProcessing.Core.Interfaces;
using ImageProcessing.Core.Models;

namespace ImageProcessing.Services.Services;

public class ImageProcessingService : IImageProcessingService
{
    private const long ThreeMegabytesInBytes = 3 * 1024 * 1024;

    private static readonly TimeSpan SquarePipelineDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan CirclePipelineMinDelay = TimeSpan.FromMilliseconds(1500);
    private static readonly TimeSpan CirclePipelineMaxDelay = TimeSpan.FromMilliseconds(3000);
    private static readonly TimeSpan SlowPipelineDelay = TimeSpan.FromMilliseconds(1500);
    private static readonly TimeSpan StarPipelineDelay = TimeSpan.FromMilliseconds(1000);

    private readonly IImageService _imageService;
    private readonly TimeProvider _timeProvider;

    public ImageProcessingService(IImageService imageService, TimeProvider? timeProvider = null)
    {
        _imageService = imageService;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task RunImagePipelineAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        var image = _imageService.GetById(imageId);

        if (image is null)
        {
            return;
        }

        await ImagePipelineAsync(image, cancellationToken);
    }

    private async Task ImagePipelineAsync(ImageItem image, CancellationToken cancellationToken)
    {
        image.PipelineHistory.Add(PipelineType.ImagePipeline);
        _imageService.Update(image);

        if (image.Width == image.Height && IsCurrentHourBetweenEightAndNineteen())
        {
            await SquarePipelineAsync(image, cancellationToken);
            return;
        }

        if (image.Width > image.Height)
        {
            await CirclePipelineAsync(image, cancellationToken);
            return;
        }

        if (image.FileSizeInBytes > ThreeMegabytesInBytes && image.Width < image.Height)
        {
            await SlowPipelineAsync(image, cancellationToken);
            return;
        }

        if (IsJpg(image.Extension))
        {
            await StarPipelineAsync(image, cancellationToken);
            return;
        }

        if (image.Width < 20)
        {
            image.Status = ImageStatus.ProcessError;
            _imageService.Update(image);
            return;
        }

        image.Status = ImageStatus.Finished;
        _imageService.Update(image);
    }

    private async Task SquarePipelineAsync(ImageItem image, CancellationToken cancellationToken)
    {
        image.PipelineHistory.Add(PipelineType.SquarePipeline);
        _imageService.Update(image);

        await Task.Delay(SquarePipelineDelay, _timeProvider, cancellationToken);

        image.Status = ImageStatus.Finished;
        _imageService.Update(image);
    }

    private async Task CirclePipelineAsync(ImageItem image, CancellationToken cancellationToken)
    {
        image.PipelineHistory.Add(PipelineType.CirclePipeline);
        _imageService.Update(image);

        var delayMilliseconds = Random.Shared.Next(
            (int)CirclePipelineMinDelay.TotalMilliseconds,
            (int)CirclePipelineMaxDelay.TotalMilliseconds + 1);
        await Task.Delay(TimeSpan.FromMilliseconds(delayMilliseconds), _timeProvider, cancellationToken);

        image.Width -= 10;
        _imageService.Update(image);

        await ImagePipelineAsync(image, cancellationToken);
    }

    private async Task SlowPipelineAsync(ImageItem image, CancellationToken cancellationToken)
    {
        image.PipelineHistory.Add(PipelineType.SlowPipeline);
        _imageService.Update(image);

        await Task.Delay(SlowPipelineDelay, _timeProvider, cancellationToken);

        image.Width *= 2;
        _imageService.Update(image);

        await ImagePipelineAsync(image, cancellationToken);
    }

    private async Task StarPipelineAsync(ImageItem image, CancellationToken cancellationToken)
    {
        image.PipelineHistory.Add(PipelineType.StarPipeline);
        _imageService.Update(image);

        await Task.Delay(StarPipelineDelay, _timeProvider, cancellationToken);

        image.Width *= 2;
        _imageService.Update(image);

        await CirclePipelineAsync(image, cancellationToken);
    }

    private bool IsCurrentHourBetweenEightAndNineteen()
    {
        var currentHour = _timeProvider.GetLocalNow().Hour;

        return currentHour >= 8 && currentHour <= 19;
    }

    private static bool IsJpg(string extension)
    {
        return string.Equals(
            extension.TrimStart('.'),
            "jpg",
            StringComparison.OrdinalIgnoreCase);
    }
}
