using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZeroMQ;
using io.cryptol.window.cloudwave.NetParser;

namespace io.cryptol.window.cloudwave
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string endpoint = "tcp://127.0.0.1:10000";


            // Create
            using (var context = new ZContext())
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                // Connect
                requester.Connect(endpoint);

                Request req = new Request();
                TRequest<Login> Treq = new TRequest<Login>();
                Login login = new NetParser.Login();
                LoginResponse loginresponse = new LoginResponse();

                login.ID = "101";
                login.loginDate = DateTime.Now;
                login.password = "123";
                login.user = "admin";
                login.response = loginresponse;
                Treq.Data = login;
                req.request = "login";
                req.reqData = ProtoSerialize<TRequest<Login>>(Treq);

                byte[] serReq = ProtoSerialize<Request>(req);

                // Send
                requester.Send(new ZFrame(serReq));


                using (ZFrame reply = requester.ReceiveFrame())
                {
                    Request answer = new Request();

                    answer = ProtoDeserialize<Request>(reply.Read());
                    Treq = ProtoDeserialize<TRequest<Login>>(answer.reqData);
                    if (Treq.Data.response.hasLogged)
                    {
                        Treq = ProtoDeserialize<TRequest<Login>>(answer.reqData);
                        richTextBox1.AppendText("Cookie: " + Treq.Data.response.cookie + Environment.NewLine + "Logged" + Treq.Data.response.hasLogged.ToString() + Environment.NewLine + "User: " + Treq.Data.response.user);

                    }
                }
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
