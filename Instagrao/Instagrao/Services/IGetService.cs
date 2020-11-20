using Instagrao.Domain.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Instagrao.Services
{
    public interface IGetService
    {
        Task<ImageMetadata> Get(string s3ObjectKey);
        Task<ImageMetadata> GetBiggestImage();
        Task<ImageMetadata> GetSmallestImage();
        Task<List<ImageExtensions>> GetImagesExtensions();
    }
}
