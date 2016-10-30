using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stegPOC
{
    public class EncodingHelper
    {

        public static byte[] DecodeToByteArray(string s)
        {
            string[] strings = s.Split('-');

            byte[] returnArr = new byte[strings.Length];

            int pointer = 0;

            while(pointer < strings.Length)
            {
                returnArr[pointer] = Convert.ToByte(Convert.ToInt32(strings[pointer]));
                pointer++;
            }

            return returnArr;
        }
        

        public static string EncodeToString(byte[] array)
        {
            string returnString = "";

            foreach(var b in array)
            {
                returnString += Convert.ToInt32(b) + "-";
            }


            return returnString.TrimEnd(new char[] {'-' });
        }


        /*
        public static string EncodeToString(byte b)
        {
            var intRes = Convert.ToInt32(b);

            if(intRes < 10)
            {
                return intRes.ToString();
            }

            switch(intRes)
            {
                case 10:
                    return "A";
                case 11:
                    return "B";
                case 12:
                    return "C";
                case 13:
                    return "D";
                case 14:
                    return "E";
                case 15:
                    return "F";
                default:
                    throw new Exception("WTF?");
            }
        }*/
    }
}