﻿using auth;
using service;
using service.Interfaces;

namespace app.DI
{
    public static class ServicesConfig
    {
        public static void AddConfigServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUpsService, UpsService>();
            services.AddScoped<ISinistroService, SinistroService>();
            services.AddScoped<IRodoviaService, RodoviaService>();

            services.AddAuth(configuration);
        }
    }
}
