using Authorization.Domain;
using Common.Tests.AspNetCore;

namespace BookingService.WebTests;

public class WebTestBase(WebTestFactory factory) : WebTestBase<Program>(factory), IClassFixture<WebTestFactory>
{
    protected string LoginUser(string login)
        => login switch
        {
            "admin" => factory.TokenGenerator.GenerateTokens(Guid.NewGuid(), login, UserRole.Admin),
            _ => factory.TokenGenerator.GenerateTokens(Guid.NewGuid(), login, UserRole.User)
        };
}