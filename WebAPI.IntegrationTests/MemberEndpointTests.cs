using Alba;
using WebAPI.DTOs;

namespace WebAPI.IntegrationTests;

[Collection(nameof(SharedCollectionFixture))]
public sealed class MemberEndpointTests(AlbaHostFixture fixture) : EndpointTestBase(fixture)
{
    [Fact]
    public async Task CreateMember()
    {
        // Arrange + Act
        string location = await assumeMemberCreated(new MemberPersona());

        // Assert
        Assert.NotEmpty(location);
    }

    [Fact]
    public async Task Create_and_Get_Member()
    {
        // Arrange
        MemberDTO createMember = new MemberPersona();

        // Act #1
        string location = await assumeMemberCreated(createMember);

        Assert.NotEmpty(location);

        // Act #2
        MemberDTO? createdMember = await _albaHost.GetAsJson<MemberDTO>(location);

        // Assert
        Assert.NotNull(createdMember);
        Assert.Equal(createMember.Name, createdMember.Name);
        // ...
    }

    [Fact]
    public async Task Create_and_Get_MemberByEmail()
    {
        // Arrange
        MemberDTO createMember = new MemberPersona();

        // Act #1
        string location = await assumeMemberCreated(createMember);

        Assert.NotEmpty(location);

        // Act #2
        MemberDTO? createdMember = await _albaHost.GetAsJson<MemberDTO>($"/Member/Email/{createMember.Email}");

        // Assert
        Assert.NotNull(createdMember);
    }

    [Fact]
    public async Task Create_2_Memebers_and_GetAll()
    {
        // Arrange
        var member1 = new MemberPersona();
        var member2 = member1 with { Email = "second@email.com" };

        // Act #1
        await assumeMemberCreated(member1);
        await assumeMemberCreated(member2);

        // Act #2
        IEnumerable<MemberDTO>? members = await _albaHost.GetAsJson<IEnumerable<MemberDTO>>("/Member");

        // Assert
        Assert.NotNull(members);
        Assert.Equal(2, members.Count());
    }

    [Fact]
    public async Task Create_and_Delete_Member()
    {
        // Act #1
        string location = await assumeMemberCreated(new MemberPersona());

        Assert.NotEmpty(location);

        // Act #2 + Assert
        _ = await _albaHost.Scenario(scenario => scenario.Delete.Url(location));
    }
}