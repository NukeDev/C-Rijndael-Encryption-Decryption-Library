using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.cryptol.window.cloudwave.NetParser
{
    [ProtoContract]
    public class Request
    {
        [ProtoMember(1)]
        public string request { get; set; }
        [ProtoMember(2)]
        public byte[] reqData { get; set; }
    }
}
