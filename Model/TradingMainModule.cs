using System;
using System.Collections.Generic;
using System.Linq;
using Bitmex.NET;
using Bitmex.NET.Dtos;
using BitMexLibrary.Automapping;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BitMexLibrary
{
    public class TradingMainModule
    {
        private Background_worker _dataReader;
        public Background_worker DataReader { get => _dataReader; set => _dataReader = value; }
        public List<TradingRobot> robots = new List<TradingRobot>();
        public delegate void ResultHandlerDelegate(ObservableCollection<TradeBucketedDto> src, List<CalculationResult> result);
        public delegate void OrderBook10Delegate(List<OrderBookModel> src);

        //public DateTime LastUpdate;
        private Dispatcher parentThread;
        public Dispatcher ParentThread { get => parentThread; set => parentThread = value; }
        public Delegate TargetFunc;
        public Delegate TargetFuncUpdateQuotes;
        private List<CalculationResult> LastResult = new List<CalculationResult>();

        private string _connectionString;
        public string ConnectionString { get => _connectionString; set => _connectionString = value; }
        // private readonly object _syncObjMain = new object();

        public TradingMainModule(string conn)
        {
            AutoMapperConfiguration.Configure();
            ConnectionString = conn;
        }

        ~TradingMainModule()
        {
            foreach (var robot in robots)
            {
                robot.TerminateRobot();
            }
            if (DataReader != null) DataReader.StopDataReader();
        }

        public void SetMainTargetAndDelegates(Dispatcher target, Delegate targetFunc, Delegate targetFuncUpdateQuotes)
        {
            parentThread = target;
            TargetFunc = targetFunc;
            TargetFuncUpdateQuotes = targetFuncUpdateQuotes;
        }

        public void CreateMainAccount(BitmexAuthorization MainAccount)
        {//Создание главного аккауна
            robots.RemoveAll(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Main);
            if (robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Main).Count() == 0)
                robots.Add(new TradingRobot(MainAccount));
        }

        public void SetMainAccountDispatcherAndDelegate(Dispatcher target, Delegate targetFunc)
        {
            //foreach(var robot in robots) robot.SetAccountDispatcherAndDelegate(target, targetFunc);
            TradingRobot tr = robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Main).First();
            tr.SetAccountDispatcherAndDelegate(target, targetFunc);
        }

        public void SetMonitorDispatcherAndDelegate(Dispatcher target, Delegate targetFunc)
        {
            foreach(var robot in robots)
                robot.SetAccountDispatcherAndDelegate(target, targetFunc);
        }

        public string AccountsAuthorization()
        {
            TradingRobot tr = robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Main).First();
            return tr.DoAuthorization();
        }

        public void RestartBackgroundWorker(string name, int numForCalc, string binSize)
        {
            if (_dataReader == null)
            {
                ResultHandlerDelegate ResultCallBack = new ResultHandlerDelegate(CallBack_Result);
                OrderBook10Delegate orderBook10CallBack = new OrderBook10Delegate(CallBack_OrderBook10);
                TradingRobot tr = robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Main).First();
                _dataReader = new Background_worker(ConnectionString, tr.RobotBitmexApiService, tr.RobotBitmexApiSocketService, Dispatcher.CurrentDispatcher, ResultCallBack, orderBook10CallBack);
                _dataReader.RestartDataReader("MainWorker", numForCalc, binSize);
            }
            else
            {
                _dataReader.RestartDataReader("MainWorker", numForCalc, binSize);
            }
        }

        public void CallBack_Result(ObservableCollection<TradeBucketedDto> src, List<CalculationResult> resultTable)
        {
            LastResult = new List<CalculationResult>(resultTable.Select(x => x.DeepClone()));

            SendLastResultToAllRobots();

            parentThread.BeginInvoke(TargetFunc, src, resultTable);
        }

        public void SendLastResultToAllRobots()
        {
            foreach (var robot in robots)
            {
                List<CalculationResult> last_val = new List<CalculationResult>(LastResult.Select(x => x.DeepClone()));
                robot.UpdateCandlesAndResultTable(last_val);
            }
        }

        public void CallBack_OrderBook10(List<OrderBookModel> src)
        {
            parentThread.BeginInvoke(TargetFuncUpdateQuotes, src);
        }

        public void ActivateAutotrading(int size, double leverage, bool isOnlyBuy)
        {
            SendLastResultToAllRobots();

            foreach (var robot in robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Main))
            {
                robot.ActivateAutotrading(size, leverage, isOnlyBuy);
            }

            foreach (var robot in robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Additional))
            {
                robot.ActivateAutotrading(isOnlyBuy);
            }
        }

        public void CheckOnlyBuy(bool isOnlyBuy)
        {
            foreach (var robot in robots) { robot.SetOnlyBuy(isOnlyBuy); }
        }

        public void DeactivateAutotrading()
        {
            foreach (var robot in robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Main))
            {
                robot.DeactivateAutotrading();
            }

            foreach (var robot in robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Additional))
            {
                robot.DeactivateAutotrading();
            }
        }

        public void doSell(int Size, double leverage)
        {
            TradingRobot tr = robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Main).First();
            tr.ActivateWork("Sell", Size, leverage);
        }

        public void doBuy(int Size, double leverage)
        {
            TradingRobot tr = robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Main).First();
            tr.ActivateWork("Buy", Size, leverage);
        }

        public void UpdateHistory()
        {
            TradingRobot tr = robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Main).First();
            tr.getAccountHistoryInfo();
        }

        public void AddAdditionalAccounts(List<BitmexAuthorization> accounts)
        {
            if (accounts.Count > 0)
            {
                foreach (var account in accounts)
                {
                    if (account.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Additional)
                    {
                        if (robots.Where(x => x.RobotBitmexAuthorization.Key == account.Key && x.RobotBitmexAuthorization.Secret == account.Secret).Count() == 0)
                            robots.Add(new TradingRobot(account));
                    }
                }
            }
        }

        public void LoginAdditionalAccounts()
        {
            foreach (var robot in robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Additional))
            {
                if (! (robot.IsConnected == "True"))
                {
                    robot.DoAuthorization();
                }
            }
        }

        public void RemoveAdditionalRobot()
        {
            foreach (var robot in robots.Where(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Additional))
            {
                robot.TerminateRobot();
            }

            robots.RemoveAll(x => x.RobotBitmexAuthorization.AccountType == Bitmex.NET.Models.BitmexAccountTypeEnvironment.Additional);
        }
    }
}
