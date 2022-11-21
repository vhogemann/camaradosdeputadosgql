namespace CamaraGQL.Test
open CamaraGQL.Test.Util
open NUnit.Framework

module Client = 
    [<SetUp>]
    let Setup () =
        ()

    [<Test>]
    let Test1 () =
        let port = RandomPort.next()
        Assert.Pass()