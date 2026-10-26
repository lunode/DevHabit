
using DevHabit.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.API.Database.Configurations;

public class HabitTagEntityTypeConfiguration : IEntityTypeConfiguration<HabitTag>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<HabitTag> builder)
    {
        builder.HasKey(ht => new { ht.HabitId, ht.TagId });

        builder.HasOne<Tag>()
            .WithMany()
            .HasForeignKey(ht => ht.TagId);

        builder.HasOne<Habit>()
            .WithMany(h => h.HabitTags)
            .HasForeignKey(ht => ht.HabitId);
    }
}
