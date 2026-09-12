using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Data.Configuration;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.Property(b => b.Id).IsRequired().ValueGeneratedNever();
        builder.Property(b => b.EventId).IsRequired();
        builder.Property(b => b.Status).HasConversion<string>().IsRequired().HasMaxLength(
            Enum.GetNames<BookingStatus>().Max(name => name.Length)
        );
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property( b => b.ProcessedAt);

        builder.HasKey(b => b.Id);

        builder.HasOne(b => b.Event)
            .WithMany()
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.CreatedAt).IsDescending().HasDatabaseName("IX_Bookings_CreatedAt_Desc");
        builder.HasIndex(b => b.EventId).HasDatabaseName("IX_Bookings_EventId");
    }
}