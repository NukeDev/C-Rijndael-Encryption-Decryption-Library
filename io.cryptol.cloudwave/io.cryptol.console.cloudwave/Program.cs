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
using System.Net;

namespace io.cryptol.console.cloudwave
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
        [ProtoMember(4)]
        public Settings encSet { get; set; }
        [ProtoMember(5)]
        public byte[] fileData { get; set; }
        [ProtoMember(6)]
        public string response { get; set; }
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

        static IPInformation ipinfo = new IPInformation();
      
        static void Main(string[] args)
        {
            string endpoint = "tcp://127.0.0.1:10000";

            using (WebClient wc = new WebClient())
            {
                string jsonIP = wc.DownloadString("http://ip-api.com/json/");
                ipinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<IPInformation>(jsonIP);
            }

            // Create
            using (var context = new ZContext())
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                // Connect
                requester.Connect(endpoint);

                
                Test test = new Test();

                test.ID = "101";
                test.user = "admin";
                test.ip = ipinfo;
                Settings set = new Settings();
                set.password = "12345";
                set.Type = Settings.encType.Decrypt;

                test.encSet = set;

                test.fileData = File.ReadAllBytes(@"C:\Users\g.varriale\Downloads\CitrixReceiver.exe.enc");
               

                Console.WriteLine(DateTime.Now + ": Sending informations... " + test.GetHashCode());
                byte[] serTest = ProtoSerialize<Test>(test);
                // Send
                requester.Send(new ZFrame(serTest));
                Console.WriteLine(DateTime.Now + ": Informations Sent!" + Environment.NewLine + "Waiting for a reply...");
                // Receive
                using (ZFrame reply = requester.ReceiveFrame())
                {
                    Test test1 = new Test();

                    test1 = ProtoDeserialize<Test>(reply.Read());

                    Console.WriteLine(DateTime.Now + ": Received: " + test1.GetHashCode() + " - " + test1.ID + " RESPONSE: " + test1.response);

                    if(test1.response != "AUTH ERROR")
                    {
                        File.WriteAllBytes(@"C:\Users\g.varriale\Downloads\CitrixReceiver11.exe", test1.fileData);
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
                throw;
            }
        }
    }
}
