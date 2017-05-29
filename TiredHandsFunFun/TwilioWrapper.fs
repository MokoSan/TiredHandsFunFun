module TwilioWrapper

    open Twilio
    open Twilio.Rest.Api.V2010.Account
    open Twilio.Types;

    open Logger

    [<Literal>]
    let myPhoneNumber = "Blahh blahh"
    [<Literal>]
    let accountSid = "accountSID"
    [<Literal>]
    let authToken = "HELLO"
    [<Literal>]
    let sendingPhoneNumber = "Sending Phone Number"

    let stringifyDifferenceSetWithDetails ( difference : Set<string> ) : string =
        let mutable stringifiedDifferenceSet = "New beers available! Including: " 

        difference
        |> Set.toList
        |> List.iter( fun x -> stringifiedDifferenceSet <- stringifiedDifferenceSet )
        stringifiedDifferenceSet
    
    let sendTextBasedOnBeerDifference ( difference : Set<string> ) : unit = 
        TwilioClient.Init( accountSid, authToken )
        let toPhoneNumber   = PhoneNumber myPhoneNumber
        let sendPhoneNumber = PhoneNumber sendingPhoneNumber
        let message = MessageResource.Create( toPhoneNumber, null, sendPhoneNumber, null, stringifyDifferenceSetWithDetails( difference ))
        logInfoEvent ( sprintf "Sent message via the Twilio API %s" message.Body )