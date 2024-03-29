module Camara.RestAPI.Deputy

open FSharp.Data
open Camara.RestAPI.Model
open Microsoft.Extensions.Logging

type DeputyRequest =
    { id: int option
      name: string option
      state: string option
      party: string option
      legislature: int option }

let EmptyDeputyRequest: DeputyRequest =
    { id = None
      name = None
      state = None
      party = None
      legislature = None }

let deputyRequestToQuery (request: DeputyRequest) =
    seq {
        yield request.id |> Option.map (fun it -> "id", $"{it}")
        yield request.name |> Option.map (fun it -> "nome", it)

        yield
            request.state
            |> Option.map (fun it -> "siglaUf", it)

        yield
            request.party
            |> Option.map (fun it -> "siglaPartido", it)

        yield
            request.legislature
            |> Option.map (fun it -> "idLegislatura", $"{it}")
    }
    |> Seq.choose id


let DeputyList (logger: ILogger) (baseUrl: string) (request: DeputyRequest) (pagination: Pagination option) =
    task {
        let query =
            request
            |> deputyRequestToQuery
            |> Seq.append (paginationToQuery pagination)
            |> Seq.toList

        let! response =
            Http.AsyncRequestString(
                $"{baseUrl}/deputados", 
                httpMethod = "GET",
                query = query, 
                headers = ["Accept", "application/json"])
            |> Async.Catch

        return
            match response with
            | Choice1Of2 payload ->
                let deputyList =
                    payload |> DeputyListResponse.Parse

                deputyList.Dados |> Ok
            | Choice2Of2 error ->
                logger.LogError("failed to fetch deputies", error)
                Error error
    }

let DeputyDetails (logger: ILogger) (baseUrl: string) (id: int) =
    task {
        let! response =
            Http.AsyncRequestString(
                $"{baseUrl}/deputados/{id}",
                httpMethod = "GET",
                headers = ["Accept", "application/json"])
            |> Async.Catch

        return
            match response with
            | Choice1Of2 payload ->
                let deputy =
                    payload |> DeputyDetailsResponse.Parse

                Ok deputy
            | Choice2Of2 error ->
                logger.LogError("Failed to fetch deputy details", error)
                Error error
    }

type DeputyExpensesRequest = {
    id: int
    year: int option
    month: int option
}

let DeputyExpenses (logger: ILogger) (baseUrl: string) (request: DeputyExpensesRequest) =
    let query =
        seq {
            yield request.year |> Option.map ( fun year -> "ano", $"{year}")
            yield request.month |> Option.map (fun month -> "mes", $"{month}")
            yield Some ("itens", "100")
        }
        |> Seq.choose id
        |> Seq.toList

    let getNext (exp: DeputyExpenseResponse.Root) =
        exp.Links
        |> Seq.tryFind (fun it ->
            match it.Rel with
            | "next" -> it.Href <> null && it.Href.Length > 0
            | _ -> false)
        |> Option.map (fun it -> it.Href)

    let rec fetch uri query (acc: DeputyExpenseResponse.Dado seq) =
        task {
            let! response =
                match query with
                | Some q -> Http.AsyncRequestString(uri, httpMethod = "GET", query = q, headers = ["Accept", "application/json"]) |> Async.Catch
                | None -> Http.AsyncRequestString(uri, headers = ["Accept", "application/json"]) |> Async.Catch

            match response with
            | Choice1Of2 payload ->
                let expenses =
                    payload |> DeputyExpenseResponse.Parse

                let data = expenses.Dados |> Seq.append acc

                match getNext expenses with
                | Some next ->
                    logger.LogInformation("Fetching next expenses page")
                    return! fetch next None data
                | None ->
                    logger.LogInformation("All expenses loaded")
                    return Ok data
            | Choice2Of2 error ->
                logger.LogError("Failed to fetch expenses", error)
                return Error error
        }

    task {
        let! expenses = fetch $"{baseUrl}/deputados/{request.id}/despesas" (Some query) Seq.empty
        return expenses |> Result.map Seq.toArray
    }
