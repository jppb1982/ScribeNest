using Microsoft.EntityFrameworkCore;
using ScribeNest.Domain.Entities;

namespace ScribeNest.Infrastructure.Data;

public static class AppDbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        var dotnet = await GetOrCreateCategoryAsync(db, ".NET");
        var angular = await GetOrCreateCategoryAsync(db, "Angular");
        var architecture = await GetOrCreateCategoryAsync(db, "Architecture");
        var data = await GetOrCreateCategoryAsync(db, "Data");
        var ai = await GetOrCreateCategoryAsync(db, "AI");
        var career = await GetOrCreateCategoryAsync(db, "Career");

        await db.SaveChangesAsync();

        var posts = new[]
        {
            new Post
            {
                Title = "Building a layered ASP.NET Core application",
                Slug = "building-layered-aspnet-core-application",
                Content = Paragraphs(
                    "A layered architecture helps keep responsibilities separated and makes the project easier to understand.",
                    "In ScribeNest, the solution is split into Domain, Application, Infrastructure and Web. Domain contains the entities, Application defines contracts, Infrastructure implements persistence with Entity Framework Core, and Web exposes MVC/API endpoints.",
                    "This structure is intentionally simple. It avoids overengineering while still showing how a real-world .NET project can be organized."),
                CategoryId = architecture.Id,
                Tags = "Architecture, Clean Code, .NET",
                PublishedAt = DateTime.UtcNow.AddDays(-12)
            },
            new Post
            {
                Title = "Repository and Unit of Work in a junior-friendly project",
                Slug = "repository-unit-of-work-junior-friendly-project",
                Content = Paragraphs(
                    "Entity Framework Core already behaves like a Unit of Work through DbContext, but implementing Repository and Unit of Work explicitly can be useful in a learning project.",
                    "The Repository pattern centralizes basic data access operations. Unit of Work groups the repositories and exposes a single SaveChangesAsync method.",
                    "The trade-off is important: this pattern adds abstraction, so it should stay simple and not hide the strengths of EF Core."),
                CategoryId = dotnet.Id,
                Tags = "Repository Pattern, Unit of Work, EF Core",
                PublishedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Post
            {
                Title = "Consuming a .NET API from Angular",
                Slug = "consuming-dotnet-api-from-angular",
                Content = Paragraphs(
                    "Angular communicates with the backend through an HTTP service. This keeps components focused on presentation and delegates API calls to a reusable service.",
                    "ScribeNest uses query parameters for search, category filtering and pagination. This makes the state shareable through the URL and easier to debug.",
                    "A clean service layer in Angular is one of the easiest ways to make a frontend project more maintainable."),
                CategoryId = angular.Id,
                Tags = "Angular, REST API, Frontend",
                PublishedAt = DateTime.UtcNow.AddDays(-8)
            },
            new Post
            {
                Title = "Why DTOs matter in REST APIs",
                Slug = "why-dtos-matter-rest-apis",
                Content = Paragraphs(
                    "DTOs define what the API exposes to the client. They prevent the frontend from depending directly on persistence entities and make the contract explicit.",
                    "In ScribeNest, list endpoints return lightweight DTOs while detail endpoints return the full content. This reduces unnecessary data transfer and keeps each response aligned with the screen that consumes it."),
                CategoryId = architecture.Id,
                Tags = "DTOs, REST API, Backend",
                PublishedAt = DateTime.UtcNow.AddDays(-6)
            },
            new Post
            {
                Title = "Adding data thinking to a full stack project",
                Slug = "adding-data-thinking-full-stack-project",
                Content = Paragraphs(
                    "A technical content platform can also expose simple analytics: posts per category, latest publications, most searched topics or simulated reading metrics.",
                    "Even a small dashboard helps connect software development with data analysis concepts. It also creates a bridge toward tools like Python, pandas and Power BI."),
                CategoryId = data.Id,
                Tags = "Data, Dashboard, Analytics",
                PublishedAt = DateTime.UtcNow.AddDays(-4)
            },
            new Post
            {
                Title = "AI-assisted writing without depending on paid APIs",
                Slug = "ai-assisted-writing-without-paid-apis",
                Content = Paragraphs(
                    "AI features do not always need to start with a paid external provider. A project can define an interface and implement a local mock assistant first.",
                    "This approach is useful for portfolio projects because it shows product thinking, separation of concerns and readiness for future integration.",
                    "In a real environment, the local assistant could later be replaced by OpenAI, Azure OpenAI or another provider."),
                CategoryId = ai.Id,
                Tags = "AI, Mock Service, Product",
                PublishedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Post
            {
                Title = "How to explain a portfolio project in interviews",
                Slug = "explain-portfolio-project-interviews",
                Content = Paragraphs(
                    "A good portfolio project is not about having every possible feature. It is about being able to explain the problem, the architecture, the trade-offs and the next steps.",
                    "ScribeNest is designed to be small enough to study, but complete enough to discuss backend, frontend, persistence, API design, validations and maintainability."),
                CategoryId = career.Id,
                Tags = "Career, Portfolio, Interviews",
                PublishedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        foreach (var seedPost in posts)
        {
            var existingPost = await db.Posts.FirstOrDefaultAsync(post => post.Slug == seedPost.Slug);

            if (existingPost is null)
            {
                db.Posts.Add(seedPost);
                continue;
            }

            existingPost.Title = seedPost.Title;
            existingPost.Content = seedPost.Content;
            existingPost.Tags = seedPost.Tags;
            existingPost.CategoryId = seedPost.CategoryId;
            existingPost.PublishedAt = seedPost.PublishedAt;
        }

        await db.SaveChangesAsync();
    }

    private static async Task<Category> GetOrCreateCategoryAsync(AppDbContext db, string name)
    {
        var category = await db.Categories.FirstOrDefaultAsync(item => item.Name == name);

        if (category is not null)
        {
            return category;
        }

        category = new Category { Name = name };
        db.Categories.Add(category);
        return category;
    }

    private static string Paragraphs(params string[] paragraphs)
    {
        return string.Join($"{Environment.NewLine}{Environment.NewLine}", paragraphs);
    }
}
