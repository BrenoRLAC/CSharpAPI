using API.Domain;
using API.Domain.Hero;
using CSharpAPI.Tests.integrationTests.Base.BaseIntegrationTest;
using CSharpAPI.Tests.integrationTests.helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CSharpAPI.Tests.integrationTests.scenarios.hero
{
    [Collection("Integration Sequence")]
    public class HeroControllerTests(IntegrationTestFactory factory) : BaseIntegrationTest(factory)
    {
        private readonly HttpClient _client = factory.CreateClient();
        private const string HeroEndpoint = "/Hero";

        #region Helpers
        private async Task AuthenticateAsync()
        {
            var bearerToken = await Auth.AuthenticatedUser();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.AccessToken);
        }

        private void ClearAuthentication() => _client.DefaultRequestHeaders.Authorization = null;
        private async Task<string> GetValidHeroIdAsync()
        {
            await AuthenticateAsync();
            var getResp = await _client.GetAsync(HeroEndpoint);
            var content = await getResp.Content.ReadFromJsonAsync<ReturnApiPaged<List<HeroResult>>>();

            if (content?.Data?.Any() != true)
            {
                await _client.PostAsJsonAsync(HeroEndpoint, DataGenerator.GetValidHeroRequest());
                var retry = await (await _client.GetAsync(HeroEndpoint)).Content.ReadFromJsonAsync<ReturnApiPaged<List<HeroResult>>>();
                return retry!.Data.First().Id;
            }

            return content.Data.First().Id;
        }

        private async Task<(string HeroId, string ImageId)> GetValidHeroWithImageId()
        {
            await AuthenticateAsync();

            var heroId = await GetValidHeroIdAsync();

            var response = await _client.GetAsync($"{HeroEndpoint}/heroes?id={Uri.EscapeDataString(heroId)}");
            var content = await response.Content.ReadFromJsonAsync<ReturnApi<HeroResult>>();

            var existingImage = content?.Data?.Images?.FirstOrDefault();

            if (existingImage != null)
            {
                return (heroId, existingImage.PublicId);
            }

            using var formData = new MultipartFormDataContent();
            formData.Add(DataGenerator.ImageContent(), "images", "integration_test.png");

            var uploadResp = await _client.PostAsync($"{HeroEndpoint}/image?id={heroId}", formData);
            uploadResp.EnsureSuccessStatusCode();

            var retryResp = await _client.GetAsync($"{HeroEndpoint}/heroes?id={Uri.EscapeDataString(heroId)}");
            var retryContent = await retryResp.Content.ReadFromJsonAsync<ReturnApi<HeroResult>>();

            return (heroId, retryContent!.Data!.Images!.First().PublicId);
        }

        private async Task<string> PostValidHeroIdAsync()
        {
            await AuthenticateAsync();

            await _client.PostAsJsonAsync(HeroEndpoint, DataGenerator.GetValidHeroRequest());
            var retry = await (await _client.GetAsync(HeroEndpoint)).Content.ReadFromJsonAsync<ReturnApiPaged<List<HeroResult>>>();
            return retry!.Data.First().Id;
        }
        #endregion

        #region Read Operations & Access Control
        [Theory(DisplayName = "Access Control: Anonymous vs Authenticated Get")]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetHeroes_ReturnsOk_RegardlessOfAuth(bool authenticated)
        {
            if (authenticated) await AuthenticateAsync();
            else ClearAuthentication();

            var response = await _client.GetAsync(HeroEndpoint);
            var content = await response.Content.ReadFromJsonAsync<ReturnApiPaged<List<HeroResult>>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNull();
            content!.StatusCode.Should().Be(200);
        }

        [Theory(DisplayName = "Success: Hero filtering and pagination")]
        [InlineData("Superman", 1, 10)]
        [InlineData("", 2, 5)]
        public async Task GetHeroes_WithParams_ReturnsCorrectResults(string name, int page, int size)
        {
            var url = $"{HeroEndpoint}?Name={name}&Page={page}&PageSize={size}";

            var response = await _client.GetAsync(url);
            var content = await response.Content.ReadFromJsonAsync<ReturnApiPaged<List<HeroResult>>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content!.PaginationResult.Should().NotBeNull();
        }

        [Fact(DisplayName = "Success: Get Hero by valid Id returns OK")]
        public async Task GetHeroById_WithValidId_ReturnsOk()
        {
            var validId = await GetValidHeroIdAsync();

            var response = await _client.GetAsync($"{HeroEndpoint}/heroes?id={Uri.EscapeDataString(validId)}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Failure: Get Hero by invalid Id returns error")]
        public async Task GetHeroById_WithInvalidId_ReturnsError()
        {
            var validId = await GetValidHeroIdAsync();
            var invalidId = validId[..^1] + (validId.EndsWith('a') ? 'b' : 'a');

            var response = await _client.GetAsync($"{HeroEndpoint}/heroes?id={Uri.EscapeDataString(invalidId)}");

            response.StatusCode.Should().Match(s => (int)s >= 400);
        }
        #endregion

        #region Hero Lifecycle (Create, Update, Delete)
        [Fact(DisplayName = "Success: Create new Hero")]
        public async Task CreateHero_ReturnsOk()
        {
            await AuthenticateAsync();
            var request = DataGenerator.GetValidHeroRequest();

            var response = await _client.PostAsJsonAsync(HeroEndpoint, request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Success: Update existing Hero")]
        public async Task UpdateHero_ReturnsOk()
        {
            var id = await GetValidHeroIdAsync();
            var request = DataGenerator.GetValidHeroRequest();

            var response = await _client.PutAsJsonAsync($"{HeroEndpoint}?id={id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Success: Delete existing Hero")]
        public async Task DeleteHero_ReturnsOk()
        {
            var id = await GetValidHeroIdAsync();
            var request = new HttpRequestMessage(HttpMethod.Delete, HeroEndpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(id), Encoding.UTF8, "application/json")
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        #endregion

        #region Address Operations
        [Fact(DisplayName = "Success: Create hero address returns OK")]
        public async Task PostHeroAddress_WithValidData_ReturnsOk()
        {
            var id = await PostValidHeroIdAsync();
            var request = DataGenerator.GetValidAddressRequest();

            var response = await _client.PostAsJsonAsync($"{HeroEndpoint}/address?id={id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Success: Update hero address returns OK")]
        public async Task PutHeroAddress_WithValidData_ReturnsOk()
        {
            var id = await GetValidHeroIdAsync();
            var request = DataGenerator.GetValidAddressRequest();

            var response = await _client.PutAsJsonAsync($"{HeroEndpoint}/address?id={id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Failure: Update hero with wrong address returns error")]
        public async Task PutHeroAddress_WithInvalidData_ReturnsError()
        {
            var id = await GetValidHeroIdAsync();
            var request = DataGenerator.GetInvalidAddressRequest();
            var response = await _client.PutAsJsonAsync($"{HeroEndpoint}/address?id={id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        #endregion

        #region Multipart & Specialized Operations
        [Fact(DisplayName = "Success: Create complete hero via Multipart")]
        public async Task SetCompleteHero_WithMultipart_ReturnsOk()
        {
            await AuthenticateAsync();

            var response = await _client.PostAsync($"{HeroEndpoint}/completehero", DataGenerator.BuildCompleteHeroFormData());

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<ReturnApi<object>>();
            content!.Message.Should().Be("Hero created successfully");
        }
        #endregion

        #region Negative Testing & Security
        [Theory(DisplayName = "Failure: Protected endpoints return Unauthorized")]
        [InlineData("POST", "")]
        [InlineData("PUT", "")]
        [InlineData("DELETE", "")]
        [InlineData("DELETE", "/image")]
        public async Task ProtectedEndpoints_WhenLoggedOut_ReturnUnauthorized(string method, string subRoute)
        {
            ClearAuthentication();
            var request = new HttpRequestMessage(new HttpMethod(method), $"{HeroEndpoint}{subRoute}");
            if (method != "GET") request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Match(s =>
                s == HttpStatusCode.Unauthorized || s == HttpStatusCode.BadRequest);
        }
        #endregion


        #region Image Operations
        [Fact(DisplayName = "Success: Upload hero image returns OK")]
        public async Task SetHeroImage_WithValidFile_ReturnsOk()
        {
            var id = await GetValidHeroIdAsync();
            await AuthenticateAsync();

            using var formData = new MultipartFormDataContent();

            var fileContent = DataGenerator.ImageContent();

            formData.Add(fileContent, "images", "hero_test.png");

            var response = await _client.PostAsync($"{HeroEndpoint}/image?id={id}", formData);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ReturnApi<object>>();
            result!.Message.Should().Be("Hero's image added successfully");
        }

        [Fact(DisplayName = "Failure: Upload hero image without file returns BadRequest")]
        public async Task SetHeroImage_NoFile_ReturnsBadRequest()
        {
            var id = await GetValidHeroIdAsync();

            using var content = new MultipartFormDataContent();

            var response = await _client.PostAsync($"{HeroEndpoint}/image?id={id}", content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "Success: Delete hero image returns OK")]
        public async Task DeleteHeroImage_ReturnsOk()
        {
            var (heroId, imageId) = await GetValidHeroWithImageId();
           
            var request = await _client.DeleteAsync($"{HeroEndpoint}/image?id={Uri.EscapeDataString(heroId)}&imageId={Uri.EscapeDataString(imageId)}");

            request.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        #endregion
    }
}