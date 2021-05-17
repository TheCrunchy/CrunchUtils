using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrunchUtilities
{
    public class ShipOffer
    {
        public Int64 price;
        public long SellerIdentityId;
        public long SellerSteamId;
        public List<long> gridsInOffer;
        public DateTime TimeOfOffer = DateTime.Now.AddSeconds(30);
    }
}
