module Camara.RestAPI.Legislature

open System
open FSharp.Data
open Camara.RestAPI.Model
open Microsoft.Extensions.Logging

type LegislatureRequest =
    { id: int option
      date: DateTime option }

let EmptyLegislatureRequest: LegislatureRequest =
    { id = None; date = None }

let legislatureRequestToQuery (request: LegislatureRequest) =
    seq {
        yield request.id |> Option.map (fun it -> "id", $"{it}")

        yield
            request.date
            |> Option.map (fun it -> "data", it.ToString("yyyy-MM-dd"))
    }
    |> Seq.choose id

let LegislatureList (logger: ILogger) (baseUrl: string) (request: LegislatureRequest) (pagination: Pagination option) =
    task {
        let query =
            request
            |> legislatureRequestToQuery
            |> Seq.append (paginationToQuery pagination)
            |> Seq.toList

        let! response =
            Http.AsyncRequestString(
                $"{baseUrl}/legislaturas", 
                httpMethod = "GET",
                query = query,
                headers = ["Accept", "application/json"])
            |> Async.Catch

        return
            match response with
            | Choice1Of2 payload ->
                let legislatures =
                    payload |> LegislatureListResponse.Parse

                legislatures.Dados |> Ok
            | Choice2Of2 error ->
                logger.LogError("Failed to fetch legislatures", error)
                Error error
    }
