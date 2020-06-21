using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bitmex.NET;
using Bitmex.NET.Models;
using Bitmex.NET.Dtos;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using NLog;
using System.Windows.Data;
using BitMexLibrary.Automapping;
using Npgsql;

namespace BitMexLibrary
{
    public class Background_worker
    {
        public Logger logger = LogManager.GetCurrentClassLogger();
        Thread thread;
        private readonly IBitmexApiService _bitmexApiService;
        private readonly IBitmexApiSocketService _bitmexApiSocketService;
        private readonly object _syncObj = new object();
        private List<TradeBucketedDto> _candleBuffer;
        private List<CalculationResult> _currentResultTable;
        private int _numForCalc;
        private string _binSize;
        public int CountForCalculation { get => _numForCalc; set => _numForCalc = value; }
        public string BinSize { get => _binSize; set => _binSize = value; }
        public List<TradeBucketedDto> CandleBuffer { get => _candleBuffer; set => _candleBuffer = value; }
        public List<CalculationResult> CurrentResultTable { get => _currentResultTable; set => _currentResultTable = value; }

        private Dispatcher parentThread;
        public Dispatcher ParentThread { get => parentThread; set => parentThread = value; }
        public Delegate TargetFunc;
        public Delegate TargetFuncUpdateQuotes;
        public Timer t;

        public DateTime LastUpdate = DateTime.UtcNow;
        public List<OrderBookModel> OrderBook10 { get; private set; }

        private string _connectionString;
        public string ConnectionString { get => _connectionString; set => _connectionString = value; }

        private readonly object _syncObjOrderBook10 = new object();

        public DateTime historyStartTime;

        public Background_worker(string conn, IBitmexApiService bitmexApiService, IBitmexApiSocketService bitmexApiSocketService, Dispatcher target, Delegate targetFunc, Delegate targetFuncUpdateQuotes)
        {
            _bitmexApiService = bitmexApiService;
            _bitmexApiSocketService = bitmexApiSocketService;
            parentThread = target;
            TargetFunc = targetFunc;
            TargetFuncUpdateQuotes = targetFuncUpdateQuotes;

            //устанавливаем, обнуяем параметры
            CandleBuffer = new List<TradeBucketedDto>();
            BindingOperations.EnableCollectionSynchronization(CandleBuffer, _syncObj);

            historyStartTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);
            historyStartTime = historyStartTime.AddDays(-3);

            ConnectionString = conn;
        }

        public void StartDataReader(string name, int numForCalc, string binSize)
        {
            logger.Debug("Start reader at " + DateTime.UtcNow.ToString());

            CountForCalculation = numForCalc;
            BinSize = binSize;

            if (binSize == "1m")
            {
                //thread = new Thread(this.ReadDataM1);
                thread = new Thread(this.ReadData_m1);
            }
            else if (binSize == "1h")
            {
                thread = new Thread(this.ReadData_h1);
            }

            thread.Name = name; 
            thread.IsBackground = true;
            thread.Start();
        }

        public void RestartDataReader(string name, int numForCalc, string binSize)
        {
            StopDataReader();
            CandleBuffer.Clear();           
            CountForCalculation = numForCalc;
            BinSize = binSize;
            CurrentResultTable = new List<CalculationResult>();

            logger.Debug("Restart reader at " + DateTime.UtcNow.ToString());            

            if (binSize == "1m" || binSize == "3m")
            {
                thread = new Thread(this.ReadData_m1);
            }
            else if (binSize == "1h")
            {
                thread = new Thread(this.ReadData_h1);
            }

            thread.Name = name;
            thread.IsBackground = true;
            thread.Start();
        }

        public void StopDataReader()
        {
            logger.Debug("Stopt reader at " + DateTime.UtcNow.ToString());
            if (thread != null) { thread.Abort(); thread.Join(); }
        }

