module Camara.RestAPI
    open FSharp.Data
    type DeputyListResponse = JsonProvider<"json/deputados.json">
    type DeputyDetailsResponse = JsonProvider<"json/detalhes_deputado.json">
    let baseUrl = "https://dadosabertos.camara.leg.br/api/v2"
    
    // ?id=&nome=&idLegislatura=-37226838&idLegislatura=20933510&siglaUf=velit Ut elit&siglaUf=esse ipsum ullamco voluptate&siglaPartido=velit Ut elit&siglaPartido=esse ipsum ullamco voluptate&siglaSexo=nulla sunt Lorem magna&pagina=41719264&itens=41719264&dataInicio=nulla sunt Lorem magna&dataFim=nulla sunt Lorem magna&ordem=ASC&ordenarPor=nome
    let DeputyList (siglaUf:string) (siglaPartido:string) = task {
        let query =
            seq {
                if siglaUf <> null then yield "siglaUf", siglaUf
                if siglaPartido <> null then yield "siglaPartido", siglaPartido
            }
            |> Seq.toList
        let! response = Http.AsyncRequestString($"{baseUrl}/deputados", query) |> Async.StartAsTask
        let deputyList =
            response
            |> DeputyListResponse.Parse
            
        return 
            deputyList.Dados
    }
    let rec DeputyDetails (id:int) = task {
        let! response = Http.AsyncRequestString($"{baseUrl}/deputados/{id}") |> Async.StartAsTask
        let deputy =
            response
            |> DeputyDetailsResponse.Parse
        return deputy
    }