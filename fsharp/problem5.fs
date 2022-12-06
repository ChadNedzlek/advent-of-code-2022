module AocF.problem5

    // For more information see https://aka.ms/fsharp-console-apps

    open System.Text.RegularExpressions
    
    let execute(lines) =
        let emptyList _ = []
        
        let rec parseAbsurdInput (data:list<string>) (stacks:list<list<char>>) =
            match data with
            | _ :: "" :: rest -> (stacks, rest)
            | line :: rest ->
                let (next, instructions) = parseAbsurdInput rest stacks
                let layer =
                    seq{1..4..(String.length line)}
                    |> Seq.map (fun i -> line[i])
                let targetSize = (Seq.length layer) - (List.length next)
                let s =
                    (next @ (List.init targetSize emptyList))
                    |> List.mapi (
                        fun i x ->
                            if line.Length > i*4 then
                                let c = line[i*4+1]
                                if c = ' ' then x else x @ [c]
                            else
                                x 
                        )
                (s, instructions)
        
        let (data, instructions) = parseAbsurdInput (List.ofSeq lines) []
            

        let repl (index:int) (value:'a) (data:list<'a>) =
            data |> List.mapi (fun i x -> if i = index then value else x)

        let move (count:int) (f:list<'a>) (t:list<'a>) =
            (f[..^count], t @ f[^(count-1)..])

        let parseLine (ins:string) =
            let m = Regex.Match(ins, @"move (\d+) from (\d+) to (\d+)")
            ((int m.Groups[1].Value), (int m.Groups[2].Value) - 1, (int m.Groups[3].Value) - 1)
            
        let parseInstruction (go: int -> int -> int -> 'a) (ins:string) =
            (parseLine ins) |||> go

        let revMove (count:int) (f:list<'a>) (t:list<'a>) =
            (f[..^count], t @ (f[^(count-1)..] |> List.rev))
            
        let step go (count:int) (f:int) (t:int) (data:list<list<'a>>) =
            let res = go count data[f] data[t]
            data
            |> repl f (fst res)
            |> repl t (snd res)

        let flip x a b = x b a

        let executed = instructions |> List.fold (flip (parseInstruction (step revMove))) data
           
        printfn "%A" (executed |> List.map List.last |> List.map string |> String.concat "")

        let executed2 = instructions |> List.fold (flip (parseInstruction (step move))) data
            
        printfn "%A" (executed2 |> List.map List.last |> List.map string |> String.concat "")
