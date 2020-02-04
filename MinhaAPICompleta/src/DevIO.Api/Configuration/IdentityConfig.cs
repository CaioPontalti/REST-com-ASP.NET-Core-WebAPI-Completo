using DevIO.Api.Data;
using DevIO.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                .AddErrorDescriber<IdentityMensagensPortugues>()  //Classe com as mensagens do Identity em Portugues
                .AddDefaultTokenProviders();


            //JWT
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //Toda autenticação é para gerar token
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; //Todo request verifica se está autenticado
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false; //apenas https
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = appSettings.ValidoEm,
                    ValidIssuer = appSettings.Emissor,
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }
}
