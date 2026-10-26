using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

namespace DevHabit.API.Services;

public sealed class DataShapingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertyCaches = new();
    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        var fieldSets = GetFieldSets(fields);
        PropertyInfo[] propertyInfos = GetProperties(typeof(T));

        if (fieldSets.Any())
        {
            propertyInfos = propertyInfos
                .Where(p => fieldSets.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .ToArray();
        }
        IDictionary<string, object?> shapedObject = new ExpandoObject();
        foreach (var propertyInfo in propertyInfos)
        {
            shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }
        return (ExpandoObject)shapedObject;
    }
    public List<ExpandoObject> ShapeCollectionData<T>(IEnumerable<T> entities, string? fields)
    {
        var fieldSets = GetFieldSets(fields);
        PropertyInfo[] propertyInfos = GetProperties(typeof(T));

        if (fieldSets.Any())
        {
            propertyInfos = propertyInfos
                .Where(p => fieldSets.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .ToArray();
        }

        List<ExpandoObject> shapedObjects = [];
        foreach (var entity in entities)
        {
            IDictionary<string, object?> shapedObject = new ExpandoObject();
            foreach (var propertyInfo in propertyInfos)
            {
                shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
            }
            shapedObjects.Add((ExpandoObject)shapedObject);
        }
        return shapedObjects;
    }
    public bool ValidateFields<T>(string? fields)
    {
        if (string.IsNullOrEmpty(fields))
        {
            return true;
        }
        var fieldSets = GetFieldSets(fields);
        var propertyInfos = GetProperties(typeof(T));
        return fieldSets.All(
            f => propertyInfos.Any(
                p => p.Name.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
    private static HashSet<string> GetFieldSets(string? fields)
    {
        return fields?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => f.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
    }

    private PropertyInfo[] GetProperties(Type type)
    {
        return propertyCaches.GetOrAdd(
            type,
            static t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
    }

}