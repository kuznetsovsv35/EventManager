using EventManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Database.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", t =>
        {
            t.HasCheckConstraint("CK_Users_Login", $@"LENGTH(""{nameof(User.Login)}"") BETWEEN 1 AND 32");
            
            const string fieldPassword = nameof(User.Password);
            t.HasCheckConstraint("CK_Users_Password", $@"""{fieldPassword}"" IS NULL OR LENGTH(""{fieldPassword}"") = 64");
        });
        
        builder.Property(u => u.Id).IsRequired().ValueGeneratedNever();
        builder.Property(u => u.Login).IsRequired().HasMaxLength(32);
        
        builder.Property(u => u.Role).HasConversion<string>().IsRequired().HasMaxLength
        (
            Enum.GetNames<UserRole>().Max(name => name.Length)
        );

        builder.Property(u => u.Password).HasMaxLength(64);
        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.DeletedAt);

        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.Login).IsUnique();
    }
}