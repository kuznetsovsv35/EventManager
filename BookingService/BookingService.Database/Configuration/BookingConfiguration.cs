using BookingService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Database.Configuration;

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
        builder.Property(b => b.ProcessedAt);

        builder.HasKey(b => b.Id);

        builder.HasIndex(b => b.CreatedAt).HasDatabaseName("IX_Bookings_CreatedAt");
        builder.HasIndex(b => b.EventId).HasDatabaseName("IX_Bookings_EventId");
        builder.HasIndex(b => b.UserId).HasDatabaseName("IX_Bookings_UserId");
    }
}