module Camara.Schema

    open HotChocolate
    type Deputy (data:RestAPI.DeputyListResponse.Dado) =
        member val Id = data.Id with get
        member val Name = data.Nome with get
        member val State = data.SiglaUf with get
        member val Party = data.SiglaPartido with get
        member _.GetDetails([<Parent>]deputy:Deputy) = task {
            let! response = RestAPI.DeputyDetails deputy.Id
            return response.Dados |> DeputyDetails
        }
    and  DeputyDetails (data:RestAPI.DeputyDetailsResponse.Dados) =
        member val Id = data.Id with get
        member val FullName = data.NomeCivil with get
        member val Cpf = data.Cpf with get
        member val Education = data.Escolaridade with get
        member val Gender = data.Sexo with get
        member val BirthDate = data.DataNascimento
        member val DeathDate = data.DataFalecimento
        member val SocialNetworks = data.RedeSocial
        
    
    type Query () =
        member _.Deputies(state:string, party:string) = task {
            let! response = RestAPI.DeputyList state party
            return
                response |> Seq.map Deputy
        }
            
        
        member _.Deputy(id:int) = task {
            let! response = RestAPI.DeputyDetails id
            return
                response.Dados |> DeputyDetails
        }
