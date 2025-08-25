using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gold.Api.Utilities
{
    public class PagedList<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; }
        public PagedList (IQueryable<T> items, int pageNum, int pageSize)
        {
            PageNumber = pageNum;
            PageSize = pageSize;
            var pageAmount = items.Count() / pageSize;
            pageAmount++;
            int next = 0;
            var temp = new T[pageAmount][];
            for (int i = 0; i < pageAmount; i++)
                temp[i] = new T[pageSize];
            
            int page = 0;
            int item = 0;

            foreach(var it in items)
            {
                
            }
        }
    }
}
