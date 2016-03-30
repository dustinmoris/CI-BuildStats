module Program = 
    open System
    open Microsoft.Owin.Hosting
    open BuildStats.Web.Fsharp

    [<EntryPoint>]
    let Main(args) =
        let exitCode = 0
        let url = "http://localhost:8080"
        use webApp = WebApp.Start<Startup>(url)
        Console.WriteLine(sprintf "Listening to %s" url)
        Console.WriteLine "Press enter to exit"
        Console.ReadLine() |> ignore
        exitCode