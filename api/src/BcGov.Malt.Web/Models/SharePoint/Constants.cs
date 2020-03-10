using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BcGov.Malt.Web.Models.SharePoint
{
    public static class Constants
    {
        public static class UserOperations
        {
            public const string Claims = "i:0e.t|adfs|";
            public const string UserType = "SP.User";
        }

        public static class SiteGroupNames
        {
            public const string Members = " Members";
            public const string Owners = " Owners";
            public const string Visitors = " Visitors";
        }
    }
}
