using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BitMexLibrary
{
    public enum State { None, Start, Close_position, Repeat_buy, Repeat_sell, Repeat_dell_all_orders, Done, Wait_order_filled, Set_leverage, Check_init_margin, Check_init_quote}

    public class StateOperation
    {        
        private State _state;
        private string operationName;
        private int vol;
        private int _request_nums;
        private bool _isManuallyActivated;
        private double _leverage;

        public int Vol { get => vol; set => vol = value; }
        public string Signal { get => operationName; set => operationName = value; }
        public State CurrState { get => _state; set => _state = value; }
        public int Request_nums { get => _request_nums; set => _request_nums = value; }
        public bool IsManuallyActivated { get => _isManuallyActivated; set => _isManuallyActivated = value; }
        public double Leverage { get => _leverage; set => _leverage = value; }

        public StateOperation(State state, int vol = 0, string operationName = "", int request_nums = 0, bool isManuallyActivated = false, double leverage = 0)
        {
            CurrState = state;
            Vol = vol;
            Signal = operationName;
            Request_nums = request_nums;
            IsManuallyActivated = isManuallyActivated;
            Leverage = leverage;
        }
    }
}