using Aniel.Connect.Core.Models;
using Aniel.Connect.Core.Service;
using Aniel.Connect.Core.Swagger;
using Sinapse.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Aniel.Connect.Core.ControllersWebApi
{
    [HideInDocs]
    public class FileApiController : ApiController
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
                                                              PostGerarPdfModel.caminhopdf, PostGerarPdfModel.filename, PostGerarPdfModel.lista_replace, 
                                                              PostGerarPdfModel.customWidth,PostGerarPdfModel.customHeight);
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

                CONSTANTES.log4Net.Error(ex);
                //throw ex;
            }

            return retorno;
        }
    }
}
