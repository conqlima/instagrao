using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Instagrao
{
    public class Function
    {
        private const string TableName = "ImageMetadata2";
        private readonly IAmazonS3 _S3Client;
        private readonly IAmazonDynamoDB _amazonDynamoDB;

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            _S3Client = new AmazonS3Client();
            var serviceColletion = new ServiceCollection();
            ConfigureServices(serviceColletion);
            var serviceProvider = serviceColletion.BuildServiceProvider();
            _amazonDynamoDB = (IAmazonDynamoDB)serviceProvider.GetService(typeof(IAmazonDynamoDB));
        }

        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(
            IAmazonS3 s3Client,
            IAmazonDynamoDB amazonDynamoDB)
        {
            _S3Client = s3Client;
            _amazonDynamoDB = amazonDynamoDB;
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandlerAsync(S3Event s3Event, ILambdaContext context)
        {
            foreach (var record in s3Event.Records)
            {
                using var objectResponse = await _S3Client.GetObjectAsync(record.S3.Bucket.Name, record.S3.Object.Key);
                using Stream responseStream = objectResponse.ResponseStream;
                Image image = Image.Load(responseStream);
                var width = image.Width;
                var height = image.Height;
                var length = responseStream.Length;

                var request = new PutItemRequest
                {
                    TableName = TableName,
                    Item = new Dictionary<string, AttributeValue>
                        {
                            { "ImageMetadataId", new AttributeValue { S = record.S3.Object.Key }},
                            { "Length", new AttributeValue { N = length.ToString() }},
                            { "Width", new AttributeValue { N = width.ToString() }},
                            { "Height", new AttributeValue { N = height.ToString() }}
                        }
                };

                try
                {
                    await _amazonDynamoDB.PutItemAsync(request);
                }
                catch (Exception e)
                {
                    throw e;
                }

                image.Dispose();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAmazonDynamoDB>(sp =>
            {
                var clientConfig = new AmazonDynamoDBConfig { ServiceURL = "http://localhost:8000" };
                return new AmazonDynamoDBClient(clientConfig);
            });
        }
    }
}
