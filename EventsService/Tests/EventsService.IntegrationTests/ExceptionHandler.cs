using Npgsql;

namespace EventsService.IntegrationTests;

public static class ExceptionHandler
{
    public static void CheckUniqueConstraint(this PostgresException exception, string table, string column)
    {
        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Equal($"PK_{table}", exception?.ConstraintName);
    }

    public static void CheckNullConstraint(this PostgresException exception, string table, string column)
    {
        Assert.Equal(PostgresErrorCodes.NotNullViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Contains(column, exception?.ColumnName);
    }

    public static void CheckValueConstraint(this PostgresException exception, string table, string name)
    {
        Assert.Equal(PostgresErrorCodes.CheckViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Equal(name, exception?.ConstraintName);
    }

    public static void CheckMaxLengthConstraint(this PostgresException exception, string table)
    {
        Assert.Equal(PostgresErrorCodes.StringDataRightTruncation, exception?.SqlState);
    }

    public static void CheckForeignKey(this PostgresException exception, string table, string name)
    {
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, exception?.SqlState);
        Assert.Contains(table, exception?.TableName);
        Assert.Equal(name, exception?.ConstraintName);
    }

}
