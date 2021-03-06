﻿using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.SharedOuterApi.Infrastructure;
using SFA.DAS.SharedOuterApi.Interfaces;

namespace SFA.DAS.SharedOuterApi.UnitTests.Infrastructure.Api
{
    public class WhenCallingGet
    {
        [Test, AutoData]
        public async Task Then_The_Endpoint_Is_Called(
            string authToken,
            int id,
            TestInnerApiConfiguration config)
        {
            //Arrange
            var azureClientCredentialHelper = new Mock<IAzureClientCredentialHelper>();
            azureClientCredentialHelper.Setup(x => x.GetAccessTokenAsync(config.Identifier)).ReturnsAsync(authToken);
            config.Url = "https://test.local";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.Accepted
            };
            var getTestRequest = new GetTestRequest(config.Url, id) {BaseUrl = config.Url };
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, getTestRequest.GetUrl);
            var client = new HttpClient(httpMessageHandler.Object);
            var hostingEnvironment = new Mock<IWebHostEnvironment>();
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            
            hostingEnvironment.Setup(x => x.EnvironmentName).Returns("Staging");
            var actual = new ApiClient<TestInnerApiConfiguration>(clientFactory.Object, config,hostingEnvironment.Object, azureClientCredentialHelper.Object);

            //Act
            await actual.Get<string>(getTestRequest);

            //Assert
            httpMessageHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(HttpMethod.Get)
                        && c.RequestUri.AbsoluteUri.Equals(getTestRequest.GetUrl)
                        && c.Headers.Authorization.Scheme.Equals("Bearer")
                        && c.Headers.FirstOrDefault(h=>h.Key.Equals("X-Version")).Value.FirstOrDefault() == "2.0"
                        && c.Headers.Authorization.Parameter.Equals(authToken)),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Test, AutoData]
        public async Task Then_Multiple_Calls_Do_Not_Result_In_Multiple_Versions_Added(string authToken,
            int id,
            TestInnerApiConfiguration config)
        {
            //Arrange
            var azureClientCredentialHelper = new Mock<IAzureClientCredentialHelper>();
            azureClientCredentialHelper.Setup(x => x.GetAccessTokenAsync(config.Identifier)).ReturnsAsync(authToken);
            config.Url = "https://test.local";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.Accepted
            };
            var getTestRequest = new GetTestRequest(config.Url, id) {BaseUrl = config.Url };
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, getTestRequest.GetUrl);
            var client = new HttpClient(httpMessageHandler.Object);
            var hostingEnvironment = new Mock<IWebHostEnvironment>();
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            
            hostingEnvironment.Setup(x => x.EnvironmentName).Returns("Staging");
            var actual = new ApiClient<TestInnerApiConfiguration>(clientFactory.Object, config,hostingEnvironment.Object, azureClientCredentialHelper.Object);

            //Act
            await actual.Get<string>(getTestRequest);
            await actual.Get<string>(getTestRequest);
            
            //Assert
            httpMessageHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync", Times.Exactly(2),
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(HttpMethod.Get)
                        && c.RequestUri.AbsoluteUri.Equals(getTestRequest.GetUrl)
                        && c.Headers.Authorization.Scheme.Equals("Bearer")
                        && c.Headers.FirstOrDefault(h=>h.Key.Equals("X-Version")).Value.Single() == "2.0"
                        && c.Headers.Authorization.Parameter.Equals(authToken)),
                    ItExpr.IsAny<CancellationToken>()
                );
        }
        
        [Test, AutoData]
         public async Task Then_The_Bearer_Token_Is_Not_Added_If_Local_And_Default_Version_If_Not_Specified(
             int id,
             TestInnerApiConfiguration config)
         {
             //Arrange
             config.Url = "https://test.local";
             var configuration = config;
             var response = new HttpResponseMessage
             {
                 Content = new StringContent(""),
                 StatusCode = HttpStatusCode.Accepted
             };
             var getTestRequest = new GetTestRequestNoVersion(config.Url,id) {BaseUrl = config.Url };
             var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, getTestRequest.GetUrl);
             var client = new HttpClient(httpMessageHandler.Object);
             var hostingEnvironment = new Mock<IWebHostEnvironment>();
             var clientFactory = new Mock<IHttpClientFactory>();
             clientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
             
             hostingEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
             var actual = new ApiClient<TestInnerApiConfiguration>(clientFactory.Object,configuration,hostingEnvironment.Object, Mock.Of<IAzureClientCredentialHelper>());

             //Act
             await actual.Get<string>(getTestRequest);
             
             //Assert
             httpMessageHandler.Protected()
                 .Verify<Task<HttpResponseMessage>>(
                     "SendAsync", Times.Once(),
                     ItExpr.Is<HttpRequestMessage>(c =>
                         c.Method.Equals(HttpMethod.Get)
                         && c.Headers.FirstOrDefault(h=>h.Key.Equals("X-Version")).Value.FirstOrDefault() == "1.0"
                         && c.RequestUri.AbsoluteUri.Equals(getTestRequest.GetUrl)
                         && c.Headers.Authorization == null),
                     ItExpr.IsAny<CancellationToken>()
                 );
         }
        
        private class GetTestRequest : IGetApiRequest
        {
            private readonly int _id;

            public string Version => "2.0";

            public GetTestRequest (string baseUrl, int id)
            {
                _id = id;
                BaseUrl = baseUrl;
            }
            public string BaseUrl { get; set; }
            public string GetUrl => $"{BaseUrl}/test-url/get{_id}";
        }
        private class GetTestRequestNoVersion : IGetApiRequest
        {
            private readonly int _id;

            public GetTestRequestNoVersion (string baseUrl, int id)
            {
                _id = id;
                BaseUrl = baseUrl;
            }
            public string BaseUrl { get; set; }
            public string GetUrl => $"{BaseUrl}/test-url/get{_id}";
        }

        
    }
}
