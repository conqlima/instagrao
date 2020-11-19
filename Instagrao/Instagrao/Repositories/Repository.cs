using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Instagrao.Domain.Interfaces;
using System.Threading.Tasks;

namespace Instagrao.Repositories
{
    public class Repository : IRepository
    {
        private readonly AppSettings _appSettings;
        private readonly IAmazonDynamoDB _amazonDynamoDB;
        public Repository(
             IAmazonDynamoDB amazonDynamoDB
            ,AppSettings appSettings)
        {
            _amazonDynamoDB = amazonDynamoDB;
            _appSettings = appSettings;
        }

        public async Task<GetItemResponse> Get(GetItemRequest request)
        {
            request.TableName = _appSettings.DbName;
            return await _amazonDynamoDB.GetItemAsync(request);
        }
    }
}
