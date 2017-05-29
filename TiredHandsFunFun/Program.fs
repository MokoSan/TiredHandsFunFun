// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open System.IO
open FSharp.Data

open AsyncTimer
open TiredHandsParserManager

(*
    getBeerNamesFromTiredHands
    |> createBeerInfoFromBeerNames 
    |> checkIfBeerNamesChanged [ Parse JSON file -> Create old beer info -> check if things have changed ]
    |> trySendAlert
*)

[<EntryPoint>]
let main argv = 
    let asyncTimer = AsyncTimer(5000, getBeerInfoFromTiredHands)

    printfn "Starting the Timer."
    asyncTimer.Start()
    System.Console.ReadKey() |> ignore

    asyncTimer.Stop()
    printfn "Stopped the Timer."
    System.Console.ReadKey() |> ignore

    0 