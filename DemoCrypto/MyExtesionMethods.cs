using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    public static class MyExtensionMethods
    {
        public static int WordCount(this String str)
        {
            return str.Split(new char[] { ' ', '.', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static DateTime ToDateTime(this long time)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime;
        }

        public static string ShortTimeStr(this long time)
        {
            return time.ToDateTime().ToShortDateString() + " " + time.ToDateTime().ToShortTimeString();
        }

        public static string LocalTimeStr(this long time)
        {
            return time.ToDateTime().ToLocalTime().ToShortDateString() + " " + time.ToDateTime().ToLocalTime().ToShortTimeString();
        }

    } // class
} // namespace
