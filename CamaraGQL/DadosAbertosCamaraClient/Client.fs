namespace Camara.RestAPI

open System.Threading.Tasks
open Camara.RestAPI.Model
open Camara.RestAPI.Deputy
open Camara.RestAPI.Legislature
open Microsoft.Extensions.Logging

type IClient =
    abstract member DeputyList:
        DeputyRequest * Pagination option -> Task<Result<Deputado[], exn>>
    abstract member DeputyDetails:
        int -> Task<Result<DetalhesDeputado, exn>>
    abstract member DeputyExpenses:
        DeputyExpensesRequest -> Task<Result<Despesa[], exn>>

    abstract member LegislatureList:
        LegislatureRequest * Pagination option -> Task<Result<Legislatura [], exn>>

type Client(logger: ILogger, baseUrl: string) =
    interface IClient with
        member _.DeputyList(request, pagination) =
            DeputyList logger baseUrl request pagination

        member _.DeputyDetails(id) = DeputyDetails logger baseUrl id

        member _.DeputyExpenses(request) = DeputyExpenses logger baseUrl request

        member _.LegislatureList(request, pagination) =
            LegislatureList logger baseUrl request pagination
