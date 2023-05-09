using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit.Abstractions;

namespace TESTS_VK_ASP_NET_API
{
    public class UserControllerTests : IClassFixture<WebApplicationFactory<VK_ASP_NET_API.Startup>>
    {
        private readonly WebApplicationFactory<VK_ASP_NET_API.Startup> _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public UserControllerTests(WebApplicationFactory<VK_ASP_NET_API.Startup> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");
            _output = output;
        }

        [Fact]
        public async Task TestGetAllWithCorrectAuthorizationHeader()//Попытка получения всех пользователей с правильными данными для basic авторизации
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            // Act
            var response = await _client.GetAsync("Users?page=1&pageSize=10");

            // Assert
            _output.WriteLine(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestGetAllWithWrongAuthorizationHeader()//Попытка получения всех пользователей с неправильными данными для basic авторизации(в нашем случае тут некорректный header запроса)
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "SomeText");

            // Act
            var response = await _client.GetAsync("Users?page=1&pageSize=10");

            // Assert
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
            _output.WriteLine(response.StatusCode.ToString());
        }

        [Fact]
        public async Task TestGetWithExistingId()//Попытка получения пользователя по существующему в БД Id
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            // Act
            var response = await _client.GetAsync("Users/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestGetWithNotExistingId()//Попытка получения пользователя по несуществующему в БД Id
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            // Act
            var response = await _client.GetAsync("Users/23");

            // Assert
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestAddUserWithCorrectJSON()//Попытка добавления пользователя с корректными входными данными в формате JSON
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            var request = new HttpRequestMessage(HttpMethod.Post, "Users");
            request.Content = new StringContent(@"{
                ""id"": 6,
                ""login"": ""USER21"",
                ""password"": ""USER"",
                ""createdDate"": ""2023-05-08T19:56:54.060657"",
                ""userGroupId"": 2,
                ""userGroup"": {
                    ""id"": 2,
                    ""code"": ""USER"",
                    ""description"": ""Обычный пользователь""
                },
                ""userStateId"": 1,
                ""userState"": {
                    ""id"": 1,
                    ""code"": ""ACTIVE"",
                    ""description"": ""Активный""
                }
            }", Encoding.UTF8, "application/json");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestAddUserWithCorrectJSONButLoginAlreadyExists()//Попытка добавления пользователя с корректными входными данными в формате JSON, но такой логин уже существует в БД.
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            var request = new HttpRequestMessage(HttpMethod.Post, "Users");
            request.Content = new StringContent(@"{
                ""id"": 6,
                ""login"": ""USER4"",
                ""password"": ""USER"",
                ""createdDate"": ""2023-05-08T19:56:54.060657"",
                ""userGroupId"": 2,
                ""userGroup"": {
                    ""id"": 2,
                    ""code"": ""USER"",
                    ""description"": ""Обычный пользователь""
                },
                ""userStateId"": 1,
                ""userState"": {
                    ""id"": 1,
                    ""code"": ""ACTIVE"",
                    ""description"": ""Активный""
                }
            }", Encoding.UTF8, "application/json");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestAddUserWithCorrectJSONButTryingToAddSecondAdmin()//Попытка добавления пользователя с корректными входными данными в формате JSON, но мы пытаемся добавить больше 1 админа.
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            var request = new HttpRequestMessage(HttpMethod.Post, "Users");
            request.Content = new StringContent(@"{
                ""id"": 6,
                ""login"": ""USER10"",
                ""password"": ""USER"",
                ""createdDate"": ""2023-05-08T19:56:54.060657"",
                ""userGroupId"": 1,
                ""userGroup"": {
                    ""id"": 1,
                    ""code"": ""ADMIN"",
                    ""description"": ""Администратор""
                },
                ""userStateId"": 1,
                ""userState"": {
                    ""id"": 1,
                    ""code"": ""ACTIVE"",
                    ""description"": ""Активный""
                }
            }", Encoding.UTF8, "application/json");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestAddUserWithCorrectJSONButInvalidUserGroup()//Попытка добавления пользователя с корректными входными данными в формате JSON, но поле User Group некорректно.
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            var request = new HttpRequestMessage(HttpMethod.Post, "Users");
            request.Content = new StringContent(@"{
                ""id"": 6,
                ""login"": ""USER10"",
                ""password"": ""USER"",
                ""createdDate"": ""2023-05-08T19:56:54.060657"",
                ""userGroupId"": 3,
                ""userGroup"": {
                    ""id"": 3,
                    ""code"": ""Text"",
                    ""description"": ""Text""
                },
                ""userStateId"": 1,
                ""userState"": {
                    ""id"": 1,
                    ""code"": ""ACTIVE"",
                    ""description"": ""Активный""
                }
            }", Encoding.UTF8, "application/json");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestAddUserWithCorrectJSONButInvalidUserState()//Попытка добавления пользователя с корректными входными данными в формате JSON, но поле User State некорректно.
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            var request = new HttpRequestMessage(HttpMethod.Post, "Users");
            request.Content = new StringContent(@"{
                ""id"": 6,
                ""login"": ""USER10"",
                ""password"": ""USER"",
                ""createdDate"": ""2023-05-08T19:56:54.060657"",
                ""userGroupId"": 2,
                ""userGroup"": {
                    ""id"": 2,
                    ""code"": ""USER"",
                    ""description"": ""Обычный пользователь""
                },
                ""userStateId"": 3,
                ""userState"": {
                    ""id"": 3,
                    ""code"": ""Text"",
                    ""description"": ""Text""
                }
            }", Encoding.UTF8, "application/json");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestDeleteExistingId()//Попытка удаления пользователя с существующим Id.
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            // Act
            var response = await _client.DeleteAsync("Users/5");

            // Assert
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
            _output.WriteLine(response.StatusCode.ToString());
        }

        [Fact]
        public async Task TestDeleteNotExistingId()//Попытка удаления пользователя с несуществующим Id.
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            // Act
            var response = await _client.DeleteAsync("Users/23");

            // Assert
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
            _output.WriteLine(response.StatusCode.ToString());
        }

        [Fact]
        public async Task TestDeleteAdmin()//Попытка удаления учётки админа.
        {
            // Arrange
            _client.BaseAddress = new Uri("https://localhost:44311/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "VVNFUjpVU0VS");

            // Act
            var response = await _client.DeleteAsync("Users/1");

            // Assert
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }
}
