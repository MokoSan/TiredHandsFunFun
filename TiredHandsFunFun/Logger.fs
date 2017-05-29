module Logger

    open System
    open System.IO

    open Logary 
    open Logary.Configuration
    open Logary.Targets
    open Logary.Metrics
    open Logary.Targets

    [<Literal>]
    let loggingPathBase = "./"

    let initializeLogger() =
        let textConf = TextWriter.TextWriterConf.create(Path.Combine(loggingPathBase, DateTime.UtcNow.ToString("yyyy-MM") + "-success.log") |> File.AppendText, Path.Combine(loggingPathBase, DateTime.UtcNow.ToString("yyyy-MM") + "-errors.log") |> File.AppendText)
        let newConf = { textConf with flush = true } 
        let logaryTextWriterCreator = Logary.Targets.TextWriter.create newConf "fileLogger"
        let logary = withLogaryManager "TextWriterLogger" (withTargets [ logaryTextWriterCreator ] >> withRules [ Rule.createForTarget "fileLogger" ])
        logary |> Hopac.Hopac.run |> ignore 

    let private logger = Logging.getCurrentLogger()

    let logInfoEvent ( eventDetailsToLog : string ) : unit = 
        Message.event Info eventDetailsToLog |> Logger.logSimple logger