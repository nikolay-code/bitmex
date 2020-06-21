using AutoMapper;
using Bitmex.NET.Dtos;
using Bitmex.NET.Models;
using System;

namespace BitMexLibrary.Automapping
{
    internal class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderDto, OrderUpdateModel>()
                .ForMember(a => a.NotificationDateTime, a => a.MapFrom(b => DateTime.Now))
                .ForMember(a => a.Symbol, a => a.Condition(b => !string.IsNullOrWhiteSpace(b.Symbol)))
                .ForMember(a => a.Side, a => a.Condition(b => !string.IsNullOrWhiteSpace(b.Side)))
                .ForMember(a => a.Price, a => a.Condition(b => b.Price.HasValue))
                .ForMember(a => a.LeavesQty, a => a.Condition(b => b.LeavesQty.HasValue))
                .ForMember(a => a.OrdStatus, a => a.Condition(b => !string.IsNullOrWhiteSpace(b.OrdStatus)))
                .ForMember(a => a.OrdType, a => a.Condition(b => !string.IsNullOrWhiteSpace(b.OrdType)));
        }
    }

    internal class OrderHistoryProfile : Profile
    {
        public OrderHistoryProfile()
        {
            CreateMap<OrderDto, OrderHistoryModel>();
        }
    }
}
