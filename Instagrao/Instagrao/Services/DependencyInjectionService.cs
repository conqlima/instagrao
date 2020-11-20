using Microsoft.Extensions.DependencyInjection;

namespace Instagrao.Services
{
    public static class DependencyInjectionService
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddTransient<IGetService, GetService>();
        }
    }
}
