using Carter;
using Mapster;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Validations;

namespace WebAPI.CarterModules;

public sealed class MemberModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var groupBuilder = app.MapGroup("/Member")
            .AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory);

        groupBuilder.MapPost("/", postMember);

        groupBuilder.MapGet("/", getAllMembers);

        groupBuilder.MapGet("/{id:int}", getMemberById);

        groupBuilder.MapDelete("/{id:int}", deleteMemberById);
    }

    private static async Task<IResult> postMember([Validate] MemberDTO createMember, WebApiContext dbContext)
    {
        // Carter has a built-in feature for validation. You can inject "HttpContext httpContext" in the method and call it in the wollowing way.
        // I created a FilterFactory in the ValidationFilter.cs. It can be activated with the "[Validate]" attribute.

        //var validationResult = httpContext.Request.Validate(memberRequest);

        //if (!validationResult.IsValid)
        //    return TypedResults.ValidationProblem(validationResult.ToDictionary());

        Member member = createMember.Adapt<Member>();

        await dbContext.Members.AddAsync(member);

        await dbContext.SaveChangesAsync();

        return TypedResults.Created($"/Member/{member.Id}");
    }

    private static async Task<IResult> getMemberById(int id, WebApiContext dbContext)
    {
        MemberDTO? memberDTO = await dbContext.Members
            .AsNoTracking()
            .Where(m => m.Id == id)
            .ProjectToType<MemberDTO>()
            .FirstOrDefaultAsync();

        return memberDTO is null ?
            TypedResults.Problem("Member was not found.", statusCode: Status404NotFound) :
            TypedResults.Ok(memberDTO);
    }

    private static async Task<IEnumerable<MemberDTO>> getAllMembers(WebApiContext dbContext)
    {
        return await dbContext.Members
            .AsNoTracking()
            .ProjectToType<MemberDTO>()
            .ToListAsync();
    }

    private static async Task<IResult> deleteMemberById(int id, WebApiContext dbContext)
    {
        int rowsUpdated = await dbContext.Members
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(setProp => setProp.SetProperty(m => m.DeletedAt, DateTime.UtcNow));

        return rowsUpdated == 0 ?
            TypedResults.Problem("Member was not found.", statusCode: Status404NotFound) :
            TypedResults.Ok();
    }
}