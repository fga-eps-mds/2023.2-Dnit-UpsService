using auth;
using app.Entidades;
using Microsoft.EntityFrameworkCore;
using Service;
using Service.Interfaces;

namespace app.DI
{
    public static class ServicesConfig
    {
        public static void AddConfigServices(this IServiceCollection services, IConfiguration configuration)
        {
            var mode = Environment.GetEnvironmentVariable("MODE");
            var conexao = mode == "container" ? "PostgreSqlDocker" : "PostgreSql";
            services.AddDbContext<AppDbContext>(optionsBuilder => optionsBuilder.UseNpgsql(configuration.GetConnectionString(conexao)));

            services.AddScoped<IUpsService, UpsService>();
            services.AddScoped<ISinistroService, SinistroService>();
            services.AddScoped<IRodoviaService, RodoviaService>();
            services.AddScoped<IEscolaService, EscolaService>();

            services.AddHttpClient<EscolaService>();
            
            services.Configure<EscolaServiceConfig>(configuration.GetSection("EscolaServiceConfig"));

            services.AddControllers(o => o.Filters.Add(typeof(HandleExceptionFilter)));

            services.AddAuth(configuration);
        }
    }
}
