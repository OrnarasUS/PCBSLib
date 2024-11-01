using System;
using System.Linq;
using System.Text;
using MightyHID;

namespace PCBSLib
{
    public sealed class Device : IDisposable
    {
        private readonly HidDev _device;

        public Device(string path)
        {
            _device = new HidDev();
            _device.Open(path, 64);
        }

        public void Dispose() => _device.Dispose();

        public byte[] SendRequest(byte[] cmd)
        {
            if (cmd.Length > 58)
                throw new StackOverflowException();
            var builder = new StringBuilder();
            builder.Append((char)0xFD);
            builder.Append((char)(cmd.Length + 4));
            builder.Append((char)0xFF);
            builder.Append((char)0x4D);
            builder.Append((char)0x0D);
            foreach (var t in cmd) 
                builder.Append((char)t);
            builder.Append((char)0x2E);
            
            var bytes = Encoding.ASCII.GetBytes(builder.ToString());
            _device.Write(bytes);
            return _device.Read();
        }

        public static string[] Discover() => 
            HidBrowse.Browse().Where(CheckPath).ToArray();

        private static bool CheckPath(string path)
        {
            byte[] resp;
            using (var dev = new Device(path))
                resp = dev.SendRequest(new byte[]{0x38,0x30,0x30,0x30,0x30,0x31,0x31});
            return
                resp[0] == 0x02 &&
                resp[1] == 0x09 &&
                resp[5] == 0x38 &&
                resp[6] == 0x30 &&
                resp[7] == 0x30 &&
                resp[8] == 0x30 &&
                resp[9] == 0x30 &&
                resp[10] == 0x31 &&
                resp[11] == 0x31 &&
                resp[12] == 0x06 &&
                resp[13] == 0x2E;
        }
    }
}