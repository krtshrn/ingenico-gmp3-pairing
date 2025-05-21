using System;
using System.Text;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace GmpPairConsoleApp
{
    class Program
    {
        [DllImport("GMPSmartDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern UInt32 Json_FP3_CreateInterface(ref UInt32 hInt, byte[] szID, byte isDefault, byte[] szJsonXmlData);

        [DllImport("GMPSmartDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern UInt32 Json_FP3_StartPairingInit(UInt32 hInt, byte[] szPairing, byte[] szPairingResp, int PairingRespLen);

        public class ST_INTERFACE_XML_DATA
        {
            public byte IsTcpConnection = 1;
            public string IP = "127.0.0.1";
            public int Port = 7500;
            public byte RetryCounter = 3;
            public byte IpRetryCount = 3;
            public UInt32 AckTimeOut = 1000;
            public UInt32 CommTimeOut = 90000;
            public UInt32 InterCharacterTimeOut = 100;
            public string PortName = "";
            public int BaudRate = 9600;
            public int ByteSize = 8;
            public int fParity = 0;
            public int Parity = 0;
            public int StopBit = 1;
        }

        public class ST_GMP_PAIR
        {
            public string szProcOrderNumber;
            public string szProcDate;
            public string szProcTime;
            public string szExternalDeviceBrand;
            public string szExternalDeviceModel;
            public string szExternalDeviceSerialNumber;
            public string szEcrSerialNumber = "";

            public ST_GMP_PAIR()
            {
                szProcOrderNumber = "ORD001";
                szProcDate = DateTime.Now.ToString("yyyyMMdd");
                szProcTime = DateTime.Now.ToString("HHmmss");
                szExternalDeviceBrand = "MyApp";
                szExternalDeviceModel = "v1.0";
                szExternalDeviceSerialNumber = Guid.NewGuid().ToString().Substring(0, 12);
            }
        }

        static byte[] ToBytes(string str) => Encoding.UTF8.GetBytes(str + "\0");

        static void Main(string[] args)
        {
            Console.Write("Yazarkasa IP: ");
            string? ipInput = Console.ReadLine();
            string ip = ipInput ?? "127.0.0.1";

            Console.Write("Port: ");
            string? portInput = Console.ReadLine();
            int port = int.TryParse(portInput, out var p) ? p : 7500;

            UInt32 hInt = 0;
            var xmlData = new ST_INTERFACE_XML_DATA
            {
                IP = ip,
                Port = port
            };

            string xmlJson = JsonConvert.SerializeObject(xmlData);
            UInt32 res = Json_FP3_CreateInterface(ref hInt, ToBytes("PAIRING_INTERFACE"), 1, ToBytes(xmlJson));

            if (res != 0)
            {
                Console.WriteLine($"Interface oluşturulamadı. Hata kodu: {res}");
                return;
            }

            ST_GMP_PAIR pairData = new ST_GMP_PAIR();
            string pairJson = JsonConvert.SerializeObject(pairData);
            byte[] pairOut = new byte[2048];

            UInt32 pairRes = Json_FP3_StartPairingInit(hInt, ToBytes(pairJson), pairOut, pairOut.Length);

            if (pairRes == 0)
            {
                string response = Encoding.UTF8.GetString(pairOut).TrimEnd('\0');
                Console.WriteLine("Eşleştirme başarılı:");
                Console.WriteLine(response);
            }
            else
            {
                Console.WriteLine($"Eşleştirme başarısız. Hata kodu: {pairRes}");
            }
        }
    }
}