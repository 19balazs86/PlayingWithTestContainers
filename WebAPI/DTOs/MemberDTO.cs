namespace WebAPI.DTOs;

public sealed class MemberDTO
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DateOfBirth { get; set; } = string.Empty;

    public string DateOfJoined { get; set; } = string.Empty;

    public string Membership { get; set; } = string.Empty;

    public string PaymentTypes { get; set; } = string.Empty;

    public ContactDetailsDTO? ContactDetails { get; set; }
}

public sealed class ContactDetailsDTO
{
    public string? Phone { get; set; }
    public AddressDTO? Address { get; set; }
}

public sealed class AddressDTO
{
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
}