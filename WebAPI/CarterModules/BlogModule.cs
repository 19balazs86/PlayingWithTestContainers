using Carter;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Validations;

namespace WebAPI.CarterModules;

public sealed class BlogModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var groupBuilder = app.MapGroup("/Blog");

        groupBuilder.MapPost("/", postBlog)
            .AddEndpointFilter<ValidationEndpointFilter<BlogDTO>>();

        groupBuilder.MapGet("/", getAllBlogs);

        groupBuilder.MapGet("/{id:int}", getBlogById);
    }

    // Stop returning IResult in Minimal APIs | Codewrinkles https://youtu.be/hubDMfLJbi8
    private static async Task<Results<Created<Blog>, ProblemHttpResult>> postBlog([Validate] BlogDTO createBlog, WebApiContext dbContext)
    {
        if (!dbContext.Members.Any(m => m.Id == createBlog.OwnerId))
            TypedResults.Problem("Owner was not found.", statusCode: Status404NotFound);

        Blog blog = createBlog.Adapt<Blog>();

        await dbContext.Blogs.AddAsync(blog);

        await dbContext.SaveChangesAsync();

        return TypedResults.Created($"/Blog/{blog.Id}", blog);
    }

    private static async Task<IEnumerable<BlogDTO>> getAllBlogs(WebApiContext dbContext)
    {
        return await dbContext.Blogs
            .AsNoTracking()
            //.Include(b => b.Owner) // No need, because of the ProjectToType
            .ProjectToType<BlogDTO>()
            .ToListAsync();
    }

    private static async Task<Results<Ok<BlogDTO>, NotFound>> getBlogById(int id, WebApiContext dbContext)
    {
        BlogDTO? blogDTO = await dbContext.Blogs
            .AsNoTracking()
            .Where(b => b.Id == id)
            .ProjectToType<BlogDTO>()
            .FirstOrDefaultAsync();

        return blogDTO is null ?
            TypedResults.NotFound() : // app.UseStatusCodePages() will generate the problem details response
            //TypedResults.Problem("Blog was not found.", statusCode: Status404NotFound) :
            TypedResults.Ok(blogDTO);
    }
}