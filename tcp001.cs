using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Comm;
using NewLife;
using NewLife.Log;
using NewLife.Net;
using Newtonsoft;
using Newtonsoft.Json;

namespace ClientTest004
{
    class Program
    {
        static List<byte> Strbuffer = new List<byte>();
        static string TagerId = "SFCS01";
        static string Sign = "1F7287609A9CA2E761B1DDD4BE9A7AB8";
        static string serverip = "10.46.1.203";
        static string port = "8121";
        static ISocketClient client;
        static Packet packet;
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            packet = new Packet(TagerId, Sign);
            //连接到服务器
            var uri = new NetUri($"tcp://{serverip}:{port}");
            client = uri.CreateRemote();
            client.Log = XTrace.Log;

            //数据收到事件
            client.Received += Client_Received;

            //注册
            var data= packet.GetLoginReq();
            client.Send(data);

            //发送心跳包
             Task.Run(async () =>
             {
                 await RunHealth();
             });

            while (true)
            {
                Console.Write("发送几个初始信息:");
                string str= Console.ReadLine();
                int sendCount;

                if(!int.TryParse(str,out sendCount))
                {
                    Console.WriteLine("请输入整数");
                }
                //发送初始化包
                for (int i = 0; i < sendCount; i++)
                {
                    data = packet.GetInitReq();
                    client.Send(data);
                    Console.WriteLine("发送初始化");
                }
            }
            
            await Task.Delay(-1);
            Console.ReadLine();
        }


        private static async Task RunHealth()
        {
            while (true)
            {
                var req = packet.GetHealthReq();

                client.Send(req);
                Console.WriteLine("发送心跳");
                await Task.Delay(10 * 1000);
            }
        }

        private static void Client_Received(object sender, ReceivedEventArgs e)
        {
            int packlength = e.Packet.Count;
            var res = new LineResponse(e.Packet.Data.Skip(4).Take(packlength).ToArray());
            if (res.FuncName == "Health")
                Console.WriteLine($"{DateTime.Now} 心跳包返回 ");
            else if(res.FuncName== "Login")
                Console.WriteLine($"{DateTime.Now} 登陆包返回 ");
            else if(res.FuncName== "GetInitStationInfo")
                Console.WriteLine($"{DateTime.Now} 站点初始化返回 ");
            else
            {
                Console.WriteLine($"{DateTime.Now} 服务器返回:{JsonConvert.SerializeObject(res, Formatting.Indented)}  \r\n----------------------------\r\n");
            }

            //Strbuffer.AddRange(e.Packet.Data.Take(e.Packet.Count));
            //string result = DecodeBag(Strbuffer);
            //if (!string.IsNullOrWhiteSpace(result))
            //{
            //    Console.WriteLine(result + "\r\n----------------------------\r\n");
            //}
        }



        private static string DecodeBag(List<byte> data)
        {
            var lengthbyte = data.Take(4).ToArray();
            Array.Reverse(lengthbyte);
            var length = BitConverter.ToInt32(lengthbyte, 0);
            if (data.Count() >= length + 4)
            {
                var onebag = data.Skip(4).Take(length);
                LineResponse msg = new LineResponse(onebag.ToArray());
                data.RemoveRange(0, length + 4);
                return $"{DateTime.Now.ToString()} Response:\n{JsonConvert.SerializeObject(msg, Formatting.Indented)} \r\n Body解码内容:{Encoding.UTF8.GetString(msg.Body)} ";
            }
            else
            {
                return "";
            }

        }
    }
}
