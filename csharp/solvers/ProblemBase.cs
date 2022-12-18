using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class AsyncProblemBase : IProblemBase
    {
        public Task ExecuteAsync(string type = "real")
        {
            var m = Regex.Match(GetType().Name, @"Problem(\d+)$");
            var id = int.Parse(m.Groups[1].Value);
            var data = Data.GetDataAsync(id, type);
            if (this is IFancyAsyncProblem fancy)
                return fancy.ExecuteFancyAsync(data);
            return ExecuteCoreAsync(data);
        }

        protected abstract Task ExecuteCoreAsync(IAsyncEnumerable<string> data);
    }

    public interface IProblemBase
    {
        Task ExecuteAsync(string type = "real");
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class SyncProblemBase : IProblemBase
    {
        public async Task ExecuteAsync(string type = "real")
        {
            var m = Regex.Match(GetType().Name, @"Problem(\d+)$");
            var id = int.Parse(m.Groups[1].Value);
            var data = await Data.GetDataAsync(id, type).ToListAsync();
            if (this is IFancyProblem fancy)
                fancy.ExecuteFancy(data);
            else
                ExecuteCore(data);
        }

        protected abstract void ExecuteCore(IEnumerable<string> data);
    }

    public interface IFancyAsyncProblem
    {
        Task ExecuteFancyAsync(IAsyncEnumerable<string> data);
    }
    
    public interface IFancyProblem
    {
        void ExecuteFancy(IEnumerable<string> data);
    }
}