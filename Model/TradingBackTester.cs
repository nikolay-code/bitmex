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
    public class TradingBackTester
    {
        private string _connectionString;
        public string ConnectionString { get => _connectionString; set => _connectionString = value; }
        public TradingBackTester(string conn)
        {
            AutoMapperConfiguration.Configure();
            ConnectionString = conn;
        }


    }
}
