using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrunchUtilities
{
    public class TempBanList
    {
        public List<TempBanItem> TempBans = new List<TempBanItem>();
    }

    public class TempBanItem
    {
        public ulong SteamId;
        public DateTime UnbannedAfter;
    }
}
