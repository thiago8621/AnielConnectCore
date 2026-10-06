using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sinapse.DAO.AspNetCore;
using System;
using System.Net.Mime;

namespace Aniel.Connect.Core.AspNetCore
{
    public class FileController<TUser> : BaseController<TUser> where TUser : class
    {
        public FileController(IHttpContextAccessor httpContextAccessor, UserManager<TUser> userManager) 
            : base(httpContextAccessor, userManager, "File", CONSTANTES_CORE.URLPortal) { }

        public IActionResult Download(String Path,String FileName,String isPdf = "n")
        {
            if (!String.IsNullOrEmpty(Path) && Path != "null")
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Path);

                if (isPdf == "s")
                    return File(fileBytes, MediaTypeNames.Application.Pdf, FileName);
                else
                    return File(fileBytes, MediaTypeNames.Application.Octet, FileName);
            }
           
            return null;
        }

        public IActionResult DownloadPdf(String Path, String FileNames, String isPdf = "n")
        {
            var lstNomes = FileNames.Split(',');

            foreach (var item in lstNomes)
            {
                if (!String.IsNullOrEmpty(Path) && Path != "null" && !String.IsNullOrEmpty(item))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(Path+item);
                    //return File(fileBytes, MediaTypeNames.Application.Octet, item);
                    return File(fileBytes, MediaTypeNames.Application.Octet, item);
                }
            }

            return null;
        }
    }
}