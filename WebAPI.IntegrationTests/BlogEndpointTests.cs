using Alba;
using WebAPI.DTOs;

namespace WebAPI.IntegrationTests;

[Collection(nameof(SharedCollectionFixture))]
public sealed class BlogEndpointTests(AlbaHostFixture fixture) : EndpointTestBase(fixture)
{
    [Fact]
    public async Task CreateBlog()
    {
        // Arrange
        string location = await assumeMemberCreated(new MemberPersona());

        int memberId = toMemberId(location);

        // Act
        location = await assumeBlogCreated(new BlogPersona { OwnerId = memberId });

        // Assert
        Assert.NotEmpty(location);
    }

    [Fact]
    public async Task FullTextSearch()
    {
        // Arrange
        const string keyword = "blog";

        await assumeMemberCreatedWithBlog();

        // Act
        BlogDTO[]? response = await _albaHost.GetAsJson<BlogDTO[]>($"/Blog/full-text-search/{keyword}");

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response);
    }

    private async Task<string> assumeBlogCreated(BlogDTO createBlog)
    {
        IScenarioResult scenarioResult = await _albaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createBlog).ToUrl("/Blog");
            scenario.StatusCodeShouldBe(Status201Created);
        });

        return scenarioResult.Context.Response.Headers.Location.SingleOrDefault() ?? string.Empty;
    }

    private async Task assumeMemberCreatedWithBlog()
    {
        string location = await assumeMemberCreated(new MemberPersona());

        int memberId = toMemberId(location);

        await assumeBlogCreated(new BlogPersona { OwnerId = memberId });
    }
}