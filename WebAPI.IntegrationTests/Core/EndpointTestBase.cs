using Alba;
using WebAPI.DTOs;

namespace WebAPI.IntegrationTests.Core;

public abstract class EndpointTestBase(AlbaHostFixture fixture) : IAsyncLifetime // Does not work with IAsyncDisposable
{
    protected readonly AlbaHostFixture _fixture = fixture;

    protected readonly IAlbaHost _albaHost = fixture.AlbaWebHost;

    protected async Task<string> assumeMemberCreated(MemberDTO createMember)
    {
        IScenarioResult scenarioResult = await _albaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createMember).ToUrl("/Member");
            scenario.StatusCodeShouldBe(Status201Created);
        });

        return scenarioResult.Context.Response.Headers.Location.SingleOrDefault() ?? string.Empty;
    }

    protected int toMemberId(string location)
    {
        return int.Parse(location.TrimStart("/Member/".ToCharArray()));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ResetDatabaseAsync();
}
