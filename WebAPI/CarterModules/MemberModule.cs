using Carter;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
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
            .AddEndpointFilterFactory(ValidationEndpointFilterFactory.Create);

        groupBuilder.MapPost("/", postMember);//.AddEndpointFilter<ValidationEndpointFilter<MemberDTO>>();

        groupBuilder.MapGet("/", getAllMembers);

        groupBuilder.MapGet("/{id:int}", getMemberById);

        groupBuilder.MapGet("/Email/{email}", getMemberByEmail);

        groupBuilder.MapDelete("/{id:int}", deleteMemberById);
    }

    // Stop returning IResult in Minimal APIs | Codewrinkles https://youtu.be/hubDMfLJbi8
    private static async Task<Created<Member>> postMember([Validate] MemberDTO createMember, WebApiContext dbContext)
    {
        // Carter has a built-in feature for validation. You can inject "HttpContext httpContext" in the method and call it in the wollowing way.
        // I created a FilterFactory in the ValidationFilter.cs. It can be activated with the "[Validate]" attribute.

        //var validationResult = httpContext.Request.Validate(memberRequest);

        //if (!validationResult.IsValid)
        //    return TypedResults.ValidationProblem(validationResult.ToDictionary());

        Member member = createMember.Adapt<Member>();

        await dbContext.Members.AddAsync(member);

        await dbContext.SaveChangesAsync();

        return TypedResults.Created($"/Member/{member.Id}", member);
    }

    private static async Task<Results<Ok<MemberDTO>, NotFound>> getMemberById(int id, WebApiContext dbContext)
    {
        MemberDTO? memberDTO = await dbContext.Members
            //.AsNoTracking() // No need for AsNoTracking(), because globally QueryTrackingBehavior is NoTracking
            .Where(m => m.Id == id)
            .ProjectToType<MemberDTO>()
            .FirstOrDefaultAsync();

        return memberDTO is null ?
            TypedResults.NotFound() : // app.UseStatusCodePages() will generate the problem details response
            //TypedResults.Problem("Member was not found.", statusCode: Status404NotFound) :
            TypedResults.Ok(memberDTO);
    }

    private static async Task<Results<Ok<MemberDTO>, ProblemHttpResult>> getMemberByEmail(string email, WebApiContext dbContext)
    {
        Member? member = await dbContext.GetMemberByEmailAsync(email);

        if (member is null)
            return TypedResults.Problem("Member was not found.", statusCode: Status404NotFound);

        return TypedResults.Ok(member.Adapt<MemberDTO>());
    }

    private static async Task<IEnumerable<MemberDTO>> getAllMembers(WebApiContext dbContext)
    {
        return await dbContext.Members
            .ProjectToType<MemberDTO>()
            .ToListAsync();
    }

    private static async Task<Results<Ok, NotFound>> deleteMemberById(int id, WebApiContext dbContext)
    {
        // Without interceptor, you need to run update for soft delete
        //int rowsUpdated = await dbContext.Members
        //    .Where(m => m.Id == id)
        //    .ExecuteUpdateAsync(setProp => setProp.SetProperty(m => m.IsDeleted, true));

        // You can run the 'hard delete', because BaseEntityExecuteDeleteInterceptor can get this querry and apply the soft delete
        int rowsUpdated = await dbContext.Members
            .Where(m => m.Id == id)
            .ExecuteDeleteAsync();

        return rowsUpdated == 0 ?
            TypedResults.NotFound() : // app.UseStatusCodePages() will generate the problem details response
            //TypedResults.Problem("Member was not found.", statusCode: Status404NotFound) :
            TypedResults.Ok();
    }
}