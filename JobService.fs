namespace OtrWeb

open OtrWeb.Options
open System
open System.Threading.Tasks
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type JobService() = 

    let mutable currentJob : DecodeJob option = None

    member this.CurrentJob
        with get () = currentJob

    member this.ProcessEpisode (episode : JObject) (options : OtrOptions) =
        match currentJob with
        |Some job when not job.IsDone -> ()
        |_ -> let tmpJob = DecodeJob()
              currentJob <- Some tmpJob
              (tmpJob.RunEpisode episode options) |> Async.Start

    member this.ProcessMovie (movie : JObject) (options : OtrOptions) =
        match currentJob with
        |Some job when not job.IsDone -> ()
        |_ -> let tmpJob = DecodeJob()
              currentJob <- Some tmpJob
              (tmpJob.RunMovie movie options) |> Async.Start