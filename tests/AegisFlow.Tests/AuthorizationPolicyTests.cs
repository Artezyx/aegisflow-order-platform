using System.Security.Claims;
using AegisFlow.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class AuthorizationPolicyTests
{
    private static IAuthorizationService Service()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:SigningKey"] = new string('x', 48),
                ["Auth:Issuer"] = "aegisflow-local",
                ["Auth:Audience"] = "aegisflow-api"
            }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOrderApiAuthentication(configuration);
        return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
    }

    [Theory]
    [InlineData("orders.read","orders.read",true)]
    [InlineData("orders.write","orders.write",true)]
    [InlineData("orders.read","orders.write",false)]
    [InlineData("inventory.read","inventory.write",true)]
    [InlineData("inventory.write","inventory.read",false)]
    public async Task EnforcesScopes(string policy, string scope, bool expected)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub","test-user"),new Claim("scope",scope)],"Bearer"));
        var response=await Service().AuthorizeAsync(user,policy);
        Assert.Equal(expected,response.Succeeded);
    }

    [Fact]
    public async Task AnonymousUsersAreRejected()
    {
        var response=await Service().AuthorizeAsync(new ClaimsPrincipal(), "orders.read");
        Assert.False(response.Succeeded);
    }

    [Fact]
    public void RefusesMissingSigningKey()
    {
        var services=new ServiceCollection();
        Assert.Throws<InvalidOperationException>(() =>
            services.AddOrderApiAuthentication(new ConfigurationBuilder().Build()));
    }
}
