open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let baseUrl = builder.Configuration["CamaraRestAPI:baseUrl"]
    builder
        .Logging
            .AddConsole()
        .Services
            .AddSingleton<Camara.RestAPI.IClient>( fun ctx ->
                let logger = ctx.GetService<ILogger<Camara.RestAPI.Client>>()
                Camara.RestAPI.Client(logger, baseUrl) :> Camara.RestAPI.IClient)
            .AddGraphQLServer()
                .AddQueryType<Camara.Schema.CamaraQuery>()
                |> ignore
    
    let app = builder.Build()

    app.MapGet("/", Func<string>(fun () -> "Hello World!")) |> ignore
    app.UseRouting() |> ignore
    app.UseEndpoints( 
        fun endpoints -> endpoints.MapGraphQL() |> ignore ) |> ignore

    app.Run()

    0 // Exit code

