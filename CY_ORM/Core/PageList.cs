using System.Collections.Generic;

namespace CY_ORM
{
    public class PageList<T> where T : class
    {
        public PageList()
        {
            PageIndex = 0;
            PageSize = 0;
            TotalRowCount = 0;
            TotalPageCount = 0;
            Rows = new List<T>();
        }

        public PageList(long pageIndex, int pageSize, List<T> rows, long totalRowCount)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalRowCount = totalRowCount;
            TotalPageCount = totalRowCount / pageSize;
            if (TotalPageCount * pageSize < totalRowCount)
            {
                TotalPageCount++;
            }
            Rows = rows;
        }

        public List<T> Rows { get; internal set; }
        public long TotalRowCount { get; internal set; }
        public long TotalPageCount { get; internal set; }
        public int PageSize { get; internal set; }
        public long PageIndex { get; internal set; }
    }
}
