using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ovicus.ShowMyHomework.Tests
{
    public class ShowMyHomeworkClientTests
    {
        private const string ApiBaseUrl = "https://api.showmyhomework.co.uk";

        [Fact]
        public async Task GetTodos_When_AskedToIncludeUncompletedItems_Should_ReturnAllItems()
        {
            // ARRANGE
            string accessToken = "abcdAccessToken";
            var responseBody = BuildGetTodosRespose();

            Mock<HttpMessageHandler> handlerMock = MockHttpMessageHandlerBuilder.Build(responseBody, HttpStatusCode.OK);
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object);

            var sut = new ShowMyHomeworkClient(accessToken, ApiBaseUrl, httpClient);

            // ACT
            var result = await sut.GetTodos(includeUncompleted: true);

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Both completed and uncompleted

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get  // we expected a GET request
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetTodos_Should_ReturnUncompletedItemsOnlyByDefault()
        {
            // ARRANGE
            var accessToken = "abcdAccessToken";
            var responseBody = BuildGetTodosRespose();

            Mock<HttpMessageHandler> handlerMock = MockHttpMessageHandlerBuilder.Build(responseBody, HttpStatusCode.OK);
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object);

            var sut = new ShowMyHomeworkClient(accessToken, ApiBaseUrl, httpClient);

            // ACT
            var result = await sut.GetTodos();

            // ASSERT
            result.Should().NotBeNull();
            result.Should().HaveCount(1); // Not completed only
            result.First().Title.Should().BeEquivalentTo("E-Safety Project");

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get  // we expected a GET request
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        private static string BuildGetTodosRespose()
        {
            string responseBody = "{" +
                "\"todos\": [" +
                    "{" +
                        "\"class_group_name\": \"7R16\"," +
                        "\"class_task_description\": \"<p>This is the description of the task</p>\"," +
                        "\"class_task_id\": 343434," +
                        "\"class_task_title\": \"Fertilisation\"," +
                        "\"class_task_type\": \"Quiz\"," +
                        "\"completed\": true," +
                        "\"due_on\": \"2019-11-04T00:00:00+00:00\"," +
                        "\"issued_on\": \"2019-10-15T00:00:00+00:00\"," +
                        "\"id\": 89898," +
                        "\"subject\": \"Science\"," +
                        "\"teacher_name\": \"Miss O. Jones\"," +
                        "\"user_id\": 1234," +
                        "\"has_attachments\": false," +
                        "\"submission_status\": \"submitted\"," +
                        "\"submission_type\": \"online_submission\"" +
                    "}," +
                    "{ " +
                        "\"class_group_name\": \"7MA\"," +
                        "\"class_task_description\": \"<p>All E-Saftey Projects are due next week during class.</p>\"," +
                        "\"class_task_id\": 232323," +
                        "\"class_task_title\": \"E-Safety Project\"," +
                        "\"class_task_type\": \"Homework\"," +
                        "\"completed\": false," +
                        "\"due_on\": \"2019-10-18T00:00:00+00:00\"," +
                        "\"issued_on\": \"2019-10-08T00:00:00+00:00\"," +
                        "\"id\": 67676," +
                        "\"subject\": \"Computer Science\"," +
                        "\"teacher_name\": \"Mr J. Doe\"," +
                        "\"user_id\": 1234," +
                        "\"has_attachments\": false," +
                        "\"submission_status\": null," +
                        "\"submission_type\": \"class_submission\"" +
                    "}]}";

            return responseBody;
        }
    }
}
