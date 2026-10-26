namespace DevHabit.API.DTOs.Tags;

public sealed record TagDto
{
    public required string Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}