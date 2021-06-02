using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTranslator
{
    
    public static class Utility
    {
        public static string convert(ArrayList arrayList)
        {
            var strings = arrayList.Cast<string>().ToArray();
            var theString = string.Join(" ", strings);
            return theString;
        }
    }
}
