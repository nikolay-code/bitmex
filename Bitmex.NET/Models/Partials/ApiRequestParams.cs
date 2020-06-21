using System;

namespace Bitmex.NET.Models
{
    public partial class ApiKeyGETRequestParams
    {
        public static ApiKeyGETRequestParams GetApiKeyGETRequest(bool reverse=false)
        {
            return new ApiKeyGETRequestParams
            {
                Reverse = reverse
            };
        }
    }

    public partial class ApiKeyPOSTRequestParams
    {
        public static ApiKeyPOSTRequestParams GetApiKeyPOSTRequest(string name, string permissions, bool enabled)
        {
            return new ApiKeyPOSTRequestParams
            {
                Name = name,
                Permissions = permissions,
                Enabled = enabled
            };
        }
    }

    public partial class ApiKeyDELETERequestParams
    {
        public static ApiKeyDELETERequestParams GetApiKeyDELETERequest(string apiKeyID)
        {
            return new ApiKeyDELETERequestParams
            {
                ApiKeyID = apiKeyID
            };
        }
    }

}


