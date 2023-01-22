using WebAPI.DTOs;

namespace WebAPI.IntegrationTests.Personas;

public record BlogPersona
{
    public string Name { get; init; } = "Test blog name";
    public int OwnerId { get; init; } = 1;

    public BlogDTO ToBlogDTO()
    {
        return new BlogDTO
        {
            Name    = Name,
            OwnerId = OwnerId
        };
    }

    public static implicit operator BlogDTO(BlogPersona persona) => persona.ToBlogDTO();
}