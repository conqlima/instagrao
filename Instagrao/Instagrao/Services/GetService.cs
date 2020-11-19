using Amazon.DynamoDBv2.Model;
using Instagrao.Domain.DTO;
using Instagrao.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Instagrao.Services
{
    public class GetService : IGetService
    {
        private readonly IRepository _repository;
        public GetService(
             IRepository repository)
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

        public async Task<ImageMetadata> GetBiggestImage()
        {
            string key = null;
            int biggestLength = 0;
            var scanResult = await _repository.Scan(new ScanRequest());
            
            foreach (Dictionary<string, AttributeValue> item in scanResult.Items)
            {
                if (Convert.ToInt32(item["Length"].N) > biggestLength)
                {
                    key = item["ImageMetadataId"].S;
                    biggestLength = Convert.ToInt32(item["Length"].N);
                }
            }

            return await Get(key);
        }

        public async Task<ImageMetadata> GetSmallestImage()
        {
            string key = null;
            var scanResult = await _repository.Scan(new ScanRequest());
            var firstItem = scanResult.Items.FirstOrDefault();
            int smallestLentgh = Convert.ToInt32(firstItem["Length"].N);
            key = firstItem["ImageMetadataId"].S;

            foreach (Dictionary<string, AttributeValue> item in scanResult.Items)
            {
                if (Convert.ToInt32(item["Length"].N) < smallestLentgh)
                {
                    key = item["ImageMetadataId"].S;
                    smallestLentgh = Convert.ToInt32(item["Length"].N);
                }
            }

            return await Get(key);
        }

        public async Task<List<ImageExtensions>> GetImagesExtensions()
        {
            var response = new List<ImageExtensions>();
            var scanResult = await _repository.Scan(new ScanRequest());
            
            foreach (Dictionary<string, AttributeValue> item in scanResult.Items)
            {
                var alreadySeen = response
                    .FirstOrDefault(x => x.Name.Equals(Path.GetExtension(item["ImageMetadataId"].S)));
                if (alreadySeen != null)
                {
                    alreadySeen.Quantity++;
                }
                else
                {
                    response.Add(new ImageExtensions { Name = Path.GetExtension(item["ImageMetadataId"].S), Quantity = 1 });
                }
            }

            return response;
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
