using FluentAssertions;
using Moq;
using Moq.Protected;
using Ovicus.ShowMyHomework.Auth;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ovicus.ShowMyHomework.Tests
{
    public class AuthenticationServiceTests
    {
        private const string ApiBaseUrl = "https://api.showmyhomework.co.uk";

        [Fact]
        public async Task When_FindSchoolsCalledWithValidFilter_Then_ActiveMatchingSchoolsShouldBeReturned()
        {
            // ARRANGE
            string responseBody = "{" +
                 "\"schools\": [" +
                     "{" +
                         "\"id\": 1234," +
                         "\"name\": \"Kings Langley School\"," +
                         "\"address\": \"Love Lane, Hertfordshire\"," +
                         "\"town\": \"Kings Langley\"," +
                         "\"post_code\": \"WD4 9HN\"," +
                         "\"subdomain\": \"kingslangley\"," +
                         "\"is_active\": true," +
                     "}," +
                     "{" +
                         "\"id\": 1235," +
                         "\"name\": \"Kingsley Academy\"," +
                         "\"address\": \"Cecil Road\"," +
                         "\"town\": \"Hounslow\"," +
                         "\"post_code\": \"TW3 1AX\"," +
                         "\"subdomain\": \"kingsleyacademy\"," +
                         "\"is_active\": false," +
                     "}," +
                     "{ " +
                         "\"id\": 1236," +
                         "\"name\": \"Kingsdale Foundation School\"," +
                         "\"address\": \"Alleyn Park, Dulwich\"," +
                         "\"town\": \"London\"," +
                         "\"post_code\": \"SE21 8SQ\"," +
                         "\"subdomain\": \"kingsdale\"," +
                         "\"is_active\": true," +
                     "}]}";

            Mock<HttpMessageHandler> handlerMock = MockHttpMessageHandlerBuilder.Build(responseBody, HttpStatusCode.OK);
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object);

            var sut = new AuthenticationService(ApiBaseUrl, httpClient);

            // ACT
            var result = await sut.FindSchools("Kings");

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.IsActive);

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get  // we expected a POST request
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task When_FindSchoolsCalledWithInvalidFilter_Then_ShouldReturnNoResults()
        {
            // ARRANGE
            string responseBody = "{ \"schools\": [] }";

            Mock<HttpMessageHandler> handlerMock = MockHttpMessageHandlerBuilder.Build(responseBody, HttpStatusCode.OK);
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object);

            var sut = new AuthenticationService(ApiBaseUrl, httpClient);

            // ACT
            var result = await sut.FindSchools("NonExistantSchoolName");

            // ASSERT
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get  // we expected a POST request
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task When_ValidCredentialsAreProvided_Then_AuthenticationSucceedAndAccessTokenCreated()
        {
            // ARRANGE
            string accessToken = "abcdAccessToken";
            string responseBody = "{" +
                        "\"access_token\": \""+ accessToken + "\"," +
                        "\"token_type\": \"bearer\"," +
                        "\"expires_in\": 7199," +
                        "\"refresh_token\": \"abcdRefreshToken\"," +
                        "\"created_at\": " + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "," +
                        "\"user_id\": 456789," +
                        "\"school_id\": 1234," +
                        "\"user_type\": \"student\"," +
                        "\"smhw_token\": \"" + accessToken + "\"}";
            
            Mock<HttpMessageHandler> handlerMock = MockHttpMessageHandlerBuilder.Build(responseBody, HttpStatusCode.OK);
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object);

            var sut = new AuthenticationService(ApiBaseUrl, httpClient);

            // ACT
            var result = await sut.Authenticate("username", "$3cret", 1234);
            var token = await sut.GetAccessToken();

            // ASSERT
            result.Should().BeTrue();
            token.Should().NotBeNullOrEmpty();
            token.Should().BeEquivalentTo(accessToken);

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a POST request
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task When_InvalidCredentialsAreProvided_Then_AuthenticationFailsAndAccessTokenIsNotCreated()
        {
            // ARRANGE
            string responseBody = "{\"errors\":{\"identification\":[\"invalid_credentials\"]}}";

            Mock<HttpMessageHandler> handlerMock = MockHttpMessageHandlerBuilder.Build(responseBody, HttpStatusCode.Unauthorized);
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object);

            var sut = new AuthenticationService(ApiBaseUrl, httpClient);

            // ACT
            var result = await sut.Authenticate("username", "inv@lidPass", 1234);

            // ASSERT
            result.Should().BeFalse();
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetAccessToken());

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a POST request
               ),
               ItExpr.IsAny<CancellationToken>()
            );

        }

        [Fact]
        public async Task When_TokenIsRequested_BeforeAuthenticate_Should_ThrowException()
        {
            // ARRANGE
            Mock<HttpMessageHandler> handlerMock = MockHttpMessageHandlerBuilder.Build(string.Empty, HttpStatusCode.OK);
            HttpClient httpClient = new HttpClient(handlerMock.Object);

            var sut = new AuthenticationService(ApiBaseUrl, httpClient);

            // ACT and ASSERT
            // Authenticate() should be called first to generate a token
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetAccessToken());

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Never(),
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
            );

        }

        [Fact]
        public async Task When_TokenIsExpired_Should_TryRefresh()
        {
            // ARRANGE (first stage)
            string accessToken = "abcdAccessToken";
            
            // Simulate an expired token, returning an already expired token in first place
            // This is not what will happen when using the real authentication endpoint,
            // but it serves to the purpose of this test
            string responseBody = "{" +
                        "\"access_token\": \"" + accessToken + "\"," +
                        "\"token_type\": \"bearer\"," +
                        "\"expires_in\": 7199," +
                        "\"refresh_token\": \"abcdRefreshToken\"," +
                        "\"created_at\": " + DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds() + "," +
                        "\"user_id\": 456789," +
                        "\"school_id\": 1234," +
                        "\"user_type\": \"student\"," +
                        "\"smhw_token\": \"" + accessToken + "\"}";


            Mock<HttpMessageHandler> handlerMock = MockHttpMessageHandlerBuilder.Build(responseBody, HttpStatusCode.OK);
            HttpClient httpClient = new HttpClient(handlerMock.Object);

            var sut = new AuthenticationService(ApiBaseUrl, httpClient);

            // ACT (first stage)
            
            // This will make a request to the authentication endpoint and retrieve the (expired) token
            // This method does not check the validity of the token, just simply trust
            // the authentication server will issue a valid token on successful authentication
            var authResult = await sut.Authenticate("username", "$3cret", 1234);

            // ARRANGE (2nd stage)
            
            // Verify the current token, previously obtained through the authentication process,
            // is still valid. If the token has expired, it will issue a request for the authentication endpoint
            // to refresh the token. In this case, call is made to the endpoint to refresh the expired token
            // and a new fresh token is returned.

            accessToken = "newAccessToken123";
            responseBody = "{" +
                        "\"access_token\": \"" + accessToken + "\"," +
                        "\"token_type\": \"bearer\"," +
                        "\"expires_in\": 7199," +
                        "\"refresh_token\": \"abcdRefreshToken\"," +
                        "\"created_at\": " + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "," +
                        "\"user_id\": 456789," +
                        "\"school_id\": 1234," +
                        "\"user_type\": \"student\"," +
                        "\"smhw_token\": \"" + accessToken + "\"}";

            handlerMock.UpdateResponse(responseBody, HttpStatusCode.OK);

            // ACT (sencond stage)
            // This call will find the current token has expired and send a request to get a new token
            var token = await sut.GetAccessToken();

            // ASSERT
            authResult.Should().BeTrue();
            token.Should().NotBeNullOrEmpty();
            token.Should().BeEquivalentTo(accessToken); // The access token should match the new one

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(2), // first during authentication first time, and second to refresh the expired token
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a POST request
               ),
               ItExpr.IsAny<CancellationToken>()
            );

        }
    }
}
