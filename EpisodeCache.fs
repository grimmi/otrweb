namespace OtrWeb

open System
open System.IO
open OtrWeb

type ShowCache() =

    let showMapFile = "shows.map"
    let episodeCacheFolder = "./episodes/"

    let getShowCacheFile showId =
        let cachePath = Path.Combine(episodeCacheFolder, (showId |> string) + ".cache")
        if not (Directory.Exists episodeCacheFolder) then
            Directory.CreateDirectory episodeCacheFolder |> ignore
        if not (File.Exists cachePath) then
            let newFile = File.Create cachePath
            newFile.Close()
        cachePath

    let getShowMapFile =
        if not (File.Exists showMapFile) then
            let newFile = File.Create showMapFile
            newFile.Close()
        showMapFile

    let serializeMapping parsed name id =
        sprintf "%s***%s***%d" parsed name id
    
    let deserializeMapping (line:string) =
        match line.Split([|"***"|], StringSplitOptions.RemoveEmptyEntries) with
        |[|parsed;name;idstring|] -> Some (parsed, name, idstring |> int)
        |_ -> None

    let loadMappings =
        File.ReadAllLines getShowMapFile
        |> Seq.choose deserializeMapping

    let saveMapping parsed name id = 
        let serialized = serializeMapping parsed name id
        if not (File.ReadAllLines(getShowMapFile) |> Seq.exists(fun line -> line = serialized)) then
            File.AppendAllText(getShowMapFile, (sprintf "%s%s" Environment.NewLine serialized))

    let serializeEpisode showid episode =
        match episode with        
        |Episode(name, number, show, _, season, aired) -> 
            let serialized = sprintf "%s***%d***%d***%s***%s%s" show season number name aired Environment.NewLine
            let cachePath = getShowCacheFile showid
            File.AppendAllText(cachePath, serialized)
        |_ -> ()

    let deserializeEpisode (cachedLine:string) =
        match cachedLine.Split([|"***"|], StringSplitOptions.RemoveEmptyEntries) with
        |[|show;seasonString;numberString;name;aired|] -> Some(Episode(name, numberString |> int, show, [||], seasonString |> int, aired))
        |_ -> None

    member this.Mappings = loadMappings

    member this.GetMappingFor parsed =
        this.Mappings
        |> Seq.filter(fun (p, _, _) -> parsed = p) |> List.ofSeq
        // |> Seq.tryFind(fun (p, n, id) -> parsed = p)

    member this.SaveMapping parsed mapped =
        let name, id = mapped
        saveMapping parsed name id

    member this.SaveEpisodes (showid:int, showname:string) episodes =
        episodes
        |> Seq.sortBy(fun e -> match e with
                               |Episode(_,number,_,_,season,_) -> (season, number)
                               |_ -> (0,0))
        |> Seq.iter(fun e -> serializeEpisode showid e)

    member this.GetEpisodes showId =
        let showFile = getShowCacheFile showId
        showFile |> File.ReadAllLines |> List.ofArray |> List.choose deserializeEpisode