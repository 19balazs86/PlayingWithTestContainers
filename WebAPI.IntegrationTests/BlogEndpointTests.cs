using Alba;
using WebAPI.DTOs;

namespace WebAPI.IntegrationTests;

[Collection(nameof(SharedCollectionFixture))]
public sealed class BlogEndpointTests : EndpointTestBase
{
    public BlogEndpointTests(AlbaHostFixture fixture) : base(fixture)
    {

    }

    [Fact]
    public async Task CreateBlog()
    {
        // Arrange
        string location = await assumeMemberCreated(new MemberPersona());

        int memberId = int.Parse(location.TrimStart("/Member/".ToCharArray()));

        // Act
        location = await assumeBlogCreated(new BlogPersona { OwnerId = memberId });

        // Assert
        Assert.NotEmpty(location);
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
}