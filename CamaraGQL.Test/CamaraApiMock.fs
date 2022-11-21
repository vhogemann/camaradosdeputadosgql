namespace CamaraGQL.Test

open RestMockCore
open RestMockCore.Interfaces

module CamaraApiMock =
    
    // https://www.nuget.org/packages/rest-mock-core/
    let server port =
        new HttpServer(port)
