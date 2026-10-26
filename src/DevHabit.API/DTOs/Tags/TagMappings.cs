using DevHabit.API.Entities;

namespace DevHabit.API.DTOs.Tags;

internal static class TagQueries
{
    public static System.Linq.Expressions.Expression<Func<Tag, TagDto>> ProjectToTagDto()
    {
        // return t => t.toDto();
        return t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            UpdatedAtUtc = t.UpdatedAtUtc,
            CreatedAtUtc = t.CreatedAtUtc
        };
    }
}
internal static class TagMappings
{
    public static TagDto toDto(this Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc
        };
    }

    public static Tag toEntity(this CreateTagDto tagDto)
    {
        return new Tag
        {
            Id = $"t_{Guid.CreateVersion7()}",
            Name = tagDto.Name,
            Description = tagDto.Description,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public static void UpdateFromDto(this Tag tag, UpdateTagDto dto)
    {
        tag.Name = dto.Name;
        tag.Description = dto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;
    }
}
