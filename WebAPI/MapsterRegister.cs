using Mapster;
using WebAPI.Data;
using WebAPI.DTOs;

namespace WebAPI
{
    public sealed class MapsterRegister : IRegister
    {
        private const string _universalDateFormat = "yyyy-MM-dd";

        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MemberDTO, Member>()
                .Ignore(dest => dest.Id)
                .Map(dest => dest.DateOfBirth,  src => DateOnly.ParseExact(src.DateOfBirth, _universalDateFormat))
                .Map(dest => dest.Membership,   src => MembershipEnum.FromName(src.Membership, true))
                .Map(dest => dest.PaymentTypes, src => PaymentTypeEnum.FromName(src.PaymentTypes, true, false).ToList());

            config.NewConfig<Member, MemberDTO>()
                .Map(dest => dest.DateOfJoined, src => src.CreatedAt.ToString(_universalDateFormat))
                .Map(dest => dest.DateOfBirth,  src => src.DateOfBirth.ToString(_universalDateFormat))
                .Map(dest => dest.Membership,   src => src.Membership.ToString())
                .Map(dest => dest.PaymentTypes, src => Member.CalculatePaymentTypesText(src.PaymentTypes))
                .Map(dest => dest.Blogs,        src => src.Blogs.Adapt<IEnumerable<MemberBlogsDTO>>());

            config.NewConfig<BlogDTO, Blog>()
                .Ignore(dest => dest.Id)
                .Map(dest => dest.OwnerId, src => MemberId.Create(src.OwnerId));

            config.NewConfig<Blog, BlogDTO>()
                .Map(dest => dest.OwnerName, src => src.Owner == null ? string.Empty : src.Owner.Name);
        }
    }
}
