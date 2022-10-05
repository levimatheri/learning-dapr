using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public abstract class Example
    {
        public abstract string DisplayName { get; }
        public abstract Task RunAsync(CancellationToken cancellationToken);
    }
}
