using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configuration;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events", t =>
        {
            t.HasCheckConstraint("CK_Events_Seats", "\"TotalSeats\" > 0 and \"ReservedCount\" >= 0");
            t.HasCheckConstraint("CK_Events_StartEnd", "\"StartAt\" < \"EndAt\"");
        });
 
        builder.Property(e => e.Id).IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.StartAt).IsRequired();
        builder.Property(e => e.EndAt).IsRequired();
        builder.Property(e => e.TotalSeats).IsRequired();
        builder.Property(e => e.ReservedCount).IsRequired().HasDefaultValue(0);
        builder.Ignore(e => e.AvailableSeats);
        
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.StartAt).IsDescending().HasDatabaseName("IX_Events_StartAt_Desc");
    }
}