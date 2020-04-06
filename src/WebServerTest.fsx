#r "../packages/suave/lib/net461/Suave.dll"
#r "../packages/suave.testing/lib/net40/Suave.Testing.dll"
#r "../packages/expecto/lib/net461/Expecto.dll"
#r "../packages/System.Net.Http/lib/net46/System.Net.Http.dll"


#load "capserver.fsx"


open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave
open Expecto
open Suave.Testing
open Suave.Web
open System.Net.Http
open Suave.Testing
open System.Text.RegularExpressions

(*
let serverConfig =
  { defaultConfig with
      bindings = [ HttpBinding.createSimple Protocol.HTTP "127.0.0.1" 8083 ] }
*)

let runWithConfig = runWith (defaultConfig)

let webPart = Capserver.app


let homePageTests = 
  
    let reponse =
        runWithConfig webPart
        |> req HttpMethod.GET "/" None
    
    testList "Home Pages Test Lists" [
        
        testCase "is a html document" <| fun _ ->
            Expect.stringStarts reponse 
                "<html>" 
                "Should match title"

        testCase "test the title element of the home page" <| fun _ ->
            Expect.stringContains reponse
                "<title>Capability-based Data Store (external home html)</title>"  
                "Should match title"
    ]            


let postDataTests = 

    let reponse = 
        runWithConfig webPart
        |> req HttpMethod.PUT "/api/create" (Some (new ByteArrayContent("OK"B)))

    let apiRead = "/api/read/" + reponse

    let readReponse = 
        runWithConfig webPart
        |> req HttpMethod.GET apiRead None

    testList "Put Request TestsList" [

        testCase "not null" <| fun _ -> Expect.isNotNull reponse "isNotNull" 
        testCase "not empty" <| fun _ -> Expect.isNotEmpty reponse "isNotEmpty" 
        
        testCase "post a doc return an id" <| fun _ ->
            Expect.isRegexMatch reponse (Regex ("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$")) "ok"

        testCase "dataExists and is retrievable" <| fun _ ->
            Expect.equal readReponse "OK" "read reponse ok"
            
    ]
 

//runTests Impl.ExpectoConfig.defaultConfig homePageTests
runTests Impl.ExpectoConfig.defaultConfig postDataTests

