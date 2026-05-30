using ImageProcessing.Core.Enums;
using ImageProcessing.Core.Interfaces;
using ImageProcessing.Core.Models;
using ImageProcessing.Services.Services;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ImageProcessing.Tests.Services;

public class ImageProcessingServiceTests
{
    private const int WorkingHour = 10;
    private const int NonWorkingHour = 2;
    private const long FourMegabytesInBytes = 4L * 1024 * 1024;

    private readonly Mock<IImageService> _imageService = new();

    [Fact]
    public async Task RunImagePipeline_SquareDuringWorkingHours_RunsSquarePipelineAndFinishes()
    {
        var time = CreateTimeProvider(WorkingHour);
        var sut = CreateSut(time);
        var image = CreateImage(width: 20, height: 20);

        await RunPipelineAsync(sut, image, time);

        Assert.Equal(
            new[] { PipelineType.ImagePipeline, PipelineType.SquarePipeline },
            image.PipelineHistory);
        Assert.Equal(ImageStatus.Finished, image.Status);
        Assert.Equal(20, image.Width);
        _imageService.Verify(s => s.GetById(image.Id), Times.Once);
        _imageService.Verify(s => s.Update(image), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunImagePipeline_WidthGreaterThanHeight_RunsCirclePipelineAndReducesWidth()
    {
        var time = CreateTimeProvider(NonWorkingHour);
        var sut = CreateSut(time);
        var image = CreateImage(width: 30, height: 25);

        await RunPipelineAsync(sut, image, time);

        Assert.Contains(PipelineType.CirclePipeline, image.PipelineHistory);
        Assert.Equal(20, image.Width); // CirclePipeline subtracts 10: 30 -> 20
        Assert.Equal(ImageStatus.Finished, image.Status);
        _imageService.Verify(s => s.Update(image), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunImagePipeline_LargeFileAndPortrait_RunsSlowPipelineAndDoublesWidth()
    {
        var time = CreateTimeProvider(NonWorkingHour);
        var sut = CreateSut(time);
        var image = CreateImage(width: 20, height: 40, fileSizeInBytes: FourMegabytesInBytes);

        await RunPipelineAsync(sut, image, time);

        Assert.Contains(PipelineType.SlowPipeline, image.PipelineHistory);
        Assert.Equal(40, image.Width); // SlowPipeline doubles width: 20 -> 40
        Assert.Equal(ImageStatus.Finished, image.Status);
        _imageService.Verify(s => s.Update(image), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunImagePipeline_JpgExtension_RunsStarPipeline()
    {
        var time = CreateTimeProvider(WorkingHour);
        var sut = CreateSut(time);
        var image = CreateImage(width: 35, height: 60, extension: "jpg");

        await RunPipelineAsync(sut, image, time);

        Assert.Contains(PipelineType.StarPipeline, image.PipelineHistory);
        Assert.Equal(ImageStatus.Finished, image.Status);
        _imageService.Verify(s => s.Update(image), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunImagePipeline_WidthBelowMinimum_SetsProcessError()
    {
        var time = CreateTimeProvider(NonWorkingHour);
        var sut = CreateSut(time);
        var image = CreateImage(width: 10, height: 50);

        await RunPipelineAsync(sut, image, time);

        Assert.Equal(ImageStatus.ProcessError, image.Status);
        Assert.Equal(new[] { PipelineType.ImagePipeline }, image.PipelineHistory);
        _imageService.Verify(s => s.Update(image), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunImagePipeline_NoMatchingRule_SetsFinished()
    {
        var time = CreateTimeProvider(NonWorkingHour);
        var sut = CreateSut(time);
        var image = CreateImage(width: 30, height: 50);

        await RunPipelineAsync(sut, image, time);

        Assert.Equal(ImageStatus.Finished, image.Status);
        Assert.Equal(new[] { PipelineType.ImagePipeline }, image.PipelineHistory);
        _imageService.Verify(s => s.Update(image), Times.AtLeastOnce);
    }

    private ImageProcessingService CreateSut(TimeProvider timeProvider)
    {
        return new ImageProcessingService(_imageService.Object, timeProvider);
    }

    private static FakeTimeProvider CreateTimeProvider(int hour)
    {
        // FakeTimeProvider's local time zone defaults to UTC, so GetLocalNow().Hour == hour.
        var start = new DateTimeOffset(2026, 1, 1, hour, 0, 0, TimeSpan.Zero);
        return new FakeTimeProvider(start);
    }

    private static ImageItem CreateImage(
        int width,
        int height,
        string extension = "png",
        long fileSizeInBytes = 1024)
    {
        return new ImageItem
        {
            Id = Guid.NewGuid(),
            FileName = "test",
            Extension = extension,
            FilePath = "/tmp/test",
            Width = width,
            Height = height,
            FileSizeInBytes = fileSizeInBytes,
            Status = ImageStatus.InProcess,
            PipelineHistory = []
        };
    }

    // The service mutates the image in place, so assertions read the live instance.
    // FakeTimeProvider keeps the (recursive) Task.Delay calls instant by advancing
    // the virtual clock until the pipeline task completes.
    private async Task RunPipelineAsync(
        ImageProcessingService sut,
        ImageItem image,
        FakeTimeProvider time)
    {
        _imageService.Setup(s => s.GetById(image.Id)).Returns(image);
        _imageService.Setup(s => s.Update(It.IsAny<ImageItem>())).Returns(true);

        var task = sut.RunImagePipelineAsync(image.Id);

        var guard = 0;
        while (!task.IsCompleted && guard++ < 10_000)
        {
            time.Advance(TimeSpan.FromSeconds(5));
            await Task.Yield();
        }

        await task;
    }
}
