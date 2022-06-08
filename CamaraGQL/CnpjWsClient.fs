module CnpjWs.RestApi
open System.Collections.Generic
open FSharp.Data

type CnpjResponse = JsonProvider<"json/cnpj_ws.json">
let baseUrl = "https://publica.cnpj.ws/cnpj"

let memoize f =
    let dict = Dictionary<_,_>()
    fun arg ->
        if dict.ContainsKey arg then
            dict.[arg]
        else
            let value = f arg
            dict.Add(arg, value)
            value

let private fetch (cnpj:string) =
    task {
        let! response = Http.AsyncRequestString($"{baseUrl}/{cnpj}")
        return response |> CnpjResponse.Parse
    }

let GetCnpj = memoize fetch