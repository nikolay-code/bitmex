using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitmex.NET.Dtos;
using Bitmex.NET.Dtos.Socket;
using Bitmex.NET.Models;
using Bitmex.NET;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using AutoMapper;
using System.Reflection;
using System.Threading;
using System.Windows.Data;
using NLog;
using BitMexLibrary.Automapping;

namespace BitMexLibrary
{
    public class TradingRobot
    {
        public Logger logger = LogManager.GetCurrentClassLogger();
        private IBitmexAuthorization _robotBitmexAuthorization;
        private IBitmexApiService _robotBitmexApiService;
        private IBitmexApiSocketService _robotBitmexApiSocketService;
        public IBitmexAuthorization RobotBitmexAuthorization { get => _robotBitmexAuthorization; set => _robotBitmexAuthorization = value; }
        public IBitmexApiService RobotBitmexApiService { get => _robotBitmexApiService; set => _robotBitmexApiService = value; }
        public IBitmexApiSocketService RobotBitmexApiSocketService { get => _robotBitmexApiSocketService; set => _robotBitmexApiSocketService = value; }

        private int _size;
        private double _leverage;
        private string _isConnected;
        private bool _tradeIsAllowed = false;
        public bool TradeIsAllowed { get => _tradeIsAllowed; set => _tradeIsAllowed = value; }
        public int Size { get => _size; set => _size = value; }
        public string IsConnected { get => _isConnected; set => _isConnected = value; }
        public double Leverage { get => _leverage; set => _leverage = value; }

        private readonly object _syncObjOper = new object();
        private readonly object _syncObjCurrentOrders = new object();
        private readonly object _syncObjRobotOrders = new object();
        private readonly object _syncObjOrderBook10 = new object();
        private readonly object _syncObjAccountUpdate = new object();
        private readonly object _syncObjPosUpdate = new object();
        private readonly object _syncObjMain = new object();
        public Stack<StateOperation> Operations = new Stack<StateOperation>();
        public List<OrderUpdateModel> CurrentOrdersInWork { get; private set; }
        public List<OrderUpdateModel> RobotOrders { get; private set; }
        public List<OrderUpdateModel> OrdersHistory { get; private set; }
        public List<APIKeyDto> RobotApikeys { get; private set; }
        public List<PositionAdditionalUpdateModel> currentPos { get; private set; }
        private bool OrdersRemovedBeforeStart = false;
        private bool LeverageSetBeforeStart = false;
        private bool OrdersRemovedAfterStop = false;
        private bool UpdateMonitoring = true;
        private bool DoUpdate = false;
        private bool StateMachineEnabled = false;
        private int counter = 0;

        private ObservableCollection<TradeBucketedDto> _candleBuffer;
        private List<CalculationResult> _currentResultTable;
        public ObservableCollection<TradeBucketedDto> CandleBuffer { get => _candleBuffer; set => _candleBuffer = value; }
        public List<CalculationResult> CurrentResultTable { get => _currentResultTable; set => _currentResultTable = value; }
        //public List<OrderBook10Dto> OrderBook10 { get; private set; }
        //public ObservableCollection<OrderUpdateModel> OrderUpdates { get; set; }
        private decimal _last_Ask = -1;
        private decimal _last_Bid = -1;
        public decimal Last_Ask {
            get {
                decimal boo;
                lock (_syncObjOrderBook10)
                {
                    boo = _last_Ask;
                }
                return boo;
            }
            set { _last_Ask = value; }
        }
        public decimal Last_Bid {
            get {
                decimal boo;
                lock (_syncObjOrderBook10)
                {
                    boo = _last_Bid;
                }
                return boo;
            }
            set { _last_Bid = value; }
        }
        public DateTime lastOrderReplacement = DateTime.UtcNow;

        private decimal _AccountNumber = 0;
        private decimal _param_Amount = 0;
        private decimal _param_Margin = 0;
        public decimal AccountNumber { get => _AccountNumber; set { _AccountNumber = value; } }
        public decimal Param_Amount { get => _param_Amount; set { _param_Amount = value; } }
        public decimal Param_Margin { get => _param_Margin; set { _param_Margin = value; } }

        public WalletDto WalletInfo;
        public MarginDto MarginInfo;
        public Thread WorkerThread;
        public Thread OrderUpdateChekerThread;
        public Thread ReconnectThread;
        public Thread AccountInitThread;
        public Thread ApiKeyInfoThread;
        public Thread DeactivatingRobotThread;
        public Thread CheckRestThread;
        public Thread CheckSocketSubscribeThread;
        public Thread UpdateAccountThread;

        private Dispatcher parentThread;
        public Dispatcher ParentThread { get => parentThread; set => parentThread = value; }
        public Dictionary<string, Delegate> ObjectApi = new Dictionary<string, Delegate>();
        public Dictionary<string, DateTime> Frequncy = new Dictionary<string, DateTime>();
        public Dictionary<string, bool> isSubscribed = new Dictionary<string, bool>();
        public Dictionary<string, ParameterizedThreadStart> ThreadStartDictionary = new Dictionary<string, ParameterizedThreadStart>();

        public delegate void ConnectionErrorDelegate(string message);
        public delegate void ConnectionRestDelegate(string[] message);

