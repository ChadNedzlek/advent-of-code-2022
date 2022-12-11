using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class ProblemBase
    {
        public Task ExecuteAsync(string type = "real")
        {
            var m = Regex.Match(GetType().Name, @"Problem(\d+)$");
            var id = int.Parse(m.Groups[1].Value);
            var data = Data.GetData(id, type);
            if (this is IFancyProblem fancy)
                return fancy.ExecuteFancyAsync(data);
            return ExecuteCoreAsync(data);
        }

        protected abstract Task ExecuteCoreAsync(IAsyncEnumerable<string> data);
    }

    public interface IFancyProblem
    {
        Task ExecuteFancyAsync(IAsyncEnumerable<string> data);
    }
}