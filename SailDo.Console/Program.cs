
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SailDo.Console.Client;
using SailDo.Console.Interfaces;
using SailDo.Console.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<ISailDoClient, SailDoClient>();

builder.Services.AddScoped<IFeedServices, FeedService>();

using var host = builder.Build();

await CallFeed(host.Services);

await host.RunAsync();
return;

static async Task CallFeed(IServiceProvider hostProvider)
{
    using var serviceScope = hostProvider.CreateScope();

    var provider = serviceScope.ServiceProvider;

    var feedService = provider.GetRequiredService<IFeedServices>();

    await feedService.ProcessFeed();
}