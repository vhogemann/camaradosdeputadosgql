module Camara.RestAPI

open System
open FSharp.Data

type DeputyListResponse = JsonProvider<"json/deputados.json">
type DeputyDetailsResponse = JsonProvider<"json/detalhes_deputado.json">
type DeputyExpenseResponse = JsonProvider<"json/despesas_deputado.json">
type LegislatureListResponse = JsonProvider<"json/legislaturas.json">

let baseUrl =
    "https://dadosabertos.camara.leg.br/api/v2"

// ?id=&nome=&idLegislatura=-37226838&idLegislatura=20933510&siglaUf=velit Ut elit&siglaUf=esse ipsum ullamco voluptate&siglaPartido=velit Ut elit&siglaPartido=esse ipsum ullamco voluptate&siglaSexo=nulla sunt Lorem magna&pagina=41719264&itens=41719264&dataInicio=nulla sunt Lorem magna&dataFim=nulla sunt Lorem magna&ordem=ASC&ordenarPor=nome


type DeputyRequest =
    { id: int option
      name: string option
      state: string option
      party: string option
      legislature: int option
      offset: int option
      limit: int option }

let EmptyDeputyRequest: DeputyRequest =
    { id = None
      name = None
      state = None
      party = None
      legislature = None
      offset = None
      limit = None }

type Pagination = { offset: int; limit: int }

let pagination limit offset =
    match Option.ofNullable (limit), Option.ofNullable (offset) with
    | Some l, Some o -> { limit = l; offset = o } |> Some
    | Some l, None -> { limit = l; offset = 0 } |> Some
    | None, Some o -> { limit = 0; offset = 0 } |> Some
    | None, None -> None

let paginationToQuery (pagination: Pagination option) =
    seq {
        yield
            pagination
            |> Option.map (fun it -> "itens", $"{it.limit}")

        yield
            pagination
            |> Option.map (fun it -> "pagina", $"{it.offset}")
    }
    |> Seq.choose id

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

let DeputyList (request: DeputyRequest) (pagination: Pagination option) =
    task {
        let query =
            request
            |> deputyRequestToQuery
            |> Seq.append (paginationToQuery pagination)
            |> Seq.toList

        let! response =
            Http.AsyncRequestString($"{baseUrl}/deputados", query)
            |> Async.StartAsTask

        let deputyList =
            response |> DeputyListResponse.Parse

        return deputyList.Dados
    }

let DeputyDetails (id: int) =
    task {
        let! response =
            Http.AsyncRequestString($"{baseUrl}/deputados/{id}")
            |> Async.StartAsTask

        let deputy =
            response |> DeputyDetailsResponse.Parse

        return deputy
    }


let DeputyExpenses (id: int) (year) (month) =
    let query =
        seq {
            if year <> null then yield "ano", year
            if month <> null then yield "mes", month
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
                | Some q -> Http.AsyncRequestString(uri, q)
                | None -> Http.AsyncRequestString uri

            let expenses =
                response |> DeputyExpenseResponse.Parse

            let data = expenses.Dados |> Seq.append acc

            match getNext expenses with
            | Some next -> return! fetch next None data
            | None -> return data
        }

    task {
        let! expenses = fetch $"{baseUrl}/deputados/{id}/despesas" (Some query) Seq.empty
        return expenses |> Seq.toArray
    }

let Legislatures (id: Nullable<int>) (date: Nullable<DateTime>) =
    task {
        let query =
            seq {
                if id.HasValue then
                    yield "id", $"{id.Value}"

                if date.HasValue then
                    yield "data", date.Value.ToString("yyyy-MM-dd")
            }
            |> Seq.toList

        let! response = Http.AsyncRequestString($"{baseUrl}/legislaturas", query)

        let legislatures =
            response |> LegislatureListResponse.Parse

        return legislatures.Dados
    }
