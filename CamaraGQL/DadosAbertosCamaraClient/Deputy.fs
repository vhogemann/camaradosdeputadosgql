namespace Camara.RestAPI

open FSharp.Data
open Camara.RestAPI.Model
open Microsoft.Extensions.Logging

module Deputy =

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

    //let baseUrl = "https://dadosabertos.camara.leg.br/api/v2"

    // ?id=&nome=&idLegislatura=-37226838&idLegislatura=20933510&siglaUf=velit Ut elit&siglaUf=esse ipsum ullamco voluptate&siglaPartido=velit Ut elit&siglaPartido=esse ipsum ullamco voluptate&siglaSexo=nulla sunt Lorem magna&pagina=41719264&itens=41719264&dataInicio=nulla sunt Lorem magna&dataFim=nulla sunt Lorem magna&ordem=ASC&ordenarPor=nome

    let DeputyList (logger: ILogger) (baseUrl: string) (request: DeputyRequest) (pagination: Pagination option) =
        task {
            let query =
                request
                |> deputyRequestToQuery
                |> Seq.append (paginationToQuery pagination)
                |> Seq.toList

            let! response =
                Http.AsyncRequestString($"{baseUrl}/deputados", query)
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
                Http.AsyncRequestString($"{baseUrl}/deputados/{id}")
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


    let DeputyExpenses (logger: ILogger) (baseUrl: string) (id: int) (year) (month) =
        let query =
            seq {
                if year <> null then yield "ano", year
                if month <> null then yield "mes", month
                yield "itens", "100"
            }
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
                    | Some q -> Http.AsyncRequestString(uri, q) |> Async.Catch
                    | None -> Http.AsyncRequestString uri |> Async.Catch

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
            let! expenses = fetch $"{baseUrl}/deputados/{id}/despesas" (Some query) Seq.empty
            return expenses |> Result.map Seq.toArray
        }
