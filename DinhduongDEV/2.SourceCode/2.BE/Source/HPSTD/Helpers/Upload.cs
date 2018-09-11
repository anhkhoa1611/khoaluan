using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Web;

namespace HPSTD.Helpers
{
    public class Upload
    {
        public static string UploadFile(string FolderName, System.Web.HttpPostedFileBase file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string refix = "[" + fileName + "]_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string extension = Path.GetExtension(file.FileName);
            string FolderPath = System.Web.HttpContext.Current.Server.MapPath("~/UploadFile/" + FolderName);
            
            var destinationPath = Path.Combine(FolderPath, refix + extension);

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            DirectoryInfo dInfo = new DirectoryInfo(FolderPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
            file.SaveAs(destinationPath);
            return Path.Combine("/UploadFile/" + FolderName, refix + extension);
        }
    }
}