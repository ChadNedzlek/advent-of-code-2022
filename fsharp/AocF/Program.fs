open System.Reflection
open System.Text.RegularExpressions
    
[<EntryPoint>]
let main args =         
    let allModules =
        Assembly.GetExecutingAssembly().GetTypes()
        |> Seq.filter (fun t -> t.IsAbstract && t.IsSealed && t.IsPublic)
        |> Seq.filter (fun t -> Regex.IsMatch(t.Name, "problem(\d+)"))
        |> Seq.sortBy (fun t -> t.FullName[7..])
        |> Seq.rev
    
    let highestModule = allModules |> Seq.head
    
    let index = int highestModule.Name[7..]
    
    let targetFile = $@"C:\Users\chadnedz\RiderProjects\advent-of-code-2022\aoc\data\data-%02d{index}-real.txt"

    let lines = System.IO.File.ReadLines(targetFile) |> Seq.cache
    
    let targetMethod = highestModule.GetMethod("execute")
    
    let toExecute =
        match targetMethod with
        | method when method.IsGenericMethod -> method.MakeGenericMethod([|lines.GetType()|])
        | x -> x
    
    toExecute.Invoke(null, [|lines|]) |> ignore
    0
    
    