using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.cryptol.window.cloudwave.NetParser
{
    [ProtoContract]
    public class TRequest<T>
    {
        [ProtoMember(1)]
        public T Data { get; set; }
    }
}
