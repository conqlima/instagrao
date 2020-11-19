using Amazon.DynamoDBv2.Model;
using Instagrao.Domain.DTO;
using Instagrao.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Instagrao.Services
{
    public class GetService : IGetService
    {
        private readonly IRepository _repository;
        public GetService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<ImageMetadata> Get(string s3ObjectKey)
        {
            var request = new GetItemRequest
            {
                Key = new Dictionary<string, AttributeValue>
                {
                     {
                      "ImageMetadataId", new AttributeValue { S = s3ObjectKey }
                     }
                }
            };
            var response = await _repository.Get(request);
            return ToImageMetadateDTO(response);
        }

        private ImageMetadata ToImageMetadateDTO(GetItemResponse itemResponse)
        {
            var response = new ImageMetadata();
            response.ImageMetadataId = itemResponse.Item[nameof(response.ImageMetadataId)].S;
            response.Length = itemResponse.Item[nameof(response.Length)].N;
            response.Width = itemResponse.Item[nameof(response.Width)].N;
            response.Height = itemResponse.Item[nameof(response.Height)].N;
            return response;
        }
    }
}
