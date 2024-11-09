using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API.Misc
{
    public class PageResponse<T> : IEnumerable<T>
        where T: class
    {
        public ReadOnlyCollection<T> List { get; }
        public string? NextPageCursor { get; }
        public string? PreviousPageCursor { get; }

        public PageResponse(List<T> list, string? nextPageCursor, string? previousPageCursor)
        {
            List = list.AsReadOnly();
            NextPageCursor = nextPageCursor;
            PreviousPageCursor = previousPageCursor;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
