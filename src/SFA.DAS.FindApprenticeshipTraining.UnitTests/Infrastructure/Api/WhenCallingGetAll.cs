using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.FindApprenticeshipTraining.Application.Configuration;
using SFA.DAS.FindApprenticeshipTraining.Application.Infrastructure.Api;
using SFA.DAS.FindApprenticeshipTraining.Application.Interfaces;

namespace SFA.DAS.FindApprenticeshipTraining.UnitTests.Infrastructure.Api
{
    public class WhenCallingGetAll
    {
        [Test, AutoData]
        public async Task Then_The_Endpoint_Is_Called(
            string authToken,
            CoursesApiConfiguration config)
        {
            //Arrange
            var azureClientCredentialHelper = new Mock<IAzureClientCredentialHelper>();
            azureClientCredentialHelper.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync(authToken);
            var configuration = new Mock<IOptions<CoursesApiConfiguration>>();
            config.Url = "https://test.local";
            configuration.Setup(x => x.Value).Returns(config);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new List<string>{"string","string"})),
                StatusCode = HttpStatusCode.Accepted
            };
            var getTestRequest = new GetAllTestRequest(config.Url) {BaseUrl = config.Url };
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, getTestRequest.GetAllUrl);
            var client = new HttpClient(httpMessageHandler.Object);
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            hostingEnvironment.Setup(x => x.EnvironmentName).Returns("Staging");
            var apiClient = new ApiClient(configuration.Object,client,hostingEnvironment.Object, azureClientCredentialHelper.Object);

            //Act
            var actual = await apiClient.GetAll<string>(getTestRequest);

            Assert.IsAssignableFrom<List<string>>(actual);
            //Assert
            httpMessageHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(HttpMethod.Get)
                        && c.RequestUri.AbsoluteUri.Equals(getTestRequest.GetAllUrl)
                        && c.Headers.Authorization.Scheme.Equals("Bearer")
                        && c.Headers.Authorization.Parameter.Equals(authToken)),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Test, AutoData]
         public async Task Then_The_Bearer_Token_Is_Not_Added_If_Local(
             CoursesApiConfiguration config)
         {
             //Arrange
             var configuration = new Mock<IOptions<CoursesApiConfiguration>>();
             config.Url = "https://test.local";
             configuration.Setup(x => x.Value).Returns(config);
             var response = new HttpResponseMessage
             {
                 Content = new StringContent(""),
                 StatusCode = HttpStatusCode.Accepted
             };
             var getTestRequest = new GetAllTestRequest(config.Url) {BaseUrl = config.Url };
             var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, getTestRequest.GetAllUrl);
             var client = new HttpClient(httpMessageHandler.Object);
             var hostingEnvironment = new Mock<IHostingEnvironment>();
             hostingEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
             var actual = new ApiClient(configuration.Object,client,hostingEnvironment.Object, Mock.Of<IAzureClientCredentialHelper>());

             //Act
             await actual.GetAll<string>(getTestRequest);
             
             //Assert
             httpMessageHandler.Protected()
                 .Verify<Task<HttpResponseMessage>>(
                     "SendAsync", Times.Once(),
                     ItExpr.Is<HttpRequestMessage>(c =>
                         c.Method.Equals(HttpMethod.Get)
                         && c.RequestUri.AbsoluteUri.Equals(getTestRequest.GetAllUrl)
                         && c.Headers.Authorization == null),
                     ItExpr.IsAny<CancellationToken>()
                 );
         }
        
        private class GetAllTestRequest : IGetAllApiRequest
        {
            public GetAllTestRequest (string baseUrl)
            {
                BaseUrl = baseUrl;
            }
            public string BaseUrl { get; set; }
            public string GetAllUrl => $"{BaseUrl}/test-url/get-all";
        }
        
    }
}