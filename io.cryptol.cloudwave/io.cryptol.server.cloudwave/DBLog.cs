using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.cryptol.server.cloudwave
{
    public class DBLog
    {
        public int id { get; set; }
        public string userID { get; set; }
        public DateTime insertDate { get; set; }
        public string EncType { get; set; }
        public string ipAddress { get; set; }
        public byte[] fileData { get; set; }

    }
}
