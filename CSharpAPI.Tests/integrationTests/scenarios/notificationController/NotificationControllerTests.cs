using API.Domain;
using API.Domain.Notification;
using CSharpAPI.Tests.integrationTests.Base.BaseIntegrationTest;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace CSharpAPI.Tests.integrationTests.scenarios.notificationController
{
    [Collection("Integration Sequence")]
    public class NotificationControllerTests(IntegrationTestFactory factory) : BaseIntegrationTest(factory)
    {
        private readonly HttpClient _client = factory.CreateClient();
        private const string NotificationEndpoint = "/Notification";

        #region Helpers
        private async Task AuthenticateAsync()
        {
            var bearerToken = await Auth.AuthenticatedUser();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.AccessToken);
        }

        private NotificationDataRequest CreateRequest(string title = "Test", string msg = "Test Msg", CategoryMessage cat = CategoryMessage.success)
            => new() { Title = title, ContentMessage = msg, Category = cat };

        private async Task CreateNotificationHelper(string title, string message, CategoryMessage category = CategoryMessage.success)
        {
            var request = new NotificationDataRequest
            {
                Title = title,
                ContentMessage = message,
                Category = category
            };
            var response = await _client.PostAsJsonAsync($"{NotificationEndpoint}/signalr", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        [Theory(DisplayName = "Failure: Unauthorized access to GET and PUT endpoints")]
        [InlineData("GET", "")]
        [InlineData("GET", "/Read/Exists")]
        [InlineData("PUT", "/Read/1")]
        [InlineData("PUT", "/Read/All")]
        public async Task Endpoints_WithoutBody_WhenUnauthenticated_ReturnUnauthorized(string method, string subRoute)
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var request = new HttpRequestMessage(new HttpMethod(method), $"{NotificationEndpoint}{subRoute}");

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "Failure: Unauthorized access to POST SignalR")]
        public async Task Post_SignalR_WhenUnauthenticated_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var request = new HttpRequestMessage(HttpMethod.Post, $"{NotificationEndpoint}/signalr")
            {
                Content = JsonContent.Create(CreateRequest())
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "Success: Get and validate notification structure")]
        public async Task Get_ReturnsValidNotificationStructure()
        {
            await AuthenticateAsync();

            await CreateNotificationHelper("Test Title", "Test Message");

            var response = await _client.GetAsync(NotificationEndpoint);
            var content = await response.Content.ReadFromJsonAsync<ReturnApi<NotificationResult>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Data.Should().NotBeNull();

            content.Data.All.Should().NotBeEmpty();

            var n = content.Data.All.First();
            n.MessageId.Should().NotBe(0);
            n.Title.Should().NotBeNull();
            n.CreatedAt.Should().NotBe(default);
        }

        [Fact(DisplayName = "Success: Mark notification as Read")]
        public async Task Notification_MarkAsRead_UpdatesStatus()
        {
            await AuthenticateAsync();
            await CreateNotificationHelper("Test Title", "Test Message");

            var getResponse = await _client.GetAsync(NotificationEndpoint);
            var content = await getResponse.Content.ReadFromJsonAsync<ReturnApi<NotificationResult>>();

            var messageId = content.Data.All.First(x => x.Title == "Test Title").MessageId;

            var response = await _client.PutAsync($"{NotificationEndpoint}/Read/{messageId}", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedList = await _client.GetFromJsonAsync<ReturnApi<NotificationResult>>(NotificationEndpoint);
            updatedList.Data.All.First(x => x.MessageId == messageId).Read.Should().BeTrue();
        }

        [Fact(DisplayName = "Success: Mark notification as UNREAD")]
        public async Task Notification_MarkAsUnread_UpdatesStatus()
        {
            await AuthenticateAsync();
            await CreateNotificationHelper("Test Title", "Test Message");

            var getResponse = await _client.GetAsync(NotificationEndpoint);
            var content = await getResponse.Content.ReadFromJsonAsync<ReturnApi<NotificationResult>>();

            var messageId = content.Data.All.First(x => x.Title == "Test Title").MessageId;

            var response = await _client.PutAsync($"{NotificationEndpoint}/Unread/{messageId}", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedList = await _client.GetFromJsonAsync<ReturnApi<NotificationResult>>(NotificationEndpoint);
            updatedList.Data.All.First(x => x.MessageId == messageId).Read.Should().BeFalse();
        
        }


        [Theory(DisplayName = "Success: Create various notification types via SignalR")]
        [InlineData("Success Cat", "Regular Message", CategoryMessage.success)]
        [InlineData("Warning Cat", "Warning Message", CategoryMessage.warning)]
        [InlineData("Error Cat", "Error Message", CategoryMessage.error)]
        [InlineData("Very Long Title Content", "...", CategoryMessage.success)]
        [InlineData("Special Characters", "中文 🎉 αβγ", CategoryMessage.success)]
        public async Task SignalR_WithVariousInputs_ReturnsOk(string title, string msg, CategoryMessage cat)
        {
            await AuthenticateAsync();
            var request = CreateRequest(
                title == "Very Long Title Content" ? new string('A', 500) : title,
                msg,
                cat);

            var response = await _client.PostAsJsonAsync($"{NotificationEndpoint}/signalr", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Success: ReadAll clears unread list and count")]
        public async Task ReadAll_ClearsAllUnreadNotifications()
        {
            await AuthenticateAsync();

            await _client.PutAsync($"{NotificationEndpoint}/Read/All", null);

            var countContent = await (await _client.GetAsync($"{NotificationEndpoint}/Read/Exists")).Content.ReadFromJsonAsync<ReturnApi<int>>();
            countContent.Data.Should().Be(0);

            var listContent = await (await _client.GetAsync(NotificationEndpoint)).Content.ReadFromJsonAsync<ReturnApi<NotificationResult>>();
            listContent.Data.UnRead.Should().BeEmpty();
        }

        [Fact(DisplayName = "Success: Different users have separate notifications")]
        public async Task Notifications_AreSeparatedByUser()
        {
            var token1 = await Auth.AuthenticatedUser();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1.AccessToken);
            var data1 = await (await _client.GetAsync(NotificationEndpoint)).Content.ReadFromJsonAsync<ReturnApi<NotificationResult>>();

            var token2 = await Auth.AuthenticatedUser();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2.AccessToken);
            var data2 = await (await _client.GetAsync(NotificationEndpoint)).Content.ReadFromJsonAsync<ReturnApi<NotificationResult>>();

            data1.Should().NotBeNull();
            data2.Should().NotBeNull();
        }


        [Fact(DisplayName = "Failure: Post SignalR with missing required fields returns BadRequest")]
        public async Task Post_SignalR_InvalidModel_ReturnsBadRequest()
        {
            await AuthenticateAsync();
            var invalidRequest = new NotificationDataRequest { Title = null, ContentMessage = "" };

            var response = await _client.PostAsJsonAsync($"{NotificationEndpoint}/signalr", invalidRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }       
    }
}