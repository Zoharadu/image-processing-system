using ImageProcessing.Core.Enums;
using ImageProcessing.Core.Interfaces;
using ImageProcessing.Core.Models;

namespace ImageProcessing.Services.Services;

public class ImageService : IImageService
{
    private readonly IImageStore _imageStore;

    public ImageService(IImageStore imageStore)
    {
        _imageStore = imageStore;
    }

    public void Add(ImageItem image)
    {
        _imageStore.Add(image);
    }

    public ImageItem? GetById(Guid id)
    {
        return _imageStore.GetById(id);
    }

    public IReadOnlyCollection<ImageItem> GetAll()
    {
        return _imageStore.GetAll();
    }

    public IReadOnlyCollection<ImageItem> GetCompleted()
    {
        return _imageStore.GetAll()
            .Where(image =>
                image.Status == ImageStatus.Finished ||
                image.Status == ImageStatus.ProcessError)
            .ToList();
    }

    public bool Update(ImageItem image)
    {
        return _imageStore.Update(image);
    }
}
