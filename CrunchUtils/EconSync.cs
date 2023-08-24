using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace CrunchUtilities
{
    [ProtoContract]
    public class EconSync
    {
        [ProtoMember(1)]
        public Dictionary<long, long> SyncThis { get; set; } = new Dictionary<long, long>();

    }
}
