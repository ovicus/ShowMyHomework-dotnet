using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ovicus.ShowMyHomework.Tests
{
    internal static class MockHttpMessageHandlerBuilder
    {
        internal static Mock<HttpMessageHandler> Build(string responseBody, HttpStatusCode statusCode)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.UpdateResponse(responseBody, statusCode);
            return handlerMock;
        }
        
        internal static void UpdateResponse(this Mock<HttpMessageHandler> handlerMock, string responseBody, HttpStatusCode statusCode)
        {
            handlerMock
                    .Protected()
                    // Setup the PROTECTED method to mock
                    .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                    )
                    // prepare the expected response of the mocked http call
                    .ReturnsAsync(new HttpResponseMessage()
                    {
                        StatusCode = statusCode,
                        Content = new StringContent(responseBody),
                    })
                    .Verifiable();
        }
    }
}
