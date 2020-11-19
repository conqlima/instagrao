using Amazon.DynamoDBv2.Model;
using System.Threading.Tasks;

namespace Instagrao.Domain.Interfaces
{
    public interface IRepository
    {
        Task<GetItemResponse> Get(GetItemRequest request);
    }
}
