using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core
{
    /// <summary>
    /// utility methods
    /// </summary>
    public class Utilities
    {

        /// <summary>
        /// returns true if item is in array
        /// </summary>
        /// <param name="array">array to be searched</param>
        /// <param name="item">item that is being searched</param>
        /// <returns></returns>
        public static bool ArrayContains(string[] array, string item)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
        
    }
}
