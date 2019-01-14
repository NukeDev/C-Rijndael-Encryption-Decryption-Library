using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.cryptol.console.cloudwave
{
    public class FileIO
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime DateOfCreation { get; set; }
        public byte[] FileData { get; set; }
    }
}
