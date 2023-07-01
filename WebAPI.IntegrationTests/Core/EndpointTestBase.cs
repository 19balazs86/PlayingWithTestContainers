using Alba;
using WebAPI.DTOs;

namespace WebAPI.IntegrationTests.Core;

public abstract class EndpointTestBase : IAsyncLifetime
{
    protected readonly AlbaHostFixture _fixture;

    protected readonly IAlbaHost _albaHost;

    public EndpointTestBase(AlbaHostFixture fixture)
    {
        _fixture = fixture;
        _albaHost = fixture.AlbaWebHost;
    }

    protected async Task<string> assumeMemberCreated(MemberDTO createMember)
    {
        IScenarioResult scenarioResult = await _albaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createMember).ToUrl("/Member");
            scenario.StatusCodeShouldBe(Status201Created);
        });

        return scenarioResult.Context.Response.Headers.Location.SingleOrDefault() ?? string.Empty;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ResetDatabaseAsync();
}
