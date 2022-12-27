using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace NetStarter.Helper
{
    public class PaginatedList<T> : List<T>
    {
        #region Variables

        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }

        #endregion Variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedList"/> class.
        /// <param name="items">The items.</param>
        /// <param name="pageCount">The page count.</param>
        /// <param name="currentPage">The current page.</param>
        /// <param name="pageSize">The page size.</param>
        /// </summary>
        public PaginatedList(List<T> items, int pageCount, int currentPage, int pageSize)
        {
            PageSize = pageSize;
            CurrentPage = currentPage;
            PageCount = pageCount;
            AddRange(items);
        }

        #endregion Constructor

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int currentPage, int pageSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (!(source is IEnumerable<T>))
                return new PaginatedList<T>(source.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList(), (int)Math.Ceiling((double)source.Count() / pageSize), currentPage, pageSize);
            else
                return new PaginatedList<T>(await source.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync(), (int)Math.Ceiling((double)await source.CountAsync() / pageSize), currentPage, pageSize);
        }

        public static PaginatedList<T> Create(List<T> source, int currentPage, int pageSize)
        {
            var count = source.Count;
            var receipts = source.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedList<T>(receipts, count, currentPage, pageSize);
        }
    }
}