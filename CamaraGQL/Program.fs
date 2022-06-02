open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    builder.Services.AddGraphQLServer().AddQueryType<Camara.Schema.Query>() |> ignore
    
    let app = builder.Build()

    app.MapGet("/", Func<string>(fun () -> "Hello World!")) |> ignore
    app.UseRouting() |> ignore
    app.UseEndpoints( 
        fun endpoints -> endpoints.MapGraphQL() |> ignore ) |> ignore

    app.Run() |> ignore

    0 // Exit code

