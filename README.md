# ScribeNest (ASP.NET Core + EF Core + SQLite)

Proyecto demo para recruiters, .NET **8 LTS**, arquitectura por capas (Web, Application, Domain, Infrastructure),
patrón Repository + UnitOfWork y datos seed para correr al instante.

## Stack
- ASP.NET Core MVC 8.0
- Entity Framework Core 8.0.20
- SQLite (archivo local `scribenest.db`)
- Clean-ish architecture para perfil junior

## Cómo correr
```bash
git clone https://github.com/<tu-usuario>/ScribeNest.git
cd ScribeNest/src/ScribeNest.Web
dotnet run
