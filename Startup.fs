﻿namespace OtrWeb

open OtrWeb.Options
open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging


type Startup private () =

    new (env: IHostingEnvironment) as this =
        Startup() then

        let builder =
            ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
                .AddJsonFile((sprintf "appsettings.%s.json" (env.EnvironmentName)), optional = true)
                .AddEnvironmentVariables()

        this.Configuration <- builder.Build()

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
        services.AddSingleton(this.Configuration) |> ignore
        services.Configure<OtrOptions>(this.Configuration.GetSection("Otr")) |> ignore
        services.Configure<KodiOptions>(this.Configuration.GetSection("Kodi")) |> ignore
        services.Configure<TvDbOptions>(this.Configuration.GetSection("TvDb")) |> ignore 
        services.Configure<MovieDbOptions>(this.Configuration.GetSection("MovieDb")) |> ignore
        services.AddSingleton<TvDbApi>() |> ignore
        services.AddSingleton<MovieDbApi>() |> ignore
        services.AddSingleton<InfoCollector>() |> ignore
        services.AddSingleton<JobService>() |> ignore
        services.AddMvc() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment, loggerFactory: ILoggerFactory) =

        loggerFactory.AddConsole(this.Configuration.GetSection("Logging")) |> ignore
        loggerFactory.AddDebug() |> ignore

        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        else
            app.UseExceptionHandler("/Home/Error") |> ignore

        app.UseStaticFiles() |> ignore

        app.UseMvc(fun routes ->
            routes.MapRoute(
                name = "default",
                template = "") |> ignore
            ) |> ignore

    member val Configuration : IConfigurationRoot = null with get, set
