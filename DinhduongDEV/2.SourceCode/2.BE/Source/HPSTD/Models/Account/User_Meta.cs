using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using ServiceStack.DataAnnotations;

namespace HPSTD.Models
{
    /// <summary>
    /// This object represents the properties and methods of a User_Meta.
    /// </summary>
    public class User_Meta
    {
        public string UserName { get; set; }
        public string Factor { get; set; }
        public string Value { get; set; }
        [AutoIncrement]
        public int RowID { get; set; }
        public DateTime RowCreatedTime { get; set; }
        public string RowCreatedUser { get; set; }
        public DateTime RowLastUpdatedTime { get; set; }
        public string RowLastUpdatedUser { get; set; }
        
    }
}