        private bool _isOnlyBuy;
        public bool TradeOnlyBuy
        {
            get => _isOnlyBuy;
            set
            {
                _isOnlyBuy = value;
            }
        }
        public TradingRobot(BitmexAuthorization ba)
        {
            RobotBitmexAuthorization = ba;
            Size = ba.Size;
            Leverage = (double)ba.Leverage;
            RobotBitmexApiService = BitmexApiService.CreateDefaultApi(RobotBitmexAuthorization, _syncObjMain);
            RobotBitmexApiSocketService = BitmexApiSocketService.CreateDefaultApi(RobotBitmexAuthorization);

            ConnectionErrorDelegate ConnectionErrorCallBack = new ConnectionErrorDelegate(CallBack_ConnectionError);
            RobotBitmexApiSocketService.SetOwnerAndDelegate(Dispatcher.CurrentDispatcher, ConnectionErrorCallBack);

            ConnectionRestDelegate ConnectionRestCallBack = new ConnectionRestDelegate(CallBack_RestError);
            RobotBitmexApiService.SetOwnerAndDelegate(Dispatcher.CurrentDispatcher, ConnectionRestCallBack);

            //Reset robot data before loading
            AccountNumber = 0;
            Param_Amount = 0;
            currentPos = null;
            CurrentOrdersInWork = null;
            RobotApikeys = null;
            OrdersRemovedBeforeStart = false;
            OrdersRemovedAfterStop = false;
            LeverageSetBeforeStart = false;
            TradeOnlyBuy = false;

            RobotOrders = new List<OrderUpdateModel>();
            //OrderUpdates = new ObservableCollection<OrderUpdateModel>();
            CurrentResultTable = new List<CalculationResult>(); 

            isSubscribed.Add("CreateOrderSubsription", false);
            isSubscribed.Add("CreateMarginSubscription", false);
            isSubscribed.Add("CreateWalletSubscription", false);
            isSubscribed.Add("CreatePositionSubsription", false);

         /*   UpdateAccountThread = new Thread(StartUpdateAccount); UpdateAccountThread.Name = "UpdateAccountThread";
            ReconnectThread = new Thread(Reconnecting); ReconnectThread.Name = "ReconnectThread";
            CheckRestThread = new Thread(StartCheckRestRequestThread); CheckRestThread.Name = "CheckRestThread";
            AccountInitThread = new Thread(StartInitRobotThread); AccountInitThread.Name = "AccountInitThread";
            ApiKeyInfoThread = new Thread(StartApiKeyInfoThread); ApiKeyInfoThread.Name = "ApiKeyInfoThread";
            OrderUpdateChekerThread = new Thread(StartOrderUpdateChekerThread); OrderUpdateChekerThread.Name = "OrderUpdateChekerThread";
            WorkerThread = new Thread(StartWork); WorkerThread.Name = "WorkerThread";
            DeactivatingRobotThread = new Thread(StartDeactivatingThread); DeactivatingRobotThread.Name = "DeactivatingRobotThread";
            CheckSocketSubscribeThread = new Thread(CheckSocketSubscribe); CheckSocketSubscribeThread.Name = "CheckSocketSubscribeThread";

            ThreadStartDictionary.Add(UpdateAccountThread.Name, new ParameterizedThreadStart(StartUpdateAccount));
            ThreadStartDictionary.Add(ReconnectThread.Name, new ParameterizedThreadStart(Reconnecting));
            ThreadStartDictionary.Add(CheckRestThread.Name, new ParameterizedThreadStart(StartCheckRestRequestThread));
            ThreadStartDictionary.Add(AccountInitThread.Name, new ParameterizedThreadStart(StartInitRobotThread));
            ThreadStartDictionary.Add(ApiKeyInfoThread.Name, new ParameterizedThreadStart(StartApiKeyInfoThread));
            ThreadStartDictionary.Add(OrderUpdateChekerThread.Name, new ParameterizedThreadStart(StartOrderUpdateChekerThread));
            ThreadStartDictionary.Add(WorkerThread.Name, new ParameterizedThreadStart(StartWork));
            ThreadStartDictionary.Add(DeactivatingRobotThread.Name, new ParameterizedThreadStart(StartDeactivatingThread));
            ThreadStartDictionary.Add(CheckSocketSubscribeThread.Name, new ParameterizedThreadStart(CheckSocketSubscribe));*/
        }

        public string DoAuthorization()
        {
            try
            {
                IsConnected = RobotBitmexApiSocketService.Connect();
                if (IsConnected == "True")
                {
                    StartThread("AccountInitThread");
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "ConnectionInfo", RobotBitmexAuthorization.Id, new[] { "Connection is ok." });
                    //StartThread("CheckSocketSubscribeThread");

                    return IsConnected.ToString();
                }
                else if (IsConnected == "False")
                {
                    return IsConnected.ToString();
                }
                else
                {//Ответ ошибки подключения
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "ConnectionInfo", RobotBitmexAuthorization.Id, new[] { IsConnected });
                    return IsConnected.ToString();
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private void CheckSocketSubscribe(object info)
        {
            while (true)
            {
                InitSubscribe();
                if (isSubscribed.Where(x => x.Value == true).Count() == 2)
                {//Connection is done. Connected and all subscrides are ok
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "ConnectionInfo", RobotBitmexAuthorization.Id, new[] { "Connection is ok." });
                    break;
                }
                Thread.Sleep(new TimeSpan(0, 0, 5));
            }
        }

        private void InitSubscribe()
        {
            if (IsConnected == "True")
            {

                /*if (!isSubscribed["CreateOrderSubsription"])
                {
                    string resCreateOrderSubsription = _robotBitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateOrderSubsription(
                        message =>
                        {
                            if (message.Data.Count() > 0)
                            {
                                StartThread(OrderUpdateChekerThread);
                            }
                        }));

                    if (resCreateOrderSubsription == "Successfully subscribed on") isSubscribed["CreateOrderSubsription"] = true;
                }*/

                /* if (!isSubscribed["CreateMarginSubscription"])
                {
                    string resCreateMarginSubscription = _robotBitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateMarginSubscription(
                              message =>
                              {
                                  foreach (var dto in message.Data)
                                  {
                                      UpdateAccountMarginInfo(MarginInfo);
                                  }
                              }));

                    if (resCreateMarginSubscription == "Successfully subscribed on") isSubscribed["CreateMarginSubscription"] = true;
                }

                if (!isSubscribed["CreateWalletSubscription"])
                {
                   string resCreateWalletSubscription = _robotBitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateWalletSubscription(
                              message =>
                              {
                                  foreach (var dto in message.Data)
                                  {
                                      UpdateAccountWalletInfo(WalletInfo);
                                  }
                              }));

                    if (resCreateWalletSubscription == "Successfully subscribed on") isSubscribed["CreateWalletSubscription"] = true;
                }

               if (!isSubscribed["CreatePositionSubsription"])
                {
                    string resCreatePositionSubsription =_robotBitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreatePositionSubsription(
                    message =>
                    {
                        lock (_syncObjPosUpdate)
                        {
                            currentPos = ConvertPosData(message.Data.ToList()); // resevedPosData;

                            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "PositionInfo", RobotBitmexAuthorization.Id, new object[] { currentPos });
                        }
                    }));

                    if (resCreatePositionSubsription == "Successfully subscribed on") isSubscribed["CreatePositionSubsription"] = true;
                }*/
            }
        }

