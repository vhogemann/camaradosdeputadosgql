module Camara.Schema

open System
open HotChocolate
open Microsoft.Extensions.Logging

type Deputy(logger: ILogger, client: RestAPI.IClient, data: RestAPI.Model.Deputado) =
    member val Id: int = data.id
    member val Name: string = data.nome
    member val State: string = data.siglaUf
    member val Party: string = data.siglaPartido
    member val Picture: string = data.urlFoto
    member val  Source: RestAPI.Model.Deputado = data

    member _.GetDetails([<Parent>] deputy: Deputy) =
        task {
            let! response = client.DeputyDetails deputy.Id

            return
                match response with
                | Ok details -> details |> DeputyDetails
                | Error err ->
                    logger.LogError("Failed to fetch deputy details", err)
                    raise (GraphQLException(err.Message))
        }

    member _.GetExpenses([<Parent>] deputy: Deputy, year: Nullable<int>, month: Nullable<int>) =
        task {
            let! response =
                client.DeputyExpenses
                    { id = deputy.Id
                      year = Option.ofNullable (year)
                      month = Option.ofNullable (month) }

            return
                match response with
                | Ok expenses ->
                    expenses
                    |> Seq.map (fun expense ->
                        try
                            Some(DeputyExpenses(expense))
                        with
                        | error ->
                            logger.LogError("Failed to initialize expense", error)
                            logger.LogDebug $"%A{expense}"
                            None)
                    |> Seq.choose id
                    |> Seq.toArray
                | Error err ->
                    logger.LogError("Failed to fetch expenses", err)
                    raise (GraphQLException(err.Message))
        }

and DeputyDetails(data: RestAPI.Model.DetalhesDeputado) =
    member val Id: int = data.id
    member val FullName: string = data.nomeCivil
    member val Cpf: string = data.cpf
    member val Education: string = data.escolaridade
    member val Gender: string = data.sexo
    member val BirthDate: string = data.dataNascimento
    member val DeathDate: string = data.dataFalecimento
    member val SocialNetworks: string [] = data.redeSocial
    member val Source: RestAPI.Model.DetalhesDeputado = data

and DeputyExpenses(data: RestAPI.Model.Despesa) =
    member val Year: decimal = data.ano
    member val Month: decimal = data.mes
    member val SupplierName: string = data.nomeFornecedor
    member val SupplierCnpjOrCpf: string = data.cnpjCpfFornecedor
    member val DocumentCode: decimal = data.codDocumento
    member val BatchCode: decimal = data.codLote
    member val Source: RestAPI.Model.Despesa = data
    member val DocumentDate: Nullable<DateTime> = data.dataDocumento |> Option.toNullable
    member val DocumentNumber: string = data.numDocumento
    member val ReimbursementNumber: string = data.numRessarcimento
    member val InstallmentNumber: decimal = data.parcela
    member val ExpenseType: string = data.tipoDespesa
    member val DocumentType: string = data.tipoDocumento
    member val DocumentTypeCode: decimal = data.codTipoDocumento
    member val DocumentValue: decimal = data.valorDocumento
    member val OverExpenseValue: decimal = data.valorGlosa
    member val NetValue: decimal = data.valorLiquido

type Legislature(logger: ILogger, client: RestAPI.IClient, data: RestAPI.Model.LegislatureListResponse.Dado) =
    member val Id: int = data.Id
    member val Start: DateTime = data.DataInicio
    member val End: DateTime = data.DataFim

    member _.GetDeputies([<Parent>] legislature: Legislature, limit: Nullable<int>, offset: Nullable<int>) =
        let pagination =
            RestAPI.Model.pagination limit offset

        let request =
            { RestAPI.Deputy.EmptyDeputyRequest with legislature = Some legislature.Id }

        task {
            let! response = client.DeputyList(request, pagination)

            return
                match response with
                | Ok deputies ->
                    deputies
                    |> Seq.map (fun deputy -> Deputy(logger, client, deputy))
                | Error err -> raise (GraphQLException(err.Message))
        }

type CamaraQuery(logger: ILogger<CamaraQuery>, client: RestAPI.IClient) =
    member _.Deputies(id: Nullable<int>, name: string, state: string, party: string, legislature: Nullable<int>) =
        let request =
            { RestAPI.Deputy.EmptyDeputyRequest with
                id = Option.ofNullable id
                name = Option.ofObj name
                state = Option.ofObj state
                party = Option.ofObj party
                legislature = Option.ofNullable legislature }

        task {
            logger.LogInformation("fetching deputies")
            let! response = client.DeputyList(request, None)

            return
                match response with
                | Ok deputies ->
                    deputies
                    |> Seq.map (fun deputy -> Deputy(logger, client, deputy))
                | Error err ->
                    logger.LogError("Failed to fetch deputies", err)
                    raise (GraphQLException(err.Message))
        }

    member _.Legislatures(id: Nullable<int>, date: Nullable<DateTime>) =
        task {
            logger.LogInformation("fetching legislatures")

            // https://github.com/dotnet/fsharp/issues/4035
            let request: RestAPI.Legislature.LegislatureRequest =
                { id = Option.ofNullable (id)
                  date = Option.ofNullable (date) }

            let! response = client.LegislatureList(request, None)

            return
                match response with
                | Ok legislatures ->
                    legislatures
                    |> Seq.map (fun legislature -> Legislature(logger, client, legislature))
                | Error err ->
                    logger.LogError("Failed to fetch legislatures", err)
                    raise (GraphQLException(err.Message))
        }
