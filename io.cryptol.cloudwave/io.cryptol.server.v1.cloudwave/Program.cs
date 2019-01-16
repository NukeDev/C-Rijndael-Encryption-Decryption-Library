using io.cryptol.server.v1.cloudwave.NetParser;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace io.cryptol.server.v1.cloudwave
{
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
            using (var ctx = new ZContext())
            using (var clients = new ZSocket(ctx, ZSocketType.ROUTER))
            using (var workers = new ZSocket(ctx, ZSocketType.DEALER))
            {
                clients.Bind("tcp://*:10000");
                workers.Bind("inproc://workers");

                // Launch pool of worker threads
                for (int i = 0; i < 5; ++i)
                {
                    new Thread(() => MTServer_Worker(ctx)).Start();
                }

                // Connect work threads to client threads via a queue proxy
                ZContext.Proxy(clients, workers);
            }
        }
        static void MTServer_Worker(ZContext ctx)
        {

            // Socket to talk to dispatcher
            using (var server = new ZSocket(ctx, ZSocketType.REP))
            {
                server.Connect("inproc://workers");

                while (true)
                {
                    using (ZFrame frame = server.ReceiveFrame())
                    {
                        Console.WriteLine("receivin request");
                        byte[] receivedBytes = frame.Read();
                        Request req = ProtoDeserialize<Request>(receivedBytes);
                        Console.WriteLine("request deserialized");
                        Console.WriteLine(req.request);
                        switch (req.request)
                        {
                            default:
                                {
                                    Console.WriteLine("default -> break");

                                    break;

                                }

                            case "login":
                                {
                                    Console.WriteLine("login request");

                                    TRequest<Login> tLogin = new TRequest<Login>();

                                    tLogin = ProtoDeserialize<TRequest<Login>>(req.reqData);

                                    LoginResponse response = new LoginResponse();

                                    response = tLogin.Data.response;

                                    response.user = tLogin.Data.user;
                                    response.ID = tLogin.Data.ID;
                                   
                                    response.cookie = "XXXXXXXXXXXXXXXXXXXX";

                                    response.hasLogged = true;

                                    tLogin.Data.response = response;

                                    req.reqData = ProtoSerialize<TRequest<Login>>(tLogin);

                                    byte[] sendT = ProtoSerialize(req);


                                    // Do some 'work'
                                    Thread.Sleep(1);


                                    // Send reply back to client

                                    server.Send(new ZFrame(sendT));

                                    break;

                                }

                        }   
                    }
                }
            }
        }
    }

}
