open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Serilog

[<EntryPoint>]
let main args =
    let logger = LoggerConfiguration().WriteTo.Console().CreateLogger()
    let builder = WebApplication.CreateBuilder(args)
    builder.Services
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

