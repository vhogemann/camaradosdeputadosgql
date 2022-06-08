module Camara.Schema

open System
open HotChocolate

type Deputy(data: RestAPI.DeputyListResponse.Dado) =
    member val Id = data.Id
    member val Name = data.Nome
    member val State = data.SiglaUf
    member val Party = data.SiglaPartido
    member val Picture = data.UrlFoto

    member _.GetDetails([<Parent>] deputy: Deputy) =
        task {
            let! response = RestAPI.DeputyDetails deputy.Id
            return response.Dados |> DeputyDetails
        }

    member _.GetExpenses([<Parent>] deputy: Deputy, year) =
        task {
            let! response = RestAPI.DeputyExpenses deputy.Id year

            return
                if response <> null then
                    response |> Seq.map DeputyExpenses
                else
                    Seq.empty
        }

and DeputyDetails(data: RestAPI.DeputyDetailsResponse.Dados) =
    member val Id = data.Id
    member val FullName = data.NomeCivil
    member val Cpf = data.Cpf
    member val Education = data.Escolaridade
    member val Gender = data.Sexo
    member val BirthDate = data.DataNascimento
    member val DeathDate = data.DataFalecimento
    member val SocialNetworks = data.RedeSocial

and DeputyExpenses(data: RestAPI.DeputyExpenseResponse.Dado) =
    member val Year = data.Ano
    member val SupplierName = data.NomeFornecedor
    member val SupplierCnpjOrCpf = data.CnpjCpfFornecedor
    member val DocumentCode = data.CodDocumento
    member val BatchCode = data.CodLote
    member val DocumentDate = data.DataDocumento
    member val DocumentNumber = data.NumDocumento
    member val ReimbursementNumber = data.NumRessarcimento
    member val InstallmentNumber = data.Parcela
    member val ExpenseType = data.TipoDespesa
    member val DocumentType = data.TipoDocumento
    member val DocumentTypeCode = data.CodTipoDocumento
    member val DocumentValue = data.ValorDocumento
    member val OverExpenseValue = data.ValorGlosa
    member val NetValue = data.ValorLiquido
    member _.GetSupplier ([<Parent>] expense:DeputyExpenses) =
        task {
            let! response = CnpjWs.RestApi.GetCnpj (expense.SupplierCnpjOrCpf)
            return response |> Supplier
        }
and Supplier (data:CnpjWs.RestApi.CnpjResponse.Root) =
    member val Name = data.RazaoSocial
    member val UpdatedAt = data.AtualizadoEm
type Legislature(data: RestAPI.LegislatureListResponse.Dado) =
    member val Id = data.Id
    member val Start = data.DataInicio
    member val End = data.DataFim

    member _.GetDeputies([<Parent>] legislature: Legislature) =
        task {
            let! response = RestAPI.DeputyList (Nullable()) (null) (null) (null) (Nullable legislature.Id)
            return response |> Seq.map Deputy
        }

type CamaraQuery() =
    member _.Deputies(id: Nullable<int>, name: string, state: string, party: string, legislature: Nullable<int>) =
        task {
            let! response = RestAPI.DeputyList id name state party legislature
            return response |> Seq.map Deputy
        }

    member _.Legislatures(id: Nullable<int>) =
        task {
            let! response = RestAPI.Legislatures id
            return response |> Seq.map Legislature
        }
