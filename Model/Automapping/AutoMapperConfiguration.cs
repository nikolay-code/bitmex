using AutoMapper;

namespace BitMexLibrary.Automapping
{
    public static class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<InstrumentProfile>();
                cfg.AddProfile<OrderProfile>();
                cfg.AddProfile<OrderBookProfile>();
                cfg.AddProfile<PosProfile>();
                cfg.AddProfile<OrderHistoryProfile>();
            });
        }
    }
}
