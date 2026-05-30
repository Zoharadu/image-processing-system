using System.Collections.Concurrent;
using ImageProcessing.Core.Interfaces;
using ImageProcessing.Core.Models;

namespace ImageProcessing.Services.Stores;

public class InMemoryImageStore : IImageStore
{
    private readonly ConcurrentDictionary<Guid, ImageItem> _images = new();

    public void Add(ImageItem image)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (image.Id == Guid.Empty)
        {
            throw new ArgumentException("Image id cannot be empty.", nameof(image));
        }

        if (!_images.TryAdd(image.Id, Clone(image)))
        {
            throw new InvalidOperationException($"Image with id '{image.Id}' already exists.");
        }
    }

    public ImageItem? GetById(Guid id)
    {
        return _images.TryGetValue(id, out var image)
            ? Clone(image)
            : null;
    }

    public IReadOnlyCollection<ImageItem> GetAll()
    {
        return _images.Values
            .Select(Clone)
            .ToList();
    }

    public bool Update(ImageItem image)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (image.Id == Guid.Empty || !_images.ContainsKey(image.Id))
        {
            return false;
        }

        _images[image.Id] = Clone(image);
        return true;
    }

    private static ImageItem Clone(ImageItem image)
    {
        return new ImageItem
        {
            Id = image.Id,
            FileName = image.FileName,
            Extension = image.Extension,
            FilePath = image.FilePath,
            Width = image.Width,
            Height = image.Height,
            FileSizeInBytes = image.FileSizeInBytes,
            Status = image.Status,
            PipelineHistory = [.. image.PipelineHistory]
        };
    }
}
