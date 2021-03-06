namespace OtrWeb.Options

type OtrOptions() =
    member val Email = "" with get, set
    member val Password = "" with get, set
    member val DecoderPath = "" with get, set
    member val KeyFilePath = "" with get, set
    member val DecodeTargetPath = "" with get, set
    member val ProcessTargetPath = "" with get, set
    member val EpisodeFolder = "" with get, set
    member val MovieFolder = "" with get, set
    member val CreateDirectories = true with get, set
    member val AutoCut = true with get, set
    member val ContinueWithoutCutlist = true with get, set