using System.Dynamic;
using System.Linq.Dynamic.Core;
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

[Route("[controller]")]
[ApiController]
public class HabitsController(
    ApplicationDbContext dbContext
) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult> GetHabits(
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService)
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

        var paginationResult = new PaginiationResult<ExpandoObject>()
        {
            Items = dataShapingService.ShapeCollectionData(data, query.Fields),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
        };
        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetHabit(
        string id,
        string? fields,
        DataShapingService dataShapingService)
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
        return Ok(dataShapingService.ShapeData(habit, fields));
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

