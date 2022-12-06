﻿module AocF.problem6
    
    let execute(lines) =
        let go (length:int) (line:string) =
            (
             line
                |> Seq.mapi (fun i _ -> line[i..(i+length-1)])
                |> Seq.map Seq.distinct
                |> Seq.map Seq.length
                |> Seq.findIndex (fun l -> l = length)
            ) + length // The packet comes after the header
        
        printfn "%A" (lines |> Seq.map (go 4))
        printfn "%A" (lines |> Seq.map (go 14))