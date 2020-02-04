using DevIO.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Configuration
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            /*DbContext usado para guardar os usuários da aplicação, usando o mesmo banco e mesma connectionString
              apenas para separar o controle de usuários do resto da aplicação. Poderiam usar o mesmo DbContext.*/
            services.AddDbContext<ApplicationDbContext>(opt =>  
                opt.UseSqlServer(configuration.GetConnectionString("DefaultConn")));

            //Configuração padrão do Identity
            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>() //DbContext usado para o Identity.
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
