using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.SharedOuterApi.Infrastructure;
using SFA.DAS.SharedOuterApi.Interfaces;

namespace SFA.DAS.SharedOuterApi.UnitTests.Infrastructure.Api
{
    public class WhenCallingDeleteResponse
    {
        [Test, AutoData]
        public async Task Then_The_Endpoint_Is_Called(
            string authToken,
            int id,
            string responseContent,
            TestInnerApiConfiguration config)
        {
            //Arrange
            var azureClientCredentialHelper = new Mock<IAzureClientCredentialHelper>();
            azureClientCredentialHelper.Setup(x => x.GetAccessTokenAsync(config.Identifier)).ReturnsAsync(authToken);
            config.Url = "https://test.local";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.Created
            };
            var deleteTestReequest = new DeleteTestRequest(config.Url, id) {BaseUrl = config.Url};
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, deleteTestReequest.DeleteUrl, "delete");
            var client = new HttpClient(httpMessageHandler.Object);
            var hostingEnvironment = new Mock<IWebHostEnvironment>();
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            
            hostingEnvironment.Setup(x => x.EnvironmentName).Returns("Staging");
            var actual = new ApiClient<TestInnerApiConfiguration>(clientFactory.Object, config,hostingEnvironment.Object, azureClientCredentialHelper.Object);

            //Act
            await actual.Delete(deleteTestReequest);

            //Assert
            httpMessageHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(HttpMethod.Delete)
                        && c.RequestUri.AbsoluteUri.Equals(deleteTestReequest.DeleteUrl)
                        && c.Headers.Authorization.Scheme.Equals("Bearer")
                        && c.Headers.FirstOrDefault(h=>h.Key.Equals("X-Version")).Value.FirstOrDefault() == "2.0"
                        && c.Headers.Authorization.Parameter.Equals(authToken)),
                    ItExpr.IsAny<CancellationToken>()
                );
            
        }
        
        private class DeleteTestRequest : IDeleteApiRequest
        {
            private readonly int _id;

            public string Version => "2.0";

            public DeleteTestRequest (string baseUrl, int id)
            {
                _id = id;
                BaseUrl = baseUrl;
            }
            public string BaseUrl { get; set; }
            public string DeleteUrl => $"{BaseUrl}/test-url/get{_id}";
        }
    }
}