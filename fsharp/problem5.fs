module AocF.problem5

    // For more information see https://aka.ms/fsharp-console-apps

    open System.Text.RegularExpressions
    
    let execute(data) =
        let data = [
            ['Z';'N'];
            ['M';'C';'D'];
            ['P'];
        ]

        let instructions = [
            "move 1 from 2 to 1"
            "move 3 from 1 to 3"
            "move 2 from 2 to 1"
            "move 1 from 1 to 2"
        ]

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
