#r "nuget: FSharp.Data.GraphQL.Client"
open FSharp.Data.GraphQL
type CamaraClient = GraphQLProvider<"http://localhost:5078/graphql">