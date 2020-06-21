using Bitmex.NET.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Bitmex.NET.Logging;
using System.Windows.Threading;
using System.Threading;

namespace Bitmex.NET
{
    public class BitmexApiService : IBitmexApiService
    {
        private readonly IBitmexApiProxy _bitmexApiProxy;
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        private readonly object _syncObjMain;

        public BitmexApiService(IBitmexApiProxy bitmexApiProxy)
        {
            _bitmexApiProxy = bitmexApiProxy;
        }

        protected BitmexApiService(IBitmexAuthorization bitmexAuthorization, object syncObjMain)
        {
            _bitmexApiProxy = new BitmexApiProxy(bitmexAuthorization);
            _syncObjMain = syncObjMain;
        }

        public void SetOwnerAndDelegate(Dispatcher target, Delegate targetFunc)
        {
            _bitmexApiProxy.Owner = target;
            _bitmexApiProxy.TargetFunc = targetFunc;
        }

        public async Task<TResult> Execute<TParams, TResult>(ApiActionAttributes<TParams, TResult> apiAction, TParams @params)
        {
            switch (apiAction.Method)
            {
                case HttpMethods.GET:
                    var getQueryParams = @params as IQueryStringParams;
                    return JsonConvert.DeserializeObject<TResult>(
                        await _bitmexApiProxy.Get(apiAction.Action, getQueryParams));
                case HttpMethods.POST:
                    var postQueryParams = @params as IJsonQueryParams;
                    return JsonConvert.DeserializeObject<TResult>(
                        await _bitmexApiProxy.Post(apiAction.Action, postQueryParams));
                case HttpMethods.PUT:
                    var putQueryParams = @params as IJsonQueryParams;
                    return JsonConvert.DeserializeObject<TResult>(
                        await _bitmexApiProxy.Put(apiAction.Action, putQueryParams));
                case HttpMethods.DELETE:
                    var deleteQueryParams = @params as IQueryStringParams;
                    return JsonConvert.DeserializeObject<TResult>(
                        await _bitmexApiProxy.Delete(apiAction.Action, deleteQueryParams));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public TResult ExecuteSync<TParams, TResult>(ApiActionAttributes<TParams, TResult> apiAction, TParams @params)
        {
            switch (apiAction.Method)
            {
                case HttpMethods.GET:
                    var getQueryParams = @params as IQueryStringParams;
                    return JsonConvert.DeserializeObject<TResult>(
                        _bitmexApiProxy.GetSync(apiAction.Action, getQueryParams));
                case HttpMethods.POST:
                    var postQueryParams = @params as IJsonQueryParams;
                    return JsonConvert.DeserializeObject<TResult>(
                        _bitmexApiProxy.PostSync(apiAction.Action, postQueryParams));
                case HttpMethods.PUT:
                    var putQueryParams = @params as IJsonQueryParams;
                    return JsonConvert.DeserializeObject<TResult>(
                        _bitmexApiProxy.PutSync(apiAction.Action, putQueryParams));
                case HttpMethods.DELETE:
                    var deleteQueryParams = @params as IQueryStringParams;
                    return JsonConvert.DeserializeObject<TResult>(
                        _bitmexApiProxy.DeleteSync(apiAction.Action, deleteQueryParams));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public TResult ExecuteSyncErrorHandler<TParams, TResult>(ApiActionAttributes<TParams, TResult> apiAction, TParams @params)
        {
                switch (apiAction.Method)
                {
                    case HttpMethods.GET:

                        TResult GetResponseResult = default(TResult);
                        lock (_syncObjMain)
                        {
                            try
                            {
                                var getQueryParams = @params as IQueryStringParams;
                                string getResponse = _bitmexApiProxy.GetSync(apiAction.Action, getQueryParams);
                                if (getResponse != "ErrorOnSendAndGetResponseSync")
                                    GetResponseResult = JsonConvert.DeserializeObject<TResult>(getResponse);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.Message + ex.StackTrace);
                            }
                            Thread.Sleep(new TimeSpan(0, 0, 0, 1, 10));
                        }

                        return GetResponseResult;
                    case HttpMethods.POST:

                        TResult PostResponseResult = default(TResult);
                        lock (_syncObjMain)
                        {
                            try
                            {
                                var postQueryParams = @params as IJsonQueryParams;
                                string postResponse = _bitmexApiProxy.PostSync(apiAction.Action, postQueryParams);
                                if (postResponse != "ErrorOnSendAndGetResponseSync")
                                    PostResponseResult = JsonConvert.DeserializeObject<TResult>(postResponse);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.Message + ex.StackTrace);
                            }
                            Thread.Sleep(new TimeSpan(0, 0, 0, 1, 10));
                        }

                        return PostResponseResult;

                    case HttpMethods.PUT:

                        TResult PutResponseResult = default(TResult);
                        lock (_syncObjMain)
                        {
                            try
                            {
                                var putQueryParams = @params as IJsonQueryParams;
                                string putResponse = _bitmexApiProxy.PutSync(apiAction.Action, putQueryParams);
                                if (putResponse != "ErrorOnSendAndGetResponseSync")
                                    PutResponseResult = JsonConvert.DeserializeObject<TResult>(putResponse);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.Message + ex.StackTrace);
                            }
                            Thread.Sleep(new TimeSpan(0, 0, 0, 1, 10));
                        }

                        return PutResponseResult;

                case HttpMethods.DELETE:
                        TResult DeleteResponseResult = default(TResult);
                        lock (_syncObjMain)
                        {
                            try
                            {
                                var deleteQueryParams = @params as IQueryStringParams;
                                string deleteResponse = _bitmexApiProxy.DeleteSync(apiAction.Action, deleteQueryParams);
                                if (deleteResponse != "ErrorOnSendAndGetResponseSync")
                                    DeleteResponseResult = JsonConvert.DeserializeObject<TResult>(deleteResponse);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.Message + ex.StackTrace);
                            }
                            Thread.Sleep(new TimeSpan(0, 0, 0, 1, 10));
                        }

                        return DeleteResponseResult;
                default:
                        throw new ArgumentOutOfRangeException();
               
                }
        }