        /*
        private void ReadDataH1()
        {
            try
            {
                if (_bitmexApiSocketService.Actions.ContainsKey(BitmetSocketSubscriptions.CreateTradeBucket1HSubsription(message => { }).SubscriptionName))
                                        _bitmexApiSocketService.Unsubscribe(BitmetSocketSubscriptions.CreateTradeBucket1HSubsription(message => { }));
            }
            catch(Exception ex)
            {
                logger.Debug("Unsubscribe error." + ex.Message);
            }

            try
            {
                if (_bitmexApiSocketService.Actions.ContainsKey(BitmetSocketSubscriptions.CreateOrderBook10Subsription(message => { }).SubscriptionName))
                                       _bitmexApiSocketService.Unsubscribe(BitmetSocketSubscriptions.CreateOrderBook10Subsription(message => { }));
                //_bitmexApiSocketService.Unsubscribe(BitmetSocketSubscriptions.CreateOrderBook10Subsription(message => { }));
            }
            catch(Exception ex)
            {
                logger.Debug("Unsubscribe error." + ex.Message);               
            }

             _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateTradeBucket1HSubsription(
             message =>
             {
                 foreach (var dto in message.Data)
                 {//Добавляем последнюю свечу
                     lock (_syncObj)
                     {
                         logger.Debug("New candle recieved " + DateTime.UtcNow.ToString());
                         if (CandleBuffer.Count > 0)
                         {
                             if (dto.Timestamp != null)
                             {
                                 TimeSpan ts = dto.Timestamp.DateTime - CandleBuffer.Last().Timestamp.DateTime;
                                 if (ts.TotalHours >= 1)
                                 {//если значение свечи меньше на 1 час
                                     logger.Debug("New candle added " + DateTime.UtcNow.ToString());
                                     CandleBuffer.Add(dto);
                                     logger.Debug("Send for calc " + DateTime.UtcNow.ToString());
                                     sendForCalc();
                                 }
                             }
                             else
                             {
                                 logger.Debug("dto.Timestamp is null " + DateTime.UtcNow.ToString());
                             }
                         }
                         else
                         {
                             logger.Debug("Init reading... " + DateTime.UtcNow.ToString());
                             readHistory();
                         }
                     }
                 }
             }));

            _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateOrderBook10Subsription(
                    message =>
                    {
                        foreach (var dto in message.Data)
                        {//Читаем последние
                            lock (_syncObjOrderBook10)
                            {
                                OrderBook10 = dto.Asks.Select(a =>
                                   new OrderBookModel { Direction = "Sell", Price = a[0], Size = a[1] }).OrderByDescending(x => x.Price)
                                      .Union(dto.Bids.Select(a =>
                                        new OrderBookModel { Direction = "Buy", Price = a[0], Size = a[1] }).OrderByDescending(x => x.Price)).ToList();

                                if ((DateTime.UtcNow - LastUpdate).TotalSeconds >= 2)
                                {//Ограничим частоту обновления
                                    LastUpdate = DateTime.UtcNow;
                                    parentThread.BeginInvoke(TargetFuncUpdateQuotes, OrderBook10);
                                }
                            }
                        }
                    }));

            MainFunc();
        }

     private void ReadDataM1()
        {
            _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateTradeBucket1MSubsription(
             message =>
             {
                 foreach (var dto in message.Data)
                 {
                     lock (_syncObj)
                     {
                         if (CandleBuffer.Count == 0)
                         {//Если список пустой то грузим историю
                             readHistory_1m();
                         }
                         else
                         {
                             //Добавляем последнюю свечу
                             TimeSpan ts = dto.Timestamp.DateTime - CandleBuffer.Last().Timestamp.DateTime;
                             if (ts.TotalMinutes >= 1)
                             {//если предыдущая меньше на 1 мин
                                 CandleBuffer.Add(dto);
                                 sendForCalc();
                             }
                         }
                     }
                 }
             }));
        }*/

