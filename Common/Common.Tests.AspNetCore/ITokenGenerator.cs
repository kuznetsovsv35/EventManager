using Authorization.Domain;

namespace Common.Tests.AspNetCore;

public interface ITokenGenerator
{
    string GenerateTokens(Guid id, string name, UserRole role);
}
