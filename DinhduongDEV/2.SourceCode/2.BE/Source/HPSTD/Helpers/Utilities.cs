using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HPSTD.Core.Entities;
using System.Web.Mvc;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;

namespace HPSTD.Helpers
{
    public class Utilities
    {
        public static IEnumerable<Menu> getAllMenu()
        {
            using (var dbConn = HPSTD.Helpers.OrmliteConnection.openConn())
            {
                var lst = dbConn.Select<Menu>();
                return lst;
            }
        }
        public static IEnumerable<Menu> getRootMenu()
        {
            using (var dbConn = HPSTD.Helpers.OrmliteConnection.openConn())
            {
                var lst = dbConn.Select<Menu>("id_cha = -1").OrderBy(s => s.thu_tu);
                return lst;
            }
        }

        public static IEnumerable<Parameters> getDomain()
        {
            using (var dbConn = HPSTD.Helpers.OrmliteConnection.openConn())
            {
                var lst = dbConn.Select<Parameters>("loai_tham_so = 'DOMAIN'");
                return lst;
            }
        }
    }
}