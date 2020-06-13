using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FluxorBlazorWeb.EffectsTutorial.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                    .UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build())
                    .UseStartup<Startup>()
                    .Build();
        }
    }
}
