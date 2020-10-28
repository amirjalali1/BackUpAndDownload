using Microsoft.Extensions.DependencyInjection;

namespace WorkerService
{
    public class JobSchedulerBuilder
    {
        internal JobSchedulerBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}