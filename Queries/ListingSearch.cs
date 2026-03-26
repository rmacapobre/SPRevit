using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specpoint.Revit2026
{
    public class ListingMatch
    {
        public string firmId { get; set; }
        public string firmName { get; set; }
    }

    public class ManufacturerSearchResultOfListingMatch
    {
        public IList<ListingMatch> matches { get; set; }
        public long count { get; set; }

    }

    public class ListingSearch
    {
        public ManufacturerSearchResultOfListingMatch listingSearch { get; set; }
    }
}
