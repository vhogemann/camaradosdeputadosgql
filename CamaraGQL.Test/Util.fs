namespace CamaraGQL.Test.Util

module RandomPort =
    open System
    open System.Net.NetworkInformation
    let isFree port =
        let props =
            IPGlobalProperties.GetIPGlobalProperties()

        let tcpListeners =
            props.GetActiveTcpListeners()
            |> Array.map (fun it -> it.Port)
            |> List.ofArray

        let udpListeners =
            props.GetActiveUdpListeners()
            |> Array.map (fun it -> it.Port)
            |> List.ofArray

        tcpListeners @ udpListeners
        |> List.forall (fun it -> it <> port)

    let next () =
        let rnd = Random()

        seq { yield rnd.Next(1, 65535) }
        |> Seq.find isFree
