using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Instagrao.Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Instagrao.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private const string TableName = "commentsTable";
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(string id, [FromServices] IAmazonDynamoDB _amazonDynamoDB)
        {
            var request = new GetItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                     {
                      "id", new AttributeValue { S = id }
                     }
                }
            };
            var response = await _amazonDynamoDB.GetItemAsync(request);
            return response.Item["username"].S;
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] CreateEntry value, [FromServices] IAmazonDynamoDB _amazonDynamoDB)
        {
            var request = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue { S = value.Id }},
                    { "username", new AttributeValue { S = value.Value }}
                }
            };

            await _amazonDynamoDB.PutItemAsync(request);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
