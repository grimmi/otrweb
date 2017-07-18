namespace OtrWeb

open System
open System.IO

type ShowCache() =

    let mapFile = "shows.map"

    let serializeMapping parsed name id =
        sprintf "%s***%s***%d" parsed name id
    
    let deserializeMapping (line:string) =
        match line.Split([|"***"|], StringSplitOptions.RemoveEmptyEntries) with
        |[|parsed;name;idstring|] -> Some (parsed, name, idstring |> int)
        |_ -> None

    let loadMappings =
        File.ReadAllLines mapFile
        |> Seq.choose deserializeMapping

    let saveMapping parsed name id = 
        let serialized = serializeMapping parsed name id
        if not (File.ReadAllLines(mapFile) |> Seq.exists(fun line -> line = serialized)) then
            File.AppendAllText(mapFile, (sprintf "%s%s" Environment.NewLine serialized))

    member this.Mappings = loadMappings

    member this.GetMappingFor parsed =
        this.Mappings
        |> Seq.tryFind(fun (p, n, id) -> parsed = p)

    member this.SaveMapping parsed mapped =
        let name, id = mapped
        saveMapping parsed name id