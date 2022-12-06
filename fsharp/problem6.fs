module ChadNedzlek.AdventOfCode.Y2022.FSharp.problem6
    
    let execute(lines) =
        let go (length:int) (line:string) =
            (
             Seq.initInfinite id
                |> Seq.map (fun i -> line[i..(i+length-1)])
                |> Seq.map Seq.distinct
                |> Seq.map Seq.length
                |> Seq.findIndex (fun l -> l = length)
            ) + length // The packet comes after the header
        
        printfn "%A" (lines |> Seq.map (go 4))
        printfn "%A" (lines |> Seq.map (go 14))
