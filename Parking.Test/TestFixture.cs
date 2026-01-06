using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parking.WebApi.Extensions;

namespace Parking.Test;

public class TestFixture
{
    public ServiceProvider ServiceProvider { get; }
    public IConfiguration Configuration { get; }

    public TestFixture()
    {
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=parking;User Id=sa;Password=Aa@123456;TrustServerCertificate=true;MultipleActiveResultSets=True;"
        };
        
        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        
        services.AddIdentityServices(Configuration);
        services.AddToDi();

        // Build provider once
        ServiceProvider = services.BuildServiceProvider();
    }
}