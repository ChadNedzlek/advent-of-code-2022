module AocF.problem5

    // For more information see https://aka.ms/fsharp-console-apps

    open System.Text.RegularExpressions
    
    let execute(lines) =
        let rightPad (size:int) (v:'a) (s:seq<'a>) =
            Seq.append s (Seq.replicate (size - (Seq.length s)) v)

        // Flop the order of the first two arguments to a function,
        // So if the input its "func arg1 arg2 ...", the returned function is
        // "func arg2 arg1"
        let flip x a b = x b a
        
        let rec parseAbsurdInput (data:list<string>) (stacks:seq<list<char>>) =
            match data with
            // This is the blank line, the current line is the pointless index line, then it's blank, then it's other stuff
            // So create the empty stacks and pass them back to our recursive parent so they can plop stuff on
            | line :: "" :: tail ->
                // These are the silly indices
                let indices = seq{1..4..(String.length line)}
                // Create an empty list for every index, include the instructions, and recurse to build up the stacks
                (Seq.replicate (Seq.length indices) [], tail)
            // We haven't found the blank line yet (because that would have matched above)
            // So we need to build the stacks
            | line :: tail ->
                // Build the stacks bottoms first with the tail, so we can add to the top
                let bottom, instructions = parseAbsurdInput tail stacks
                let toAdd =
                    // The interesting letter indices
                    seq{1..4..(String.length line)}
                    |> Seq.map (flip Seq.item line)
                    |> rightPad (Seq.length bottom) ' '
                let completed =
                    (bottom, toAdd)
                    ||> Seq.map2 (fun s a -> if a = ' ' then s else s @ [a])
                (completed, instructions)
        
        let dataSeq, instructions = parseAbsurdInput (List.ofSeq lines) []
        
        let data = List.ofSeq dataSeq

        // Replace the item at the given index in the input list
        let repl (index:int) (value:'a) (data:list<'a>) =
            data |> List.mapi (fun i x -> if i = index then value else x)

        // Move "count" items from f to t and return the 
        let move (count:int) (f:list<'a>, t:list<'a>) =
            (f[..^count], t @ f[^(count-1)..])

        let revMove (count:int) (f:list<'a>, t:list<'a>) =
            (f[..^count], t @ (f[^(count-1)..] |> List.rev))

        // Parse the instruction and return a tuple of (count, fromIndex, toIndex)
        let parseLine (ins:string) =
            let m = Regex.Match(ins, @"move (\d+) from (\d+) to (\d+)")
            ((int m.Groups[1].Value), (int m.Groups[2].Value) - 1, (int m.Groups[3].Value) - 1)
            
        // Build the transformation of applying the "go" method for the instruction
        let parseInstruction (handleInstruction: int -> int -> int -> 'a) (ins:string) =
            (parseLine ins) |||> handleInstruction
            
        // Apply a single step of the "go" method to the stacks
        let step handleInstruction (count:int) (iFrom:int) (iTo:int) (stacks:list<list<'a>>) =
            let (newFrom, newTo) = handleInstruction count (stacks[iFrom], stacks[iTo])
            stacks |> repl iFrom newFrom |> repl iTo newTo
            
        let formatOutput stacks = stacks |> List.map List.last |> List.map string |> String.concat ""

        let part1Handler = (flip (parseInstruction (step revMove)))
        let endState1 = instructions |> List.fold part1Handler data        
        printfn $"%A{endState1 |> formatOutput}"

        let part1Handler = (flip (parseInstruction (step move)))
        let endState2 = instructions |> List.fold part1Handler data            
        printfn $"%A{endState2 |> formatOutput}"
