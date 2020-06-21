using System;

namespace Bitmex.NET.Models
{
    public partial class UserMarginGETRequestParams
    {
        public static UserMarginGETRequestParams GetUserMarginGETRequest(string currency)
        {
            return new UserMarginGETRequestParams
            {
                Currency = currency
            };
        }
    }
}


