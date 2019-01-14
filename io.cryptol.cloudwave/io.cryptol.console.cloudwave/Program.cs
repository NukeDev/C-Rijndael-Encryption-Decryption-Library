using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using io.cryptol.cloudwave;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using ZeroMQ;
using ProtoBuf;

namespace io.cryptol.console.cloudwave
{

    [ProtoContract]
    public class Test
    {
        [ProtoMember (1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public string user { get; set; }
    }

    class Program
    {
      
        static void Main(string[] args)
        {
            string endpoint = "tcp://127.0.0.1:10000";

            // Create
            using (var context = new ZContext())
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                // Connect
                requester.Connect(endpoint);

                for (int n = 0; n < 10; ++n)
                {
                    Test test = new Test();

                    test.ID = 100;
                    test.user = "user";

                    byte[] serTest = ProtoSerialize<Test>(test);
                    // Send
                    requester.Send(new ZFrame(serTest));

                    // Receive
                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        Test test1 = new Test();

                        test1 = ProtoDeserialize<Test>(reply.Read());

                        Console.WriteLine("Received: " + test1.ID.ToString() + " - " + test1.user);
                    }
                }

                Console.Read();
            }
        }
        public static byte[] ProtoSerialize<T>(T record) where T : class
        {
            if (null == record) return null;

            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, record);
                    return stream.ToArray();
                }
            }
            catch
            {
                // Log error
                throw;
            }
        }
        public static T ProtoDeserialize<T>(byte[] data) where T : class
        {
            if (null == data) return null;

            try
            {
                using (var stream = new MemoryStream(data))
                {
                    return Serializer.Deserialize<T>(stream);
                }
            }
            catch
            {
                // Log error
                throw;
            }
        }
    }
}
