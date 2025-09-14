using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using ExamSystem.API.Models.DTOs;

namespace ExamSystem.E2ETests;

/// <summary>
/// End-to-End tests for the Exam System API
/// Tests complete workflows from authentication to exam management
/// </summary>
public class ExamSystemE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExamSystemE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [Fact]
    public async Task AuthenticationWorkflow_ShouldWork()
    {
        // Admin login
        var adminLogin = new LoginDto { Username = "admin", Password = "admin123" };
        var adminResponse = await PostAsync("/api/auth/login", adminLogin);
        
        adminResponse.Should().NotBeNull();
        var adminToken = adminResponse.GetProperty("token").GetString();
        adminToken.Should().NotBeNullOrEmpty();

        // Student login
        var studentLogin = new LoginDto { Username = "student", Password = "student123" };
        var studentResponse = await PostAsync("/api/auth/login", studentLogin);
        
        studentResponse.Should().NotBeNull();
        var studentToken = studentResponse.GetProperty("token").GetString();
        studentToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AdminExamManagement_ShouldWork()
    {
        // Login as admin
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        // Create exam
        var createExam = new CreateExamDto
        {
            Title = "E2E Test Exam",
            MaxAttempts = 3,
            CooldownMinutes = 30
        };

        var createdExam = await PostAsync("/api/exam", createExam);
        createdExam.Should().NotBeNull();
        createdExam.GetProperty("title").GetString().Should().Be("E2E Test Exam");

        var examId = createdExam.GetProperty("examId").GetGuid();
        examId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task StudentExamAttempt_ShouldWork()
    {
        // Setup: Create exam as admin
        var adminToken = await GetAdminTokenAsync();
        SetAuthHeader(adminToken);

        var createExam = new CreateExamDto
        {
            Title = "Student Test Exam",
            MaxAttempts = 2,
            CooldownMinutes = 15
        };

        var exam = await PostAsync("/api/exam", createExam);
        var examId = exam.GetProperty("examId").GetGuid();

        // Switch to student
        var studentToken = await GetStudentTokenAsync();
        SetAuthHeader(studentToken);

        // Start attempt
        var attempt = await PostAsync($"/api/attempt/start/{examId}", new { });
        attempt.Should().NotBeNull();
        var attemptId = attempt.GetProperty("attemptId").GetGuid();

        // Submit attempt
        var submittedAttempt = await PostAsync($"/api/attempt/{attemptId}/submit", new { });
        submittedAttempt.Should().NotBeNull();

        // Test completed successfully
    }

    [Fact]
    public async Task UnauthorizedAccess_ShouldBeDenied()
    {
        // Try to access protected endpoints without token
        var response1 = await _client.GetAsync("/api/attempt/student");
        response1.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        var response2 = await _client.GetAsync("/api/attempt/all");
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        // Try student accessing admin endpoint
        var studentToken = await GetStudentTokenAsync();
        SetAuthHeader(studentToken);

        var response3 = await _client.GetAsync("/api/attempt/all");
        response3.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    // Helper methods
    private async Task<string> GetAdminTokenAsync()
    {
        var login = new LoginDto { Username = "admin", Password = "admin123" };
        var response = await PostAsync("/api/auth/login", login);
        return response.GetProperty("token").GetString()!;
    }

    private async Task<string> GetStudentTokenAsync()
    {
        var login = new LoginDto { Username = "student", Password = "student123" };
        var response = await PostAsync("/api/auth/login", login);
        return response.GetProperty("token").GetString()!;
    }

    private void SetAuthHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<JsonElement> PostAsync<TRequest>(string endpoint, TRequest request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(endpoint, content);
        
        response.IsSuccessStatusCode.Should().BeTrue($"POST {endpoint} failed with {response.StatusCode}");
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(responseJson, _jsonOptions);
    }
}