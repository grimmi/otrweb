namespace OtrWeb

open OtrDecoder
open OtrWeb.Options
open System
open System.IO
open System.Threading.Tasks
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type DecodeJob() =
    let id = Guid.NewGuid()
    let invalidChars = Array.concat[|Path.GetInvalidFileNameChars(); Path.GetInvalidPathChars()|]        
    let isValid c = not (Seq.exists(fun ch -> ch = c) invalidChars)
    let makeFileName show name season number =
        let cleanShow = show |> String.filter isValid
        let cleanEpisode = name |> String.filter isValid
        sprintf "%s %dx%d %s" cleanShow season number cleanEpisode

    let makeMovieFileName movie =
        sprintf "%s" (movie |> String.filter isValid)
    
    member this.Id
        with get () = id

    member val internal ProgValue = 0. with get, set

    member this.Progress 
        with get () = Decimal.Parse(this.ProgValue.ToString("N2"))

    member val CurrentStep = "" with get, set

    member val Files : string list = [] with get, set

    member val Cutlist : bool = false with get, set

    member val IsDone = false with get, set

    member this.RunEpisode (episode:JObject) (options:OtrOptions) = async {
        let decoder = OtrFileDecoder()
        let filename = Path.GetFileName(episode.Value<string>("file"))
        this.CurrentStep <- filename
        let source = Path.Combine (options.KeyFilePath, filename)
        printfn "sourcepath: %s" source
        let decodeResult = decoder.DecodeFile source options
        this.Cutlist <- decoder.UsedCutlist
        printfn "decoderesult: %s" decodeResult
        let show = episode.Value<string>("show")
        let targetname = makeFileName show (episode.Value<string>("name")) (episode.Value<int>("season")) (episode.Value<int>("number"))
        let targetpath = Path.Combine(options.ProcessTargetPath, options.EpisodeFolder, show, targetname) + ".avi"
        printfn "targetpath: %s" targetpath
        if not (Directory.Exists(Path.Combine(options.ProcessTargetPath, options.EpisodeFolder))) then
            Directory.CreateDirectory(Path.Combine(options.ProcessTargetPath, options.EpisodeFolder)) |> ignore
        File.Copy(decodeResult, targetpath)
        if (File.Exists(Path.Combine(options.KeyFilePath, "processed", filename))) then
            File.Delete(Path.Combine(options.KeyFilePath, "processed", filename))
        File.Move(Path.Combine(source), Path.Combine(options.KeyFilePath, "processed", filename))
        this.IsDone <- true
    }

    member this.RunMovie (movie:JObject) (options:OtrOptions) = async {
        let decoder = OtrFileDecoder()
        let filename = Path.GetFileName(movie.Value<string>("file"))
        this.CurrentStep <- filename
        let source = Path.Combine (options.KeyFilePath, filename)
        printfn "source: %s" source
        let decodeResult = decoder.DecodeFile source options
        printfn "decoderesult: %s" decodeResult
        let name = movie.Value<string>("name") |> makeMovieFileName
        let targetpath = Path.Combine(options.ProcessTargetPath, options.MovieFolder, name + ".avi")
        printfn "targetpath: %s" targetpath
        if not (Directory.Exists(Path.Combine(options.ProcessTargetPath, options.MovieFolder))) then
            Directory.CreateDirectory(Path.Combine(options.ProcessTargetPath, options.MovieFolder)) |> ignore
        File.Copy(decodeResult, targetpath)
        if (File.Exists(Path.Combine(options.KeyFilePath, "processed", filename))) then
            File.Delete(Path.Combine(options.KeyFilePath, "processed", filename))
        File.Move(Path.Combine(source), Path.Combine(options.KeyFilePath, "processed", filename))
        this.IsDone <- true
    }

    member this.ToJson() =
        let json = JObject()
        json.Add("id", JToken.FromObject(this.Id))
        json.Add("progress", JToken.FromObject(this.Progress))
        json.Add("currentstep", JToken.FromObject(this.CurrentStep))
        json.Add("done", JToken.FromObject(this.IsDone))
        json.Add("cutlist", JToken.FromObject(this.Cutlist))

        json