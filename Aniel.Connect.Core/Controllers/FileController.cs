using Aniel.Connect.ControllersWebApi;
using Aniel.Connect.Core;
using Aniel.Connect.Core.Models;
using Aniel.Connect.Core.Service;
using Sinapse.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Aniel.Connect.Core
{
    public class FileController : BaseController
    {

        public FileController() : base("File",CONSTANTES.URLPortal)
                                                   
        {

        }

        // GET: File

        public ActionResult Download(String Path,String FileName,String isPdf = "n")
        {
            if (!String.IsNullOrEmpty(Path) && Path != "null")
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Path);

                if(isPdf == "s")
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, FileName);
                else
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);
            }
           
            return null;
        }


        public ActionResult DownloadPdf(String Path, String FileNames, String isPdf = "n")
        {
            var lstNomes = FileNames.Split(',');


            foreach (var item in lstNomes)
            {
                if (!String.IsNullOrEmpty(Path) && Path != "null" && !String.IsNullOrEmpty(item))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(Path+item);
                    //return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, item);
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, item);
                }
            }

            

            return null;
        }

    }
}