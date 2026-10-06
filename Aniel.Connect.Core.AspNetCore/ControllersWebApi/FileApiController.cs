using Aniel.Connect.Core.AspNetCore.Models;
using Aniel.Connect.Core.AspNetCore.Service;
using Aniel.Connect.Core.AspNetCore.Swagger;
using Microsoft.AspNetCore.Mvc;
using Sinapse.DAO.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aniel.Connect.Core.AspNetCore.ControllersWebApi
{
    [HideInDocs]
    public class FileApiController : Controller
    {
        [HttpPost]
        [Route("api/FileApi/GerarArquivoDownload")]
        public PostApiRetorno GerarArquivoDownload(PostGerarPdfModel PostGerarPdfModel)
        {
            PostApiRetorno retorno = new PostApiRetorno();

            try
            {
                if (!String.IsNullOrEmpty(PostGerarPdfModel.caminho) && PostGerarPdfModel.caminho != "null")
                {
                    if (PostGerarPdfModel.lista_replace == null)
                        PostGerarPdfModel.lista_replace = new List<ReplaceDocXModel>();
                    else
                        PostGerarPdfModel.lista_replace = PostGerarPdfModel.lista_replace.Select(p =>
                        {
                            p.ReplaceValue = p.ReplaceValue ?? String.Empty;
                            return p;
                        }).ToList();

                    ConvertToPdfService.modificarDocXGenerico(PostGerarPdfModel.caminho, PostGerarPdfModel.caminhooutput, PostGerarPdfModel.caminhohtml,
                                                              PostGerarPdfModel.caminhopdf, PostGerarPdfModel.filename, PostGerarPdfModel.lista_replace);
                    retorno.Ok = true;
                }
            }
            catch (Exception ex)
            {
                
                retorno.Ok = false;

                if (ex.GetType().Name == "FileNotFoundException")
                    retorno.Mensagem = "Arquivo não Encontrado";
                else
                    retorno.Mensagem = ex.Message;

                if (ex.InnerException != null)
                    retorno.InnerException = ex.InnerException.Message;
                
                CONSTANTES_CORE.log4Net.Error(ex);
                //throw ex;
            }

            return retorno;
        }
    }
}
