using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPqDASR.Extensions
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converte uma string para enumerador
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (Exception)
            {
                return (T)Enum.Parse(typeof(T), "RECOGNIZED", true);
            }
        }
    }
}
