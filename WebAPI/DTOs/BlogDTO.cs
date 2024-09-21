namespace WebAPI.DTOs;

public sealed class BlogDTO
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int OwnerId { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new List<string>();
}
