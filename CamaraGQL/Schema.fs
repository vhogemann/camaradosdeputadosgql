module Camara.Schema

open System
open HotChocolate
open Microsoft.Extensions.Logging

type Deputy(logger: ILogger, client: RestAPI.IClient, data: RestAPI.Model.DeputyListResponse.Dado) =
    member val Id: int = data.Id
    member val Name: string = data.Nome
    member val State: string = data.SiglaUf
    member val Party: string = data.SiglaPartido
    member val Picture: string = data.UrlFoto

    member _.GetDetails([<Parent>] deputy: Deputy) =
        task {
            let! response = client.DeputyDetails deputy.Id

            return
                match response with
                | Ok details -> details.Dados |> DeputyDetails
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

and DeputyDetails(data: RestAPI.Model.DeputyDetailsResponse.Dados) =
    member val Id: int = data.Id
    member val FullName: string = data.NomeCivil
    member val Cpf: string = data.Cpf
    member val Education: string = data.Escolaridade
    member val Gender: string = data.Sexo
    member val BirthDate: string = data.DataNascimento
    member val DeathDate: string = data.DataFalecimento
    member val SocialNetworks: string [] = data.RedeSocial

and DeputyExpenses(data: RestAPI.Model.DeputyExpenseResponse.Dado) =
    member val Year: decimal = data.Ano
    member val Month: decimal = data.Mes
    member val SupplierName: string = data.NomeFornecedor
    member val SupplierCnpjOrCpf: string = data.CnpjCpfFornecedor
    member val DocumentCode: decimal = data.CodDocumento
    member val BatchCode: decimal = data.CodLote

    member val DocumentDate: Nullable<DateTime> =
        if data.DataDocumento <> null then
            DateTime.Parse(data.DataDocumento) |> Nullable
        else
            Nullable()

    member val DocumentNumber: string = data.NumDocumento
    member val ReimbursementNumber: string = data.NumRessarcimento
    member val InstallmentNumber: decimal = data.Parcela
    member val ExpenseType: string = data.TipoDespesa
    member val DocumentType: string = data.TipoDocumento
    member val DocumentTypeCode: decimal = data.CodTipoDocumento
    member val DocumentValue: decimal = data.ValorDocumento
    member val OverExpenseValue: decimal = data.ValorGlosa
    member val NetValue: decimal = data.ValorLiquido

type Legislature(logger: ILogger, client: RestAPI.IClient, data: RestAPI.Model.LegislatureListResponse.Dado) =
    member val Id: int = data.Id
    member val Start: DateTime = data.DataInicio
    member val End: DateTime = data.DataFim

    member _.GetDeputies([<Parent>] legislature: Legislature, id: Nullable<int>, name: string, state: string, party: string, limit: Nullable<int>, offset: Nullable<int>) =
        let pagination =
            RestAPI.Model.pagination limit offset

        let request =
            { RestAPI.Deputy.EmptyDeputyRequest with 
                legislature = Some legislature.Id
                id = Option.ofNullable id
                name = Option.ofObj name
                state = Option.ofObj state
                party = Option.ofObj party }

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
