﻿using AutoMapper;
using DevIO.Api.Configuration;
using DevIO.Api.Extensions;
using DevIO.Data.Context;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            //Add as configurações do Identity e do JWT
            services.AddIdentityConfiguration(Configuration);

            //Add as configurações do AutoMapper
            services.AddAutoMapper(typeof(Startup));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //Configura o versionamento da API
            services.AddApiVersioning(opt =>
            {
                opt.AssumeDefaultVersionWhenUnspecified = true; //Assume a versão padrão quando não for passado versão.
                opt.DefaultApiVersion = new ApiVersion(1,0); // Versão Default
                opt.ReportApiVersions = true; // informa se a versão da API está obsoleta.
            });

            //Configura o versionamento da API
            services.AddVersionedApiExplorer(opt => {
                opt.GroupNameFormat = "'v'VVV"; //v = Versão, VVV = V versão maior, V versão menor, V Patch
                opt.SubstituteApiVersionInUrl = true; //Substitui na url a versão padrão da API
            });

            //desabilita o retorno automatico da validação da model, para personalizar as msgs de retorno de erro no response.
            services.Configure<ApiBehaviorOptions>(opt =>
            {
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

            //Add Swagger
            services.AddSwaggerConfig();

            //Add para resolver as injeções de dependencia
            services.ResolveDependencies();

            //Config HealthCheck
            services.AddHealthChecks()
                .AddSqlServer(Configuration.GetConnectionString("DefaultConn"), name: "SqlServer")
                .AddCheck("Produtos", new SqlServerHealthCheck(Configuration.GetConnectionString("DefaultConn")))
                .AddCheck("Usuários", new UsuariosHealthCheck(Configuration.GetConnectionString("DefaultConn")));

            services.AddHealthChecksUI();
        }

       
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider provider)
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

            app.UseMiddleware<TesteMiddleware>();

            /* Para chamada https. Navegador guarda em cache essa informação. Redireciona o http para o https */
            app.UseHttpsRedirection();

            //Sempre antes da configuração do MVC.
            app.UseAuthentication();

            app.UseMvc();

            //Use Swagger. provider recebido no parametro do Configure.
            app.UseSwaggerConfig(provider);

            //Use HealthCheck
            app.UseHealthChecks("/api/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(opt =>
            {
                opt.UIPath = "/api/hc-ui";
            });
        }
    }
}
