using Mapster;
using Mapster.Models;
using System.Linq.Expressions;
using WebAPI.Data;
using WebAPI.DTOs;

namespace WebAPI;

public static class TypeAdapterConfigExtensions
{
    /// <summary>
    /// Example: var expression = TypeAdapterConfig.GlobalSettings.CreateProjectionExpression<Member, MemberDTO>();
    /// </summary>
    /// <returns>The return value can be used in the Select(expression) method to transform TSource to TDest</returns>
    public static Expression<Func<TSource, TDest>> CreateProjectionExpression<TSource, TDest>(this TypeAdapterConfig typeAdapterConfig)
    {
        Type sourceType = typeof(TSource);
        Type destType   = typeof(TDest);

        var typeTuple = new TypeTuple(sourceType, destType);

        return (Expression<Func<TSource, TDest>>) typeAdapterConfig.CreateMapExpression(typeTuple, MapType.Projection);
    }
}

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
