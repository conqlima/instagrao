using Instagrao.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Instagrao.Repositories
{
    public static class DependencyInjectionRepositories
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IRepository, Repository>();
        }
    }
}
