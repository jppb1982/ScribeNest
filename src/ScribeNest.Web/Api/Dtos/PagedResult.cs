namespace ScribeNest.Web.Api.Dtos;

public record PagedResult<T>(IEnumerable<T> Items, int TotalCount);
