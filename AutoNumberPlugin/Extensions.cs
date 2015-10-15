using System;

namespace AutoNumberPlugin
{
    public static class Extensions
    {
        public static decimal ToDecimal(this object o)
        {
            decimal r = 0;
            if (string.IsNullOrEmpty(o.ToString()))
            {
                return r;
            }
            try
            {
                r = Convert.ToDecimal(o);
            }
            catch
            {
            }
            return r;
        }
 
    }
}