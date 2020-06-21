using Bitmex.NET.Dtos;
using Bitmex.NET.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using BitMexLibrary;
using BitMexLibrary.Automapping;
using System.Threading;
using System.Windows.Controls;
using NLog;
using System.Threading.Tasks;

using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;
using LiveCharts.Defaults;

namespace Bitmex.NET.MainApp
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Main vars
        public Logger logger = LogManager.GetCurrentClassLogger();      
        private double _leverage;
        private string _key;
        private string _secret;
        private bool _isConnected;
        private int _tabSelectedIndex;
        private bool _startAdditionalButtonEnabled = true;
        private bool needCheck = true;                
        private int _delta;
        private bool _isRealAccount;
        private bool _isOnlyBuy;
        private bool _isAllowedToPlace = false;
        private string _currentTime;
        private string _accountType;
        private string _AccountNumber;
        private string _param_Amount;
        private string _param_Margin = "";
        private string _param_Ask;
        private string _param_Bid;
        private string _param_Connection;
        private readonly object _syncObj = new object();

        //public ObservableCollection<InstrumentModel> Instruments { get; }
        public ObservableCollection<OrderUpdateModel> OrderUpdates { get; }
        public ObservableCollection<OrderBookModel> OrderBookL2 { get; }

        private List<OrderBookModel> _OrderBook10;
        public List<OrderBookModel> OrderBook10 { get => _OrderBook10; set { _OrderBook10 = value; OnPropertyChanged(nameof(OrderBook10)); } }
        private ObservableCollection<TradeBucketedDto> _CandlesList;
        public ObservableCollection<TradeBucketedDto> CandlesList { get => _CandlesList; set { _CandlesList = value; OnPropertyChanged(nameof(CandlesList)); } }
        private List<CalculationResult> _ResultCalculationList;
        public List<CalculationResult> ResultCalculationList { get => _ResultCalculationList; set { _ResultCalculationList = value; OnPropertyChanged(nameof(ResultCalculationList)); } }
        private List<OrderUpdateModel> _CurrentOrdersInWork;
        public List<OrderUpdateModel> CurrentOrdersInWork { get => _CurrentOrdersInWork; set { _CurrentOrdersInWork = value; OnPropertyChanged(nameof(CurrentOrdersInWork)); } }
        private List<PositionUpdateModel> _CurrentPosition;
        public List<PositionUpdateModel> CurrentPosition { get => _CurrentPosition; set { _CurrentPosition = value; OnPropertyChanged(nameof(CurrentPosition)); } }
        private List<WalletHistoryDbo> _WalletHistory;
        public List<WalletHistoryDbo> WalletHistory { get => _WalletHistory; set { _WalletHistory = value; OnPropertyChanged(nameof(WalletHistory)); } }
        private List<OrderHistoryModel> _TradeHistory;
        public List<OrderHistoryModel> TradeHistory { get => _TradeHistory; set { _TradeHistory = value; OnPropertyChanged(nameof(TradeHistory)); } }
        private List<APIKeyDto> _ApiKeys;
        public List<APIKeyDto> ApiKeys { get => _ApiKeys; set { _ApiKeys = value; OnPropertyChanged(nameof(ApiKeys)); } }
        private List<BitmexAuthorization> _Accounts;
        public List<BitmexAuthorization> Accounts { get => _Accounts; set { _Accounts = value; OnPropertyChanged(nameof(Accounts)); } }

        private List<BitmexAuthorization> _AccountsForUpdate;
        public List<BitmexAuthorization> AccountsForUpdate { get => _AccountsForUpdate; set { _AccountsForUpdate = value; OnPropertyChanged(nameof(AccountsForUpdate)); } }
        private Configuration AccountConfig;

        public delegate void UpdateCandleViewDelegate(ObservableCollection<TradeBucketedDto> src, List<CalculationResult> result);
        public delegate void OrderBook10Delegate(List<OrderBookModel> src);
        //public delegate void AccountInfoDelegate(WalletDto wallet, MarginDto margin);
        //public delegate void OrdersInfoDelegate(List<OrderUpdateModel> ordersInfo, List<PositionUpdateModel> posInfo);
        public delegate void HistoryInfoDelegate(List<OrderHistoryModel> ordersInfo, List<WalletHistoryDbo> posInfo);
        //public delegate void ApiKeysDelegate(List<APIKeyDto> src);
        //public delegate void OnConnectionMessageDelegate(string src);
        public delegate void ActivateManualDelegate(string src);
        public delegate void UpdateAdditionalAccountsDelegate(string message, int Id, object[] src);

        public string Secret
        {
            get { return _secret; }
            set
            {
                _secret = value;
                OnPropertyChanged();
                StartLoadSymbolsCmd.RaiseCanExecuteChanged();
            }
        }

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged();
                StartLoadSymbolsCmd.RaiseCanExecuteChanged();
            }
        }

        private int _size;
        public int Size { get { return _size; }  set {_size = value; OnPropertyChanged(); OnPropertyChanged(nameof(Size)); }  }
        public double Leverage { get => _leverage; set { _leverage = value; CalcSize(); OnPropertyChanged(nameof(Leverage)); OnPropertyChanged(nameof(Size)); } }

        public bool IsConnected
        {
            get {
                return _isConnected;
            }
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotConnected));
            }
        }

        public bool IsNotConnected => !IsConnected;

        public ICommand BuyCmd { get; }
        public ICommand SellCmd { get; }
        public ICommand AutotradingCmd { get; }
        public ICommand UpdateHistoryCmd { get; }
        public ICommand CheckKeysCmd { get; }
        public ICommand ActivateAdditionalAccountsCmd { get; }
        public ICommand CalcSizeCmd { get; }
        //public ICommand UpdateAPIkeysCmd { get; }        

        public DelegateCommand StartLoadSymbolsCmd { get; }

        public int TabSelectedIndex {
            get { return _tabSelectedIndex; }
            set
            {
                _tabSelectedIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TabSelectedIndex));
            }
        }

        private string _startButtonName;
        public string StartButtonName {
            get { return _startButtonName; }
            set
            {
                _startButtonName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartButtonName));
            }
        }

        private bool _startButtonEnabled = false;
        public bool StartButtonEnabled {
            get { return _startButtonEnabled; }
            set
            {
                _startButtonEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartButtonEnabled));
            }
        }

        public bool StartAdditionalButtonEnabled
        {
            get { return _startAdditionalButtonEnabled; }
            set
            {
                _startAdditionalButtonEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartAdditionalButtonEnabled));
            }
        }

        private int _candlesToCount;
        public int CandlesToCount { get { return _candlesToCount; } set
            {
                _candlesToCount = value; OnPropertyChanged();
                if (value > 0 && TimeFrameValue != "" && TradingMainModuleInstance != null)
                TradingMainModuleInstance.RestartBackgroundWorker("MainWorker", value, TimeFrameValue);
            }
        }

        private string _timeFrameValue;
        public string TimeFrameValue { 
            get { return _timeFrameValue; }
            set
            {
                _timeFrameValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeFrameValue));
                if (CandlesToCount > 0 && value != "" && TradingMainModuleInstance != null)
                    TradingMainModuleInstance.RestartBackgroundWorker("MainWorker", CandlesToCount, value);
            }
        }

        public int Delta { get { return _delta; } set { _delta = value; OnPropertyChanged(); } }

        public bool IsRealAccount { get => _isRealAccount;
            set
            {
                _isRealAccount = value;
                AccountType = _isRealAccount ? "real" : "demo";
                OnPropertyChanged();
            }
        }
        
        public bool IsAllowedToPlace {get => _isAllowedToPlace; set {  _isAllowedToPlace = value;  OnPropertyChanged(nameof(IsAllowedToPlace)); } }
        public string CurrentTime { get { return _currentTime; } set {  _currentTime = value; OnPropertyChanged(); } }
        public string AccountType { get { return _accountType; } set { _accountType = value; OnPropertyChanged(nameof(AccountType)); } }
        public string Param_Ask { get => _param_Ask; set { _param_Ask = value; OnPropertyChanged(nameof(Param_Ask)); } }
        public string Param_Bid { get => _param_Bid; set { _param_Bid = value; OnPropertyChanged(nameof(Param_Bid)); } }

        public string AccountNumber { get => _AccountNumber; set { _AccountNumber = value; OnPropertyChanged(nameof(AccountNumber)); } }
        public string Param_Amount { get => _param_Amount; set { _param_Amount = value; OnPropertyChanged(nameof(Param_Amount)); } }
        public string Param_Margin { get => _param_Margin; set { _param_Margin = value; OnPropertyChanged(nameof(Param_Margin)); } }
        public string Param_Connection { get => _param_Connection; set { _param_Connection = value; OnPropertyChanged(nameof(Param_Connection)); } }

        public Timer t;
        private bool priceInited = false;
        private TradingMainModule _tradingMainModuleInstance;
        public TradingMainModule TradingMainModuleInstance { get => _tradingMainModuleInstance; set => _tradingMainModuleInstance = value; }
       
        public event PropertyChangedEventHandler PropertyChanged;
        private BitmexAuthorization mainAccount;
        private Window parentWindows;

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        
        public bool isOnlyBuy
        {
            get => _isOnlyBuy;
            set
            {
                _isOnlyBuy = value;
                if (TradingMainModuleInstance != null)  TradingMainModuleInstance.CheckOnlyBuy(value);
                OnPropertyChanged();
            }
        }
        #endregion 

        #region Tester vars
        public ICommand TesterStartCmd { get; }
        
        private int _testerCandlesToCount;
        public int TesterCandlesToCount { get { return _testerCandlesToCount; } set { _testerCandlesToCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(TesterCandlesToCount)); } }

        private int _testerStartDeposite;
        public int TesterStartDeposite { get { return _testerStartDeposite; } set { _testerStartDeposite = value; OnPropertyChanged(); OnPropertyChanged(nameof(TesterStartDeposite)); } }

        private int _testerSize;
        public int TesterSize { get { return _testerSize; } set { _testerSize = value; OnPropertyChanged(); OnPropertyChanged(nameof(TesterSize)); } }

        private string _testerStartButtonName;
        public string TesterStartButtonName
        {
            get { return _testerStartButtonName; }
            set
            {
                _testerStartButtonName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TesterStartButtonName));
            }
        }

        private bool _testerStartButtonEnabled = true;
        public bool TesterStartButtonEnabled
        {
            get { return _testerStartButtonEnabled; }
            set
            {
                _testerStartButtonEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TesterStartButtonEnabled));
            }
        }

        private string _testerTimeFrameValue;
        public string TesterTimeFrameValue
        {
            get { return _testerTimeFrameValue; }
            set
            {
                _testerTimeFrameValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TesterTimeFrameValue));
            }
        }

        private DateTime _testerStartDate;
        public DateTime TesterStartDate
        {
            get { return _testerStartDate; }
            set
            {
                _testerStartDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TesterStartDate));
            }
        }

        private DateTime _testerEndDate;
        public DateTime TesterEndDate
        {
            get { return _testerEndDate; }
            set
            {
                _testerEndDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TesterEndDate));
            }
        }
        #endregion
        public void PointShapeLineExample()
        {

            SeriesCollection = new SeriesCollection
            {
                new CandleSeries
                {
                    Title = "xbtusd",
                    Values = new ChartValues<OhlcPoint>
                    {
                        new OhlcPoint(32, 35, 30, 32),
                        new OhlcPoint(33, 38, 31, 37),
                        new OhlcPoint(35, 42, 30, 40),
                        new OhlcPoint(37, 40, 35, 38),
                        new OhlcPoint(35, 38, 32, 33)
                    }
                }
                /*,
                new CandleSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 6, 7, 3, 4 ,6 },
                    PointGeometry = null
                },
                new CandleSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }*/
            };

            

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString("C");

            //modifying the series collection will animate and update the chart
            /*SeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });*/

            //modifying any series values will also animate and update the chart
            //SeriesCollection[3].Values.Add(5d);
        }
        public MainWindowViewModel(Window src)
        {
            parentWindows = src;
            TabSelectedIndex = 1;
            BuyCmd = new DelegateCommand(Buy);
            SellCmd = new DelegateCommand(Sell);
            AutotradingCmd = new DelegateCommand(Autotrading);
            UpdateHistoryCmd = new DelegateCommand(UpdateHistory);
            ActivateAdditionalAccountsCmd = new DelegateCommand(ActivateAdditionalAccounts);            
            StartLoadSymbolsCmd = new DelegateCommand(StartLoad, CanStart);
            CalcSizeCmd = new DelegateCommand(DoCalc);
            //UpdateAPIkeysCmd = new DelegateCommand(UpdateAPIkeys);
            Size = 100;
            OrderUpdates = new ObservableCollection<OrderUpdateModel>();
            OrderBookL2 = new ObservableCollection<OrderBookModel>();
            CurrentOrdersInWork = new List<OrderUpdateModel>();
            Accounts = new List<BitmexAuthorization>();

            Key = "iYk3qeWRcCEnHz3GTwP_bIkC";
            Secret = "jOngTkv8Aj46DPjdeFqaKBz0kFpJsC9CELFJFfW_yFBVkBc_";

            CandlesList = new ObservableCollection<TradeBucketedDto>();
            setStartButton();
            //setAdditionalAccountButton();
            CandlesToCount = 1000;
            TimeFrameValue = "1m";
            Delta = 0;
            IsRealAccount = false;
            isOnlyBuy = false;

            TradingMainModuleInstance = new TradingMainModule("Host=localhost;Port=5433;Username=postgres;Password=SM710v;Database=Bitmex_bot");
            //TradingMainModuleInstance = new TradingMainModule("Host=localhost;Port=5432;Username=postgres;Password=12345;Database=Bitmex_bot");
            PointShapeLineExample();
            //StartButtonEnabled = false;
            //IsAllowedToPlace = false;
            TesterStartCmd = new DelegateCommand(TesterStart);
            TesterTimeFrameValue = "1m";
            TesterCandlesToCount = 1000;
            TesterStartDeposite = 1;
            TesterSize = 100;
            setTesterStartButton();
            TesterStartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);
            TesterStartDate = TesterStartDate.AddDays(-1);
            TesterEndDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);
            t = new System.Threading.Timer(new System.Threading.TimerCallback(ResetTimer), null, 0, System.Threading.Timeout.Infinite);

        }

        private void ResetTimer(object state)
        {//Сбро таймера до наступления новой свечи           
            CurrentTime = DateTime.UtcNow.ToString();
            t.Change(Convert.ToInt32((new TimeSpan(0, 0, 1)).TotalMilliseconds), System.Threading.Timeout.Infinite);
        }

        private void setStartButton()
        {
            StartButtonName = StartButtonName == "Start autotrading" ? "Stop trading" : "Start autotrading";
        }
        private void setTesterStartButton()
        {
            TesterStartButtonName = TesterStartButtonName == "Start testing" ? "Stop testing" : "Start testing";
        }

        private bool CanStart()
        {
            return !string.IsNullOrWhiteSpace(Key) && !string.IsNullOrWhiteSpace(Secret);
        }

        private void StartLoad()
        {
            mainAccount = new BitmexAuthorization { BitmexEnvironment = IsRealAccount ? BitmexEnvironment.Prod : BitmexEnvironment.Test, AccountType = BitmexAccountTypeEnvironment.Main, Key = Key, Secret = Secret, Size = 100, Leverage = 0, Id = 0 };
   
            TradingMainModuleInstance.CreateMainAccount(mainAccount);
            Accounts.RemoveAll(x => x.AccountType == BitmexAccountTypeEnvironment.Main);
            Accounts.Insert(0, mainAccount);
            CreateMonitor();

            TradingMainModuleInstance.SetMainTargetAndDelegates(Dispatcher.CurrentDispatcher, new UpdateCandleViewDelegate(CallBack_ResultCandleView), new OrderBook10Delegate(CallBack_OrderBook10));
            TradingMainModuleInstance.SetMainAccountDispatcherAndDelegate(Dispatcher.CurrentDispatcher, new HistoryInfoDelegate(CallBack_HistoryInfo));
            TradingMainModuleInstance.SetMainAccountDispatcherAndDelegate(Dispatcher.CurrentDispatcher, new ActivateManualDelegate(CallBack_ActivateManual));
            TradingMainModuleInstance.SetMonitorDispatcherAndDelegate(Dispatcher.CurrentDispatcher, new UpdateAdditionalAccountsDelegate(CallBack_UpdateAdditionalAccounts));

            String response = TradingMainModuleInstance.AccountsAuthorization();

            if (response == "True")
            {
                IsConnected = true;               
            }
            else if (response.Contains("This request has expired"))
            {
                MessageBox.Show("Проверте врмея на ПК. Ensure your clock is synced with NTP.");
            }
            else if (response == "False")
            {
                MessageBox.Show("Ошибка подключения.");
            }
            else
            {
                MessageBox.Show(response);
            }        

        }

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Autotrading()
        {
            if (StartButtonName == "Start autotrading")
            {
                if (CalcSize())
                {
                    StartButtonEnabled = false;
                    IsAllowedToPlace = false;
                    StartAdditionalButtonEnabled = false;
                    TradingMainModuleInstance.ActivateAutotrading(Size, Leverage, isOnlyBuy);
                    //TabSelectedIndex = 2;
                    setStartButton();
                    StartButtonEnabled = true;
                }
            }
            else if (StartButtonName == "Stop trading")
            {
                StartButtonEnabled = false;
                TradingMainModuleInstance.DeactivateAutotrading();
                setStartButton();
                StartButtonEnabled = true;
                IsAllowedToPlace = true;
                StartAdditionalButtonEnabled = true;
            }
        }

        private void TesterStart()
        {
            if (TesterStartButtonName == "Start testing")
            {
                //if (CalcSize())
                //{
                    TesterStartButtonEnabled = false;                    
                    setTesterStartButton();
                    TesterStartButtonEnabled = true;
                //}
            }
            else if (TesterStartButtonName == "Stop testing")
            {
                TesterStartButtonEnabled = false;
                setTesterStartButton();
                TesterStartButtonEnabled = true;
            }
        }

        public void CallBack_ResultCandleView(ObservableCollection<TradeBucketedDto> candles, List<CalculationResult> resultTable)
        {
            //Отображаем результаты
            if (candles != null)
            {
                CandlesList = candles;

                /*ChartValues<OhlcPoint> CandlesData = new ChartValues<OhlcPoint>();
                List<string> timestamps = new List<string>();

                int i = 0;
                ObservableCollection < TradeBucketedDto > temp = new ObservableCollection<TradeBucketedDto> (candles.Reverse());

                foreach (var candle in candles)
                {
                    CandlesData.Add(new OhlcPoint((double)candle.Open, (double)candle.High, (double)candle.Low, (double)candle.Close));
                    timestamps.Add(candle.Timestamp.ToString());
                    i++;
                    if (i == 100) { break; }
                }

                CandlesData = new ChartValues<OhlcPoint>(CandlesData.Reverse());
                SeriesCollection[0].Values = CandlesData;


                Labels = timestamps.ToArray();*/
                //YFormatter = value => value.ToString("C");

            }
            if (resultTable != null) ResultCalculationList = resultTable;
        }

        public void CallBack_OrderBook10(List<OrderBookModel> src)
        {
            if (src != null)
            {
                OrderBook10 = src;

                Param_Ask = src.Where(x => x.Direction == "Sell").Select(x => x.Price).Min().ToString();
                Param_Bid = src.Where(x => x.Direction == "Buy").Select(x => x.Price).Max().ToString();
                
                if (!priceInited == true) { priceInited = true; };
                if (needCheck) CheckInit();
            }
        }

        private void CallBack_HistoryInfo(List<OrderHistoryModel> trades_src, List<WalletHistoryDbo> wallet_src)
        {
            if (trades_src != null) TradeHistory = trades_src;
            if (wallet_src != null) WalletHistory = wallet_src;
        }

        private void CallBack_UpdateAdditionalAccounts(string MessageType, int Id, object[] src)
        {
            if (Id >= 0)
            {
                Grid mainGrid = (Grid)parentWindows.FindName("MainContainer");
                Grid Container = mainGrid != null ? (Grid)mainGrid.FindName("Container" + Id.ToString()) : null;
                Panel mainPanel = Container != null ? (Panel)Container.FindName("AccountId" + Id.ToString()) : null;
                Panel AccountParamId = Container != null ? (Panel)Container.FindName("AccountParamId" + Id.ToString()) : null;

                if (mainPanel != null && AccountParamId != null)
                {
                    try
                    {
                        switch (MessageType)
                        {
                            case "ConnectionInfo":
                                if (Id == 0 && src[0].ToString() == "Connection is ok.")
                                    TradingMainModuleInstance.RestartBackgroundWorker("MainWorker", CandlesToCount, TimeFrameValue);

                                Label connectionLabel = (Label)AccountParamId.FindName("ConnectionInfo" + Id.ToString());
                                connectionLabel.Content = "WebSocketAPI: " + src[0].ToString();

                                connectionLabel.Foreground = (!src[0].ToString().Contains("Connection is ok.")) ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;

                                break;

                            case "ConnectionRestInfo":

                                Label connectionRestLabel = (Label)AccountParamId.FindName("ConnectionRestInfo" + Id.ToString());
                                if (src.Count() > 0 && src[0] != null)
                                {
                                    string[] arr = (string[])src[0];
                                    connectionRestLabel.Content = "RestAPI: " + arr[0].ToString() +
                                                                  " X-RateLimit-Limit " + arr[1].ToString() +
                                                                  " X-RateLimit-Remaining " + arr[2].ToString();

                                    connectionRestLabel.Foreground = (arr[0].ToString() != "OK") ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;
                                }
                                break;
                            case "LastActionInfo":
                                Label LastActionInfoLabel = (Label)AccountParamId.FindName("LastActionInfo" + Id.ToString());
                                LastActionInfoLabel.Content = "Action: " + src[0].ToString();
                                break;
                            case "WalletInfo":

                                Label walletLabel = (Label)AccountParamId.FindName("WalletInfo" + Id.ToString());

                                WalletDto wallet = (src[0] != null) ? (WalletDto)src[0] : null;
                                if (wallet != null)
                                {
                                    if (wallet.Account != null)
                                    {
                                        decimal acc_num = (decimal)wallet.Account;
                                        if (Id == 0) AccountNumber = acc_num.ToString();
                                        walletLabel.Content = "Account: " + acc_num.ToString();
                                    }
                                }

                                break;
                            case "MarginInfo":
                                Label marginLabel = (Label)AccountParamId.FindName("MarginInfo" + Id.ToString());

                                MarginDto margin = (src[0] != null) ? (MarginDto)src[0] : null;
                                if (margin != null)
                                {
                                    if (margin.MarginBalance != null)
                                    {
                                        decimal margin_num = (decimal)margin.MarginBalance;
                                        if (Id == 0)
                                        {
                                            Param_Amount = margin_num.ToString();
                                            if (needCheck) CheckInit();
                                        }
                                        marginLabel.Content = "Balance: " + ((double)margin_num * 0.00000001).ToString();
                                    }
                                }
                                break;
                            case "ApiKeysInfo":
                                Label lvApi = (Label)AccountParamId.FindName("ApiKeysInfo" + Id.ToString());

                                if (src[0] != null) lvApi.Content = "ApiKeys: " + ((List<APIKeyDto>)src[0]).Count.ToString();
                                break;
                            case "PositionInfo":
                                ListView lvPos = (ListView)mainPanel.FindName("PositionInfo" + Id.ToString());
                                if (src[0] != null) lvPos.ItemsSource = new List<PositionAdditionalUpdateModel>((List<PositionAdditionalUpdateModel>)src[0]);
                                break;
                            case "OrderInfo":
                                ListView lvOrder = (ListView)mainPanel.FindName("OrderInfo" + Id.ToString());
                                if (src[0] != null)
                                {
                                    List<OrderUpdateModel> message = new List<OrderUpdateModel>((List<OrderUpdateModel>)src[0]);
                                    lvOrder.ItemsSource = message;
                                    lvOrder.Visibility = message.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                                }
                                break;
                            default:

                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }   
            }
        }

        private void CheckInit()
        {
            if (Param_Amount != "" && priceInited)
            {
                needCheck = false;
                CallBack_ActivateManual("");
            }
        }

        private void CallBack_ActivateManual(string src)
        {
            IsAllowedToPlace = true;
            StartButtonEnabled = true;
           // StartAdditionalButtonEnabled = true;
        }

        private void DoCalc()
        {
            CalcSize();
        }

        private bool CalcSize()
        {
            try
            {
                double amount = double.Parse(Param_Amount) * 0.00000001;
                double price = double.Parse(Param_Bid);
                if (Leverage <= 10 && Leverage > 0)
                {
                    Size = (int)(amount * price * 0.99 * Leverage);
                    return true;
                }
                else if (Leverage == 0)
                {
                    if (Size <= 0)
                    {
                        Size = 100;
                        MessageBox.Show("Size должен быть больше 0.");
                        return false;
                    }                       
                    return true;
                }
                else
                {
                    MessageBox.Show("Плечо от 0 до 10. Leverage should be from 0 to 10.");
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка выставления плеча и размера контракта.");
                return false;
            }
        }

        private void Sell()
        {
            IsAllowedToPlace = false;
            StartButtonEnabled = false;
            if (CalcSize()) TradingMainModuleInstance.doSell(Size, Leverage);
        }
        
        private void Buy()
        {
            IsAllowedToPlace = false;
            StartButtonEnabled = false;
            if (CalcSize()) TradingMainModuleInstance.doBuy(Size, Leverage);        
        }

        private void UpdateHistory()
        {
            TradingMainModuleInstance.UpdateHistory();
        }

        private List<BitmexAuthorization> ReadXML()
        {
            try
            {
                AccountConfig = Configuration.Deserialize("Settings.xml");

                List<BitmexAuthorization> ba = new List<BitmexAuthorization>();

                int i = 1;
                foreach (var account in AccountConfig.Items)
                {
                    if (account.Type == "additional")
                    {
                        bool isReal;
                        if (!Boolean.TryParse(account.IsReal, out isReal))
                        {
                            throw new ArgumentException("Invalid 'IsReal' param.");
                        }

                        int size;
                        if (!Int32.TryParse(account.Size, out size))
                        {
                            throw new ArgumentException("Invalid 'Size' param.");
                        }

                        decimal leverage;
                        if (!Decimal.TryParse(account.Leverage, out leverage))
                        {
                            throw new ArgumentException("Invalid 'Leverage' param.");
                        }

                        ba.Add(new BitmexAuthorization { BitmexEnvironment = isReal ? BitmexEnvironment.Prod : BitmexEnvironment.Test, AccountType = BitmexAccountTypeEnvironment.Additional, Key = account.APIKey, Secret = account.APISecret, Size = size, Leverage = leverage, Id = i });
                        i++;
                    }
                }

                ba.Insert(0, mainAccount);

                return ba;
         
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new List<BitmexAuthorization>();
            }
        }

        private void ActivateAdditionalAccounts()
        {
            //Remove prev robots 
            TradingMainModuleInstance.RemoveAdditionalRobot();
            //Read New
            Accounts = ReadXML();

            CreateMonitor();

            TradingMainModuleInstance.AddAdditionalAccounts(Accounts);
            TradingMainModuleInstance.SetMonitorDispatcherAndDelegate(Dispatcher.CurrentDispatcher, new UpdateAdditionalAccountsDelegate(CallBack_UpdateAdditionalAccounts));
            TradingMainModuleInstance.LoginAdditionalAccounts();

        }

        private void CreateMonitor()
        {
            Grid gridTemp = (Grid)parentWindows.FindName("MainContainer");
            Grid containerFind = (Grid)gridTemp.FindName("Container0");
            int i = 0;
            if (containerFind != null)
            {
                if (gridTemp.Children.Count > 1)
                {
                    foreach (var control in gridTemp.Children)
                    {
                        if ("Container0" != ((Grid)control).Name) gridTemp.UnregisterName(((Grid)control).Name);
                    }
                    gridTemp.RowDefinitions.RemoveRange(1, gridTemp.Children.Count - 1);
                    gridTemp.Children.RemoveRange(1, gridTemp.Children.Count - 1);                   
                }
                i = 1;
            }
            else
            {
                NameScope.SetNameScope(gridTemp, new NameScope());
            }

            foreach (var account in (containerFind == null ? Accounts : Accounts.Where(x => x.AccountType == BitmexAccountTypeEnvironment.Additional)))
            {
                gridTemp.RowDefinitions.Add(new RowDefinition {  });

                Grid container = new Grid { Name = "Container"+ account.Id.ToString() };
                NameScope.SetNameScope(container, new NameScope());
                container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                container.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.0, GridUnitType.Star) });

                StackPanel stackPanel = new StackPanel { Orientation = Orientation.Vertical, Name = "AccountId" + account.Id.ToString() };
                NameScope.SetNameScope(stackPanel, new NameScope());
                container.RegisterName(stackPanel.Name, stackPanel);

                StackPanel stackAccountPanel = new StackPanel { Orientation = Orientation.Horizontal, Name = "AccountParamId" + account.Id.ToString() };
                NameScope.SetNameScope(stackAccountPanel, new NameScope());
                container.RegisterName(stackAccountPanel.Name, stackAccountPanel);

                Label LeverageInfo = (new Label
                {
                    Content = "Leverage: "+ account.Leverage,
                    Name = "LeverageInfo" + account.Id.ToString()
                });
                stackAccountPanel.RegisterName(LeverageInfo.Name, LeverageInfo);
                stackAccountPanel.Children.Add(LeverageInfo);

                Label WalletInfo = (new Label
                {
                    Content = "WalletInfo: ",
                    Name = "WalletInfo" + account.Id.ToString()
                });
                stackAccountPanel.RegisterName(WalletInfo.Name, WalletInfo);
                stackAccountPanel.Children.Add(WalletInfo);

                Label MarginInfo = (new Label
                {
                    Content = "Balance: ",
                    Name = "MarginInfo" + account.Id.ToString()
                });
                stackAccountPanel.RegisterName(MarginInfo.Name, MarginInfo);
                stackAccountPanel.Children.Add(MarginInfo);

                /*Label BalanceInfo = (new Label
                {
                    Content = "Balance: ",
                    Name = "BalanceInfo" + account.Id.ToString()
                });
                stackAccountPanel.RegisterName(BalanceInfo.Name, BalanceInfo);
                stackAccountPanel.Children.Add(BalanceInfo);*/

                Label ApiKeysInfo = (new Label
                {
                    Content = "Key number: ",
                    Name = "ApiKeysInfo" + account.Id.ToString()
                });
                stackAccountPanel.RegisterName(ApiKeysInfo.Name, ApiKeysInfo);
                stackAccountPanel.Children.Add(ApiKeysInfo);

                Label ConnectionInfo = (new Label
                {
                    Content = "WebScoketAPI: ",
                    Name = "ConnectionInfo" + account.Id.ToString()
                });
                stackAccountPanel.RegisterName(ConnectionInfo.Name, ConnectionInfo);
                stackAccountPanel.Children.Add(ConnectionInfo);

                Label ConnectionRestInfo = (new Label
                {
                    Content = "RestInfo: ",
                    Name = "ConnectionRestInfo" + account.Id.ToString()
                });
                stackAccountPanel.RegisterName(ConnectionRestInfo.Name, ConnectionRestInfo);
                stackAccountPanel.Children.Add(ConnectionRestInfo);

                Label LastActionInfo = (new Label
                {
                    Content = "Last action: ",
                    Name = "LastActionInfo" + account.Id.ToString()
                });
                stackAccountPanel.RegisterName(LastActionInfo.Name, LastActionInfo);
                stackAccountPanel.Children.Add(LastActionInfo);

                ListView PositionInfo = new ListView { Name = "PositionInfo" + account.Id.ToString() };
                PositionInfo.View = new GridView { };
                DynamicBindingListView.SetGenerateColumnsGridView(PositionInfo, true);
                stackPanel.RegisterName(PositionInfo.Name, PositionInfo);
                stackPanel.Children.Add(PositionInfo);

                ListView OrderInfo = new ListView { Name = "OrderInfo" + account.Id.ToString(), Visibility = Visibility.Collapsed };
                OrderInfo.View = new GridView { };
                DynamicBindingListView.SetGenerateColumnsGridView(OrderInfo, true);
                stackPanel.RegisterName(OrderInfo.Name, OrderInfo);
                stackPanel.Children.Add(OrderInfo);

                /*ListView ApiKeysInfo = new ListView { Name = "ApiKeysInfo" + account.Id.ToString() };
                ApiKeysInfo.View = new GridView { };
                DynamicBindingListView.SetGenerateColumnsGridView(ApiKeysInfo, true);
                stackPanel.RegisterName(ApiKeysInfo.Name, ApiKeysInfo);
                stackPanel.Children.Add(ApiKeysInfo);*/

          

                stackPanel.Children.Add(new Label());

                container.Children.Add(stackPanel);
                container.Children.Add(stackAccountPanel); 
                
                Grid.SetRow(stackAccountPanel, 0); 
                Grid.SetRow(stackPanel, 1);

                gridTemp.RegisterName(container.Name, container);
                gridTemp.Children.Add(container);
                Grid.SetRow(container, i);
                i++;
            }

            if (gridTemp.RowDefinitions.Count > 0)
            gridTemp.RowDefinitions[gridTemp.RowDefinitions.Count - 1].Height = new GridLength(1.0, GridUnitType.Star);

            TabSelectedIndex = 6;
        }

        /*private void UpdateAPIkeys()
        {
            ReadXML();

            foreach (var accountXML in AccountConfig.Items.Where(x => x.Type == "additional"))
            {
                bool isReal;
                if (!Boolean.TryParse(accountXML.IsReal, out isReal))
                {
                    throw new ArgumentException("Invalid 'IsReal' param.");
                }

                bool isUpdateAPI;
                if (!Boolean.TryParse(accountXML.IsAPIupdated, out isUpdateAPI))
                {
                    throw new ArgumentException("Invalid 'IsAPIupdated' param.");
                }

                if (isUpdateAPI) continue;

                BitmexAuthorization account = new BitmexAuthorization { BitmexEnvironment = isReal ? BitmexEnvironment.Prod : BitmexEnvironment.Test, AccountType = BitmexAccountTypeEnvironment.Additional, Key = accountXML.APIKey, Secret = accountXML.APISecret };
                
                TradingRobot tr = new TradingRobot(account);
                if (tr.DoAuthorization() == "True")
                {
                    var ApiKeyPOSTRequest = ApiKeyPOSTRequestParams.GetApiKeyPOSTRequest("tests12393", "order", false);
                    APIKeyDto RobotNewKey = tr.RobotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.APIKey.PostApiKey, ApiKeyPOSTRequest);

                    if (RobotNewKey != null)
                    {
                        accountXML.APIKey = RobotNewKey.Id;
                        accountXML.APISecret = RobotNewKey.Secret;
                        account.Key = RobotNewKey.Id;
                        account.Secret = RobotNewKey.Secret;

                        tr.TerminateRobot();

                        tr = new TradingRobot(account);
                        if (tr.DoAuthorization() == "True")
                        {
                            var ApiKeyGETRequest = ApiKeyGETRequestParams.GetApiKeyGETRequest();
                            List<APIKeyDto> RobotApikeys = tr.RobotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.APIKey.GetApiKey, ApiKeyGETRequest);

                            if (RobotApikeys != null)
                            {
                                int i = RobotApikeys.Count;
                                foreach (var key in RobotApikeys)
                                {
                                    if (key.Id != account.Key)
                                    {
                                        var ApiKeyDELETERequest = ApiKeyDELETERequestParams.GetApiKeyDELETERequest(key.Id);
                                        APIKeyDeleteDto deleted = tr.RobotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.APIKey.DeleteApiKey, ApiKeyDELETERequest);
                                        if (deleted == null)
                                        {
                                            MessageBox.Show("Error in removing old Keys For new Key = " + account.Key + ". Try repeat.");
                                            break;
                                        }
                                        else
                                        {
                                            i--;
                                        }
                                    }
                                }
                                if (i == 1) accountXML.IsAPIupdated = "true";
                            }
                            else
                            {
                                MessageBox.Show("Error in removing old Keys For new Key = " + account.Key + ". Try repeat.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Cant connect with new Key = " + account.Key);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cant create new Key for old Key = " + account.Key + ". Try repeat.");
                    }
                }
                else
                {
                    MessageBox.Show("Cant connect with old Key = " + account.Key);
                }               
            }

            //Save results
            Configuration.Serialize("Settings.xml", AccountConfig);
        }*/
    }
}

