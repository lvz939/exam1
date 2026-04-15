using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using  ExamPrepIdeaCreate.Models;

namespace ExamPrepIdeaCreate
{
    [TestFixture]

    public class Tests
    {
        
        private RestClient client;
        private static string lastCreateadIdeaId;

        private const string BaseUrl = "http://144.91.123.158:82";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI2OTZhMjJkYi1lZDkzLTQwM2ItOTU2Yi1hYmVjMDIzZDk5MDEiLCJpYXQiOiIwNC8xNS8yMDI2IDExOjAzOjIyIiwiVXNlcklkIjoiMzEzZjFkODktYTkwNC00YzE4LTUzNzEtMDhkZTc2YTJkM2VjIiwiRW1haWwiOiJ1c2VyMTIzQGV4YW1wbGUuY29tIiwiVXNlck5hbWUiOiJzdHJpbmdxYSIsImV4cCI6MTc3NjI3MjYwMiwiaXNzIjoiSWRlYUNlbnRlcl9BcHBfU29mdFVuaSIsImF1ZCI6IklkZWFDZW50ZXJfV2ViQVBJX1NvZnRVbmkifQ.bE1dWrEIuM-Wvp20kcfBWwCHeuUtIr176aiQ-O2A5kQ";
        private const string LoginEmail = "user123@example.com";
        private const string LoginPassword = "string4321";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;
            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }
        private string GetJwtToken(string email, string password)
        {
           var tempClient = new RestClient(BaseUrl);
           var request = new RestRequest("/api/User/Authentication", Method.Post);
           request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status Code: {response.StatusCode}, Response: {response.Content}");
            }

        }

        [Order(1)]
        [Test]
        public void CreateIdea_WithRequiredFields_ShouldReturnSuccess()
        {

            var ideqData = new IdeaDTO
            {
                Title = "Test Idea ",
                Description = "This is a test idea description.",
                Url = ""
            };

          var request = new RestRequest("/api/Idea/Create", Method.Post);
          request.AddJsonBody(ideqData);

          var response = this.client.Execute(request);

          var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

          Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Expected status code 200 OK ");
          Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));
        }

        [Order(2)]
        [Test]
        public void GetAllIdeas_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Idea/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(responseItems, Is.Not.Empty);
            Assert.That(responseItems, Is.Not.Null);

            lastCreateadIdeaId = responseItems.LastOrDefault()?.Id;

        }

        [Order(3)]
        [Test]

        public void EditExistingIdea_ShouldReturnSuccess()
        {
            var editRequestData = new IdeaDTO
            {
                Title = "Edited Idea",
                Description = "This is a edited idea description.",
                Url = ""
            };


            var request = new RestRequest("/api/Idea/Edit", Method.Put);

            request.AddQueryParameter("ideaId", lastCreateadIdeaId);
            request.AddJsonBody(editRequestData);

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));
        }


        [Order(4)]
        [Test]

        public void DeleteIdea_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", lastCreateadIdeaId);
            var response = this.client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));
        }

        [Order(5)]
        [Test]
        public void CreateIdea_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            var ideaData = new IdeaDTO
            {
                Title = "",
                Description = "This is a test idea description.",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaData);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }

        [Order(6)]
        [Test]

        public void EditNonExistingIdea_ShouldReturnNotFound()
        {
            string nonExistingIdeaId = "9999999";
            var editRequestData = new IdeaDTO
            {
                Title = "Edited Idea",
                Description = "This is a edited idea description.",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            request.AddJsonBody(editRequestData);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingIdea_ShouldReturnNotFound()
        {
            string nonExistingIdeaId = "9999999";

            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            var response = this.client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }

        [Test]
        public void TestToFail()
        {
            string nonExistingIdeaId = "9999999";

            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            var response = this.client.Execute(request);



            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");

        }


        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
