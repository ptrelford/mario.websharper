namespace Samples

open System
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.JQuery

[<JavaScript>]
module Keyboard =
    let mutable keys = Set.empty  
    let init () =        
        JQuery.Of("html")
            .Keydown(fun _ event -> keys <- Set.add event.Which keys)
            .Keyup(fun _ event -> keys <- Set.remove event.Which keys)
        |> ignore
    let code x = if keys.Contains(x) then 1 else 0
    let arrows () = (code 39 - code 37, code 38 - code 40)

[<JavaScript>]
module Image = 
    let src (image:Element) value = 
        if   image.GetAttribute("src") <> value 
        then image.SetAttribute("src", value)

    let position (parent:Element) (element:Element) (x,y) =
        let origin = JQuery.Of(parent.Body).Position()
        "position:absolute;" +
        "left:" + (x + float origin.Left).ToString() + "px;" +
        "top:" + (y + float origin.Top).ToString() + "px;" 
        |> element.SetStyle 
        
[<JavaScript>]
module Mario =    

    type mario = { x:float; y:float; vx:float; vy:float; dir:string }

    let jump (_,y) m = if y > 0 && m.y = 0. then  { m with vy = 5. } else m
    let gravity m = if m.y > 0. then { m with vy = m.vy - 0.1 } else m
    let physics m = { m with x = m.x + m.vx; y = max 0. (m.y + m.vy) }
    let walk (x,_) m = 
        { m with vx = float x 
                 dir = if x < 0 then "left" elif x > 0 then "right" else m.dir }

    let step dir mario = mario |> physics |> walk dir |> gravity |> jump dir
     
    let placeMario parent (img:Element) (w,h) (mario:mario) =
        let verb =
            if mario.y > 0. then "jump"
            elif mario.vx <> 0. then "walk"
            else "stand"
        "mario" + verb + mario.dir + ".gif" |> Image.src img 
        (w/2.-16.+mario.x,  h-50.-31.-mario.y) |> Image.position parent img

    let background (ctx:CanvasRenderingContext2D) (w,h) =
        ctx.FillStyle <- "rgb(174,238,238)"
        ctx.FillRect (0., 0., w, h)
        ctx.FillStyle <- "rgb(74,163,41)"
        ctx.FillRect(0., h-50., w, 50.) 

    let Main () =           
        let element = HTML5.Tags.Canvas []
        let canvas  = As<CanvasElement> element.Dom     
        let width, height = 512, 384
        canvas.Width  <- width
        canvas.Height <- height
        let w,h = float width, float height
        background (canvas.GetContext "2d") (w,h)
        let mario = ref { x=0.; y=0.; vx=0.;vy=0.;dir="right" }
        let img = Img [Attr.Style "position:absolute"]
        Keyboard.init ()
        let update () =
            mario := !mario |> step (Keyboard.arrows())
            placeMario element img (w,h) !mario        
        JavaScript.SetInterval update (1000/60) |> ignore        
        Div [ 
            H1 [Text "Mario WebSharper"] 
            element 
            img 
        ]

type CanvasViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = Mario.Main () :> _