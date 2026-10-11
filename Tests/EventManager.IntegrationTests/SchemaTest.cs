using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using EventManager.Domain.ValueObjects;
using EventManager.Application.DataTransferObjects;
using EventManager.Database;
using Npgsql;
using EventManager.Application.Cryptography;
using EventManager.Infrastructure.Services;

namespace EventManager.IntegrationTests;

[Collection(nameof(TestContainersCollection))]
public class SchemaTest(TestContainerWrapper<AppDbContext> testContainer) : DatabaseTestBase(testContainer)
{
    [Fact]
    public async Task MigrationTest()
    {
        // Given
        await ResetDatabase();
        await using var context = CreateDbContext();
        const string migrationId = $@"""{nameof(EFMigration.MigrationId)}""";
        const string productVersion = $@"""{nameof(EFMigration.ProductVersion)}""";
        const string tableName = $@"""__EFMigrationsHistory""";

        // When
        var expectedMigrations = typeof(AppDbContext).Assembly
            .GetTypes()
            .Where(t => typeof(Migration).IsAssignableFrom(t))
            .Select(t => t.Name)
            .Order()
            .ToList();

        var query = context.Database
            .SqlQueryRaw<EFMigration>($@"
                SELECT {migrationId}, {productVersion} 
                FROM {tableName}
                ORDER BY {migrationId}
                ");

        var actualMigrations = await query
            .AsAsyncEnumerable()
            .Select(m => m.MigrationId.Split('_').Last())
            .Order()
            .ToListAsync();

        // Then
        Assert.Equal(expectedMigrations, actualMigrations);
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task DatabaseConnect_Success()
    {
        // Given
        await ResetDatabase();

        // When
        await using var context = CreateDbContext();

        // Then
        Assert.True(context.Database.CanConnect());
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task EventsColumnsTest_Success()
    {
        // Given
        await ResetDatabase();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        var info = @event.ToOutputData();

        // When
        await using var work = CreateDbContext();
        work.Events.Add(@event);
        await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext();
        var eventFound = await verify.Events.FromSqlInterpolated($@"
                SELECT *
                FROM ""Events""
                WHERE ""Id"" = {@event.Id}
            ")
            .SingleOrDefaultAsync();

        var infoFound = eventFound?.ToOutputData();

        Assert.NotNull(infoFound);
        Assert.Equal(info, infoFound);
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task BookingsColumnsTest_Success()
    {
        // Given
        await ResetDatabase();

        var userId = await GetAdminUser();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        var infoEvent = @event.ToOutputData();

        var booking = new Booking(@event.Id, userId);
        var infoBooking = booking.ToInfo();

        // When
        await using var work = CreateDbContext();
        work.Events.Add(@event);
        work.Bookings.Add(booking);
        await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext();
        var bookingFound = await verify
            .Bookings.FromSqlInterpolated($@"
                SELECT b.""Id"", b.""EventId"", b.""UserId"", b.""Status"", b.""CreatedAt"", b.""ProcessedAt""
                FROM ""Events"" e
                INNER JOIN ""Bookings"" b ON b.""EventId"" = e.""Id""
                INNER JOIN ""Users"" u ON b.""UserId"" = u.""Id""
                WHERE b.""Id"" = {booking.Id}
            ")
            .AsNoTracking()
            .Include(b => b.Event)
            .Include(b => b.User)
            .SingleOrDefaultAsync();

        Assert.NotNull(bookingFound?.Event);
        Assert.Equal(infoEvent.Id, bookingFound.EventId);
        Assert.NotNull(bookingFound.User);
        Assert.Equal(userId, bookingFound.UserId);
        Assert.Equal(infoBooking, bookingFound.ToInfo());
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task EventsPrimaryKeyTest()
    {
        // Given
        await ResetDatabase();
        await using var context = CreateDbContext();
        var db = context.Database;
        var id = Guid.NewGuid();
        var title = "Simple event";
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddHours(10);
        var totalSeats = 10;

        // When
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Events""(""Id"", ""Title"", ""StartAt"", ""EndAt"", ""TotalSeats"") 
            VALUES({id}, {title}, {startAt}, {endAt}, {totalSeats})
        ");

        // Then
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(
            async () => await db.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Events""(""Id"", ""Title"", ""StartAt"", ""EndAt"", ""TotalSeats"") 
                VALUES({id}, {title}, {startAt}, {endAt}, {totalSeats})
            "));

        CheckUniqueConstraint(ex, "Events", "Id");
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task EventsConstraintsTest()
    {
        // Given
        await ResetDatabase();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };

        await using var context = CreateDbContext();
        context.Events.Add(@event);
        await context.SaveChangesAsync();

        // When
        await using var work = CreateDbContext();

        var exIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""Id"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));
        var exTitleNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""Title"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));

        var exStartAtNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""StartAt"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));
        var exEndAtNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""EndAt"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));

        var exTotalSeatsNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""TotalSeats"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));

        var exReservedCountNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events"" 
            SET ""ReservedSeats"" = NULL 
            WHERE ""Id"" = {@event.Id};
            "));

        var exTotalSeats = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events""
            SET ""TotalSeats"" = 0
            WHERE ""Id"" = {@event.Id}
            "));

        var exReservedCount = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events""
            SET ""ReservedSeats"" = -1
            WHERE ""Id"" = {@event.Id}
            "));

        var startAt = DateTime.UtcNow;
        var exStartEndAt = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Events""
            SET
                ""StartAt"" = {startAt},
                ""EndAt"" = {startAt}
            WHERE ""Id"" = {@event.Id}
            "));

        // Then        
        CheckNullConstraint(exIdNull, "Events", "Id");
        CheckNullConstraint(exTitleNull, "Events", "Title");
        CheckNullConstraint(exStartAtNull, "Events", "StartAt");
        CheckNullConstraint(exEndAtNull, "Events", "EndAt");
        CheckNullConstraint(exTotalSeatsNull, "Events", "TotalSeats");
        CheckNullConstraint(exReservedCountNull, "Events", "ReservedSeats");

        CheckValueConstraint(exTotalSeats, "Events", "CK_Events_Seats");
        CheckValueConstraint(exReservedCount, "Events", "CK_Events_Seats");
        CheckValueConstraint(exStartEndAt, "Events", "CK_Events_StartEnd");
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task BookingsPrimaryKeyTest()
    {
        // Given
        await ResetDatabase();
        await using var context = CreateDbContext();
        var db = context.Database;
        var userId = await GetAdminUser();
        var eventId = Guid.NewGuid();
        var title = "Simple event";
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddHours(10);
        var totalSeats = 10;
        var bookingId = Guid.NewGuid();
        var status = BookingStatus.Pending;
        var createdAt = DateTime.UtcNow;
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Events""(""Id"", ""Title"", ""StartAt"", ""EndAt"", ""TotalSeats"") 
            VALUES({eventId}, {title}, {startAt}, {endAt}, {totalSeats})
        ");

        // When
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Bookings""(""Id"", ""EventId"", ""UserId"", ""Status"", ""CreatedAt"")
            VALUES({bookingId}, {eventId}, {userId}, {status}, {createdAt})
        ");

        // Then
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(
            async () => await db.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Bookings""(""Id"", ""EventId"", ""UserId"", ""Status"", ""CreatedAt"")
                VALUES({bookingId}, {eventId}, {userId}, {status}, {createdAt})
            "));
        CheckUniqueConstraint(ex, "Bookings", "Id");
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task BookingsConstraintsTest()
    {
        // Given
        await ResetDatabase();

        var userId = await GetAdminUser();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };

        var booking = new Booking(@event.Id, userId);

        await using var context = CreateDbContext();
        context.Events.Add(@event);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // When
        await using var work = CreateDbContext();
        var exIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""Id"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        var exEventIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""EventId"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        var exStatusNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""Status"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        var exCreatedAtNull = await Assert.ThrowsAnyAsync<PostgresException>(async () => await work.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Bookings""
            SET ""CreatedAt"" = NULL
            WHERE ""Id"" = {booking.Id}
            "));

        // Then
        CheckNullConstraint(exIdNull, "Bookings", "Id");
        CheckNullConstraint(exEventIdNull, "Bookings", "EventId");
        CheckNullConstraint(exStatusNull, "Booking", "Status");
        CheckNullConstraint(exCreatedAtNull, "Bookings", "CreatedAt");
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task BookingsForeignKeyTest()
    {
        // Given
        await ResetDatabase();

        var userId = await GetAdminUser();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        var infoEvent = @event.ToOutputData();

        var booking = new Booking(@event.Id, userId);
        var infoBooking = booking.ToInfo();
        var booking2 = new Booking(Guid.NewGuid(), userId);

        await using var context = CreateDbContext();
        context.Events.Add(@event);
        await context.SaveChangesAsync();

        DbUpdateException? exBook2 = null;

        // When
        {
            await using var work = CreateDbContext();
            work.Bookings.Add(booking);
            await work.SaveChangesAsync();
        }
        {
            await using var work = CreateDbContext();
            work.Bookings.Add(booking2);
            exBook2 = await Assert.ThrowsAnyAsync<DbUpdateException>(async () => await work.SaveChangesAsync());
        }

        // Then
        await using var verify = CreateDbContext();
        var bookingFound = await verify.Bookings.AsNoTracking()
            .Where(b => b.Id == booking.Id)
            .Include(b => b.Event)
            .SingleOrDefaultAsync();
        var bookingCount = await verify.Bookings.AsNoTracking().CountAsync();

        Assert.NotNull(bookingFound?.Event);
        Assert.Equal(infoEvent.Id, bookingFound.EventId);
        Assert.Equal(infoBooking, bookingFound.ToInfo());
        Assert.Equal(1, bookingCount);
        CheckForeignKey(exBook2?.InnerException as PostgresException, "Booking", "FK_Bookings_Events_EventId");
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task UsersColumnsTest_Success()
    {
        // Given
        await ResetDatabase();

        const string password = nameof(password);
        IPasswordHasher hasher = new PasswordHasher();

        User user = new("test")
        {
            Role = UserRole.Admin,
            Password = hasher.Hash(password),
        };

        UserInfo info = user.ToInfo();
    
        // When
        await using var workContext = CreateDbContext();
        workContext.Users.Add(user);
        await workContext.SaveChangesAsync();
    
        // Then
        await using var verifyContext = CreateDbContext();
        var userFound = await verifyContext.Users
            .FromSqlInterpolated($@"
                SELECT * FROM ""Users""
                WHERE ""Id"" = {user.Id}
                ")
            .AsNoTracking()
            .SingleOrDefaultAsync();
        
        var infoFound = userFound?.ToInfo();

        Assert.NotNull(infoFound);
        Assert.Equal(info, infoFound);
        Assert.True(userFound is { Password: string p } && hasher.Verify(p, password));
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task UsersPrimaryKeyTest()
    {
        // Given
        await ResetDatabase();

        var id = Guid.NewGuid();
        const string login1 = "test-1";
        const string login2 = "test-2";
        const UserRole role = default;
        var createdAt = DateTime.UtcNow;
        const bool isActive = default;
        await using var context = CreateDbContext();
        var db = context.Database;
    
        // When
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Users"" (""Id"", ""Login"", ""Role"", ""IsActive"", ""CreatedAt"")
            VALUES ({id}, {login1}, {role}, {isActive}, {createdAt})
        ");
    
        // Then
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(async() =>
        {
            await db.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Users"" (""Id"", ""Login"", ""Role"", ""IsActive"", ""CreatedAt"")
                VALUES ({id}, {login2}, {role}, {isActive}, {createdAt})
            ");
        });

        CheckUniqueConstraint(ex, "Users", "Id");
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task UsersConstraintsTest()
    {
        // Given
        await ResetDatabase();

        const string password = nameof(password);
        string loginMinLength = string.Empty;
        string loginMaxLength = new('l', 33);
        const string invalidPassword = "123";

        IPasswordHasher hasher = new PasswordHasher();

        User user = new("test")
        {
            Role = UserRole.Admin,
            Password = hasher.Hash(password),
        };
        await using var context = CreateDbContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // When
        var verify = CreateDbContext();

        var exIdNull = await Assert.ThrowsAnyAsync<PostgresException>(async() =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users"" 
                SET ""Id"" = NULL
                WHERE ""Id"" = {user.Id}
            ");
        });

        var exLoginNull = await Assert.ThrowsAnyAsync<PostgresException>(async() =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Login"" = NULL
                WHERE ""Id"" = {user.Id}
            "); 
        });

        var exLoginMinLength = await Assert.ThrowsAnyAsync<PostgresException>(async() =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Login"" = {loginMinLength}
                WHERE ""Id"" = {user.Id}
            "); 
        });
        var exLoginMaxLength = await Assert.ThrowsAnyAsync<PostgresException>(async() =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Login"" = {loginMaxLength}
                WHERE ""Id"" = {user.Id}
            "); 
        });

        var exPassword = await Assert.ThrowsAnyAsync<PostgresException>(async() =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Password"" = {invalidPassword}
                WHERE ""Id"" = {user.Id}
            ") ;
        });

        var exRoleNull = await Assert.ThrowsAnyAsync<PostgresException>(async() =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""Role"" = NULL
                WHERE ""Id"" = {user.Id}
            ");
        });

        var exIsActiveNull = await Assert.ThrowsAnyAsync<PostgresException>(async() =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""IsActive"" = NULL
                WHERE ""Id"" = {user.Id}
            ");
        });

        var exCreatedAtNull = await Assert.ThrowsAnyAsync<PostgresException>(async() =>
        {
            await verify.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Users""
                SET ""CreatedAt"" = NULL
            ");
        });
    
        // Then
        CheckNullConstraint(exIdNull, "Users", "Id");
        
        CheckNullConstraint(exLoginNull, "Users", "Login");
        CheckValueConstraint(exLoginMinLength, "Users", "CK_Users_Login");
        CheckMaxLengthConstraint(exLoginMaxLength, "Users");

        CheckValueConstraint(exPassword, "Users", "CK_Users_Password");

        CheckNullConstraint(exRoleNull, "Users", "Role");
        CheckNullConstraint(exIsActiveNull, "Users", "IsActive");
        CheckNullConstraint(exCreatedAtNull, "Users", "CreatedAt");
    }

    void CheckUniqueConstraint(PostgresException? exception, string table, string column)
    {
        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Equal($"PK_{table}", exception?.ConstraintName);
    }

    void CheckNullConstraint(PostgresException? exception, string table, string column)
    {
        Assert.Equal(PostgresErrorCodes.NotNullViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Contains(column, exception?.ColumnName);
    }

    void CheckValueConstraint(PostgresException? exception, string table, string name)
    {
        Assert.Equal(PostgresErrorCodes.CheckViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Equal(name, exception?.ConstraintName);
    }

    void CheckMaxLengthConstraint(PostgresException? exception, string table)
    {
        Assert.Equal(PostgresErrorCodes.StringDataRightTruncation, exception?.SqlState);
    }

    void CheckForeignKey(PostgresException? exception, string table, string name)
    {
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Equal(name, exception?.ConstraintName);
    }
}
