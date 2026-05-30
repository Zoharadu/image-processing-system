using ImageProcessing.Core.Models;

namespace ImageProcessing.Core.Interfaces;

public interface IImageService
{
    void Add(ImageItem image);

    ImageItem? GetById(Guid id);

    IReadOnlyCollection<ImageItem> GetAll();

    IReadOnlyCollection<ImageItem> GetCompleted();

    bool Update(ImageItem image);
}
