using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using io.cryptol.cloudwave;
using ZeroMQ;

namespace io.cryptol.server.cloudwave
{
    [ProtoContract]
    public class Test
    {
        [ProtoMember(1)]
        public string ID { get; set; }
        [ProtoMember(2)]
        public string user { get; set; }
        [ProtoMember(3)]
        public IPInformation ip { get; set; }
        [ProtoMember (4)]
        public Settings encSet { get; set; }
        [ProtoMember(5)]
        public byte[] fileData { get; set; }
    }
    [ProtoContract]
    public class Settings
    {

        [ProtoMember(1)]
        public string password { get; set; }
        public enum encType
        {
            Encrypt,
            Decrypt
        }
        [ProtoMember(2)]
        public encType Type { get; set; }

    }

    [ProtoContract]
    public class IPInformation
    {
        [ProtoMember(1)]
        public string @as { get; set; }
        [ProtoMember(2)]
        public string city { get; set; }
        [ProtoMember(3)]
        public string country { get; set; }
        [ProtoMember(4)]
        public string countryCode { get; set; }
        [ProtoMember(5)]
        public string isp { get; set; }
        [ProtoMember(6)]
        public double lat { get; set; }
        [ProtoMember(7)]
        public double lon { get; set; }
        [ProtoMember(8)]
        public string org { get; set; }
        [ProtoMember(9)]
        public string query { get; set; }
        [ProtoMember(10)]
        public string region { get; set; }
        [ProtoMember(11)]
        public string regionName { get; set; }
        [ProtoMember(12)]
        public string status { get; set; }
        [ProtoMember(13)]
        public string timezone { get; set; }
        [ProtoMember(14)]
        public string zip { get; set; }
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

        public static string GetUniqueKey(int size)
        {
            char[] chars =
                "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            byte[] data = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
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
                        byte[] receivedBytes = frame.Read();
                        Test test = ProtoDeserialize<Test>(receivedBytes);

                        Console.WriteLine(DateTime.Now + ": New Request of " + test.encSet.Type.ToString() + " from " + test.ip.query);
                       
                        
                        if(test.fileData != null)
                        {
                            float fileSize = (test.fileData.Length / 1024f) / 1024f;
                            Console.WriteLine(DateTime.Now + ": File Size -> " + fileSize.ToString() + "Mbytes |" + " Hash -> " + test.fileData.GetHashCode());
                            string tempDir = GetUniqueKey(10);
                            Directory.CreateDirectory(tempDir);
                            File.WriteAllBytes(tempDir + "\\temp", test.fileData);

                            EncSettings settings = new EncSettings();

                            settings.inputFile = tempDir + "\\temp";
                            settings.outputFile = tempDir + "\\temp.enc";
                            settings.password = "mysuperpassword" + test.encSet.password;
                            settings.Type = EncSettings.encType.Encrypt;

                            Cryptol crp = new Cryptol();
                            Console.WriteLine(DateTime.Now + ": Trying to " + test.encSet.Type.ToString() + " for " + test.ip.query);

                            Console.WriteLine(DateTime.Now + ": Status: " + crp.CryptolLoad(settings) + "for " + test.ip.query + " from " + test.ip.regionName);



                            test.fileData = File.ReadAllBytes(tempDir + "\\temp.enc");


                        }
                      


                        // Do some 'work'
                        Thread.Sleep(1);

                        Console.WriteLine(DateTime.Now + ": Sending data for " + test.ip.query + " from " + test.ip.region);

                        // Send reply back to client
                        byte[] toSendBytes = ProtoSerialize<Test>(test);
                      
                        server.Send(new ZFrame(toSendBytes));

                        Console.WriteLine(DateTime.Now + ": Data sent for " + test.ip.query + " from " + test.ip.region);


                    }
                }
            }
        }

    }
}
