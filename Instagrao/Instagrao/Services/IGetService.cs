using Instagrao.Domain.DTO;
using System.Threading.Tasks;

namespace Instagrao.Services
{
    public interface IGetService
    {
        Task<ImageMetadata> Get(string s3ObjectKey);
    }
}
