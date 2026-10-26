
using DevHabit.API.DTOs.Common;

namespace DevHabit.API.DTOs.Tags;

public sealed record TagCollectionDto : ICollectionResponse<TagDto>
{
    public required List<TagDto> Items { get; init; }
}
