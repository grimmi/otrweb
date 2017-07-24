namespace OtrWeb

open System
open System.IO
open OtrWeb

type ShowCache() =

    let showMapFile = "shows.map"
    let episodeCacheFolder = "./episodes/"

    let serializeMapping parsed name id =
        sprintf "%s***%s***%d" parsed name id
    
    let deserializeMapping (line:string) =
        match line.Split([|"***"|], StringSplitOptions.RemoveEmptyEntries) with
        |[|parsed;name;idstring|] -> Some (parsed, name, idstring |> int)
        |_ -> None

    let loadMappings =
        File.ReadAllLines showMapFile
        |> Seq.choose deserializeMapping

    let saveMapping parsed name id = 
        let serialized = serializeMapping parsed name id
        if not (File.ReadAllLines(showMapFile) |> Seq.exists(fun line -> line = serialized)) then
            File.AppendAllText(showMapFile, (sprintf "%s%s" Environment.NewLine serialized))

    let serializeEpisode (showid : int) (episode : FileType) =
        match episode with        
        |Episode(name, number, show, season, aired) -> 
            let serialized = sprintf "%s***%d***%d***%s***%s" show season number name aired
            let cachePath = Path.Combine(episodeCacheFolder, (showid |> string) + ".cache")
            File.AppendAllText(cachePath, serialized)
        |_ -> ()

    let deserializeEpisode (cachedLine:string) =
        match cachedLine.Split([|"***"|], StringSplitOptions.RemoveEmptyEntries) with
        |[|show;seasonString;numberString;name;aired|] -> Some(Episode(name, numberString |> int, show, seasonString |> int, aired))
        |_ -> None

    member this.Mappings = loadMappings

    member this.GetMappingFor parsed =
        this.Mappings
        |> Seq.tryFind(fun (p, n, id) -> parsed = p)

    member this.SaveMapping parsed mapped =
        let name, id = mapped
        saveMapping parsed name id

    member this.SaveEpisodes (show : string * int) (episodes : FileType seq) =
        let showname, showid = show
        Seq.iter(fun e -> serializeEpisode showid e) episodes

    member this.GetEpisodes showId =
        let showFile = Path.Combine(episodeCacheFolder, (showId |> string) + ".cache")
        let episodes = match File.Exists showFile with
                       |true -> showFile |> File.ReadAllLines |> List.ofArray |> List.choose deserializeEpisode
                       |_ -> List.empty<FileType>
        episodes