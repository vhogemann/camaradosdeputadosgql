module Camara.Schema

open System
open HotChocolate
open Microsoft.Extensions.Logging

type Deputy(data: RestAPI.DeputyListResponse.Dado) =
    member val Id: int = data.Id
    member val Name: string = data.Nome
    member val State: string = data.SiglaUf
    member val Party: string = data.SiglaPartido
    member val Picture: string = data.UrlFoto

    member _.GetDetails([<Parent>] deputy: Deputy) =
        task {
            let! response = RestAPI.DeputyDetails deputy.Id
            return
                match response with
                | Ok details -> details.Dados |> DeputyDetails
                | Error err ->
                    raise (GraphQLException(err.Message))
        }

    member _.GetExpenses([<Parent>] deputy: Deputy, year, month) =
        task {
            let! response = RestAPI.DeputyExpenses deputy.Id year month
            return response |> Seq.map DeputyExpenses |> Seq.toArray
        }

and DeputyDetails(data: RestAPI.DeputyDetailsResponse.Dados) =
    member val Id: int = data.Id
    member val FullName: string = data.NomeCivil
    member val Cpf: string = data.Cpf
    member val Education: string = data.Escolaridade
    member val Gender: string = data.Sexo
    member val BirthDate: string = data.DataNascimento
    member val DeathDate: string = data.DataFalecimento
    member val SocialNetworks: string [] = data.RedeSocial

and DeputyExpenses(data: RestAPI.DeputyExpenseResponse.Dado) =
    member val Year: int = data.Ano
    member val SupplierName: string = data.NomeFornecedor
    member val SupplierCnpjOrCpf: string = data.CnpjCpfFornecedor
    member val DocumentCode: int = data.CodDocumento
    member val BatchCode: int = data.CodLote
    member val DocumentDate: DateTime = data.DataDocumento
    member val DocumentNumber: string = data.NumDocumento
    member val ReimbursementNumber: string = data.NumRessarcimento
    member val InstallmentNumber: int = data.Parcela
    member val ExpenseType: string = data.TipoDespesa
    member val DocumentType: string = data.TipoDocumento
    member val DocumentTypeCode: int = data.CodTipoDocumento
    member val DocumentValue: decimal = data.ValorDocumento
    member val OverExpenseValue: int = data.ValorGlosa
    member val NetValue: decimal = data.ValorLiquido

type Legislature(data: RestAPI.LegislatureListResponse.Dado) =
    member val Id: int = data.Id
    member val Start: DateTime = data.DataInicio
    member val End: DateTime = data.DataFim

    member _.GetDeputies([<Parent>] legislature: Legislature, limit: Nullable<int>, offset: Nullable<int>) =
        let pagination =
            RestAPI.pagination limit offset

        let request =
            { RestAPI.EmptyDeputyRequest with legislature = Some legislature.Id }

        task {
            let! response = RestAPI.DeputyList request pagination
            return
                match response with
                | Ok deputies -> deputies |> Seq.map Deputy
                | Error err -> raise(GraphQLException(err.Message))
                
        }

type CamaraQuery(logger: ILogger<CamaraQuery>) =
    member _.Deputies(id: Nullable<int>, name: string, state: string, party: string, legislature: Nullable<int>) =
        let request =
            { RestAPI.EmptyDeputyRequest with
                id = Option.ofNullable id
                name = Option.ofObj name
                state = Option.ofObj state
                party = Option.ofObj party
                legislature = Option.ofNullable legislature }

        task {
            logger.LogInformation("fetching deputies")
            let! response = RestAPI.DeputyList request None
            return
                match response with
                | Ok deputies -> deputies |> Seq.map Deputy
                | Error err ->
                    logger.LogError("Failed to fetch deputies", err)
                    raise(GraphQLException(err.Message))
        }

    member _.Legislatures(id: Nullable<int>, date: Nullable<DateTime>) =
        task {
            logger.LogInformation("fetching legislatures")
            let! response = RestAPI.Legislatures id date
            return
                match response with
                | Ok legislatures -> legislatures |> Seq.map Legislature
                | Error err ->
                    logger.LogError("Failed to fetch legislatures", err)
                    raise (GraphQLException(err.Message))
        }
