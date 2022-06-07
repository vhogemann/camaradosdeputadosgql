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
let DeputyList (id: Nullable<int>) (name: string) (state: string) (party: string) (legislature: Nullable<int>) =
    task {
        let query =
            seq {
                if id.HasValue then
                    yield "id", $"{id.Value}"

                if name <> null then yield "nome", name

                if state <> null then
                    yield "siglaUf", state

                if party <> null then
                    yield "siglaPartido", party

                if legislature.HasValue then
                    yield "idLegislatura", $"{legislature.Value}"
            }
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

let DeputyExpenses (id: int) (year) =
    task {
        let query =
            seq { if year <> null then yield "ano", year }
            |> Seq.toList

        let! response = Http.AsyncRequestString($"{baseUrl}/deputados/{id}/despesas", query)

        let expenses =
            response |> DeputyExpenseResponse.Parse

        return expenses.Dados
    }

let Legislatures (id: Nullable<int>) =
    task {
        let query =
            seq {
                if id.HasValue then
                    yield "id", $"{id.Value}"
            }
            |> Seq.toList

        let! response = Http.AsyncRequestString($"{baseUrl}/legislaturas", query)

        let legislatures =
            response |> LegislatureListResponse.Parse

        return legislatures.Dados
    }
