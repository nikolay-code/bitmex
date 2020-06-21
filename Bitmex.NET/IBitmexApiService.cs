using System.Threading.Tasks;
using System;
using System.Windows.Threading;

namespace Bitmex.NET
{
	public interface IBitmexApiService
	{
		Task<TResult> Execute<TParams, TResult>(ApiActionAttributes<TParams, TResult> apiAction, TParams @params);
        TResult ExecuteSync<TParams, TResult>(ApiActionAttributes<TParams, TResult> apiAction, TParams @params);
        TResult ExecuteSyncErrorHandler<TParams, TResult>(ApiActionAttributes<TParams, TResult> apiAction, TParams @params);
        TResult ExecuteSyncErrorHandlerNew<TParams, TResult>(out bool Executed, ApiActionAttributes<TParams, TResult> apiAction, TParams @params);
        void SetOwnerAndDelegate(Dispatcher target, Delegate targetFunc);
    }
}
