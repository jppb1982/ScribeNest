using Microsoft.EntityFrameworkCore;
using ScribeNest.Application.Interfaces;
using ScribeNest.Infrastructure.Data;
using ScribeNest.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")
                 ?? "Data Source=scribenest.db"));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddControllersWithViews();

// CORS para permitir el front en http://localhost:4200
builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowFront", p => p
        .WithOrigins("http://localhost:4200", "https://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Errors/Http/500");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowFront");

app.UseStatusCodePagesWithReExecute("/Errors/Http/{0}");

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await AppDbSeeder.SeedAsync(db);
}

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Posts}/{action=Index}/{id?}");

app.Run();
