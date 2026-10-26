using FluentValidation;

namespace DevHabit.API.DTOs.Tags;


public sealed record CreateTagDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(1).MaximumLength(32);

        RuleFor(x => x.Description).MaximumLength(50);
    }
}