using System.Linq.Dynamic.Core;

namespace DevHabit.API.Services.Sort;

public sealed record SortMapping(string SortField, string PropertyName, bool Reverse = false);

public interface ISortMappingDefinition;

public class SortMappingDefinition<TSource, TDestination> : ISortMappingDefinition
{
    public required SortMapping[] Mappings { get; set; }
}

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions)
{
    public SortMapping[] GetMappings<TSource, TDestination>()
    {
        SortMappingDefinition<TSource, TDestination>? sortMappingDefinition =
             sortMappingDefinitions.OfType<SortMappingDefinition<TSource, TDestination>>().FirstOrDefault();

        if (sortMappingDefinition is null)
        {
            throw new InvalidOperationException($"The mappings from '{typeof(TSource).Name}' to '{typeof(TDestination).Name}' is not defined.");
        }

        return sortMappingDefinition.Mappings;
    }

    public bool ValidateMappings<TSource, TDestination>(string? sort)
    {
        if (string.IsNullOrEmpty(sort))
        {
            return true;
        }
        var mappings = GetMappings<TSource, TDestination>();
        var sortFields = sort.Split(',').Select(s => s.Trim().Split(' ')[0]).Where(s => !string.IsNullOrEmpty(s));
        return sortFields.All(f => mappings.Any(m => m.SortField.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}

internal static class QueryExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sort,
        SortMapping[] sortMappings,
        string defaultOrderBy = "Id")
    {
        if (string.IsNullOrEmpty(sort))
        {
            return query.OrderBy(defaultOrderBy);
        }
        string[] sortFields = sort.Split(',')
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToArray();
        var orderByParts = new List<string>();
        foreach (var item in sortFields)
        {
            var (sortField, isDescending) = ParseSortField(item);
            var sortMapping = sortMappings.First(m => m.SortField.Equals(sortField, StringComparison.OrdinalIgnoreCase));
            string direction = (isDescending, sortMapping.Reverse) switch
            {
                (false, false) => "ASC",
                (false, true) => "DESC",
                (true, false) => "DESC",
                (true, true) => "ASC",
            };
            orderByParts.Add($"{sortMapping.PropertyName} {direction}");
        }
        string orderBy = string.Join(',', orderByParts);
        return query.OrderBy(orderBy);
    }
    private static (string SortField, bool IsDescending) ParseSortField(string field)
    {
        string[] parts = field.Split(' ');
        string sortField = parts[0];
        bool isDescending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
        return (sortField, isDescending);
    }
}