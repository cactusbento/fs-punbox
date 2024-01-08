open Gtk
open FsHttp
open System.Text.Json

let envVars =
    System.Environment.GetEnvironmentVariables()
    |> Seq.cast<System.Collections.DictionaryEntry>
    |> Seq.map (fun d -> d.Key :?> string, d.Value :?> string)
    |> dict

let user = envVars["USER"]
printfn $"Hello, {user}!"

type Pun = {
    id: int
    pun: string
}

[<EntryPoint>]
let main argv =
    Application.Init()
    use main_window = new Window($"Hello, {user}")
    main_window.SetDefaultSize(200, 200)
    main_window.DeleteEvent.Add(fun _ -> Application.Quit())

    use flex = new Gtk.Paned(Gtk.Orientation.Vertical)
    flex.Expand <- true

    use text = new Gtk.TextView()
    text.HeightRequest <- 150
    text.Editable <- false
    text.Monospace <- true
    text.Buffer.Text <- "Click GET to get a pun!"

    use getBtn = new Button("GET")
    getBtn.Clicked.Add(fun _ -> 
        async {
            printfn "Sending GET Request"

            let! res = 
                http {
                    GET "https://punapi.rest/api/pun"
                } |> Request.sendAsync
            printfn "Recieved Response"
            let! content = res |> Response.toStreamAsync
            let pun = JsonSerializer.Deserialize<Pun>(content)
            printfn $"{pun.pun}"
            text.Buffer.Text <- pun.pun
        } |> Async.Start
    )


    flex.Add(text)
    flex.Add(getBtn)

    main_window.Add(flex)

    main_window.ShowAll()
    Application.Run()
    0

