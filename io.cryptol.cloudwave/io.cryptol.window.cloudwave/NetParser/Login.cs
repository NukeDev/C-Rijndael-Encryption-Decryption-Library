using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.cryptol.window.cloudwave.NetParser
{
    [ProtoContract]
    public class Login
    {
        [ProtoMember(1)]
        public string ID { get; set; }
        [ProtoMember(2)]
        public string user { get; set; }
        [ProtoMember(3)]
        public string password { get; set; }
        [ProtoMember(4)]
        public DateTime loginDate { get; set; }
        [ProtoMember(5)]
        public LoginResponse response { get; set; } 
    }
    [ProtoContract]
    public class LoginResponse
    {
        [ProtoMember(1)]
        public string ID { get; set; }
        [ProtoMember(2)]
        public string user { get; set; }
        [ProtoMember(3)]
        public string cookie { get; set; }
        [ProtoMember(4)]
        public bool hasLogged { get; set; }

    }
    
}
