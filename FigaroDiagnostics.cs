using System.Text;
using Nancy.Diagnostics;

namespace Nancy.Demos.Figaro
{
    public class FigaroDiagnostics: IDiagnosticsProvider
    {
        private readonly FigaroDataContext context;
        public FigaroDiagnostics(FigaroDataContext dataContext)
        {
            context = dataContext;
        }

        public string Name => "Figaro Statistics";
        public string Description => "Get Figaro statistics for the different subsystems available.";
        public object DiagnosticObject => this;

        
        [Description("Prints the transaction statistics.")]
        [Template("<h1>Transaction Statistics</h1><pre>{{model.result}}</pre>")]
        public string TransactionStatistics()
        {
            var stats = context.Environment.GetTransactionStatistics(false);
            return stats.ToString();
        }

        [Description("Prints the memory statistics.")]
        [Template("<h1>Memory Statistics</h1><pre>{{model.result}}</pre>")]
        public string MemoryStatistics()
        {
            var stats = context.Environment.GetMemoryPoolStatistics(false);
            return stats.ToString();
        }

        [Description("Prints the mutex statistics.")]
        [Template("<h1>Mutex Statistics</h1><pre>{{model.result}}</pre>")]
        public string MutexStatistics()
        {
            var stats = context.Environment.GetMutexStatistics(false);
            return stats.ToString();
        }

        [Description("Prints the lock statistics.")]
        [Template("<h1>Lock Statistics</h1><pre>{{model.result}}</pre>")]
        public string LockStatistics()
        {
            var stats = context.Environment.GetLockStatistics(false);
            return stats.ToString();
        }

        [Description("Prints the log statistics.")]
        [Template("<h1>Log Statistics</h1><pre>{{model.result}}</pre>")]
        public string LogStatistics()
        {
            var stats = context.Environment.GetLogStatistics(false);
            return stats.ToString();
        }

        [Description("Prints the replication statistics.")]
        [Template("<h1>Replication Statistics</h1><pre>{{model.result}}</pre>")]
        public string RepStatistics()
        {
            var stats = context.Environment.ReplicationManager.GetStatistics(false);
            return stats.ToString();
        }

    }
}
