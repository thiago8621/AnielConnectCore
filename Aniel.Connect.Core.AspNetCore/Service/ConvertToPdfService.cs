using Aniel.Connect.Core.AspNetCore.Models;
using DocumentFormat.OpenXml.Packaging;
using NHibernate;
using OpenHtmlToPdf;
using OpenXmlPowerTools;
using Sinapse.DAO.AspNetCore.repository;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace Aniel.Connect.Core.AspNetCore.Service
{
    public class ConvertToPdfService
    {
        public static void modificarDocXGenerico(string caminho, string caminhooutput, string caminhohtml, string caminhopdf, string filename, List<ReplaceDocXModel> lista_replace)
        {
            using (DocX documento = DocX.Load(caminho))
            {
                documento.RemoveProtection();

                foreach (ReplaceDocXModel replace in lista_replace)
                {
                    documento.ReplaceText(replace.Placeholder, replace.ReplaceValue);
                }

                AddLogo(documento, filename); 
                documento.SaveAs(caminhooutput);
            }

            convertDocxToHtml(caminhohtml, caminhooutput);
            convertHtmlToPdf(caminhopdf, caminhohtml);
        }

        public static void AddLogo(DocX documento, string caminho_logo)
        {
            using (ISession session = HibernateUtil.GetSession())
            {
                documento.RemoveProtection();

                if (!File.Exists(caminho_logo))
                {
                    throw new Exception("A logo fornecida não foi encontrada");
                }
                //var caminhoFoto = $@"C:\Users\Sinapse\Downloads\logo_ola.png";

                bool tag_encontrada = false;

                foreach (var paragrafo in documento.Paragraphs)
                {
                    if (paragrafo.Text.Trim() == "(LOGOMARCA)")
                    {
                        Image logo = documento.AddImage(caminho_logo);
                        Picture pictureLogo = logo.CreatePicture();

                        pictureLogo.Height = 130;
                        pictureLogo.Width = 180;

                        paragrafo.AppendPicture(pictureLogo);
                        documento.ReplaceText("(LOGOMARCA)", "");

                        tag_encontrada = true;
                    }
                }

                if (!tag_encontrada)
                {
                    throw new Exception("Não foi encontrada a tag (LOGOMARCA)");
                } 
            }
        }

        public static void modificarDocX(string caminho)
        {
            using (DocX documento = DocX.Load(caminho))
            {
                Int32 somaLinha = 0;
                Int32 LinhaQuiz = CriaLinhasTabela(documento, "(CHECKLIST)", somaLinha);
                Int32 CellInicChecklist = GetCellByTag(documento, LinhaQuiz, "(CHECKLIST)");

                documento.RemoveProtection();
                Image logo = documento.AddImage(@"C:\Doc\win98.png");
                Picture pictureLogo = logo.CreatePicture();
                pictureLogo.Height = 48;
                pictureLogo.Width = 240;
                documento.Tables[0].Rows[0].Cells[0].Paragraphs[0].AppendPicture(pictureLogo);

                documento.ReplaceText("(NUM_DOC)", "12345");
                documento.ReplaceText("(TITULAR)", "TESTE");

                documento.ReplaceText("(FOTOS)", String.Empty);


                Image imagem = documento.AddImage(@"C:\Doc\Quake2.png");
                Picture picture = imagem.CreatePicture();
                picture.Height = 200;
                picture.Width = 100;
                //documento.InsertParagraph().AppendPicture(picture);
                documento.Tables[0].Rows[LinhaQuiz].Cells[CellInicChecklist + 1].Paragraphs[0].AppendPicture(picture);

                documento.SaveAs(@"C:\Doc\modeloAlterado.docx");
                System.Console.WriteLine("DocX Modificado!");
            }
        }
        public static void convertDocxToHtml(string caminhoHtml, string caminhoDocx)
        {
            byte[] byteArray = File.ReadAllBytes(caminhoDocx);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteArray, 0, byteArray.Length);
                using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                {
                    int imageCounter = 0;
                    HtmlConverterSettings settings = new HtmlConverterSettings()
                    {
                        
                        PageTitle = "Relatório",
                        //O ImageHandler checa a extensão da imagem que existe no arquivo Word para poder convertê-la corretamente para o HTML
                        ImageHandler = imageInfo =>
                        {
                            ++imageCounter;
                            string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                            ImageFormat imageFormat = null;
                            if (extension == "png") imageFormat = ImageFormat.Png;
                            else if (extension == "gif") imageFormat = ImageFormat.Gif;
                            else if (extension == "bmp") imageFormat = ImageFormat.Bmp;
                            else if ((extension == "jpeg") || (extension == "jpg")) imageFormat = ImageFormat.Jpeg;
                            else if (extension == "tiff")
                            {
                                extension = "gif";
                                imageFormat = ImageFormat.Gif;
                            }
                            else if (extension == "x-wmf")
                            {
                                extension = "wmf";
                                imageFormat = ImageFormat.Wmf;
                            }

                            if (imageFormat == null) return null;

                            string base64 = null;
                            try
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    imageInfo.Bitmap.Save(ms, imageFormat);
                                    var ba = ms.ToArray();
                                    base64 = System.Convert.ToBase64String(ba);
                                }
                            }
                            catch (System.Runtime.InteropServices.ExternalException)
                            { return null; }

                            ImageFormat format = imageInfo.Bitmap.RawFormat;
                            ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders()
                                                      .First(c => c.FormatID == format.Guid);
                            string mimeType = codec.MimeType;

                            string imageSource =
                                   string.Format("data:{0};base64,{1}", mimeType, base64);

                            XElement img = new XElement(Xhtml.img,
                                  new XAttribute(NoNamespace.src, imageSource),
                                  imageInfo.ImgStyleAttribute,
                                  imageInfo.AltText != null ?
                                       new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                            return img;
                        }

                    };
                    XElement html = HtmlConverter.ConvertToHtml(doc, settings);
                    //string caminho_html = @"C:\Doc\Test.html";

                    File.WriteAllText(caminhoHtml, html.ToStringNewLineOnAttributes());
                }
            }
        }
        public static void convertHtmlToPdf(string caminhoPdf, string caminhoHtml)
        {
            String html = IncluirBody(File.ReadAllText(caminhoHtml));

            var pdfBytes = getPdfBytesFromHtmlString(html);
            File.WriteAllBytes(caminhoPdf, pdfBytes);
        }

        public static byte[] getPdfBytesFromHtmlString(string html)
        {
            return Pdf.From(html)
                  .WithGlobalSetting("size.paperSize", "A4")
                  .WithObjectSetting("web.printMediaType", "true")
                  .WithObjectSetting("load.zoomFactor", "1.3")
                  .WithObjectSetting("web.defaultEncoding", "utf-8")
                  .WithObjectSetting("imageDPI", "1")
                  .WithMargins(1.Centimeters())
                .Content();
        }

        private static String IncluirBody(String html)
        {
            //String css = "body { backgroud-color: #00FF00 !important; }";

            return $@"<!DOCTYPE html>
                        <html lang='pt'>
                          <head>
                            <meta charset='UTF-8' />
                            <meta name='viewport' content='width=device-width, initial-scale=1.0' />                           
                          </head>
                         <body style= 'background-color: #FFFFFF !important'>{html}</body>
                      </html>";
        }

        private static Int32 GetCellByTag(DocX document, Int32 row, String Tag)
        {
            Int32 intCell = 0;
            foreach (Xceed.Document.NET.Cell cell in document.Tables[0].Rows[row].Cells)
            {
                intCell++;
                if (cell.Xml.Value.Contains(Tag))
                {
                    intCell--;
                    break;
                }
            }
            return intCell;
        }
        private static Int32 GetLinhaTabela(DocX document, String Tag)
        {
            Int32 linha = 0;
            foreach (Xceed.Document.NET.Row row in document.Tables[0].Rows)
            {
                linha++;
                if (row.Xml.Value.Contains(Tag))
                {
                    linha--;
                    break;
                }
            }
            return linha;
        }
        private static Int32 CriaLinhasTabela(DocX document, String Tag, Int32 QtdeLinhas)
        {
            Int32 linha = GetLinhaTabela(document, Tag);

            if (document.Tables[0].RowCount <= linha)
                return 0;

            try
            {
                for (int i = 0; i < QtdeLinhas; i++)
                {
                    document.Tables[0].InsertRow(document.Tables[0].Rows[linha], linha);
                }
            }
            catch { }

            return linha;
        }
    }
}
