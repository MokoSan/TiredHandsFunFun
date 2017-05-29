module AsyncTimer

    // https://stackoverflow.com/questions/29114973/can-i-use-system-timers-timer-in-an-f-pcl-library
    type AsyncTimer(intervalInMilliseconds, callback) = 
        let mb = new MailboxProcessor<bool>(fun inbox ->
                async { 
                    let stop = ref false
                    while not !stop do

                        // Sleep for our interval time
                        do! Async.Sleep intervalInMilliseconds

                        // Timers raise on threadpool threads - mimic that behavior here
                        do! Async.SwitchToThreadPool()
                        callback()

                        // Check for our stop message
                        let! msg = inbox.TryReceive(1)
                        stop := defaultArg msg false
                })

        member __.Start() = mb.Start()
        member __.Stop()  = mb.Post true