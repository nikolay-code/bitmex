using Bitmex.NET.Models;

namespace Bitmex.NET
{
	public interface IBitmexAuthorization
	{
		BitmexEnvironment BitmexEnvironment { get; set; }
        BitmexAccountTypeEnvironment AccountType { get; set; }
        string Key { get; set; }
		string Secret { get; set; }
        int Size { get; set; }
        decimal Leverage { get; set; }
        int Id { get; set; }
    }
}
