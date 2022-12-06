module ChadNedzlek.AdventOfCode.Y2022.FSharp.problem4    
    let tup2 (arr:array<string>) =
        match arr with
        | [|a;b|] -> (a,b)
        
    let tupOfSeq arr =
        match arr |> List.ofSeq with
        | a :: t ->
            match t with
            | b :: t -> (a,b)

    let execute (data:seq<string>) =
        let assignments =
            data
            |> Seq.map (fun x -> x.Split(','))
            |> Seq.map (fun t ->
                t
                |> Seq.map (fun x -> x.Split('-') |> Array.map int)
                |> Seq.map (fun x -> seq{x[0]..x[1]} |> Set.ofSeq)
            )
            |> Seq.map tupOfSeq
            
        let overlaps =
            assignments
            |> Seq.map (fun (a,b) -> ((Set.ofSeq(b), Set.ofSeq(a)) ||> Set.intersect))
        
        let totalOverlap =
            (assignments, overlaps)
            ||> Seq.zip
            |> Seq.filter (fun (a,o) -> (fst a) = o || (snd a) = o)
        
        let partialOverlap = 
            overlaps
            |> Seq.filter (fun o -> not(Set.isEmpty(o)))
            
        printfn $"Total overlap count: %d{totalOverlap |> Seq.length}"
        printfn $"Partial overlap count: %d{partialOverlap |> Seq.length}"
        
        0
