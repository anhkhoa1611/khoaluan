using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HPSTD.Models
{
    public class uFile
    {
        //
        // GET: /uFile/
        public string FileName { get; set; }

        public string PID { get; set; }
        

       
         public uFile()
        { }
         public uFile(string fname, string pid)
         {
             this.FileName = fname;
             this.PID = pid;
         
         }

    }
}
