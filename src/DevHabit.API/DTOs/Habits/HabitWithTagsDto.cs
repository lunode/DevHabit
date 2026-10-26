using DevHabit.API.Entities;
using Newtonsoft.Json;

namespace DevHabit.API.DTOs.Habits;

public sealed record HabitWithTagsDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required HabitType Type { get; init; }
    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public required HabitStatus Status { get; init; }
    public required bool IsArchived { get; init; }
    public DateOnly? EndDate { get; init; }
    public MilestoneDto? Milestone { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? LastCompletedAtUtc { get; init; }
    // 默认 Tags 会在 JSON 序列化的第一个字段，使其排在最末
    [JsonProperty(Order = int.MaxValue)]
    public string[] Tags { get; init; }
}

public sealed record HabitWithTagsDtoV2
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required HabitType Type { get; init; }
    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public required HabitStatus Status { get; init; }
    public required bool IsArchived { get; init; }
    public DateOnly? EndDate { get; init; }
    public MilestoneDto? Milestone { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? LastCompletedAt { get; init; }
    // 默认 Tags 会在 JSON 序列化的第一个字段，使其排在最末
    [JsonProperty(Order = int.MaxValue)]
    public string[] Tags { get; init; }
}