        public void StateMachine(object state = null)
        {
            lock (_syncObjOper)
            {
                if (Operations.Count > 0 && Operations.Peek().CurrState != State.None)
                {
                    StateOperation currState = Operations.Peek();

                    switch (currState.CurrState)
                    {
                        case State.Start:
                            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Start handle signal" });
                            logger.Warn("State " + currState.CurrState.ToString());
                            //Operations.Push(new StateOperation(State.Repeat_dell_all_orders));
                            Operations.Push(new StateOperation(State.Close_position));

                            break;
                        case State.Repeat_buy:
                            logger.Warn("State " + currState.CurrState.ToString());
                            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Put buy order" });

                            //OrderGETRequestParams CheckBeforeBuyRequestParams = OrderGETRequestParams.GetOrderGetRequest("XBTUSD", 100);
                            //CheckBeforeBuyRequestParams.Filter = new Dictionary<string, string> { { "open", "true" } };
                            //List<OrderDto> CheckBeforeBuyResponse = RobotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.Order.GetOrder, CheckBeforeBuyRequestParams);

                            lock (_syncObjRobotOrders)
                            {
                                if (RobotOrders.Count == 0)
                                {
                                    if (SendBuyOrderLimit(currState.Vol))
                                    {
                                        Operations.Pop(); //Operation is done, remove curr operation 
                                        Operations.Push(new StateOperation(State.Wait_order_filled));
                                    }
                                    else
                                    {
                                        Operations.Pop();
                                        Operations.Push(new StateOperation(State.Close_position));
                                    }
                                }
                                else
                                {
                                    Operations.Pop();
                                    Operations.Push(new StateOperation(State.Wait_order_filled));
                                }
                            }
                            break;

                        case State.Repeat_sell:
                            logger.Warn("State " + currState.CurrState.ToString());
                            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Put sell order" });

                            //OrderGETRequestParams CheckBeforeSellRequestParams = OrderGETRequestParams.GetOrderGetRequest("XBTUSD", 100);
                            //CheckBeforeSellRequestParams.Filter = new Dictionary<string, string> { { "open", "true" } };
                            //List<OrderDto> CheckBeforeSellResponse = RobotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.Order.GetOrder, CheckBeforeSellRequestParams);

                            lock (_syncObjRobotOrders)
                            {
                                if (RobotOrders.Count == 0)
                                {
                                    if (SendSellOrderLimit(currState.Vol))
                                    {
                                        Operations.Pop();
                                        Operations.Push(new StateOperation(State.Wait_order_filled));
                                    }
                                    else
                                    {
                                        Operations.Pop();
                                        Operations.Push(new StateOperation(State.Close_position));
                                    }
                                }
                                else
                                {
                                    Operations.Pop();
                                    Operations.Push(new StateOperation(State.Wait_order_filled));
                                }
                            }

                            break;
                        case State.Close_position:
                            logger.Warn("State " + currState.CurrState.ToString());
                            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Amend position" });
                            bool Executed = false;
                            List<PositionDto> currPos = _robotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.Position.GetPosition, PositionGETRequestParams.GetPositionGetRequest(100));
                                 
                            if (Executed && currPos != null && UpdateBalance() && GetLastBidQuote())
                            { 
                                Operations.Pop();
                                currState = Operations.Peek();

                                    currPos = currPos != null ? currPos.Where(x => x.Symbol == "XBTUSD").ToList() : new List<PositionDto>();
                                    decimal currPosQty = currPos.Count > 0 ? currPos[0].CurrentQty : 0;

                                    if (Leverage <= 10 && Leverage > 0)
                                    {
                                        double amount = (double)(Param_Amount) * 0.00000001;
                                        double price = (double)Last_Bid;
                                        int temp_Size = (int)(Math.Floor((amount * price * 0.99 * Leverage)));
                                        if (temp_Size > 0) Size = temp_Size;
                                    }
                                    else if (Leverage == 0)
                                    {
                                        if (Size <= 0) Size = 100;
                                    }

                                    //If signal sell and TradeOnlyBuy then target = 0
                                    decimal targetVolume = GetLastUnhandledSignal() == "Buy" ? 
                                        ( Size ) 
                                        : 
                                        ( GetLastUnhandledSignal() == "Sell" ? 
                                            (  !TradeOnlyBuy ? ((-1) * Size) : 0)
                                            :
                                            100);

                                    if (targetVolume > 0)
                                    {
                                        if (currPosQty > 0)
                                        {
                                            targetVolume = 0;
                                        }
                                        else if (currPosQty < 0)
                                        {
                                            targetVolume = targetVolume + Math.Abs(currPosQty);
                                        }
                                    }
                                    else if (targetVolume < 0)
                                    {
                                        if (currPosQty < 0)
                                        {
                                            targetVolume = 0;
                                        }
                                        else if (currPosQty > 0)
                                        {
                                            targetVolume = (Math.Abs(targetVolume) + Math.Abs(currPosQty)) * -1;
                                        }
                                    }
                                    else if (targetVolume == 0)
                                    {
                                        if (currPosQty < 0)
                                        {
                                            targetVolume = Math.Abs(currPosQty);
                                        }
                                        else if (currPosQty > 0)
                                        {
                                            targetVolume = (Math.Abs(currPosQty)) * -1;
                                        }
                                    }

                                    if (Math.Abs(targetVolume) > 0)
                                    {
                                        //Operations.Push(new StateOperation(State.Done));
                                        //Operations.Push(new StateOperation(State.Repeat_dell_all_orders));
                                        //Operations.Push(new StateOperation(State.Set_leverage, request_nums: 0, leverage: 0));
                                        Operations.Push(new StateOperation(targetVolume > 0 ? State.Repeat_buy : State.Repeat_sell, (int)Math.Abs(Math.Floor(targetVolume))));
                                        //Operations.Push(new StateOperation(State.Set_leverage, request_nums: 0, leverage: 0));
                                    }
                                    else
                                    {
                                        Operations.Push(new StateOperation(State.Done));
                                        Operations.Push(new StateOperation(State.Repeat_dell_all_orders));
                                        Operations.Push(new StateOperation(State.Set_leverage, request_nums: 0, leverage: 0));
                                }                                
                            }
                            break;
                        case State.Repeat_dell_all_orders:
                            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Remove all orders" });
                            logger.Warn("State " + currState.CurrState.ToString());
                            if (DeleteAllOrders())
                            {
                                Operations.Pop();
                               // Operations.Push(new StateOperation(State.Close_position));
                            }
                            break;

                        case State.Wait_order_filled:
                            logger.Warn("State " + currState.CurrState.ToString());
                            DoUpdate = true;
                            Thread.Sleep(new TimeSpan(0, 0, 1));
                            //StartThread(OrderUpdateChekerThread);
                            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Waiting for filling" });
                                                        
                            lock (_syncObjRobotOrders)
                            {

                                if (RobotOrders.Where(x => x.OrdStatus == "Filled").Count() == 1 && RobotOrders.Count == 1)
                                {
                                    OrderUpdateModel oum = RobotOrders.Where(x => x.OrdStatus == "Filled").First();
                                    if (oum.LeavesQty != null && oum.LeavesQty == 0)
                                    {
                                        logger.Warn(" waiting all filled");
                                        RobotOrders.Clear();
                                        Operations.Pop();
                                        DoUpdate = false;
                                        //Завершаем
                                        getPosData();
                                        Operations.Push(new StateOperation(State.Done));
                                        Operations.Push(new StateOperation(State.Repeat_dell_all_orders));
                                        Operations.Push(new StateOperation(State.Set_leverage, request_nums: 0, leverage: 0));
                                    }
                                    else if (oum.LeavesQty != null && oum.LeavesQty > 0 && (oum.Side == "Buy" || oum.Side == "Sell"))
                                    {
                                        logger.Warn(" waiting partially params " + oum.Side + "|" + oum.LeavesQty.ToString());
                                        RobotOrders.Clear();
                                        Operations.Pop();
                                        DoUpdate = false;
                                        Operations.Push(new StateOperation(oum.Side == "Buy" ? State.Repeat_buy : State.Repeat_sell, (int)oum.LeavesQty));
                                            
                                    }
                                }
                                else if (RobotOrders.Where(x => (x.OrdStatus == "Canceled" || x.OrdStatus == "Rejected")).Count() == 1 && RobotOrders.Count == 1)
                                {
                                    logger.Warn(" waiting have one rejected or canceled");
                                    OrderUpdateModel cancledOrder = RobotOrders.Where(x => (x.OrdStatus == "Canceled" || x.OrdStatus == "Rejected")).First();
                                    if (cancledOrder != null)
                                    {
                                        string OrderSide = cancledOrder.Side != null ? cancledOrder.Side : "";
                                        int OrderQty = cancledOrder.OrderQty != null ? (int)cancledOrder.OrderQty : 0;
                                        logger.Warn(" waiting params " + OrderSide + "|"+ OrderQty.ToString());

                                        if ((OrderSide == "Buy" || OrderSide == "Sell") && OrderQty > 0)
                                        {
                                             logger.Warn(" waiting orders count = 0");
                                             Operations.Pop();
                                             DoUpdate = false;
                                             Operations.Push(new StateOperation(OrderSide.Contains("Buy") ? State.Repeat_buy : State.Repeat_sell, OrderQty));
                                             //Operations.Push(new StateOperation(State.Set_leverage, request_nums: 0, leverage: 0));
                                             RobotOrders.Clear();                                       
                                        }
                                    }
                                }
                            }
                            break;

                        case State.Set_leverage:
                            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Setting leverage" });
                            logger.Warn("State " + currState.CurrState.ToString());

                            if (ChangeLeverage("XBTUSD", Convert.ToDecimal(currState.Leverage)) || (currState.Request_nums <= 0 && currState.Leverage > 0))
                            {
                                Operations.Pop();
                                //Operations.Push(new StateOperation(State.Done));
                            }
                            else
                            {
                                currState.Request_nums--;
                            }

                            break;
                        case State.Done:
                            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Signal handled" });
                            logger.Warn("State " + currState.CurrState.ToString());
                            while (Operations.Count > 0)
                            {
                                if (Operations.Peek().CurrState == State.Start)
                                {
                                    currState = Operations.Peek();
                                    break;
                                }
                                Operations.Pop();
                            }
                            if (currState.CurrState == State.Start)
                            {
                                //завершаем
                                if (!currState.IsManuallyActivated)
                                {
                                    SetLastSignalDone();
                                }
                                else
                                {
                                    if (RobotBitmexAuthorization.AccountType == BitmexAccountTypeEnvironment.Main)
                                    {
                                        SendMessage(ParentThread, "CallBack_ActivateManual", "Activate.");
                                    }
                                }
                            }
                            else
                            {
                                if (RobotBitmexAuthorization.AccountType == BitmexAccountTypeEnvironment.Main)
                                {
                                    SendMessage(ParentThread, "CallBack_ActivateManual", "Activate.");
                                }
                            }
                            Operations.Clear();
                            Operations.Push(new StateOperation(State.None));
                            StopStateMachine();
                            break;
                    }
                }
                else
                {
                    StopStateMachine();
                }
            }


        }

