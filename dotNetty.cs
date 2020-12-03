using Coldairarrow.DotNettySocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NettyClientDemo
{
    class Program
    {
        public const string Host = "10.46.1.203";
        public const int Port = 8121;
        public const string Code = "SFCS01";
        public const string CodeSecert = "1F7287609A9CA2E761B1DDD4BE9A7AB8";
        public static ITcpSocketClient theClient;
        static async Task Main(string[] args)
        {
           
            Console.WriteLine("Hello World!");
            theClient = await SocketBuilderFactory.GetTcpSocketClientBuilder(Host, Port)
                .SetLengthFieldEncoder(4)
                .SetLengthFieldDecoder(int.MaxValue, 0, 4, 0, 4)
                .OnClientClose(server =>
                {
                    Console.WriteLine($"客户端关闭");
                })
                .OnException(ex =>
                {
                    Console.WriteLine($"客户端异常:{ex.Message}");
                })
                .OnRecieve((server, bytes) =>
                {
                    var res = new LineResponse(bytes);
                    if (res.FuncName == "Health")
                        Console.WriteLine($"{DateTime.Now} 心跳包返回");
                    else if (res.FuncName == "Login")
                        Console.WriteLine($"{DateTime.Now} 登陆包返回 \r\n----------------------------\r\n");
                    else if (res.FuncName == "GetInitStationInfo")
                        Console.WriteLine($"{DateTime.Now} 站点初始化返回 \r\n----------------------------\r\n");
                    else {
                        Console.WriteLine($"{DateTime.Now} 服务器返回:{JsonConvert.SerializeObject(res, Formatting.Indented)}");
                    }
                    
                })
                .OnClientStarted(server =>
                {
                    Console.WriteLine($"客户端启动");
                }).BuildAsync();

            await theClient.Send(GetLogin());
            Task.Run(async ()=> {
                await RunHealth();
            });

            Console.ReadLine();
            await CallALL();

            await Task.Delay(-1);
        }

        private static async Task CallALL()
        {
            #region lists
            var lists = new Dictionary<string, string>();
            lists.Add("Bottom_Cover", "J1CQ0002UL");
            lists.Add("Input", "J1CQ0002UN");
            lists.Add("PCB_Assy", "J1CQ0002UV");
            lists.Add("Membrane_Assy", "J1CQ0002UU");
            lists.Add("Rubber_Assy", "J1CQ0002UW");
            lists.Add("Lock_Screw_PCBA", "J1CQ0002UQ");
            lists.Add("Top_Cover", "J1CQ0002UM");
            lists.Add("Put_Top_Cover", "J1CQ0002UP");
            lists.Add("Stitching", "J1CQ0002UX");
            lists.Add("Reversal_1", "J1CQ0002UY");
            lists.Add("Lock_Screw_1", "J1CQ0002UR");
            lists.Add("Lock_Screw_2", "J1CQ0002US");
            lists.Add("Lock_Screw_3", "J1CQ0002UT");
            lists.Add("Label_Match", "J1CQ0002V8");
            lists.Add("Reversal_2", "J1CQ0002V0");
            lists.Add("ATE_1", "J1CQ0002V2");
            lists.Add("ATE_2", "J1CQ0002V3");
            lists.Add("ATE_3", "J1CQ0002V4");
            lists.Add("Output", "J1CQ0002V5");
            lists.Add("Open_Box", "J1CQ0002V6");
            lists.Add("Binning", "J1CQ0002V7");
            lists.Add("Online_Label_1", "J1CQ0002V9");
            lists.Add("Online_Label_2", "J1CQ0002VA");
            lists.Add("Online_Weight", "J1CQ0002VB");
            lists.Add("AGV", "J1CQ0002VC");
            lists.Add("Lifter", "J1CQ0002VD");
            lists.Add("Stocker", "J1CQ0002VE");
            #endregion

            foreach (var t in lists)
            {
                //var req = new LineRequset();
                //req.TargetId = Code;
                //req.Sign = CodeSecert;
                //req.FuncGroup = "InitStation";
                //req.FuncName = "GetInitStationInfo";
                //req.SetBodyByJson(new
                //{
                //    LineCode = "CK31",
                //    ProcessNo = t.Key,
                //    MachineNo = t.Value,
                //});
                var req = new LineRequset();
                req.TargetId = Code;
                req.Sign = CodeSecert;
                req.FuncGroup = "InitStation";
                req.FuncName = "GetInitStationInfo";
                req.SetBody(JsonConvert.SerializeObject(new { databaseName = "Normal", LineCode = "123", ProcessNO = "Input", MachineNo = "dd" }));

                Console.WriteLine($"{DateTime.Now} 请求初始化站点:{t.Key}  {t.Value}");
                await theClient.Send(req.HeadSerialize().Concat(req.Body).ToArray());
            }
        }

        private static async Task RunHealth() {
            while (true) {
                var req = new LineRequset();
                req.TargetId = Code;
                req.Sign = CodeSecert;
                req.FuncGroup = "System";
                req.FuncName = "Health";
                req.SetBody("OK");

                await theClient.Send(req.HeadSerialize().Concat(req.Body).ToArray());

                await Task.Delay(10 * 1000);
            }
        }

        private static byte[] GetLogin() {
            var req = new LineRequset();
            req.TargetId = Code;
            req.Sign = CodeSecert;
            req.FuncGroup = "System";
            req.FuncName = "Login";
            req.BodyLength = 0;
            return req.HeadSerialize().ToArray();
        }
    }
}
