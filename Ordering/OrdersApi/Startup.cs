using MassTransit;
using Messaging.Sharedlib.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using GreenPipes;
using System;
using OrdersApi.Messages.Consumers;
using OrdersApi.Persistence;
using Microsoft.EntityFrameworkCore;
using OrdersApi.Persistence.Repository;
using OrdersApi.Hubs;
using Messaging.Services.SharedServices;
using OrdersApi.Settings;
using OrdersApi.Services;

namespace OrdersApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<OrderSettings>(Configuration);

            services.AddDbContext<OrdersContext>(options => options.UseSqlServer
                (
                Configuration["OrdersContextConnection"]
                ));

            services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                });

            services.AddHttpClient();
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed((host) => true)
                    .AllowCredentials()
                    );
            });
            services.AddTransient<IOrderRepository, OrderRepository>();

            services.AddMassTransit(
                c =>
                {
                    c.AddConsumer<RegisterOrderCommandConsumer>();
                    c.AddConsumer<OrderDispatchedEventConsumer>();
                }
                );

            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(
                cfg =>
                {
                    cfg.Host("rabbitmq", "/", h => { });

                    cfg.ReceiveEndpoint(RabbitMqMassTransitConstants.RegisterOrderCommandQueue,
                        e =>
                        {
                            e.PrefetchCount = 16;
                            e.UseMessageRetry(x => x.Interval(2, TimeSpan.FromSeconds(10)));
                            e.Consumer<RegisterOrderCommandConsumer>(provider);
                        });

                    cfg.ReceiveEndpoint(RabbitMqMassTransitConstants.OrderDispatchedServiceQueue,
                        e =>
                        {
                            e.PrefetchCount = 16;
                            e.UseMessageRetry(x => x.Interval(2, 100));
                            e.Consumer<OrderDispatchedEventConsumer>(provider);
                        });

                    //cfg.ConfigureEndpoints(provider);
                }
                ));

            services.AddSingleton<IHostedService, BusService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrdersApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrdersApi v1"));
            }
            app.UseCors("CorsPolicy");
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<OrderHub>("/orderhub");
            });

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetService<OrdersContext>().MigrateDB();
        }
    }
}
