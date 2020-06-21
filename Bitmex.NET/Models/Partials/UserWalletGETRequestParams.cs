using System;

namespace Bitmex.NET.Models
{
    public partial class UserWalletGETRequestParams
    {
        public static UserWalletGETRequestParams GetUserWalletGETRequest(string currency)
        {
            return new UserWalletGETRequestParams
            {
                Currency = currency
            };
        }
    }

    public partial class UserWalletHistoryGETRequestParams
    {
        public static UserWalletHistoryGETRequestParams UserWalletHistoryGETRequest(string currency)
        {
            return new UserWalletHistoryGETRequestParams
            {
                Currency = currency
            };
        }
    }
}


