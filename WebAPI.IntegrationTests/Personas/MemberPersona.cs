using WebAPI.Data;
using WebAPI.DTOs;

namespace WebAPI.IntegrationTests.Personas;
public record MemberPersona
{
    public string Name { get; init; }           = "Member Persona";
    public string Email { get; init; }          = "member@email.com";
    public string DateOfBirth { get; init; }    = "1980-10-15";
    public string Membership { get; init; }     = MembershipEnum.Basic.Name;
    public string PaymentTypes { get; init; }   = PaymentTypeEnum.FromValueToString(PaymentTypeEnum.Card | PaymentTypeEnum.Paypal);
    public ContactDetailsPersona ContactDetails => new ContactDetailsPersona();

    public MemberDTO ToMemberDTO()
    {
        return new MemberDTO
        {
            Name           = Name,
            Email          = Email,
            DateOfBirth    = DateOfBirth,
            Membership     = Membership,
            PaymentTypes   = PaymentTypes,
            ContactDetails = ContactDetails
        };
    }

    public static implicit operator MemberDTO(MemberPersona persona) => persona.ToMemberDTO();
}

public readonly record struct ContactDetailsPersona
{
    public string Phone => "Persona phone";
    public AddressPersona Address => new AddressPersona();

    public static implicit operator ContactDetailsDTO(ContactDetailsPersona persona)
    {
        return new ContactDetailsDTO
        {
            Phone   = persona.Phone,
            Address = persona.Address
        };
    }
}


public readonly record struct AddressPersona
{
    public string Country => "Country";
    public string City => "City";
    public string Street => "Street";

    public static implicit operator AddressDTO(AddressPersona persona)
    {
        return new AddressDTO
        {
            Country = persona.Country,
            City    = persona.City,
            Street  = persona.Street,
        };
    }
}