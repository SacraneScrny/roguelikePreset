using System.Collections.Generic;

namespace Sackrany.Extensions
{
    public static class ListExtensions
    {
        public static bool FastRemove<T>(this List<T> list, T item)
            where T : class
        {
            int index = list.IndexOf(item);
            if (index < 0) return false;
    
            int lastIndex = list.Count - 1;
            if (index != lastIndex)
            {
                list[index] = list[lastIndex];
            }
    
            list.RemoveAt(lastIndex); 
            return true;
        }
    }
}