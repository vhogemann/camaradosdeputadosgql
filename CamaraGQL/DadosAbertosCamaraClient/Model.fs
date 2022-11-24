namespace Camara.RestAPI

open System

module Model =
    open FSharp.Data

    [<ReferenceEquality>]
    type Envelope<'T> = { dados: 'T; links: Link [] }

    and [<ReferenceEquality>] Link =
        { href: string
          rel: string
          ``type``: string }

    [<ReferenceEquality>]
    type Deputado =
        { email: string
          id: int
          idLegislatura: decimal
          nome: string
          siglaPartido: string
          siglaUf: string
          uri: string
          uriPartido: string
          urlFoto: string }

    type DeputadoResponse = Envelope<Deputado []>

    [<ReferenceEquality>]
    type Despesa =
        { ano: decimal
          mes: decimal
          tipoDespesa: string
          codDocumento: decimal
          tipoDocumento: string
          codTipoDocumento: decimal
          dataDocumento: DateTime option //TODO: custom deserializer
          numDocumento: string
          valorDocumento: decimal
          urlDocumento: string
          nomeFornecedor: string
          cnpjCpfFornecedor: string
          valorLiquido: decimal
          valorGlosa: decimal
          numRessarcimento: string
          codLote: decimal
          parcela: decimal }

    type DespesaResponse = Envelope<Despesa []>

    [<ReferenceEquality>]
    type DetalhesDeputado =
        { cpf: string
          dataFalecimento: string
          dataNascimento: string
          escolaridade: string
          id: int
          municipioNascimento: string
          nomeCivil: string
          redeSocial: string []
          sexo: string
          ufNascimento: string
          ultimoStatus: StatusDeputado
          uri: string
          urlWebsite: string }

    and [<ReferenceEquality>] StatusDeputado =
        { condicaoEleitoral: string
          data: string
          descricaoStatus: string
          email: string
          gabinete: GabineteDeputado
          id: decimal
          idLegislatura: decimal
          nome: string
          nomeEleitoral: string
          siglaPartido: string
          siglaUf: string
          situacao: string
          uri: string
          uriPartido: string
          urlFoto: string }

    and [<ReferenceEquality>] GabineteDeputado =
        { andar: string
          email: string
          nome: string
          predio: string
          sala: string
          telefone: string }

    type DetalhesDeputadoResponse = Envelope<DetalhesDeputado>
    
    [<ReferenceEquality>]
    type Legislatura = {
        id: int
        uri: string
        dataInicio: DateTime
        dataFim: DateTime
    }
    
    type LegislaturaResponse = Envelope<Legislatura[]>

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
