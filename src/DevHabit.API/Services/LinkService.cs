using DevHabit.API.DTOs.Common;

namespace DevHabit.API.Services;

public sealed class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
{
    public LinkDto Create(
        string endpointName,
        string rel,
        string method,
        object? values = null,
        string? controller = null
    )
    {
        var href = linkGenerator.GetUriByAction(
            httpContextAccessor.HttpContext!,
            endpointName,
            controller,
            values);

        return new LinkDto
        {
            Href = href ?? throw new ArgumentException("Invalid endpoint name provided"),
            Rel = rel,
            Method = method,
        };
    }
}