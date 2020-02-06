﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DevIO.Api.Configuration;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevIO.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

       
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MeuDbContext>(opt =>
            {
                opt.UseSqlServer(Configuration.GetConnectionString("DefaultConn"));
            });

            //Add as configurações do Identity
            services.AddIdentityConfiguration(Configuration);

            //Add as configurações do AutoMapper
            services.AddAutoMapper(typeof(Startup));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<ApiBehaviorOptions>(opt =>
            {
                //desabilita o retorno automatico da validação da model, para personalizar as msgs de retorno de erro no response.
                opt.SuppressModelStateInvalidFilter = true; 
            });

            //Para resolver os problemas de CORS
            services.AddCors(options =>
            {
                options.AddPolicy("Development", //Ambiente
                       builder => builder.AllowAnyOrigin()
                                         .AllowAnyMethod()
                                         .AllowAnyHeader()
                                         .AllowCredentials());

                options.AddPolicy("Production", //Ambiente
                      builder => builder.WithMethods("GET", "PUT") //Quais metodos permite
                                        .WithOrigins("http://desenvolvedor.io", "http://teste.com") //Apenas esses dominios
                                        .SetIsOriginAllowedToAllowWildcardSubdomains()//permite sub-dominios
                                        .AllowAnyHeader()); //permite com qualquer Header
            });

            //Add para resolver as injeções de dependencia
            services.ResolveDependencies();
        }

       
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                
                app.UseCors("Development"); //CORS
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseCors("Production"); //CORS
                app.UseHsts(); //Para chamada https
            }

            /* Para chamada https. Navegador guarda em cache essa informação. Redireciona o http para o https */
            app.UseHttpsRedirection();

            //Sempre antes da configuração do MVC.
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
