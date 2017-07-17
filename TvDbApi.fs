namespace OtrWeb

open OtrWeb.Options
open Microsoft.Extensions.Options

type TvDbApi(options: IOptions<TvDbOptions>) =

    member this.Options = options.Value