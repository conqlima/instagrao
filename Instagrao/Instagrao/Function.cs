using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Instagrao.Domain.DTO;
using Instagrao.Domain.Interfaces;
using Instagrao.Repositories;
using Instagrao.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Instagrao
{
    public class Function
    {
        private readonly IAmazonS3 _S3Client;
        private readonly IGetService getService;
        private readonly IRepository repository;

        private const string ID_QUERY_STRING_NAME = "s3ObjectKey";

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            _S3Client = new AmazonS3Client();

            var serviceColletion = new ServiceCollection();

            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            ConfigureServices(serviceColletion, configuration);

            var serviceProvider = serviceColletion.BuildServiceProvider();
            getService = (IGetService)serviceProvider.GetService(typeof(IGetService));
            repository = (IRepository)serviceProvider.GetService(typeof(IRepository));

        }

        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(IAmazonS3 s3Client)
        {
            _S3Client = s3Client;
        }

        /// <summary>
        /// A Lambda function that extracts metadata from every image uploaded to S3 Bucket.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ExtractMetadataAsync(S3Event s3Event, ILambdaContext context)
        {
            if (s3Event.Records != null)
            {
                foreach (var record in s3Event.Records)
                {
                    try
                    {
                        context.Logger.LogLine(HttpUtility.UrlDecode(record.S3.Object.Key));
                        using GetObjectResponse objectResponse = await _S3Client.GetObjectAsync(record.S3.Bucket.Name, HttpUtility.UrlDecode(record.S3.Object.Key));

                        using Stream responseStream = objectResponse.ResponseStream;
                        using Image image = Image.Load(responseStream);
                        var width = image.Width;
                        var height = image.Height;
                        var length = responseStream.Length;

                        var request = new PutItemRequest
                        {
                            Item = new Dictionary<string, AttributeValue>
                            {
                                { "ImageMetadataId", new AttributeValue { S = HttpUtility.UrlDecode(record.S3.Object.Key) }},
                                { "Length", new AttributeValue { N = length.ToString() }},
                                { "Width", new AttributeValue { N = width.ToString() }},
                                { "Height", new AttributeValue { N = height.ToString() }}
                            }
                        };
                        await repository.Put(request);
                    }
                    catch (Exception e)
                    {
                        context.Logger.LogLine(e.Message);
                    }
                }
            }

        }

        /// <summary>
        /// A Lambda function that returns images metadata from S3 Bucket by s3ObjectKey.
        /// path for http request: /{s3ObjectKey}
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> GetMetadataAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string s3ObjectKey = GetRequestParams(request);

            if (string.IsNullOrEmpty(s3ObjectKey))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {ID_QUERY_STRING_NAME}"
                };
            }

            context.Logger.LogLine($"Getting object {s3ObjectKey}");
            try
            {
                var resultado = await getService.Get(s3ObjectKey);
                context.Logger.LogLine($"Found object: {resultado.ImageMetadataId != null}");

                var response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = JsonConvert.SerializeObject(resultado),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
                return response;
            }
            catch (Exception e)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Body = e.Message
                };
            }
        }

        /// <summary>
        /// A Lambda function that downloads the image from S3 Bucket by s3ObjectKey.
        /// path for http request: download/{s3ObjectKey}
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> GetImageAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string s3ObjectKey = GetRequestParams(request);

            if (string.IsNullOrEmpty(s3ObjectKey))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {ID_QUERY_STRING_NAME}"
                };
            }

            context.Logger.LogLine($"Getting object {s3ObjectKey}");
            try
            {
                using GetObjectResponse objectResponse = await _S3Client.GetObjectAsync("conqlimabucket", s3ObjectKey);
                context.Logger.LogLine($"Found object: {s3ObjectKey != null}");

                var response = new APIGatewayProxyResponse
                {
                    IsBase64Encoded = true,
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = Convert.ToBase64String(ReadStream(objectResponse.ResponseStream)),
                    Headers = new Dictionary<string, string> { { "Content-Type", "*/*" } }
                };

                return response;
            }
            catch (Exception e)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Body = e.Message
                };
            }
        }

        /// <summary>
        /// A Lambda function that gets the biggest, smallest and the images extensions info about
        /// all the S3 Bucket imges.
        /// path for http request: /
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> InfoImageAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var info = new InfoImage
            {
                BiggestImage = await getService.GetBiggestImage(),
                SmallestImage = await getService.GetSmallestImage(),
                ImageExtensions = await getService.GetImagesExtensions()
            };
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(info),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            return response;
        }
        
        private static byte[] ReadStream(Stream responseStream)
        {
            using MemoryStream ms = new MemoryStream();
            responseStream.CopyTo(ms);
            return ms.ToArray();
        }

        private string GetRequestParams(APIGatewayProxyRequest request)
        {
            string s3ObjectKey = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(ID_QUERY_STRING_NAME))
                s3ObjectKey = request.PathParameters[ID_QUERY_STRING_NAME];
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(ID_QUERY_STRING_NAME))
                s3ObjectKey = request.QueryStringParameters[ID_QUERY_STRING_NAME];
            return HttpUtility.UrlDecode(s3ObjectKey);
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var dynamoDbConfig = configuration.GetSection("DynamoDb");
            bool runLocalDynamoDb = dynamoDbConfig.GetValue<bool>("LocalMode");

            if (runLocalDynamoDb)
            {
                services.AddSingleton<IAmazonDynamoDB>(sp =>
                {
                    var clientConfig = new AmazonDynamoDBConfig { ServiceURL = dynamoDbConfig.GetValue<string>("LocalServiceUrl") };
                    return new AmazonDynamoDBClient(clientConfig);
                });
            }
            else
            {
                services.AddAWSService<IAmazonDynamoDB>();
            }

            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
            services.AddSingleton(configuration.GetSection("DynamoDb").Get<AppSettings>());
            services.AddServices();
            services.AddRepositories();
        }
    }
}