        private void ReadData_h1()
        {
            try
            {
                if (_bitmexApiSocketService.Actions.ContainsKey(BitmetSocketSubscriptions.CreateTradeBucket1HSubsription(message => { }).SubscriptionName))
                    _bitmexApiSocketService.Unsubscribe(BitmetSocketSubscriptions.CreateTradeBucket1HSubsription(message => { }));
            }
            catch (Exception ex)
            {
                logger.Debug("Unsubscribe error." + ex.Message);
            }

            try
            {
                if (_bitmexApiSocketService.Actions.ContainsKey(BitmetSocketSubscriptions.CreateOrderBook10Subsription(message => { }).SubscriptionName))
                    _bitmexApiSocketService.Unsubscribe(BitmetSocketSubscriptions.CreateOrderBook10Subsription(message => { }));
            }
            catch (Exception ex)
            {
                logger.Debug("Unsubscribe error." + ex.Message);
            }

            readHistory_1h();
            //GetCandlesForCalc(BinSize, CountForCalculation);

            _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateTradeBucket1HSubsription(
            message =>
            {
                foreach (var dto in message.Data)
                {//Добавляем последнюю свечу
                    lock (_syncObj)
                    {
                        readHistory_1h();
                    }
                }
            }));

            MainFunc();

            _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateOrderBook10Subsription(
            message =>
            {
                foreach (var dto in message.Data)
                {//Читаем последние
                                lock (_syncObjOrderBook10)
                    {
                        OrderBook10 = dto.Asks.Select(a =>
                           new OrderBookModel { Direction = "Sell", Price = a[0], Size = a[1] }).OrderByDescending(x => x.Price)
                              .Union(dto.Bids.Select(a =>
                                new OrderBookModel { Direction = "Buy", Price = a[0], Size = a[1] }).OrderByDescending(x => x.Price)).ToList();

                        if ((DateTime.UtcNow - LastUpdate).TotalSeconds >= 2)
                        {//Ограничим частоту обновления
                                        LastUpdate = DateTime.UtcNow;
                            parentThread.BeginInvoke(TargetFuncUpdateQuotes, OrderBook10);
                        }
                    }
                }
            }));
        }

        private void ReadData_m1()
        {
            try
            {
                if (_bitmexApiSocketService.Actions.ContainsKey(BitmetSocketSubscriptions.CreateTradeBucket1MSubsription(message => { }).SubscriptionName))
                    _bitmexApiSocketService.Unsubscribe(BitmetSocketSubscriptions.CreateTradeBucket1MSubsription(message => { }));
            }
            catch (Exception ex)
            {
                logger.Debug("Unsubscribe error." + ex.Message);
            }

            try
            {
                if (_bitmexApiSocketService.Actions.ContainsKey(BitmetSocketSubscriptions.CreateOrderBook10Subsription(message => { }).SubscriptionName))
                    _bitmexApiSocketService.Unsubscribe(BitmetSocketSubscriptions.CreateOrderBook10Subsription(message => { }));
            }
            catch (Exception ex)
            {
                logger.Debug("Unsubscribe error." + ex.Message);
            }

            readHistory_1m();
            //GetCandlesForCalc(BinSize, CountForCalculation);

            _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateTradeBucket1MSubsription(
             message =>
             {
                 foreach (var dto in message.Data)
                 {
                     lock (_syncObj)
                     {
                         readHistory_1m();
                     }
                 }
             }));

            MainFunc();

            _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateOrderBook10Subsription(
            message =>
            {
                foreach (var dto in message.Data)
                {//Читаем последние
                                lock (_syncObjOrderBook10)
                    {
                        OrderBook10 = dto.Asks.Select(a =>
                           new OrderBookModel { Direction = "Sell", Price = a[0], Size = a[1] }).OrderByDescending(x => x.Price)
                              .Union(dto.Bids.Select(a =>
                                new OrderBookModel { Direction = "Buy", Price = a[0], Size = a[1] }).OrderByDescending(x => x.Price)).ToList();

                        if ((DateTime.UtcNow - LastUpdate).TotalSeconds >= 2)
                        {//Ограничим частоту обновления
                                        LastUpdate = DateTime.UtcNow;
                            parentThread.BeginInvoke(TargetFuncUpdateQuotes, OrderBook10);
                        }
                    }
                }
            }));
        }

        public DateTime getLastCandle(string timeframe)
        {
            using (var DbConnection = new NpgsqlConnection(ConnectionString))
            {
                DbConnection.Open();

                // Retrieve all rows
                using (var cmd = new NpgsqlCommand("SELECT timestamp_candle FROM public." + '"' + "XBTUSD_"+ timeframe + '"' + " order by timestamp_candle desc limit 1;", DbConnection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            historyStartTime = reader.GetTimeStamp(0).ToDateTime();
                        }
                    }
                    else
                    {
                        historyStartTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);
                        if (timeframe == "1m")
                        {
                            historyStartTime = historyStartTime.AddDays(-3);
                        }
                        else if (timeframe == "1h")
                        {
                            historyStartTime = historyStartTime.AddDays(-60);
                        }
                    }
                }
            }

            return historyStartTime;
        }
        private void readHistory_1m()
        {
            //считываем с последней точки до текущего времени
            DateTime StartTime = getLastCandle("1m");
            DateTime currMin = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);
            TimeSpan ts_1m = currMin - StartTime;
            while (ts_1m.TotalMinutes > 0)
            {
                int candlesCount = ts_1m.TotalMinutes >= 720 ? 720 : ((int)ts_1m.TotalMinutes > 0 ? ((int)ts_1m.TotalMinutes + 1) : 1);

                var tradeBucketedParams = TradeBucketedGETRequestParams.GetCandleHistory("XBTUSD", /*720*/ candlesCount, "1m", startTime: StartTime);
                List<TradeBucketedDto> CandleResponse = _bitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.Trade.GetTradeBucketed, tradeBucketedParams);

                if (CandleResponse != null && CandleResponse.Count > 0)
                {
                    //Save candels to db
                    saveCandles(CandleResponse, "1m");

                    StartTime = getLastCandle("1m");
                    currMin = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);
                    ts_1m = currMin - StartTime;

                    DateTime StartTime_3m = getLastCandle("3m");
                    TimeSpan ts_3m = currMin - StartTime_3m;

                    /*if ((ts_1m.TotalMinutes == 0 && BinSize == "1m") || (((currMin.Minute) % 3 == 0) && BinSize == "3m") ||
                            ( CurrentResultTable.Count == 0 && ( BinSize == "1m" || BinSize == "3m")))
                    {//Если получили последнюю свечу или пустая таблица с расчетом
                        GetCandlesForCalc(BinSize, CountForCalculation);
                    }*/
                }
                else
                {
                    logger.Debug("Error in reading history, empty response.");
                }
            }


            if ((ts_1m.TotalMinutes == 0 && BinSize == "1m") || (((currMin.Minute) % 3 == 0) && BinSize == "3m") ||
                       (CurrentResultTable.Count == 0 && (BinSize == "1m" || BinSize == "3m")))
            {//Если получили последнюю свечу или пустая таблица с расчетом
                GetCandlesForCalc(BinSize, CountForCalculation);
            }
        }

        private void readHistory_1h()
        {
            //считываем с последней точки до текущего времени
            DateTime StartTime = getLastCandle("1h");            
            DateTime currMin = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0);
            TimeSpan ts_1h = currMin - StartTime;

            while(ts_1h.TotalHours > 0)
            {
                int candlesCount = ts_1h.TotalHours >= 720 ? 720 : ( (int)ts_1h.TotalHours > 0 ? ((int)ts_1h.TotalHours + 1) : 1);

                var tradeBucketedParams = TradeBucketedGETRequestParams.GetCandleHistory("XBTUSD", /*720*/ candlesCount, "1h", startTime: StartTime);
                List <TradeBucketedDto> CandleResponse = _bitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.Trade.GetTradeBucketed, tradeBucketedParams);

                if (CandleResponse != null && CandleResponse.Count > 0)
                {
                    //Save candels to db
                    saveCandles(CandleResponse, "1h");

                    StartTime = getLastCandle("1h");
                    currMin = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0);
                    ts_1h = currMin - StartTime;

                    /*if ( (ts_1h.TotalHours == 0 && BinSize == "1h") || ( CurrentResultTable.Count == 0 && BinSize == "1h"))
                    {//Если получили последнюю свечу или пустая таблица с расчетом
                        GetCandlesForCalc(BinSize, CountForCalculation);
                    }*/
                }
                else
                {
                    logger.Debug("Error in reading history, empty response.");
                }
            }

            if ((ts_1h.TotalHours == 0 && BinSize == "1h") || (CurrentResultTable.Count == 0 && BinSize == "1h"))
            {//Если получили последнюю свечу или пустая таблица с расчетом
                GetCandlesForCalc(BinSize, CountForCalculation);
            }
        }

        public void saveCandles(List<TradeBucketedDto> CandleResponse, string timeframe)
        {
            using (var DbConnection = new NpgsqlConnection(ConnectionString))
            {
                try { DbConnection.Open(); } catch (Exception ex) { logger.Debug("Connection db error..." + ex.Message); }

                try
                {
                    foreach (var candle in CandleResponse)
                    {
                        using (var cmd = new NpgsqlCommand())
                        {
                            cmd.Connection = DbConnection;
                            cmd.CommandText = "CALL store_candle_v1(@candle_time,@BinSize, @symbol,@price_open,@price_high,@price_low,@price_close,@volume)";
                            cmd.Parameters.AddWithValue("candle_time", candle.Timestamp.DateTime);
                            cmd.Parameters.AddWithValue("BinSize", timeframe);
                            cmd.Parameters.AddWithValue("symbol", candle.Symbol);
                            cmd.Parameters.AddWithValue("price_open", candle.Open);
                            cmd.Parameters.AddWithValue("price_high", candle.High);
                            cmd.Parameters.AddWithValue("price_low", candle.Low);
                            cmd.Parameters.AddWithValue("price_close", candle.Close);
                            cmd.Parameters.AddWithValue("volume", candle.Volume);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("Cant save candle... " + ex.Message);
                }
            }
        }

        public List<TradeBucketedDto> GetCandlesFromDB(string timeframe, int count)
        {
            List<TradeBucketedDto> buff = new List<TradeBucketedDto>();

            using (var DbConnection = new NpgsqlConnection(ConnectionString))
            {
                DbConnection.Open();

                string table_name = "XBTUSD_" + timeframe;

                try
                {
                    // Retrieve all rows
                    using (var cmd = new NpgsqlCommand("SELECT * FROM (SELECT id, symbol, price_open, price_high, price_low, price_close, trades, volume, vwap, last_size, timestamp_candle, timeframe " +
                                                       "FROM public." + '"' + table_name + '"' + " " +
                                                       "Order by timestamp_candle desc limit " + count + " ) t Order by t.timestamp_candle asc ;", DbConnection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                buff.Add(
                                       new TradeBucketedDto
                                       {
                                           Timestamp = reader.GetTimeStamp(10).ToDateTime(),
                                           Open = reader.GetDecimal(2),
                                           High = reader.GetDecimal(3),
                                           Low = reader.GetDecimal(4),
                                           Close = reader.GetDecimal(5),
                                           Symbol = reader.GetString(1),
                                           Volume = reader.GetDecimal(7)
                                       }
                                );
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("Retrieve all rows candle... " + ex.Message);
                }
            }

            return buff;
        }

        private void GetCandlesForCalc(string timeframe, int count)
        {
            logger.Debug("Start recalc.....");

            List<TradeBucketedDto> buff = GetCandlesFromDB(timeframe, count);           

            logger.Debug("Candles in buffer " + buff.Count.ToString() + " | ManagedThreadId " + Thread.CurrentThread.ManagedThreadId.ToString());

            if (buff.Count > 0)
            {
                try
                {
                    //Запускаем расчет
                    List<CalculationResult> ResultTable = new List<CalculationResult>();
                    TradingSystem tradingAlro = new TradingSystem();
                    ResultTable = tradingAlro.RunSystem(buff);
                    CurrentResultTable = new List<CalculationResult>(ResultTable.Select(x => x.DeepClone()));

                    ObservableCollection<TradeBucketedDto> _CandlesViewSrc = new ObservableCollection<TradeBucketedDto>(buff.Select(x => x.DeepClone()).Reverse());

                    //Получили данные (свечи) выводим в форме, отображаем в ListView 
                    if (ResultTable.Count > 0)
                    {
                        if (ResultTable.First().TimeStamp != null)
                            logger.Debug("ResTableLastCandle " + ResultTable.First().TimeStamp.ToString() + " | " + ResultTable.First().LongShortCond.ToString());
                    }
                    logger.Debug("Send data to View");
                    parentThread.BeginInvoke(TargetFunc, _CandlesViewSrc, ResultTable);
                    logger.Debug("End send data. End recalc.....");

                }
                catch (Exception ex)
                {
                    logger.Debug("System calc exception "); logger.Debug(ex.Message);
                }
            }
        }


        private void CheckRead(object state = null)
        {//Потаймеру      
            logger.Debug("Init check start....");
            lock (_syncObj)
            {
                logger.Debug("Check start....");

                if (BinSize == "1m")
                {
                    DateTime StartTime_1m = getLastCandle("1m");
                    TimeSpan ts = DateTime.UtcNow - StartTime_1m;
                    if (ts.TotalMinutes >= 1)
                    {
                        logger.Debug("Diff in time. Reload history....");
                        readHistory_1m();
                        DelayTimer(5);
                    }
                    else
                    {
                        logger.Debug("History is ok....");
                        ResetTimerToNextCandle();
                    }
                }

                if (BinSize == "1h")
                {
                    DateTime StartTime_1h = getLastCandle("1h");
                    TimeSpan ts = DateTime.UtcNow - StartTime_1h;
                    if (ts.TotalHours >= 1)
                    {
                        logger.Debug("Diff in time. Reload history....");
                        readHistory_1h();
                        DelayTimer(5);
                    }
                    else
                    {
                        logger.Debug("History is ok....");
                        ResetTimerToNextCandle();
                    }
                }
            }
        }

        /*
        private void readHistory()
        {
            logger.Debug("Start read history....");
            DateTime StartTime = DateTime.UtcNow;

            if (BinSize == "1m")
            {
                DateTime currMin = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);
                StartTime = currMin.AddMinutes(-CountForCalculation + 1);
            }
            else if (BinSize == "1h")
            {
                DateTime currMin = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0);
                StartTime = currMin.AddHours(-CountForCalculation + 1);
            }

            var tradeBucketedParams = TradeBucketedGETRequestParams.GetCandleHistory("XBTUSD", CountForCalculation, BinSize, startTime: StartTime);
            List<TradeBucketedDto> response = _bitmexApiService.ExecuteSyncErrorHandler(BitmexApiUrls.Trade.GetTradeBucketed, tradeBucketedParams);

            if (response != null && response.Count > 0)
            {
                CandleBuffer.AddRange(response);
                sendForCalc();
            }
            else
            {
                logger.Debug("Error in reading history....");
            }
        }

        private void CheckRead(object state = null)
        {//Потаймеру      
            logger.Debug("Init check start....");
            lock (_syncObj)
            {
                logger.Debug("Check start....");    
                if (CandleBuffer == null)
                {
                    logger.Debug("Candle is null. Reload history....");
                    readHistory();

                    DelayTimer1min();
                }
                else if (CandleBuffer.Count >= 0 && CandleBuffer.Count < 200)
                {
                    logger.Debug("Candle count == 0. Reload history....");
                    readHistory();

                    DelayTimer1min();
                }
                else if (CandleBuffer.Count > 0)              
                {//проверяем считали ли последнюю свечу если нет читаем историю
                    if (CandleBuffer.Last().Timestamp != null)
                    {
                        TimeSpan ts = DateTime.UtcNow - CandleBuffer.Last().Timestamp.DateTime;
                        if (ts.TotalHours >= 1)
                        {
                            logger.Debug("Diff in time. Reload history....");
                            readHistory();
                            DelayTimer1min();
                        }
                        else
                        {
                            logger.Debug("History is ok....");
                            ResetTimer();
                        }
                    }
                    else
                    {
                        logger.Debug("Last candle is null.. Reload history....");
                        readHistory();
                        DelayTimer1min();
                    }
                }
                else
                {
                    logger.Debug("History is ok....");
                    ResetTimer();
                }
            }
        }

        private void sendForCalc()
        {
            logger.Debug("Start recalc.....");
            if (CandleBuffer.Count > CountForCalculation) CandleBuffer.RemoveRange(0, CandleBuffer.Count - CountForCalculation);
            logger.Debug("Candles in buffer " + CandleBuffer.Count.ToString() + " | ManagedThreadId " + Thread.CurrentThread.ManagedThreadId.ToString());

            List<CalculationResult> ResultTable = new List<CalculationResult>();
            try
            {
                //Запускаем расчет
                TradingSystem tradingAlro = new TradingSystem();
                ResultTable = tradingAlro.RunSystem(CandleBuffer);
                CurrentResultTable = new List<CalculationResult>(ResultTable.Select(x => x.DeepClone()));
            }
            catch (Exception ex)
            {
                logger.Debug("System calc exception ");
                logger.Debug(ex.Message);
                MessageBox.Show((ex.InnerException).Message);
            }

            ObservableCollection<TradeBucketedDto> _CandlesViewSrc = new ObservableCollection<TradeBucketedDto>( CandleBuffer.Select(x => x.DeepClone()).Reverse() );
            //_CandlesViewSrc = new ObservableCollection<TradeBucketedDto>(_CandlesViewSrc.Reverse());
            //Получили данные (свечи) выводим в форме, отображаем в ListView 
            if (ResultTable.Count > 0)
            {
                if (ResultTable.First().TimeStamp != null)
                logger.Debug("ResTableLastCandle " + ResultTable.First().TimeStamp.ToString() + " | " + ResultTable.First().LongShortCond.ToString());
            }
            logger.Debug("Send data to View");
            parentThread.BeginInvoke(TargetFunc, _CandlesViewSrc, ResultTable);
            logger.Debug("End send data....");
            logger.Debug("End recalc.....");
        }*/

        void MainFunc()//Функция потока, передаем параметр
        {
            t = new System.Threading.Timer(new System.Threading.TimerCallback(CheckRead), CandleBuffer, 0, System.Threading.Timeout.Infinite);
        }

        private void DelayTimer(int seconds)
        {
            t.Change(Convert.ToInt32((new TimeSpan(0, 0, seconds)).TotalMilliseconds), System.Threading.Timeout.Infinite);
            logger.Debug("Time event " + DateTime.UtcNow.ToString() + " | Next check after 5 sec" + " | ManagedThreadId " + Thread.CurrentThread.ManagedThreadId.ToString());           
        }

        private void ResetTimerToNextCandle()
        {//Сбро таймера до наступления новой свечи
            logger.Debug("Start check reset.........................");
            DateTime dueTime;
            switch (BinSize)
            {
                case "1m":
                    dueTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);
                    dueTime = dueTime.AddMinutes(1);
                    break;
                case "1h":
                    dueTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0);
                    dueTime = dueTime.AddHours(1);
                    break;
                default:
                    dueTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);
                    break;
            }
            TimeSpan timeRemaining = dueTime.Subtract(DateTime.UtcNow);
            timeRemaining = timeRemaining.Add(new TimeSpan(0, 0, 20));
            t.Change(Convert.ToInt32(timeRemaining.TotalMilliseconds), System.Threading.Timeout.Infinite);
            logger.Debug("Time event " + DateTime.UtcNow.ToString() + " | Next check after " + timeRemaining.ToString() + " | ManagedThreadId " + Thread.CurrentThread.ManagedThreadId.ToString());
            logger.Debug("RESETING IS DONE.................................");
        }        
    }
}
