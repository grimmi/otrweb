namespace OtrWeb

open OtrWeb.Options
open Microsoft.Extensions.Options

type MovieDbApi(options : IOptions<MovieDbOptions>)=

    member this.Options = options.Value