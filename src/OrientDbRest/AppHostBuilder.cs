using System.Net.Http.Headers;
using System.Text;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace MyApp;

public class AppHostBuilder
{
    public IHost BuildHost(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(ConfigureServices)
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .UseConsoleLifetime()
            .Build();

        return host;
    }
    private void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config)
    {
        config.AddJsonFile("appsettings.json");
        config.AddEnvironmentVariables();
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection
            .AddMediatR(typeof(Program).Assembly)
            .AddSingleton<IApplicationBuilder, ApplicationBuilder>()
            ;

        var uri = context.Configuration.GetRequiredSection("OrientDb:Url").Value;
        var database = context.Configuration.GetRequiredSection("OrientDb:DatabaseName").Value;
        var username = context.Configuration.GetRequiredSection("OrientDb:UserName").Value;
        var password = context.Configuration.GetRequiredSection("OrientDb:Password").Value;

        collection.AddHttpClient("orientdb", config =>
        {
            config.BaseAddress = new Uri($"{uri}/query/{database}/sql/");

            config.DefaultRequestHeaders.Clear();

            config.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            config.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username}:{password}"));

            config.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        });

    }
}
