using EventsService.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Database.Configuration;

public class EventBookingConfiguration : IEntityTypeConfiguration<EventBooking>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<EventBooking> builder)
    {
        builder.ToTable("event-bookings", t =>
        {
            t.HasCheckConstraint("CK_ReservedSeats", $@"""{nameof(EventBooking.ReservedSeats)}"" >= 0");
        });
        
        builder.Property(eb => eb.BookingId).IsRequired();
        builder.Property(eb => eb.EventId).IsRequired();
        builder.Property(eb => eb.ReservedSeats).IsRequired().HasDefaultValue(0);
        builder.Property(eb => eb.UserId);
        builder.Property(eb => eb.ConfirmedAt);
        builder.Property(eb => eb.CancelledAt);

        builder.HasKey(eb => eb.BookingId);

        builder.HasOne(eb => eb.Event)
            .WithMany()
            .HasForeignKey(eb => eb.EventId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(eb => eb.EventId).HasDatabaseName("IX_EventBookings_EventId");
    }
}
