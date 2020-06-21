using Bitmex.NET.Models;

namespace Bitmex.NET
{
	public class BitmexAuthorization : IBitmexAuthorization
	{
		public BitmexEnvironment BitmexEnvironment { get; set; }
        public BitmexAccountTypeEnvironment AccountType { get; set; }
        public string Key { get; set; }
		public string Secret { get; set; }
        public int Size { get; set; }
        public decimal Leverage { get; set; }
        public int Id { get; set; }
    }
}
