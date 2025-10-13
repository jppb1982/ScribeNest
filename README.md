# ScribeNest

ScribeNest es un mini blog desarrollado con .NET 8 (ASP.NET Core MVC + API REST) y Angular 16.
El proyecto se diseñó como un ejercicio práctico para aplicar una arquitectura en capas, implementar los patrones Repository y Unit of Work, y conectar un backend en .NET con un frontend moderno en Angular.

La aplicación utiliza SQLite como base de datos local para simplificar la ejecución: al iniciar el proyecto se generan datos de ejemplo automáticamente.
Esto permite que cualquier persona (por ejemplo, un reclutador o evaluador técnico) pueda clonar, ejecutar y probar la aplicación sin necesidad de instalar o configurar un servidor de base de datos.

## Estructura del repositorio

```
ScribeNest/
├─ src/
│  ├─ ScribeNest.Web/                # Backend MVC + API
│  │  ├─ Controllers/
│  │  │  ├─ PostsApiController.cs
│  │  │  └─ CategoriesApiController.cs
│  │  ├─ Api/Dtos/
│  │  │  ├─ PagedResult.cs
│  │  │  ├─ PostListItemDto.cs
│  │  │  ├─ PostDetailDto.cs
│  │  │  └─ CategoryDto.cs
│  │  └─ Program.cs
│  ├─ ScribeNest.Application/
│  ├─ ScribeNest.Domain/
│  └─ ScribeNest.Infrastructure/
└─ scribenest-front/                 # Frontend Angular 16
   ├─ src/
   │  ├─ app/
   │  │  ├─ core/                    # models/ y services/
   │  │  ├─ features/                # home/, post-detail/, about/, not-found/
   │  │  ├─ app.routes.ts
   │  │  └─ app.component.ts
   │  ├─ environments/environment.ts
   │  └─ index.html
```

## Stack

### Backend
- .NET 8 (ASP.NET Core MVC)
- Entity Framework Core 8 + SQLite
- Repository + Unit of Work

### Frontend
- Angular 16 (standalone components)
- Bootstrap 5 con tema Lux por CDN

### Desarrollo
- CORS habilitado para `http://localhost:4200` y `https://localhost:4200`
- Certificado de desarrollo HTTPS de .NET confiado

---

## API

Base: `https://localhost:7188/api`

La API expone tres endpoints principales bajo la ruta base:

`https://localhost:7188/api`


### Endpoints disponibles

- GET /api/posts
  Devuelve el listado de publicaciones con búsqueda, filtro por categoría y paginación.
  Parámetros opcionales:
  - q: texto de búsqueda
  - categoryId: id de categoría
  - page y pageSize: para paginar resultados
  El formato de respuesta es un objeto con la lista y el total de registros (PagedResult<PostListItemDto>).

- GET /api/posts/{id}
  Devuelve el detalle de un post específico, incluyendo el contenido completo (PostDetailDto).

- GET /api/categories
  Retorna todas las categorías disponibles ordenadas por nombre (CategoryDto).

- Notas técnicas:
  - Las consultas usan AsNoTracking() para lecturas más livianas.
  - Los resultados se ordenan por fecha de publicación descendente.
  - Se puede buscar por título, contenido o categoría (sin distinción de mayúsculas/minúsculas).

---

## Frontend

### Configuración mínima
- `environment.ts`:
  ```ts
  export const environment = { apiBaseUrl: 'https://localhost:7188/api' };
  ```
Proveedores en `main.ts`: `provideHttpClient(withFetch())` y `provideRouter(routes)`
`index.html` incluye Lux y el bundle JS de Bootstrap por CDN

Comportamiento
- Home: búsqueda, filtro por categoría, paginación y sincronización de estado vía querystring
- Detalle: `/post/:id`
- About y 404 como rutas separadas
- Interceptor opcional para manejo básico de errores

## Ejecución

Backend
```
cd ScribeNest/src/ScribeNest.Web
dotnet dev-certs https --trust
dotnet run
# Comprobación:
# https://localhost:7188/api/posts?page=1&pageSize=5
# https://localhost:7188/api/categories
```

Frontend
```
cd scribenest-front
npm i
ng serve --open
```

## Próximos pasos

1. CRUD completo en Angular (reactive forms y validaciones).
2. Autenticación con JWT y guards.
3. Tests de servicios y componentes; manejo de errores más robusto.
4. Lazy loading y caching simple.
5. Validaciones en backend (DataAnnotations/FluentValidation).
6. Logs con Serilog y manejo global de errores.
7. Docker y flujo básico de CI/CD.
