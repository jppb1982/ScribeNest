ScribeNest

ScribeNest es un mini blog desarrollado en .NET 8 con Entity Framework Core y SQLite.
El objetivo es presentar un ejemplo claro de arquitectura en capas, el uso del patrón Repository + Unit of Work, y un CRUD funcional que se puede clonar y ejecutar rápidamente.

Tecnologías utilizadas

ASP.NET Core MVC 8.0

Entity Framework Core 8.0.20

SQLite (base de datos local en archivo .db)

Bootstrap 5

Patrón Repository + Unit of Work

Estructura del proyecto
src/
├─ ScribeNest.Web/            # ASP.NET Core MVC (UI)
├─ ScribeNest.Application/    # Interfaces y contratos (Repos, UoW)
├─ ScribeNest.Domain/         # Entidades del dominio
└─ ScribeNest.Infrastructure/ # EF Core, repositorios e inicialización de datos

Funcionalidades

Listado de posts con búsqueda.

CRUD completo (crear, leer, editar, eliminar).

Seed automático con datos de ejemplo.

Página personalizada de error 404.

Sección "Acerca de" con información personal y del proyecto.

Requisitos previos

.NET 8 SDK instalado

Editor o IDE compatible (Visual Studio, Rider o VS Code)

Cómo ejecutar el proyecto

Clonar el repositorio:

git clone https://github.com/jppb1982/ScribeNest.git
cd ScribeNest/src/ScribeNest.Web


Restaurar dependencias y ejecutar:

dotnet restore
dotnet run


Abrir en el navegador:

https://localhost:7188