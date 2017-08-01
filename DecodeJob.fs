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

    member val CurrentStep = "" with get, set

    member val Cutlist : bool = false with get, set

    member val IsDone = false with get, set

    member this.RunEpisode (episode:JObject) (options:OtrOptions) = async {
        let filename = Path.GetFileName(episode.Value<string>("file"))
        let show = episode.Value<string>("show")
        let targetname = makeFileName show (episode.Value<string>("name")) (episode.Value<int>("season")) (episode.Value<int>("number"))
        let subFolder = show + "/" + options.EpisodeFolder
        this.Run filename targetname subFolder options |> Async.Start
    }

    member this.RunMovie (movie:JObject) (options:OtrOptions) = async {        
        let decoder = OtrFileDecoder()
        let file = Path.GetFileName(movie.Value<string>("file"))
        let name = movie.Value<string>("name") |> makeMovieFileName

        this.Run file name options.MovieFolder options |> Async.Start
    }

    member this.Run filename targetName targetSubFolder (options:OtrOptions) = async {
        let decoder = OtrFileDecoder()
        this.CurrentStep <- filename
        let source = Path.Combine(options.KeyFilePath, filename)
        let decodeResult = decoder.DecodeFile source options
        let targetPath = Path.Combine(options.ProcessTargetPath, targetSubFolder, targetName + ".avi")
        let targetDir = Path.GetDirectoryName targetPath
        if not(Directory.Exists targetDir) then
            Directory.CreateDirectory targetDir |> ignore
        let processedPath = Path.Combine(options.KeyFilePath, "processed", filename)
        if File.Exists processedPath then
            File.Delete processedPath
        File.Move(source, processedPath)
        this.IsDone <- true
    }

    member this.ToJson() =
        let json = JObject()
        json.Add("id", JToken.FromObject(this.Id))
        json.Add("currentstep", JToken.FromObject(this.CurrentStep))
        json.Add("done", JToken.FromObject(this.IsDone))
        json.Add("cutlist", JToken.FromObject(this.Cutlist))

        json