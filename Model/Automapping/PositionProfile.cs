using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Bitmex.NET.Dtos;

namespace BitMexLibrary.Automapping
{
    internal class PosProfile : Profile
    {
        public PosProfile()
        {
            CreateMap<PositionDto, PositionUpdateModel>();
            CreateMap<PositionDto, PositionAdditionalUpdateModel>();
            /* .ForMember(a => a.NotificationDateTime, a => a.MapFrom(b => DateTime.Now))
             .ForMember(a => a.Symbol, a => a.Condition(b => !string.IsNullOrWhiteSpace(b.Symbol)))
             .ForMember(a => a.Side, a => a.Condition(b => !string.IsNullOrWhiteSpace(b.Side)))
             .ForMember(a => a.Price, a => a.Condition(b => b.Price.HasValue))
             .ForMember(a => a.LeavesQty, a => a.Condition(b => b.LeavesQty.HasValue))
             .ForMember(a => a.OrdStatus, a => a.Condition(b => !string.IsNullOrWhiteSpace(b.OrdStatus)))
             .ForMember(a => a.OrdType, a => a.Condition(b => !string.IsNullOrWhiteSpace(b.OrdType)));*/
        }
    }
}
