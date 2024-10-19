using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Validations;

namespace WebAPI.Endpoints;

public static class BlogEndpoints
{
    public static void MapBlogEndpoints(this IEndpointRouteBuilder app)
    {
        var groupBuilder = app.MapGroup("/Blog");

        groupBuilder.MapPost("/", postBlog)
            .AddEndpointFilter<ValidationEndpointFilter<BlogDTO>>();

        groupBuilder.MapGet("/", getAllBlogs);

        groupBuilder.MapGet("/{id:int}", getBlogById);

        groupBuilder.MapGet("/full-text-search/{keyword}", fullTextSearch);
    }

    // Stop returning IResult in Minimal APIs | Codewrinkles https://youtu.be/hubDMfLJbi8
    private static async Task<Results<Created<Blog>, ProblemHttpResult>> postBlog([Validate] BlogDTO createBlog, WebApiContext dbContext)
    {
        bool isOwnerExists = await dbContext.Members.AnyAsync(m => m.Id == createBlog.OwnerId);

        if (!isOwnerExists)
        {
            return TypedResults.Problem("Owner was not found.", statusCode: Status404NotFound);
        }

        Blog blog = createBlog.Adapt<Blog>();

        await dbContext.Blogs.AddAsync(blog);

        await dbContext.SaveChangesAsync();

        return TypedResults.Created($"/Blog/{blog.Id}", blog);
    }

    private static async Task<IEnumerable<BlogDTO>> getAllBlogs(WebApiContext dbContext)
    {
        return await dbContext.Blogs
            //.AsNoTracking() // No need for AsNoTracking(), because globally QueryTrackingBehavior is NoTracking
            //.Include(b => b.Owner) // No need, because of the ProjectToType
            .ProjectToType<BlogDTO>()
            .ToListAsync();
    }

    private static async Task<Results<Ok<BlogDTO>, NotFound>> getBlogById(int id, WebApiContext dbContext)
    {
        BlogDTO? blogDTO = await dbContext.Blogs
            .Where(b => b.Id == id)
            .ProjectToType<BlogDTO>()
            .FirstOrDefaultAsync();

        return blogDTO is null ?
            TypedResults.NotFound() : // app.UseStatusCodePages() will generate the problem details response
                                      //TypedResults.Problem("Blog was not found.", statusCode: Status404NotFound) :
            TypedResults.Ok(blogDTO);
    }

    private static async IAsyncEnumerable<BlogDTO> fullTextSearch(
        string keyword,
        WebApiContext dbContext,
        [EnumeratorCancellation] CancellationToken ct)
    {
        /* Give it a try with the following keywords:
         * - comfortable (more in Hotel blog)
         * - adventure (more in Dog blog)
         * - stroll (balanced)
         * - explore (balanced)
        */

        // Storing the tsvector in the database VS calculating it on-the-fly

        // 1) Calculate tsvector on-the-fly, if the dataset is small, searches are infrequent, or minor performance impacts are acceptable

        // 2) Store the tsvector, if frequently performing full-text searches on large text volumes or if performance is critical
        // Ability to create an index on the stored tsvector, enhancing search speed, especially for large datasets
        // Cons: Requires additional storage space in the database

        IAsyncEnumerable<BlogDTO> asyncEnum = dbContext.Blogs
            .Where(b  => b.FullTextSearchVector.Matches(EF.Functions.PhraseToTsQuery("english", keyword)))
            .Select(b => new BlogDTO
            {
                Id         = b.Id,
                Title      = b.Title,
                Content    = b.Content,
                SearchRank = b.FullTextSearchVector.Rank(EF.Functions.PhraseToTsQuery("english", keyword))
            })
            .OrderByDescending(b => b.SearchRank)
            .AsAsyncEnumerable();

        await foreach (BlogDTO blogDTO in asyncEnum.WithCancellation(ct))
        {
            yield return blogDTO;
        }
    }

    private static IAsyncEnumerable<BlogDTO> fullTextSearch_OnTheFly(string keyword, WebApiContext dbContext)
    {
        return dbContext.Blogs
            .Where(b => EF.Functions.ToTsVector("english", b.Title + " " + b.Content)
                                    .Matches(EF.Functions.PhraseToTsQuery("english", keyword)))
            .Select(b => new BlogDTO
            {
                Id         = b.Id,
                Title      = b.Title,
                Content    = b.Content,
                SearchRank = EF.Functions.ToTsVector("english", b.Title + " " + b.Content)
                                         .Rank(EF.Functions.PhraseToTsQuery("english", keyword))
            })
            .OrderByDescending(b => b.SearchRank)
            .AsAsyncEnumerable();
    }
}