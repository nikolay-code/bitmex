using Bitmex.NET.Models;
using System.Threading.Tasks;
using System.Windows.Threading;
using System;

namespace Bitmex.NET
{
    public interface IBitmexApiProxy
    {
        Task<string> Get(string action, IQueryStringParams parameters);
        Task<string> Post(string action, IJsonQueryParams parameters);
        Task<string> Put(string action, IJsonQueryParams parameters);
        Task<string> Delete(string action, IQueryStringParams parameters);
        string GetSync(string action, IQueryStringParams parameters);
        string PostSync(string action, IJsonQueryParams parameters);
        string PutSync(string action, IJsonQueryParams parameters);
        string DeleteSync(string action, IQueryStringParams parameters);
        Dispatcher Owner { get; set; }
        Delegate TargetFunc { get; set; }
    }
}
