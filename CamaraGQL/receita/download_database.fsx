// Adapted from https://github.com/okfn-brasil/receita/blob/main/tasks/download.py
#r "nuget: HtmlAgilityPack, 1.11.43"
#r "nuget: FSharp.Data"

open System
open HtmlAgilityPack

let url =
    "https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/cadastros/consultas/dados-publicos-cnpj"

let web = HtmlWeb()
let html = web.Load url

let nodes =
    html.DocumentNode.SelectNodes("//a[contains(@href, '.zip')]/@href")

let links =
    nodes
    |> Seq.filter (fun node ->
        node.NodeType = HtmlNodeType.Element
        && node.Attributes.Contains("href"))
    |> Seq.map (fun node -> node.Attributes.["href"].Value)
    |> Seq.map Uri