        public bool DeleteAllOrders()
        {
            var OrderAllDELETERequest = OrderAllDELETERequestParams.GetOrderAllDELETERequest("XBTUSD", "", "");
            bool Executed = false;
            List<OrderDto> rez = _robotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.Order.DeleteOrderAll, OrderAllDELETERequest);
            return (Executed && rez != null) ? true : false;
        }

        public bool GetLastBidQuote()
        {
            bool Executed = false;
            OrderBookL2GETRequestParams getQuoteParams = OrderBookL2GETRequestParams.getOrderBookL2GETRequest("XBTUSD", 1);
            List<OrderBookDto> quote = _robotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.OrderBook.GetOrderBookL2, getQuoteParams);

            if (Executed && quote != null && quote.Count > 0)
            {
                if (quote.Where(x => x.Side == "Buy").Count() > 0)
                {
                    Last_Bid = (decimal)((double)quote.Where(x => x.Side == "Buy").First().Price);// - 0.5);
                    return true;
                }
                else if(quote.Where(x => x.Side == "Sell").Count() > 0)
                {
                    Last_Bid = (decimal)((double)quote.Where(x => x.Side == "Sell").First().Price);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool GetLastAskQuote()
        {
            bool Executed = false;
            OrderBookL2GETRequestParams getQuoteParams = OrderBookL2GETRequestParams.getOrderBookL2GETRequest("XBTUSD", 1);
            List<OrderBookDto> quote = _robotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.OrderBook.GetOrderBookL2, getQuoteParams);

            if (Executed && quote != null && quote.Count > 0)
            {
                if (quote.Where(x => x.Side == "Sell").Count() > 0)
                {
                    Last_Ask = (decimal)((double)quote.Where(x => x.Side == "Sell").First().Price);//.Price+0.5);
                    return true;
                }
                else if (quote.Where(x => x.Side == "Buy").Count() > 0)
                {
                    Last_Ask = (decimal)((double)quote.Where(x => x.Side == "Buy").First().Price);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool GetLastQuote()
        {
            var getQuoteParams = QuoteGETRequestParams.GetQuoteRequest("XBT", 1);
            List<QuoteDto> quote = _robotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.Quote.GetQuote, getQuoteParams);
            if (quote != null)
            {
                if (quote.Count > 0)
                {
                    if (quote.First().AskPrice != null) Last_Ask = (decimal)quote.First().AskPrice;
                    if (quote.First().BidPrice != null) Last_Bid = (decimal)quote.First().BidPrice;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool SendSellOrderLimit(int OrderSize)
        {
            if (GetLastAskQuote())
            {
                bool Executed = false;
                var posOrderParams = OrderPOSTRequestParams.CreateSimpleLimit("XBTUSD", OrderSize, Last_Ask, OrderSide.Sell);
                OrderDto order = _robotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.Order.PostOrder, posOrderParams);
                if (Executed && order != null && (order.OrdStatus == "New" || order.OrdStatus == "PartiallyFilled" || order.OrdStatus == "Filled"))
                {
                    lock (_syncObjRobotOrders)
                    {
                        OrderUpdateModel temp = Mapper.Map<OrderDto, OrderUpdateModel>(order);
                        RobotOrders.Insert(0, temp.DeepClone());
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool SendBuyOrderLimit(int OrderSize)
        {
            if (GetLastBidQuote())
            {
                bool Executed = false;
                var posOrderParams = OrderPOSTRequestParams.CreateSimpleLimit("XBTUSD", OrderSize, Last_Bid, OrderSide.Buy);
                OrderDto order = _robotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.Order.PostOrder, posOrderParams);
                if (Executed && order != null && (order.OrdStatus == "New" || order.OrdStatus == "PartiallyFilled" || order.OrdStatus == "Filled"))
                {
                    lock (_syncObjRobotOrders)
                    {
                        OrderUpdateModel temp = Mapper.Map<OrderDto, OrderUpdateModel>(order);
                        RobotOrders.Insert(0, temp.DeepClone());
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool ChangeLeverage(string symbol, decimal leverage)
        {
            var PositionLeveragePOSTRequest = PositionLeveragePOSTRequestParams.GetPositionPOSTRequest(symbol, leverage);
            bool Executed = false;
            PositionDto rez = _robotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.Position.PostPositionLeverage, PositionLeveragePOSTRequest);
            return (Executed && rez != null) ? true : false;
        }

        public bool UpdateBalance()
        {
            var UserMarginGETRequest = UserMarginGETRequestParams.GetUserMarginGETRequest("XBt");
            bool Executed = false;
            MarginDto MarginInfo = RobotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.User.GetUserMargin, UserMarginGETRequest);
            if (Executed && MarginInfo != null)
                if (MarginInfo.MarginBalance != null)
                {
                    Param_Amount = (decimal)MarginInfo.MarginBalance;
                    return true;
                }
            return false;
        }
        
        public void getAccountInfo()
        {
            getWallet();
            getMargin();
        }

        private void getWallet()
        {
            var UserWalletGETRequest = UserWalletGETRequestParams.GetUserWalletGETRequest("XBt");
            WalletInfo = RobotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.User.GetUserWallet, UserWalletGETRequest);
            UpdateAccountWalletInfo(WalletInfo);
        }

        private void getMargin()
        {
            var UserMarginGETRequest = UserMarginGETRequestParams.GetUserMarginGETRequest("XBt");
            MarginInfo = RobotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.User.GetUserMargin, UserMarginGETRequest);
            UpdateAccountMarginInfo(MarginInfo);
        }

        private void getPosData()
        {
            lock (_syncObjPosUpdate)
            {
                bool Executed = false;
                List<PositionDto> currPos = _robotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.Position.GetPosition, PositionGETRequestParams.GetPositionGetRequest(100));

                if (Executed && currPos != null)
                {
                    currentPos = ConvertPosData(currPos);
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "PositionInfo", RobotBitmexAuthorization.Id, new object[] { currentPos });
                }
            }
        }

        private List<PositionAdditionalUpdateModel> ConvertPosData(List<PositionDto> src)
        {
            List<PositionAdditionalUpdateModel> resevedPosData = new List<PositionAdditionalUpdateModel>();

            if (src != null)
            {
                foreach(var pos in src)
                    resevedPosData.Add(Mapper.Map<PositionDto, PositionAdditionalUpdateModel>(pos));

                foreach (var pos in resevedPosData) pos.UnrealisedPnl = (decimal)((double)pos.UnrealisedPnl * 0.00000001);
            }

            return resevedPosData;
        }

        public void getAccountHistoryInfo()
        {
            var UserWalletHistoryGETRequest = UserWalletHistoryGETRequestParams.UserWalletHistoryGETRequest("XBt");
            List<WalletHistoryDbo> whd = RobotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.User.GetUserWalletHistory, UserWalletHistoryGETRequest);

            var OrderGETRequest = OrderGETRequestParams.GetOrderGetRequest("XBTUSD", 500);
            List<OrderDto> od = RobotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.Order.GetOrder, OrderGETRequest);
            List<OrderHistoryModel> viewOrderHistoryModel = new List<OrderHistoryModel>();

            foreach (var item in od) viewOrderHistoryModel.Add(Mapper.Map<OrderDto, OrderHistoryModel>(item));

            if (RobotBitmexAuthorization.AccountType == BitmexAccountTypeEnvironment.Main)
            {
                SendMessage(ParentThread, "CallBack_HistoryInfo", viewOrderHistoryModel, whd);
            }
        }

        public void getApiKeysInfo()
        {
            var ApiKeyGETRequest = ApiKeyGETRequestParams.GetApiKeyGETRequest();
            bool Executed = false;
            RobotApikeys = RobotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.APIKey.GetApiKey, ApiKeyGETRequest);

            if (Executed && RobotApikeys != null)
            {
                SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "ApiKeysInfo", RobotBitmexAuthorization.Id, new object[] { RobotApikeys });
            }
        }

        private void UpdateAccountWalletInfo(WalletDto wallet)
        {
            if (wallet != null) if (wallet.Account != null) AccountNumber = (decimal)wallet.Account;
            //if (wallet != null) if (wallet.Amount != null) Param_Amount = (decimal)wallet.Amount;
            if (wallet != null)
                SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "WalletInfo", RobotBitmexAuthorization.Id, new object[] { wallet });
        }

        private void UpdateAccountMarginInfo(MarginDto margin)
        {
            if (margin != null) if (margin.MarginBalance != null) Param_Amount = (decimal)margin.MarginBalance;
            if (margin != null)
                SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "MarginInfo", RobotBitmexAuthorization.Id, new object[] { margin });
        }

        public void UpdateCandlesAndResultTable(List<CalculationResult> resultTable)
        {
            CurrentResultTable = resultTable;
            if (TradeIsAllowed) ActivateWork();
        }

        public void ActivateAutotrading(int size, double leverage, bool isOnlyBuy)
        {
            TradeIsAllowed = true;
            Size = size;
            Leverage = leverage;
            TradeOnlyBuy = isOnlyBuy;
            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "AutoTrading is activated." });
            ActivateWork();
        }

        public void ActivateAutotrading(bool isOnlyBuy)
        {
            TradeIsAllowed = true;
            TradeOnlyBuy = isOnlyBuy;
            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "AutoTrading is activated." });
            ActivateWork();
        }

        public void DeactivateAutotrading()
        {
            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "AutoTrading is deactivating.." });
            TradeIsAllowed = false;
            OrdersRemovedAfterStop = false;
            Operations.Clear();
            Operations.Push(new StateOperation(State.None));
            StopThread(OrderUpdateChekerThread);
            StopStateMachine();
            StartThread("DeactivatingRobotThread");
        }

        private void StartDeactivatingThread(object info)
        {
            while (true)
            {
                if (!OrdersRemovedAfterStop)
                {
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Removing orders.." });
                    OrdersRemovedAfterStop = DeleteAllOrders();
                }
                else
                {
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "AutoTrading is deactivated." });
                    break;
                }
                Thread.Sleep(new TimeSpan(0, 0, 2));
            }
        }

        public void TerminateRobot()
        {
            //StopStateMachine();
            StopThread(WorkerThread);
            //StopThread(OrderUpdateChekerThread);

            StopThread(OrderUpdateChekerThread);
            StopThread(ReconnectThread);
            StopThread(ApiKeyInfoThread);
            StopThread(AccountInitThread);           
            StopThread(DeactivatingRobotThread);
            StopThread(CheckRestThread);
            StopThread(CheckSocketSubscribeThread);
            StopThread(UpdateAccountThread);

            RobotBitmexApiSocketService.Dispose();
        }

        private string GetLastUnhandledSignal()
        {
            if (CurrentResultTable.Count > 2)
            {
                if (CurrentResultTable[0].Signal == "Buy" && CurrentResultTable[0].SignalHandler != "Handled") return "Buy";
                if (CurrentResultTable[0].Signal == "Sell" && CurrentResultTable[0].SignalHandler != "Handled") return "Sell";
            }
            return "none";
        }

        private void SetLastSignalDone()
        {
            if ((CurrentResultTable[0].Signal == "Buy" || CurrentResultTable[0].Signal == "Sell") &&
                CurrentResultTable[0].SignalHandler != "Handled")
            {
                CurrentResultTable[0].SignalHandler = "Handled";
            }
        }

        public void ActivateWork(string manualSignal = "", int size = 0, double leverage = 0)
        {
            if (manualSignal != "")
            {//A
                Size = size;
                Leverage = leverage;
            }

            if ((GetLastUnhandledSignal() == "Buy" && manualSignal == "") || manualSignal == "Buy")
            {
                Operations.Clear();
                Operations.Push(new StateOperation(State.Start, operationName: "Buy", isManuallyActivated: manualSignal == "Buy" ? true : false));
                //Operations.Push(new StateOperation(State.Repeat_dell_all_orders));
                //Operations.Push(new StateOperation(State.Set_leverage, request_nums: 0, leverage: 0));
                StartStateMachine();
            }
            else if ((GetLastUnhandledSignal() == "Sell" && manualSignal == "") || manualSignal == "Sell")
            {
                Operations.Clear();
                Operations.Push(new StateOperation(State.Start, operationName: "Sell", isManuallyActivated: manualSignal == "Sell" ? true : false));
                //Operations.Push(new StateOperation(State.Repeat_dell_all_orders));
                //Operations.Push(new StateOperation(State.Set_leverage, request_nums: 0, leverage: 0));
                StartStateMachine();
            }
        }

        private void StartStateMachine()
        {
            UpdateMonitoring = false;
            DoUpdate = false;
            StateMachineEnabled = true;
            //StopThread(WorkerThread);
            StartThread("WorkerThread");
            StartThread("OrderUpdateChekerThread");
        }

        private void StopStateMachine()
        {
            UpdateMonitoring = true;
            DoUpdate = false;
            StateMachineEnabled = false;
            //StopThread(WorkerThread);
            //StopThread(OrderUpdateChekerThread);
        }

        private void StartWork(object src)
        {
            while (StateMachineEnabled)
            {
                //Thread.Sleep(new TimeSpan(0, 0, 1));
                StateMachine();
            }
        }

        private void StartOrderUpdateChekerThread(object info)
        {
            while (StateMachineEnabled)
            {
                UpdateOrders();
                Thread.Sleep(new TimeSpan(0, 0, 1));
            }
        }

        private void StartApiKeyInfoThread(object info)
        {
            while (true)
            {
                if (UpdateMonitoring)
                {
                    getApiKeysInfo();
                    Thread.Sleep(new TimeSpan(0, 30, 0));
                }
                else
                {
                    Thread.Sleep(new TimeSpan(0, 0, 30));
                }
            }
        }

        private void StartInitRobotThread(object info)
        {
            while (true)
            {
                SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Loading account data.." });
                if (AccountNumber == 0 || Param_Amount == 0)
                {
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Loading wallet info.." });
                    getAccountInfo();
                }                
                /*if (Param_Amount == 0)
                {
                    SendMessage(ParentThread, ObjectApi["CallBack_UpdateAdditionalAccounts"], "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Loading balance info.." });
                    getMargin();
                }*/
                if (CurrentOrdersInWork == null)
                {
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Loading order's info.." });
                    UpdateOrders(true);
                }
                if (RobotApikeys == null)
                {
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Loading apikey info.." });
                    getApiKeysInfo();
                }
                if (currentPos == null)
                {
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Loading position info.." });
                    getPosData();
                }
                if (!OrdersRemovedBeforeStart) {
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Remove orders before starting.." });
                    OrdersRemovedBeforeStart = DeleteAllOrders();
                }
                if (!LeverageSetBeforeStart)
                {
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Set margin cross before starting.." });
                    LeverageSetBeforeStart = ChangeLeverage("XBTUSD", 0);
                }

                if (Param_Amount > 0 && CurrentOrdersInWork != null && RobotApikeys != null && currentPos != null && OrdersRemovedBeforeStart && AccountNumber > 0 && LeverageSetBeforeStart)
                {//Account Inited
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "LastActionInfo", RobotBitmexAuthorization.Id, new[] { "Account is ready (Data is loaded.)" });
                    StartThread("ApiKeyInfoThread");
                    StartThread("UpdateAccountThread");
                    break;
                }
                Thread.Sleep(new TimeSpan(0, 0, 2));
            }
        } 

        private void UpdateOrders(bool forceExecute = false)
        {
            lock (_syncObjCurrentOrders)
            {
                lock (_syncObjRobotOrders)
                {                   

                    if (DoUpdate || forceExecute)
                    {
                        DateTime? startFilterTime = null;
                        if (RobotOrders.Count() > 0) startFilterTime = RobotOrders.Min(x => x.Timestamp).UtcDateTime;
                        OrderGETRequestParams GetOrderGetRequestParams;

                        if (startFilterTime != null)
                        {
                            GetOrderGetRequestParams = OrderGETRequestParams.GetOrderGetRequest("XBTUSD", 100, startTime: startFilterTime);
                        }
                        else
                        {
                            GetOrderGetRequestParams = OrderGETRequestParams.GetOrderGetRequest("XBTUSD", 100);
                            GetOrderGetRequestParams.Filter = new Dictionary<string, string> { { "open", "true" } };
                        }

                        bool Executed = false;
                        List<OrderDto> od = RobotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.Order.GetOrder, GetOrderGetRequestParams);

                        if (Executed && od != null)
                        {
                            if (od.Count() > 0)
                            {
                                foreach (var item in RobotOrders)
                                {
                                    if (od.Where(x => x.OrderId == item.OrderId).Count() > 0)
                                    {
                                        OrderDto updated = od.Where(x => x.OrderId == item.OrderId).First();
                                        if (updated != null)
                                        {
                                            if (updated.OrdStatus != null && updated.LeavesQty != null)
                                            {
                                                item.OrdStatus = updated.OrdStatus;
                                                item.LeavesQty = updated.LeavesQty;
                                            }
                                        }
                                    }
                                }
                            }

                            od = od.Where(x => ((x.OrdStatus == "New" || x.OrdStatus == "PartiallyFilled") && x.OrdType == "Limit")).ToList();

                            counter++;                            
                            if (od.Count > 0 && counter % 3 == 0)
                            {
                                foreach (var order in od)
                                {
                                    if (order.Side == "Buy")
                                    {
                                        if (GetLastBidQuote())
                                        {
                                            if (Last_Bid > order.Price && Last_Bid > 0)
                                            {//Если цена повысилась
                                                var param = OrderPUTRequestParams.GetOrderPutRequest(order.OrderId, Last_Bid);
                                                OrderDto response = _robotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.Order.PutOrder, param);
                                            }
                                        }
                                    }
                                    else if (order.Side == "Sell")
                                    {
                                        if (GetLastAskQuote())
                                        {
                                            if (Last_Ask < order.Price && Last_Ask > 0)
                                            {//Если цена понизилась 
                                                var param = OrderPUTRequestParams.GetOrderPutRequest(order.OrderId, Last_Ask);
                                                _robotBitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.Order.PutOrder, param);
                                            }
                                        }
                                    }
                                }
                            }
                            if (counter > 1000) counter = 0;

                            List<OrderUpdateModel> buf = new List<OrderUpdateModel>();
                            foreach (var order in od) { buf.Add(Mapper.Map<OrderDto, OrderUpdateModel>(order)); }
                            CurrentOrdersInWork = buf;
                            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "OrderInfo", RobotBitmexAuthorization.Id, new object[] { CurrentOrdersInWork });
                        }


                    }

                }
            }
            
        }

        private void SendMessage(Dispatcher target, string targetFuncStr, params object[] args)
        {
            if (target != null)
            {
                if (ObjectApi.ContainsKey(targetFuncStr))
                {
                    Delegate targetFunc = ObjectApi[targetFuncStr];

                    string KeyMethod = "";

                    string dopParam = "";
                    if (args != null)
                    {
                        if (args.Count() > 0 && args[0] != null) dopParam += args[0].ToString();
                        if (args.Count() > 1 && args[1] != null) dopParam += args[1].ToString();
                        if (args.Count() > 0 && args[0] != null && "LastActionInfo" == args[0].ToString())
                        {
                            if (args.Count() > 2 && args[2] != null)
                            {
                                try
                                {
                                    string[] d = (string[])args[2];
                                    dopParam += d[0].ToString();
                                }
                                catch (Exception ex)
                                {
                                    string exd = ex.Message;
                                }
                            }
                        }
                    }

                    KeyMethod = targetFunc.Method.Name + "_" + dopParam;

                    if (NeedToCheckFrequncy(KeyMethod))
                    {
                        if (!Frequncy.ContainsKey(KeyMethod))
                        {
                            Frequncy.Add(KeyMethod, DateTime.UtcNow);
                            if (Frequncy.Count > 1000) Frequncy.Clear();
                            target.BeginInvoke(targetFunc, args);
                        }
                        else
                        {
                            if ((DateTime.UtcNow - Frequncy[KeyMethod]).TotalSeconds >= 2)
                            {//Ограничим частоту обновления
                                Frequncy[KeyMethod] = DateTime.UtcNow;
                                target.BeginInvoke(targetFunc, args);
                            }
                        }
                    }
                    else
                    {
                        target.BeginInvoke(targetFunc, args);
                    }
                }
            }
        }

        private bool NeedToCheckFrequncy(string Method)
        {
            if (
                Method.Contains("CallBack_OnConnectionMessage") ||
                Method.Contains("CallBack_UpdateAdditionalAccounts_ConnectionInfo") ||
                Method.Contains("CallBack_OrdersInfo")                
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void CallBack_ConnectionError(string message)
        {
            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "ConnectionInfo", RobotBitmexAuthorization.Id, new[] { "Connection is lost..." });

            isSubscribed["CreateOrderSubsription"] = false;
            isSubscribed["CreateMarginSubscription"] = false;
            isSubscribed["CreateWalletSubscription"] = false;
            isSubscribed["CreatePositionSubsription"] = false;

            StartThread("ReconnectThread");
        }

        private void CallBack_RestError(string[] message)
        {           
            logger.Warn("Status Code: " + message[0] + " " + (message.Count() >= 4 ? message[4] : " ") + " " + (message.Count() >= 3 ? message[3] : " "));

            SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "ConnectionRestInfo", RobotBitmexAuthorization.Id, new[] { message });

            /*if (message[0] != System.Net.HttpStatusCode.OK.ToString())
            {//start check Rest
                StartThread(CheckRestThread);
            }
            else
            {//stop check
                //StopThread(CheckRestThread);
            }*/
        }

        private void StartCheckRestRequestThread(object info)
        {
            while (true)
            {
                getWallet();
                Thread.Sleep(new TimeSpan(0, 0, 20));
            }
        }

        private void Reconnecting(object info)
        {
            while (true)
            {
                IsConnected = RobotBitmexApiSocketService.Connect();

                if (IsConnected == "True")
                {
                    //StartThread("CheckSocketSubscribeThread"); 
                    logger.Warn("Reconnected to server.");
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "ConnectionInfo", RobotBitmexAuthorization.Id, new[] { "Connection is ok." });
                    StopThread(ReconnectThread);
                    break;
                }                
                else if (IsConnected == "False")
                {
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "ConnectionInfo", RobotBitmexAuthorization.Id, new[] { "Reconnecting..." });
                }
                else
                {
                    SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "ConnectionInfo", RobotBitmexAuthorization.Id, new[] { IsConnected });
                }

                Thread.Sleep(new TimeSpan(0, 0, 10));
            }            
        }
        
        public void SetAccountDispatcherAndDelegate(Dispatcher target, Delegate targetFunc)
        {
            ParentThread = target;
            if (!ObjectApi.ContainsKey(targetFunc.Method.Name))
            {
                ObjectApi.Add(targetFunc.Method.Name, targetFunc);
            }
            else
            {
                ObjectApi.Remove(targetFunc.Method.Name);
                ObjectApi.Add(targetFunc.Method.Name, targetFunc);
            }
        }

        private void StartUpdateAccount(object info)
        {
            while (true)
            {
                if (UpdateMonitoring)
                {
                    getPosData();
                    getAccountInfo();

                    OrderGETRequestParams GetOrderGetRequestParams = OrderGETRequestParams.GetOrderGetRequest("XBTUSD", 100);
                    GetOrderGetRequestParams.Filter = new Dictionary<string, string> { { "open", "true" } };
                    bool Executed = false;
                    List<OrderDto> od = RobotBitmexApiService.ExecuteSyncErrorHandlerNew(out Executed, BitmexApiUrls.Order.GetOrder, GetOrderGetRequestParams);
                    if (Executed && od != null)
                    {
                        List<OrderUpdateModel> buf = new List<OrderUpdateModel>();
                        foreach (var order in od)
                        {
                            buf.Add(Mapper.Map<OrderDto, OrderUpdateModel>(order));
                        }

                        CurrentOrdersInWork = buf;
                        SendMessage(ParentThread, "CallBack_UpdateAdditionalAccounts", "OrderInfo", RobotBitmexAuthorization.Id, new object[] { CurrentOrdersInWork });
                    }
                    Thread.Sleep(new TimeSpan(0, 0, 30));
                }
                else
                {
                    Thread.Sleep(new TimeSpan(0, 0, 5));
                }
            }
        }

        private void StartThread(string Name)
        {
            try
            {
                switch (Name)
                {
                    case "UpdateAccountThread":
                        if ((UpdateAccountThread == null) || (UpdateAccountThread != null && !UpdateAccountThread.IsAlive))
                        {
                            UpdateAccountThread = new Thread(StartUpdateAccount);
                            UpdateAccountThread.IsBackground = true;
                            UpdateAccountThread.Start();
                        }
                        break;
                    case "ReconnectThread":
                        if ((ReconnectThread == null) || (ReconnectThread != null && !ReconnectThread.IsAlive))
                        {
                            ReconnectThread = new Thread(Reconnecting);
                            ReconnectThread.IsBackground = true;
                            ReconnectThread.Start();
                        }
                        break;
                    case "CheckRestThread":
                        if ((CheckRestThread == null) || (CheckRestThread != null && !CheckRestThread.IsAlive))
                        {
                            CheckRestThread = new Thread(StartCheckRestRequestThread);
                            CheckRestThread.IsBackground = true;
                            CheckRestThread.Start();
                        }
                        break;
                    case "AccountInitThread":
                        if ((AccountInitThread == null) || (AccountInitThread != null && !AccountInitThread.IsAlive))
                        {
                            AccountInitThread = new Thread(StartInitRobotThread);
                            AccountInitThread.IsBackground = true;
                            AccountInitThread.Start();
                        }
                        break;
                    case "ApiKeyInfoThread":
                        if ((ApiKeyInfoThread == null) || (ApiKeyInfoThread != null && !ApiKeyInfoThread.IsAlive))
                        {
                            ApiKeyInfoThread = new Thread(StartApiKeyInfoThread);
                            ApiKeyInfoThread.IsBackground = true;
                            ApiKeyInfoThread.Start();
                        }
                        break;
                    case "OrderUpdateChekerThread":
                        if ((OrderUpdateChekerThread == null) || (OrderUpdateChekerThread != null && !OrderUpdateChekerThread.IsAlive))
                        {
                            OrderUpdateChekerThread = new Thread(StartOrderUpdateChekerThread);
                            OrderUpdateChekerThread.IsBackground = true;
                            OrderUpdateChekerThread.Start();
                        }
                        break;
                    case "WorkerThread":
                        if ((WorkerThread == null) || (WorkerThread != null && !WorkerThread.IsAlive))
                        {
                            WorkerThread = new Thread(StartWork);
                            WorkerThread.IsBackground = true;
                            WorkerThread.Start();
                        }
                        break;
                    case "DeactivatingRobotThread":
                        if ((DeactivatingRobotThread == null) || (DeactivatingRobotThread != null && !DeactivatingRobotThread.IsAlive))
                        {
                            DeactivatingRobotThread = new Thread(StartDeactivatingThread);
                            DeactivatingRobotThread.IsBackground = true;
                            DeactivatingRobotThread.Start();
                        }
                        break;
                    case "CheckSocketSubscribeThread":
                        if ((CheckSocketSubscribeThread == null) || (CheckSocketSubscribeThread != null && !CheckSocketSubscribeThread.IsAlive))
                        {
                            CheckSocketSubscribeThread = new Thread(CheckSocketSubscribe);
                            CheckSocketSubscribeThread.IsBackground = true;
                            CheckSocketSubscribeThread.Start();
                        }
                        break;
                    default:
                        string d = "";
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Thread start error: " + ex.Message);
            }
        }

        private void StopThread(Thread targetThread)
        {
            try
            {
                if (targetThread != null) {
                    targetThread.Abort();
                    targetThread.Join();
                }
            }
            catch (Exception ex)
            {
                if (targetThread != null)
                logger.Warn("Thread stop error: "+ targetThread.Name + " " + ex.Message);
            }
        }
        public void SetOnlyBuy(bool isOnlyBuy)
        {
            TradeOnlyBuy = isOnlyBuy;
        }
    }
}
