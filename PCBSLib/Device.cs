using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            try
            {
                if (cmd.Length > 58)
                    throw new StackOverflowException();
                var bytes = new List<byte>();
                bytes.Add(0xFD);
                bytes.Add((byte)(cmd.Length + 4));
                bytes.Add(0xFF);
                bytes.Add(0x4D);
                bytes.Add(0x0D);
                bytes.AddRange(cmd);
                bytes.Add(0x2E);
                for (var i = 0; i < 64 - bytes.Count; i++)
                    bytes.Add(0);
                _device.Write(bytes.ToArray());
                return _device.Read();
            }
            catch (IOException)
            {
                return Array.Empty<byte>();
            }
        }

        public static string[] Discover() => 
            HidBrowse.Browse().Where(CheckPath).ToArray();

        private static bool CheckPath(string path)
        {
            var cmd = new byte[] { 0x38, 0x30, 0x30, 0x30, 0x30, 0x31, 0x31 };
            var resp = new byte[14];
            using (var dev = new Device(path))
            {
                var temp = dev.SendRequest(cmd);
                if (temp.Length == 0) return false;
                Array.Copy(temp, resp, resp.Length);
            }
            return resp.SequenceEqual(
                new byte[] { 0x02, 0x09, 0x00, 0x00, 0x00, 0x38, 0x30, 0x30, 0x30, 0x30, 0x31, 0x31, 0x06, 0x2E });
        }
    }
}