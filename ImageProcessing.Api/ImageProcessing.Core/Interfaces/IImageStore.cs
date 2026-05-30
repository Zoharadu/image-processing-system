using ImageProcessing.Core.Models;

namespace ImageProcessing.Core.Interfaces;

public interface IImageStore
{
    void Add(ImageItem image);

    ImageItem? GetById(Guid id);

    IReadOnlyCollection<ImageItem> GetAll();

    bool Update(ImageItem image);
}
