using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace io.cryptol.server.cloudwave
{
    [ProtoContract]
    public class Test
    {
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public string user { get; set; }
    }
    class Program
    {

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

        static void Main(string[] args)
        {
            // Create
            using (var context = new ZContext())
            using (var responder = new ZSocket(context, ZSocketType.REP))
            {
                // Bind
                responder.Bind("tcp://*:10000");

                while (true)
                {
                    // Receive
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                        Test test = ProtoDeserialize<Test>(request.Read());

                       

                        test.ID++;
                        test.user += "_READ";

                        // Do some work
                        Thread.Sleep(1);

                        // Send
                        responder.Send(new ZFrame(ProtoSerialize<Test>(test)));
                    }
                }
            }
        }
    }
}