        public TResult ExecuteSyncErrorHandlerNew<TParams, TResult>(out bool Executed, ApiActionAttributes<TParams, TResult> apiAction, TParams @params)
        {
            switch (apiAction.Method)
            {
                case HttpMethods.GET:

                    Executed = false;
                    TResult GetResponseResult = default(TResult);
                    lock (_syncObjMain)
                    {
                        try
                        {
                            var getQueryParams = @params as IQueryStringParams;
                            string getResponse = _bitmexApiProxy.GetSync(apiAction.Action, getQueryParams);
                            if (getResponse != "ErrorOnSendAndGetResponseSync")
                            {                                                                
                                GetResponseResult = JsonConvert.DeserializeObject<TResult>(getResponse);
                                Executed = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message + ex.StackTrace);                            
                        }
                        Thread.Sleep(new TimeSpan(0, 0, 0, 1, 10));
                    }

                    return GetResponseResult;
                case HttpMethods.POST:

                    Executed = false;
                    TResult PostResponseResult = default(TResult);
                    lock (_syncObjMain)
                    {
                        try
                        {
                            var postQueryParams = @params as IJsonQueryParams;
                            string postResponse = _bitmexApiProxy.PostSync(apiAction.Action, postQueryParams);
                            if (postResponse != "ErrorOnSendAndGetResponseSync")
                            {
                                PostResponseResult = JsonConvert.DeserializeObject<TResult>(postResponse);
                                Executed = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message + ex.StackTrace);
                        }
                        Thread.Sleep(new TimeSpan(0, 0, 0, 1, 10));
                    }

                    return PostResponseResult;

                case HttpMethods.PUT:

                    Executed = false;
                    TResult PutResponseResult = default(TResult);
                    lock (_syncObjMain)
                    {
                        try
                        {
                            var putQueryParams = @params as IJsonQueryParams;
                            string putResponse = _bitmexApiProxy.PutSync(apiAction.Action, putQueryParams);
                            if (putResponse != "ErrorOnSendAndGetResponseSync")
                            {
                                PutResponseResult = JsonConvert.DeserializeObject<TResult>(putResponse);
                                Executed = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message + ex.StackTrace);
                        }
                        Thread.Sleep(new TimeSpan(0, 0, 0, 1, 10));
                    }

                    return PutResponseResult;

                case HttpMethods.DELETE:
                    Executed = false;
                    TResult DeleteResponseResult = default(TResult);
                    lock (_syncObjMain)
                    {
                        try
                        {
                            var deleteQueryParams = @params as IQueryStringParams;
                            string deleteResponse = _bitmexApiProxy.DeleteSync(apiAction.Action, deleteQueryParams);
                            if (deleteResponse != "ErrorOnSendAndGetResponseSync")
                            {
                                DeleteResponseResult = JsonConvert.DeserializeObject<TResult>(deleteResponse);
                                Executed = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message + ex.StackTrace);
                        }
                        Thread.Sleep(new TimeSpan(0, 0, 0, 1, 10));
                    }

                    return DeleteResponseResult;
                default:
                    throw new ArgumentOutOfRangeException();

            }
        }

        public static IBitmexApiService CreateDefaultApi(IBitmexAuthorization bitmexAuthorization, object syncObjMain)
        {
            return new BitmexApiService(bitmexAuthorization, syncObjMain);
        }
    }
}
