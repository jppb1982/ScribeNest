using Microsoft.EntityFrameworkCore;
using ScribeNest.Domain.Entities;

namespace ScribeNest.Infrastructure.Data;

public static class AppDbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!db.Categories.Any())
        {
            var life = new Category { Name = "Lifestyle" };
            var travel = new Category { Name = "Viajes" };
            var food = new Category { Name = "Gastronomía" };
            db.Categories.AddRange(life, travel, food);

            db.Posts.AddRange(
                new Post
                {
                    Title = "Cómo arrancar la semana con más energía",
                    Slug = "arrancar-semana-energia",
                    Content = """
                              Todos tenemos esos lunes pesados.  
                              Pero con pequeños hábitos podés cambiar el tono de tu semana:
                              - Preparar la ropa y el desayuno la noche anterior  
                              - Escuchar música motivadora al despertar  
                              - Caminar 10 minutos al aire libre antes de sentarte a trabajar  
                              Probalo y vas a ver cómo cambia tu humor.
                              """,
                    Category = life,
                    PublishedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Post
                {
                    Title = "Un paseo inesperado por la costa",
                    Slug = "paseo-inesperado-costa",
                    Content = """
                              El sábado salí sin rumbo fijo y terminé frente al mar.  
                              Sin planearlo, caminé horas, probé un café en un barcito escondido
                              y saqué fotos que parecen postales.  
                              A veces lo mejor que podemos hacer es perdernos un rato.
                              """,
                    Category = travel,
                    PublishedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Post
                {
                    Title = "La pizza casera más fácil del mundo",
                    Slug = "pizza-casera-facil",
                    Content = """
                              No necesitás ser chef para preparar una buena pizza en casa.  
                              Ingredientes:
                              - Harina común  
                              - Agua, sal, levadura  
                              - Salsa de tomate, mozzarella y lo que tengas en la heladera  

                              En una hora tenés lista una pizza que no envidia a la de la pizzería.  
                              Consejo: probá con cebolla caramelizada, es un golazo.
                              """,
                    Category = food,
                    PublishedAt = DateTime.UtcNow.AddDays(-1)
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
