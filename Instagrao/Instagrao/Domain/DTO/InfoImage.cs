using System.Collections.Generic;

namespace Instagrao.Domain.DTO
{
    public class InfoImage
    {
        public ImageMetadata BiggestImage { get; set; }
        public ImageMetadata SmallestImage { get; set; }
        public List<ImageExtensions> ImageExtensions { get; set; }
    }

    public class ImageExtensions
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
