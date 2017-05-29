module BeerInfo

    open System 

    open Chiron
    open Chiron.Operators

    type BeerInfo = { TimeOfLookup : DateTime; Beers : string list } with
        static member ToJson( beerInfo : BeerInfo ) =
            Json.write "TimeOfLookup" beerInfo.TimeOfLookup
            *> Json.write "Beers" beerInfo.Beers

        static member FromJson( _ : BeerInfo ) =
            fun timeOfLookup beers -> { TimeOfLookup = timeOfLookup; Beers = beers }
            <!> Json.read "TimeOfLookup" 
            <*> Json.read "Beers"

        static member GetBeerDifferences( beerInfo1 : BeerInfo ) ( beerInfo2 : BeerInfo ) =
            let beerSet1 = Set< string > ( beerInfo1.Beers ) 
            let beerSet2 = Set< string > ( beerInfo2.Beers ) 
            beerSet1 - beerSet2 

    let createBeerInfoFromBeerNames ( beers : string list ) : BeerInfo = 
        { TimeOfLookup = DateTime.Now; Beers = beers } 

    let serializeBeerInfo ( beerInfo : BeerInfo ) : string = 
        beerInfo 
        |> Json.serialize
        |> Json.formatWith JsonFormattingOptions.Pretty
        
    let deserializeBeerInfo ( jsonizedBeerInfo : string ): BeerInfo = 
        jsonizedBeerInfo 
        |> Json.parse
        |> Json.deserialize