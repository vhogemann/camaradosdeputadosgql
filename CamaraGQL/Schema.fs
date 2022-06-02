module Camara.Schema
    [<ReferenceEquality>]
    type Deputado = {
        id: int
        name: string
        partidoSigla: string
    }

    type Query () =
        member __.GetDeputado():Deputado = { id = 0; name = "Deputado"; partidoSigla = "SG" } 
