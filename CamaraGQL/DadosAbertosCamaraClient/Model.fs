namespace Camara.RestAPI
module Model =
    open FSharp.Data

    type DeputyListResponse = JsonProvider<"json/deputados.json">
    type DeputyDetailsResponse = JsonProvider<"json/detalhes_deputado.json">
    type DeputyExpenseResponse = JsonProvider<"json/despesas_deputado.json", InferTypesFromValues=false>
    type LegislatureListResponse = JsonProvider<"json/legislaturas.json">

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

    

