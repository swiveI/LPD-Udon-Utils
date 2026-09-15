using System;
using UnityEngine;

namespace LocalPoliceDepartment.Utilities.Compression
{
    public static class LpdNetworkCompression
    {
        private const float FloatPrecision = 1000f; // 3 decimal places

        // =============================================================
        // Packing (Type -> byte[])
        // =============================================================

        public static byte[] PackInt(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] PackFloat(float value)
        {
            // Truncate to 3 decimal places and store as int (4 bytes -> usually 2-3 bytes worth of info)
            int truncated = (int)Mathf.Round(value * FloatPrecision);
            return BitConverter.GetBytes(truncated);
        }

        public static byte[] PackVector3(Vector3 v)
        {
            byte[] bytes = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes((int)Mathf.Round(v.x * FloatPrecision)), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((int)Mathf.Round(v.y * FloatPrecision)), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((int)Mathf.Round(v.z * FloatPrecision)), 0, bytes, 8, 4);
            return bytes;
        }

        public static byte[] PackQuaternion(Quaternion q)
        {
            // Drop the largest component and store sign + 3 floats as shorts
            float x = q.x, y = q.y, z = q.z, w = q.w;

            // Find largest absolute value
            int largestIndex = 0;
            float largest = Mathf.Abs(x);
            if (Mathf.Abs(y) > largest) { largest = Mathf.Abs(y); largestIndex = 1; }
            if (Mathf.Abs(z) > largest) { largest = Mathf.Abs(z); largestIndex = 2; }
            if (Mathf.Abs(w) > largest) { largest = Mathf.Abs(w); largestIndex = 3; }

            byte[] bytes = new byte[7]; // 1 byte header + 3*2 bytes shorts

            // Header: which component was dropped (2 bits) + sign of dropped (1 bit) + padding
            byte header = (byte)largestIndex;

            // Store the three smaller components as shorts (-32768 to 32767)
            int idx = 1;
            if (largestIndex != 0) { WriteShort(bytes, idx, x); idx += 2; }
            if (largestIndex != 1) { WriteShort(bytes, idx, y); idx += 2; }
            if (largestIndex != 2) { WriteShort(bytes, idx, z); idx += 2; }
            if (largestIndex != 3) { WriteShort(bytes, idx, w); }

            bytes[0] = header;
            return bytes;
        }

        public static byte[] PackString(string text)
        {
            if (string.IsNullOrEmpty(text)) return new byte[4]; // just length 0

            byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(text);
            byte[] lenBytes = BitConverter.GetBytes(strBytes.Length);

            byte[] result = new byte[4 + strBytes.Length];
            Buffer.BlockCopy(lenBytes, 0, result, 0, 4);
            Buffer.BlockCopy(strBytes, 0, result, 4, strBytes.Length);
            return result;
        }

        // =============================================================
        // Unpacking (byte[] -> Type)
        // =============================================================

        public static int UnpackInt(byte[] data, ref int offset)
        {
            int value = BitConverter.ToInt32(data, offset);
            offset += 4;
            return value;
        }

        public static float UnpackFloat(byte[] data, ref int offset)
        {
            int truncated = BitConverter.ToInt32(data, offset);
            offset += 4;
            return truncated / FloatPrecision;
        }

        public static Vector3 UnpackVector3(byte[] data, ref int offset)
        {
            Vector3 v = new Vector3(
                UnpackFloat(data, ref offset),
                UnpackFloat(data, ref offset),
                UnpackFloat(data, ref offset)
            );
            return v;
        }

        public static Quaternion UnpackQuaternion(byte[] data, ref int offset)
        {
            byte header = data[offset];
            offset += 1;

            int droppedIndex = header & 0x03; // first 2 bits

            float a = 0, b = 0, c = 0;
            int readIdx = offset;

            // Read the three stored values
            if (droppedIndex != 0) a = ReadShort(data, ref readIdx) / 32767f;
            if (droppedIndex != 1) b = ReadShort(data, ref readIdx) / 32767f;
            if (droppedIndex != 2) c = ReadShort(data, ref readIdx) / 32767f;
            //if (droppedIndex != 3) /* d would be read here */;

            offset = readIdx;

            float d = Mathf.Sqrt(1f - a*a - b*b - c*c); // reconstruct dropped value

            // Assign back to x,y,z,w based on dropped index
            Quaternion q = Quaternion.identity;
            switch (droppedIndex)
            {
                case 0: q = new Quaternion(d, a, b, c); break;
                case 1: q = new Quaternion(a, d, b, c); break;
                case 2: q = new Quaternion(a, b, d, c); break;
                case 3: q = new Quaternion(a, b, c, d); break;
            }

            return q;
        }

        public static string UnpackString(byte[] data, ref int offset)
        {
            int len = BitConverter.ToInt32(data, offset);
            offset += 4;
            if (len <= 0) return "";

            string text = System.Text.Encoding.UTF8.GetString(data, offset, len);
            offset += len;
            return text;
        }

        // =============================================================
        // Internal Helpers
        // =============================================================
        private static void WriteShort(byte[] bytes, int offset, float value)
        {
            short s = (short)Mathf.Clamp(value * 32767f, -32767, 32767);
            Buffer.BlockCopy(BitConverter.GetBytes(s), 0, bytes, offset, 2);
        }

        private static float ReadShort(byte[] bytes, ref int offset)
        {
            short s = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            return s / 32767f;
        }
    }
}