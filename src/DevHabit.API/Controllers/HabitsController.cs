using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Net.Mime;
using Asp.Versioning;
using DevHabit.API.Database;
using DevHabit.API.DTOs.Common;
using DevHabit.API.DTOs.Habits;
using DevHabit.API.Entities;
using DevHabit.API.Services;
using DevHabit.API.Services.Sort;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.API.Controllers;

[ApiVersion(1.0)]
[Route("[controller]")]
[ApiController]
public class HabitsController(
    ApplicationDbContext dbContext,
    LinkService linkService
) : ControllerBase
{
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json, CustomMediaTypeNames.Application.HateoasJson)]
    public async Task<ActionResult> GetHabits(
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService
       )
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{query.Sort}'");
        }
        if (!dataShapingService.ValidateFields<HabitDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{query.Fields}'");
        }
        query.Search ??= query.Search?.Trim().ToLower();
        IQueryable<HabitDto> habitsQuery = dbContext.Habits
            .Where(h => query.Search == null ||
                h.Name.ToLower().Contains(query.Search) ||
                h.Description != null && h.Description.ToLower().Contains(query.Search))
            .Where(h => query.Type == null || h.Type == query.Type)
            .Where(h => query.Status == null || h.Status == query.Status)
            .ApplySort(query.Sort, sortMappingProvider.GetMappings<HabitDto, Habit>())
            .Select(HabitQueries.ProjectToDto());
        var totalCount = await habitsQuery.CountAsync();
        var data = await habitsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
        bool includeLinks = query.Accpet == CustomMediaTypeNames.Application.HateoasJson;
        var paginationResult = new PaginiationResult<ExpandoObject>()
        {
            Items = dataShapingService.ShapeCollectionData(
                data,
                query.Fields,
                includeLinks ? h => CreateLinksForHabit(h.Id, query.Fields) : null),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
        };
        if (includeLinks)
        {
            paginationResult.Links = CreateLinksForHabits(query, paginationResult.HasPreviousPage, paginationResult.HasNextPage);
        }
        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    [ApiVersion(1.0)]
    public async Task<ActionResult> GetHabit(
        string id,
        string? fields,
        DataShapingService dataShapingService,
        [FromHeader(Name = "Accept")] string? accept)
    {
        if (!dataShapingService.ValidateFields<HabitWithTagsDto>(fields))
        {
            return Problem(
            statusCode: StatusCodes.Status400BadRequest,
            detail: $"The provided data shaping fields aren't valid: '{fields}'");
        }
        var habit = await dbContext
                    .Habits
                    .Where(h => h.Id == id)
                    .Select(HabitQueries.ProjectToDtoWithTags())
                    .FirstOrDefaultAsync();
        if (habit is null)
        {
            return NotFound();
        }

        var dataShaped = dataShapingService.ShapeData(habit, fields);
        if (accept == CustomMediaTypeNames.Application.HateoasJson)
        {

            List<LinkDto> links = CreateLinksForHabit(id, fields);
            dataShaped.TryAdd("links", links);
        }
        return Ok(dataShaped);
    }

    [HttpGet("{id}")]
    [ApiVersion(2.0)]
    public async Task<ActionResult> GetHabitV2(
        string id,
        string? fields,
        DataShapingService dataShapingService,
        [FromHeader(Name = "Accept")] string? accept)
    {
        if (!dataShapingService.ValidateFields<HabitWithTagsDtoV2>(fields))
        {
            return Problem(
            statusCode: StatusCodes.Status400BadRequest,
            detail: $"The provided data shaping fields aren't valid: '{fields}'");
        }
        var habit = await dbContext
                    .Habits
                    .Where(h => h.Id == id)
                    .Select(HabitQueries.ProjectToDtoWithTagsV2())
                    .FirstOrDefaultAsync();
        if (habit is null)
        {
            return NotFound();
        }

        var dataShaped = dataShapingService.ShapeData(habit, fields);
        if (accept == CustomMediaTypeNames.Application.HateoasJson)
        {
            List<LinkDto> links = CreateLinksForHabit(id, fields);
            dataShaped.TryAdd("links", links);
        }
        return Ok(dataShaped);
    }

    private List<LinkDto> CreateLinksForHabit(string id, string? fields)
    {
        return
        [
            linkService.Create(nameof(GetHabit), "self", HttpMethods.Get, new {id, fields}),
            linkService.Create(nameof(UpdateHabit), "update", HttpMethods.Put, new {id}),
            linkService.Create(nameof(PatchRabbit), "partial-update", HttpMethods.Patch, new {id}),
            linkService.Create(nameof(DeleteHabit), "self", HttpMethods.Delete, new {id}),
            linkService.Create(
                nameof(HabitTagsController.UpsertHabitTags),
                "upsert-tags",
                HttpMethods.Put,
                new {HabitId = id},
                HabitTagsController.Name)
        ];
    }

    private List<LinkDto> CreateLinksForHabits(HabitsQueryParameters queryParameters, bool hasPreviousPage, bool hasNextPage)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new {
                page = queryParameters.Page,
                pageSize = queryParameters.PageSize,
                q = queryParameters.Search,
                sort = queryParameters.Sort,
                status = queryParameters.Status,
                type = queryParameters.Type,
                fields = queryParameters.Fields,
            }),
            linkService.Create(nameof(CreateHabit), "create", HttpMethods.Post)
        ];
        if (hasPreviousPage)
        {
            links.Add(
                linkService.Create(nameof(GetHabits), "previous-page", HttpMethods.Get, new
                {
                    page = queryParameters.Page - 1,
                    pageSize = queryParameters.PageSize,
                    q = queryParameters.Search,
                    sort = queryParameters.Sort,
                    status = queryParameters.Status,
                    type = queryParameters.Type,
                    fields = queryParameters.Fields,
                })
            );
        }
        if (hasNextPage)
        {
            links.Add(
                linkService.Create(nameof(GetHabits), "next-page", HttpMethods.Get, new
                {
                    page = queryParameters.Page + 1,
                    pageSize = queryParameters.PageSize,
                    q = queryParameters.Search,
                    sort = queryParameters.Sort,
                    status = queryParameters.Status,
                    type = queryParameters.Type,
                    fields = queryParameters.Fields,
                })
            );
        }
        return links;
    }


    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(
        CreateHabitDto createHabitDto,
        IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);
        var habit = createHabitDto.ToEntity();
        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();
        var habitDto = habit.ToDto();
        habitDto.Links = CreateLinksForHabit(habitDto.Id, null);
        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }
        habit.UpdateFromDto(updateHabitDto);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchRabbit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }
        var habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto, ModelState);
        // [{
        //     "op": "replace",
        //     "path": "/name",
        // }]
        // if (!ModelState.IsValid)
        // {
        //     return ValidationProblem(ModelState);
        // }
        // value 为 null 也能更新成功，使用 TryValidateModel 来拦截
        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        // Only allow to update the name and description property.
        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }
        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}

