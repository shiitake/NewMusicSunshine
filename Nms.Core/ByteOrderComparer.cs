using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nms.Core
{
    public class ByteOrderComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            byte[] xBytes = Encoding.UTF8.GetBytes(x);
            byte[] yBytes = Encoding.UTF8.GetBytes(y);

            if (xBytes == null && yBytes == null) return 0;

            int bytesToCompare = Math.Min(xBytes.Length, yBytes.Length);

            //Compare the bytes
            for (int index = 0; index < bytesToCompare; ++index)
            {
                byte xByte = xBytes[index];
                byte yByte = yBytes[index];

                int compareResult = Comparer<byte>.Default.Compare(xByte, yByte);

                //if not the same, then return the result of the comparison
                if (compareResult != 0) return compareResult;
            }
            if (xBytes.Length == yBytes.Length) return 0;

            return xBytes.Length < yBytes.Length ? -1 : 1;
        }
    }
}
