namespace Bitmex.NET.Models
{
    public partial class PositionGETRequestParams
    {
        public static PositionGETRequestParams GetPositionGetRequest(decimal? count, string columns = null)
        {
            return new PositionGETRequestParams
            {
                Columns = columns,
                Count = count
            };
        }
    }

    public partial class PositionLeveragePOSTRequestParams
    {
        public static PositionLeveragePOSTRequestParams GetPositionPOSTRequest(string symbol, decimal leverage)
        {
            return new PositionLeveragePOSTRequestParams
            {
                Symbol = symbol,
                Leverage = leverage
            };
        }
    }
}


