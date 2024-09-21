using WebAPI.DTOs;

namespace WebAPI.IntegrationTests.Personas;

public record BlogPersona
{
    public string Title { get; init; }      = "Test blog title";
    public int OwnerId { get; init; }       = 1;
    public List<string> Tags { get; init; } = ["Tag1", "Tag2"];

    public BlogDTO ToBlogDTO()
    {
        return new BlogDTO
        {
            Title   = Title,
            OwnerId = OwnerId,
            Tags    = [..Tags]
        };
    }

    public static implicit operator BlogDTO(BlogPersona persona) => persona.ToBlogDTO();
}