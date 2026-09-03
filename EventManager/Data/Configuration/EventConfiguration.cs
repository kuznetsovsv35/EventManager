using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configuration;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.Property(e => e.Id).IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.StartAt).IsRequired();
        builder.Property(e => e.EndAt).IsRequired();
        builder.Property(e => e.TotalSeats).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.ReservedCount).IsRequired().HasDefaultValue(0);
        builder.Ignore(e => e.AvailableSeats);

        builder.HasKey(e => e.Id);
    }
}