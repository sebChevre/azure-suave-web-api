#load "capserver.fsx"

open System
open System.Net
open Suave

let port =
  match
    Environment.GetCommandLineArgs()
    |> Array.tryFind (fun arg ->
      arg.StartsWith("port=")
    ) with
  | Some arg -> arg.Split([|'='|]).[1] |> Sockets.Port.Parse
  | None -> 8083us

let serverConfig =
  { defaultConfig with
      bindings = [ HttpBinding.createSimple Protocol.HTTP "127.0.0.1" (int(port)) ] }

startWebServer serverConfig Capserver.app
