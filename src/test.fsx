#r "../packages/FsCheck/lib/net45/FsCheck.dll"

#load "capserver.fsx"

open System
open System.Text
open FsCheck
open Suave
open Suave.Http

let makeReq method urlPath data =
  let b = HttpBinding.createSimple Protocol.HTTP "127.0.0.1" 8080

  { HttpRequest.empty with
        rawMethod = method.ToString ()
        binding = b
        rawPath = urlPath
        //url = new Uri(sprintf "http://localhost/%s" urlPath)
        rawForm = data
    }
  (*
  { HttpRequest.empty with
      method = method
      rawPath = urlPath
      rawHost = "http://localhost/"
      //url = new Uri(sprintf "http://localhost/%s" urlPath)
      rawForm = data
  }
  *)

let makeCreate data =
  makeReq Http.PUT "api/create" data

let makeRead guid =
  makeReq Http.GET (sprintf "api/read/%s" (guid.ToString())) Array.empty

let makeDelegate rud guid =
  makeReq Http.GET
    (sprintf "api/delegate/%s/%s" rud (guid.ToString())) Array.empty

let makeUpdate data guid =
  makeReq Http.POST (sprintf "api/update/%s" (guid.ToString())) data

let makeDelete guid =
  makeReq Http.DELETE (sprintf "api/delete/%s" (guid.ToString())) Array.empty

let sendReq req =
  let c = Capserver.app { HttpContext.empty with request = req }
  printfn "c: %A" c

  let a = c |> Async.RunSynchronously
  printfn "a: %A" a

  let o = a |> Option.get
  printfn "o: %A" o

  o
 // |> Async.RunSynchronously
 // |> Option.get

let getResponse req =
  let context = sendReq req
  match context.response.content with
  | Bytes x -> x
  | _ -> Array.empty

let getGuid req =
  getResponse req
  |> Encoding.UTF8.GetString
  |> Guid.Parse

let getOk req =
  let context = sendReq req
  context.response.status = {code=200;reason="OK"}

let getCode req =
  printfn "request: %A" req
  let context = sendReq req
  printfn "ctx: %A" context

  printfn "rsp: %A" (context.response.status)
  context.response.status

type Properties =
  static member ``Let's make a bunch of fake docs and leave them on the server`` data =
    let mk = makeCreate data
    printfn "mk data: %A" mk
    
    let c = mk |> getCode
    printfn "code: %A" c
    
    true
    //(makeCreate data |> getCode) = {code=201;reason="Created"}

(*
  static member ``Can create and then read`` data =
    
    printfn "***Data: %A" data
    
    let r = makeCreate data
    printfn "requ: %A" r

    let g = r |> getGuid
    printfn "guid: %A" g

    let m = g |> makeRead
    printfn "read : %A" m

    let res = m |> getResponse
    printfn "resp: %A" res

    res = data


    let dataResponse =
      makeCreate data
      |> getGuid
      |> makeRead
      |> getResponse
    dataResponse = data

  static member ``Can create and then delete`` data =
    let status =
      makeCreate data
      |> getGuid
      |> makeDelete
      |> getCode
    status = {code=204;reason="No Content"}

  static member ``Can create and then update and then delete`` data1 data2 =
    let guid = makeCreate data1 |> getGuid
    let status1 = makeUpdate data2 guid |> getCode
    let dataResponse = makeRead guid |> getResponse
    let status2 = (makeDelete guid |> getCode)
    dataResponse = data2
    && status1 = {code=204;reason="No Content"}
    && status2 = {code=204;reason="No Content"}

  static member ``Can't read after delete`` data =
    let guid = makeCreate data |> getGuid
    (makeDelete guid |> getCode) = {code=204;reason="No Content"} &&
    (makeRead guid |> getCode) = {code=400;reason="Bad Request"}

  static member ``An invented GUID won't have a doc`` (guid: Guid) =
    (makeRead guid |> getCode) = {code=400;reason="Bad Request"}

  static member ``Can create and then delegate and then read`` data =
    let guid1 = makeCreate data |> getGuid
    let guid2 = makeDelegate "r" guid1 |> getGuid
    let dataResponse = makeRead guid2 |> getResponse
    let status1 = (makeDelete guid1 |> getCode)
    let status2 = (makeDelete guid2 |> getCode)
    dataResponse = data
    && status1 = {code=204;reason="No Content"}
    && status2 = {code=204;reason="No Content"}
*)

Check.All (Config.QuickThrowOnFailure, typeof<Properties>)
