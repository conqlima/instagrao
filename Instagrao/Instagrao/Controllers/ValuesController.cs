using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;

namespace Instagrao.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private const string TableName = "commentsTable";
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id, [FromServices] IAmazonDynamoDB _amazonDynamoDB)
        {
            var request = new GetItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                     {
                      "id",
                      new AttributeValue
                      {
                        S = id.ToString()
                      }
                     }
                }
            };
            var response = await _amazonDynamoDB.GetItemAsync(request);
            return response.Item["username"].S;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
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
