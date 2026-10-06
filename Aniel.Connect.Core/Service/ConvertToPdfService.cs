using Aniel.Connect.Core.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using NHibernate;
using OpenHtmlToPdf;
using OpenXmlPowerTools;
using RestSharp;
using SelectPdf;
using Sinapse.DAO;
using Sinapse.DAO.repository;
using Sinapse.Model.Identity;
using Sinapse.Util;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using Xceed.Words.NET;
using Alignment = Xceed.Words.NET.Alignment;
using Picture = Xceed.Words.NET.Picture;

namespace Aniel.Connect.Core.Service
{
    public class ConvertToPdfService
    {
        public static void modificarDocXGenerico(string caminho,
                                                 string caminhooutput,
                                                 string caminhohtml,
                                                 string caminhopdf,
                                                 string filename,
                                                 List<ReplaceDocXModel> lista_replace,
                                                 int? customWidth,
                                                 int? customHeight)
        {
            using (DocX documento = DocX.Load(caminho))
            {
                documento.RemoveProtection();

                foreach (ReplaceDocXModel replace in lista_replace)
                {
                    documento.ReplaceText(replace.Placeholder, replace.ReplaceValue);
                }

                if (!String.IsNullOrEmpty(filename))
                {
                    AddLogo(documento, filename);
                }
                documento.SaveAs(caminhooutput);
            }

            if (!String.IsNullOrEmpty(caminhohtml))
            {
                convertDocxToHtml(caminhohtml, caminhooutput);
            }

            if (!String.IsNullOrEmpty(caminhopdf))
            {
                if (customWidth.HasValue && customHeight.HasValue)
                {
                    var bytesPdf = gerarBoletoPDFPersonalizado(File.ReadAllText(caminhohtml), customWidth.Value, customHeight.Value);
                    File.WriteAllBytes(caminhopdf, bytesPdf);
                }
                else
                    convertHtmlToPdf(caminhopdf, caminhohtml);
            }

        }

        public static void incluirAceiteDigitalDocx(string caminho, string aceiteDigital)
        {
            using (DocX documento = DocX.Load(caminho))
            {
                documento.RemoveProtection();

                if (!documento.Paragraphs.Any(p => p.Text.Contains("Registro de Aceite Digital")))
                {
                    var partes = Regex.Split(aceiteDigital, @"(<b>.*?</b>)");

                    var aceiteDigitalFormatado = documento.InsertParagraph().FontSize(9);

                    foreach (var parte in partes)
                    {
                        if (parte.StartsWith("<b>") && parte.EndsWith("</b>"))
                        {
                            aceiteDigitalFormatado.Append(parte.Replace("<b>", "").Replace("</b>", "")).Bold();
                        }
                        else
                        {
                            aceiteDigitalFormatado.Append(parte);
                        }
                    }
                }

                documento.SaveAs(caminho);
            }
        }

        public static bool docContemRegistroDigital(string caminho)
        {
            using (DocX documento = DocX.Load(caminho))
            {
                documento.RemoveProtection();

                if (documento.Paragraphs.Any(p => p.Text.Contains("Registro de Aceite Digital")))
                    return true;
                else
                    return false;
            }
        }

        public static void modificarDocXContrato(ConvertToPDFServiceRequestModel request)
        {
            using (DocX documento = DocX.Load(request.caminho))
            {
                documento.AddFooters();
                documento.RemoveProtection();
                var variaveisEnumerados = new[] { "VAR_VALOR_FINAL_TOTAL_", "VAR_DESCONTO_", "VAR_VALOR_DESCONTO_TEMPORARIO_" };
                foreach (ReplaceDocXModel replace in request.lista_replace)
                {
                    try
                    {
                        // Se for uma variável enumerada (_1, _2, etc.)
                        var prefix = variaveisEnumerados.FirstOrDefault(v => replace.Placeholder.StartsWith(v));
                        if (prefix != null)
                        {
                            // Aqui você já sabe que é algo como VAR_DESCONTO_1, VAR_DESCONTO_2...
                            documento.ReplaceText(replace.Placeholder, replace.ReplaceValue, false, RegexOptions.None);
                            continue;
                        }

                        if (replace.Placeholder == "VAR_TERMO_ADESAO" || replace.Placeholder == "VAR_TERMO_ASSINATURA")
                        {
                            AddAssinatura(documento, replace.ReplaceValue);
                        }
                        else
                        {
                            var list_variaveis_not_bold = new List<string>() { "VAR_DETALHES_COMBO", "VAR_COMPOSICAO_COMBO", "VAR_PLANOS_PRODUTOS", "VAR_NUMERO_CONTRATO" };
                            var isBold = !list_variaveis_not_bold.Contains(replace.Placeholder);

                            if (replace.ReplaceValue.Any(P => P == '\n' || P == '\t'))
                            {
                                replace.ReplaceValue = new string(replace.ReplaceValue.Where(p => Char.GetUnicodeCategory(p) != UnicodeCategory.Format).ToArray());
                                var replaces = replace.ReplaceValue.Replace("\t", "\u00a0\u00a0").Split('\n').ToList();
                                try
                                {
                                    /*documento.ReplaceText(replace.Placeholder, $"{replaces.ElementAtOrDefault(0, replace.ReplaceValue)}" + "NEXT_TEXT", false, RegexOptions.None, new Formatting { Bold = isBold });
                                    var x = replaces.Where((p, i) => i > 0)?.ToList();
                                    if(x != null && x.Count > 0)
                                        documento.ReplaceText("NEXT_TEXT", $"{Environment.NewLine}{string.Join(Environment.NewLine, x)}", false, RegexOptions.None, new Formatting { Bold = false });
                                    else
                                        documento.ReplaceText("NEXT_TEXT", $"{Environment.NewLine}", false, RegexOptions.None, new Formatting { Bold = false });*/

                                    var buscaPlaceholder = documento.FindAll(replace.Placeholder);
                                    int? indexParagrafo = buscaPlaceholder.Count > 0 ? (int?)buscaPlaceholder.FirstOrDefault() : null;
                                    if (indexParagrafo != null)
                                    {
                                        Paragraph paragrafo = null;
                                        for (var i = 0; i < replaces.Count; i++)
                                        {
                                            //Somente a primeira linha em negrito
                                            if (i == 0)
                                            {
                                                paragrafo = documento.InsertParagraph((indexParagrafo.Value + replace.Placeholder.Length), replaces[i] + Environment.NewLine, false, new Formatting { Bold = isBold });
                                                indexParagrafo = documento.FindAll(replaces[i]).FirstOrDefault();
                                            }
                                            else
                                            {
                                                paragrafo = documento.Paragraphs.FirstOrDefault(p => p.Text == paragrafo.Text)?.Append(replaces[i] + Environment.NewLine);

                                                //documento.InsertParagraph(indexParagrafo, replaces[i], false, new Formatting { Bold = false });
                                                //indexParagrafo = documento.FindAll(replaces[i]).FirstOrDefault();
                                            }
                                        }

                                        documento.ReplaceText(replace.Placeholder, string.Empty, false, RegexOptions.None, new Formatting { Bold = false });
                                    }
                                    

                                    
                                }
                                catch (ArgumentOutOfRangeException ex)
                                {
                                    documento.ReplaceText(replace.Placeholder, replaces[0] + "$$", false, RegexOptions.None, new Formatting { Bold = isBold });
                                    var x = replaces.Where((p, i) => i != 0).ToList();
                                    documento.ReplaceText("$$", string.Join(Environment.NewLine, x), false, RegexOptions.None, new Formatting { Bold = false });
                                }
                                finally
                                {
                                    // 1. Corpo principal
                                    documento?.ReplaceText("\u200e", "");
                                    documento.Headers?.Odd?.ReplaceText("\u200e", "");
                                    documento.Headers?.Even?.ReplaceText("\u200e", "");
                                    documento.Headers?.First?.ReplaceText("\u200e", "");
                                    documento.Footers?.Odd?.ReplaceText("\u200e", "");
                                    documento.Footers?.Even?.ReplaceText("\u200e", "");
                                    documento.Footers?.First?.ReplaceText("\u200e", "");
                                }
                            }
                            else
                            {
                                //if (documento.Text.Contains(replace.Placeholder))
                                //{
                                //    var paragrafosEncontrados = documento.Paragraphs.Sum(p => Regex.Matches(p.Text, Regex.Escape(replace.Placeholder)).Count);
                                //    if (paragrafosEncontrados > 1)
                                //    {
                                //        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
                                //        foreach (var paragrafo in documento.Paragraphs.Where(p => Regex.IsMatch(p.Text, Regex.Escape(replace.Placeholder))))
                                //        {
                                //            var textNodes = paragrafo.Xml.Descendants(w + "t").ToList();

                                //            if (textNodes.Count > 0)
                                //            {   
                                //                string fullText = string.Concat(textNodes.Select(t => t.Value));
                                //                string newText = fullText.Replace(replace.Placeholder, replace.ReplaceValue);

                                //                textNodes[0].Value = newText;
                                //                for (int i = 1; i < textNodes.Count; i++)
                                //                {
                                //                    textNodes[i].Value = string.Empty;
                                //                }
                                //            }
                                //        }
                                //        //foreach (var paragrafo in documento.Paragraphs.Where(p => Regex.IsMatch(p.Text, Regex.Escape(replace.Placeholder))))
                                //        //{
                                //        //    var textNodes = paragrafo.Xml.Descendants(w + "t").ToList();

                                //        //    foreach (var node in textNodes)
                                //        //    {
                                //        //        if (node.Value == replace.Placeholder)
                                //        //        {
                                //        //            node.Value = replace.ReplaceValue;
                                //        //        }
                                //        //    }
                                //        //}
                                //    }
                                //    else
                                //    {
                                ReplaceExact(documento, replace.Placeholder, replace.ReplaceValue);
                                //    }
                                //}
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }

                if (!String.IsNullOrEmpty(request.filename))
                {
                    if (File.Exists(request.filename))
                    {
                        AddLogoTermo(documento, request.filename, request.LogoVar);
                    }
                }

                if (!String.IsNullOrEmpty(request.FilenameFooter))
                {
                    if (File.Exists(request.FilenameFooter))
                    {
                        AddFooterTermo(documento, request.FilenameFooter, request.FooterVar);
                    }
                }

                string caminhoTermoAdesao = $@"{CONSTANTES.CaminhoFisico}\Arquivos\TermoAdesao";

                if (!Directory.Exists(caminhoTermoAdesao))
                {
                    Directory.CreateDirectory(caminhoTermoAdesao);
                }

                //if (!File.Exists(request.caminhooutput))
                //{
                //    File.Create(request.caminhooutput);
                //}

                documento.SaveAs(request.caminhooutput);
            }

            if (!String.IsNullOrEmpty(request.caminhohtml))
            {
                convertDocxToHtml(request.caminhohtml, request.caminhooutput);
            }

            if (!String.IsNullOrEmpty(request.caminhopdf))
            {
                string orientation = !String.IsNullOrEmpty(request.orientationPdf) ? request.orientationPdf : "portrait";
                string zoomFactor = !String.IsNullOrEmpty(request.zoomFactorPdf) ? request.zoomFactorPdf : "1.3";
                convertHtmlToPdf(request.caminhopdf, request.caminhohtml, orientation, zoomFactor);
            }

        }

        public static void ReplaceExact(DocX documento, string placeholder, string replaceValue, bool bold = false)
        {
            XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

            foreach (var paragrafo in documento.Paragraphs)
            {
                // Reconstrói o texto inteiro do parágrafo
                string fullText = string.Concat(paragrafo.Xml.Descendants(w + "t").Select(t => t.Value));

                if (Regex.IsMatch(fullText, $@"\b{Regex.Escape(placeholder)}\b"))
                {
                    // Substitui todas as ocorrências
                    string newText = fullText.Replace(placeholder, replaceValue);

                    // Limpa todos os nós e coloca o texto novo apenas no primeiro
                    var textNodes = paragrafo.Xml.Descendants(w + "t").ToList();
                    if (textNodes.Count > 0)
                    {
                        textNodes[0].Value = newText;
                        for (int i = 1; i < textNodes.Count; i++)
                        {
                            textNodes[i].Value = string.Empty;
                        }
                    }
                }
            }
        }

        public static void converterDocXContrato(ConvertToPDFServiceRequestModel request)
        {
            using (DocX documento = DocX.Load(request.caminho))
            {
                documento.RemoveProtection();

                foreach (ReplaceDocXModel replace in request.lista_replace)
                {
                    if (replace.Placeholder == "VAR_TERMO_ADESAO" || replace.Placeholder == "VAR_TERMO_ASSINATURA")
                    {
                        AddAssinatura(documento, replace.ReplaceValue);
                    }
                    else
                    {
                        if (replace.ReplaceValue.Any(P => P == '\n' || P == '\t'))
                        {
                            replace.ReplaceValue = new string(replace.ReplaceValue.Where(p => Char.GetUnicodeCategory(p) != UnicodeCategory.Format).ToArray());
                            try
                            {
                                documento.ReplaceText(replace.Placeholder, replace.ReplaceValue, false, RegexOptions.None, new Formatting());
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                documento.ReplaceText(replace.Placeholder, replace.ReplaceValue, false, RegexOptions.None, new Formatting());
                            }
                            finally
                            {
                                // 1. Corpo principal
                                documento?.ReplaceText("\u200e", "");
                                documento.Headers?.Odd?.ReplaceText("\u200e", "");
                                documento.Headers?.Even?.ReplaceText("\u200e", "");
                                documento.Headers?.First?.ReplaceText("\u200e", "");
                                documento.Footers?.Odd?.ReplaceText("\u200e", "");
                                documento.Footers?.Even?.ReplaceText("\u200e", "");
                                documento.Footers?.First?.ReplaceText("\u200e", "");
                            }
                        }
                        else
                        {
                            documento.ReplaceText(replace.Placeholder, replace.ReplaceValue);
                        }
                    }
                }

                if (!String.IsNullOrEmpty(request.filename))
                {
                    if (File.Exists(request.filename))
                    {
                        AddLogoTermo(documento, request.filename, request.LogoVar);
                    }
                }

                if (!String.IsNullOrEmpty(request.FilenameFooter))
                {
                    if (File.Exists(request.FilenameFooter))
                    {
                        AddFooterTermo(documento, request.FilenameFooter, request.FooterVar);
                    }
                }

                string caminhoContrato = $@"{CONSTANTES.CaminhoFisico}\Arquivos\Modelo_Contrato";

                if (!Directory.Exists(caminhoContrato))
                {
                    Directory.CreateDirectory(caminhoContrato);
                }

                documento.SaveAs(request.caminhooutput);
            }

            if (!String.IsNullOrEmpty(request.caminhohtml))
            {
                convertDocxToHtml(request.caminhohtml, request.caminhooutput);
            }

            if (!String.IsNullOrEmpty(request.caminhopdf))
            {
                string orientation = !String.IsNullOrEmpty(request.orientationPdf) ? request.orientationPdf : "portrait";
                string zoomFactor = !String.IsNullOrEmpty(request.zoomFactorPdf) ? request.zoomFactorPdf : "1.3";
                convertHtmlToPdf(request.caminhopdf, request.caminhohtml, orientation, zoomFactor);
            }
        }

        public static void AddAssinatura(DocX documento, string caminho_assinatura)
        {
            using (ISession session = HibernateUtil.GetSession())
            using (var repoUser = new Conexao<IdentityUser>(session))
            {
                documento.RemoveProtection();

                if (!File.Exists(caminho_assinatura))
                {
                    throw new Exception("A assinatura fornecida não foi encontrada");
                }
                //var caminhoFoto = $@"C:\Users\Sinapse\Downloads\logo_ola.png";
                string termo = "TERMO_DE_ADESAO";

                /*    =---=   Listagem.    =---=*/
                bool tag_encontrada = documento.Paragraphs.ToList()

                                                          /*    =---=   Condicao de Captura.    =---=   */
                                                          .Where(

                                                                 p => p.Text.Trim() == termo
                                                                )

                                                          /*    =---=   Inserção da Assinatura no Documento.    =---=   */
                                                          .Select(p =>
                                                                      {
                                                                          try
                                                                          {
                                                                              var stream = new MemoryStream();
                                                                              var data = File.ReadAllBytes(caminho_assinatura);
                                                                              System.Drawing.Image imagem = ImageService.BytesToImage(data);
                                                                              imagem.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                                                              stream.Position = 0;

                                                                              Image assinatura = documento.AddImage(stream);
                                                                              Picture pictureAssinatura = assinatura.CreatePicture(130, 180);

                                                                              p.AppendPicture(pictureAssinatura);
                                                                              documento.ReplaceText(termo, "");

                                                                              return true;
                                                                          }
                                                                          catch (Exception)
                                                                          {
                                                                              return false;
                                                                          }

                                                                      })
                                                          .First();

                if (!tag_encontrada)
                {
                    throw new Exception($"Não foi encontrada a tag ({termo})");
                }
            }
        }

        public static void AddLogo(DocX documento, string caminho_logo)
        {
            using (ISession session = HibernateUtil.GetSession())
            using (var repoUser = new Conexao<IdentityUser>(session))
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
                    if (paragrafo.Text.Trim() == "(LOGO)")
                    {
                        Image logo = documento.AddImage(caminho_logo);
                        Picture pictureLogo = logo.CreatePicture();

                        pictureLogo.Height = 130;
                        pictureLogo.Width = 180;

                        paragrafo.AppendPicture(pictureLogo);
                        documento.ReplaceText("(LOGO)", "");

                        tag_encontrada = true;
                    }
                }

                if (!tag_encontrada)
                {
                    throw new Exception("Não foi encontrada a tag (LOGO)");
                }
            }
        }

        public static void AddLogoTermo(DocX documento, string caminho_logo, string logo_name)
        {
            using (ISession session = HibernateUtil.GetSession())
            using (var repoUser = new Conexao<IdentityUser>(session))
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
                    if (paragrafo.Text.Trim() == logo_name)
                    {
                        var stream = new MemoryStream();
                        var data = File.ReadAllBytes(caminho_logo);
                        System.Drawing.Image imagem = ImageService.BytesToImage(data);
                        imagem.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        stream.Position = 0;

                        Image logo = documento.AddImage(stream);
                        Picture pictureLogo = logo.CreatePicture();

                        pictureLogo.Height = imagem.Height;
                        pictureLogo.Width = imagem.Width;
                        //pictureLogo.Height = 130;
                        //pictureLogo.Width = 180;                       

                        paragrafo.AppendPicture(pictureLogo);
                        documento.ReplaceText(logo_name, String.Empty);
                        paragrafo.Alignment = Alignment.center;

                        tag_encontrada = true;
                    }
                }

                if (!tag_encontrada)
                {
                    //throw new Exception("Não foi encontrada a tag (LOGO)");
                }
            }
        }

        public static void AddFooterTermo(DocX documento, string caminho_footer, string footer_name)
        {
            using (ISession session = HibernateUtil.GetSession())
            using (var repoUser = new Conexao<IdentityUser>(session))
            {
                documento.RemoveProtection();

                if (!File.Exists(caminho_footer))
                {
                    throw new Exception("O footer fornecido não foi encontrado");
                }
                //var caminhoFoto = $@"C:\Users\Sinapse\Downloads\logo_ola.png";

                bool tag_encontrada = false;

                foreach (var paragrafo in documento.Paragraphs)
                {
                    if (paragrafo.Text.Trim() == footer_name)
                    {
                        /*Image footer = documento.AddImage(caminho_footer);
                        Picture pictureFooter = footer.CreatePicture();

                        pictureFooter.Height = 130;
                        pictureFooter.Width = Convert.ToInt32(documento.PageWidth);

                        paragrafo.AppendPicture(pictureFooter);
                        documento.ReplaceText(footer_name, String.Empty);

                        tag_encontrada = true;*/
                        //--------------------
                        var stream = new MemoryStream();
                        var data = File.ReadAllBytes(caminho_footer);
                        System.Drawing.Image imagem = ImageService.BytesToImage(data);
                        imagem.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        stream.Position = 0;

                        //Image imageFoto = new ImageUtils().Base64ToImage(foto64);
                        Xceed.Words.NET.Image image = documento.AddImage(stream);

                        Xceed.Words.NET.Picture pictureFooter = image.CreatePicture();
                        pictureFooter.Height = 150;
                        pictureFooter.Width = 1000;

                        documento.ReplaceText(footer_name, String.Empty);
                        paragrafo.AppendPicture(pictureFooter).Alignment = Alignment.left;

                        //paragrafo.Alignment = Alignment.center;


                        tag_encontrada = true;
                    }
                }

                if (!tag_encontrada)
                {
                    //throw new Exception("Não foi encontrada a tag (LOGO)");
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
                Xceed.Words.NET.Image logo = documento.AddImage(@"C:\Doc\win98.png");
                Xceed.Words.NET.Picture pictureLogo = logo.CreatePicture();
                pictureLogo.Height = 48;
                pictureLogo.Width = 240;
                documento.Tables[0].Rows[0].Cells[0].Paragraphs[0].AppendPicture(pictureLogo);

                documento.ReplaceText("(NUM_DOC)", "12345");
                documento.ReplaceText("(TITULAR)", "TESTE");

                documento.ReplaceText("(FOTOS)", String.Empty);


                Xceed.Words.NET.Image imagem = documento.AddImage(@"C:\Doc\Quake2.png");
                Xceed.Words.NET.Picture picture = imagem.CreatePicture();
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
                        AdditionalCss = @"p { margin-top: 0cm !important; margin-bottom: 0cm !important; }
                        span:not([class]) { font-weight: bold; }",

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
                    var head = html.Element(html.Name.Namespace + "head");

                    if (head != null)
                    {
                        var styleElement = head.Element(html.Name.Namespace + "style");

                        if (styleElement != null)
                        {
                            string originalCss = styleElement.Value;

                            // Regex para remover a regra span { white-space: pre-wrap; }
                            // Esta Regex lida com espaços, quebras de linha e comentários ao redor.
                            const string SpanCssRegex = @"\s*span\s*\{\s*white-space\s*:\s*pre-wrap\s*;\s*\}\s*";

                            string newCss = Regex.Replace(originalCss,
                                                          SpanCssRegex,
                                                          "",
                                                          RegexOptions.IgnoreCase | RegexOptions.Multiline).Trim();

                            // 4. Atualizar o conteúdo do <style> ou removê-lo se estiver vazio
                            if (string.IsNullOrWhiteSpace(newCss))
                            {
                                styleElement.Remove();
                            }
                            else
                            {
                                styleElement.Value = newCss;
                            }
                        }
                    }

                    // Regex para capturar <span ...>https://... </span>
                    string pattern = @"<span[^>]*>(https?://[^<]+)</span>";

                    // Substitui pelo <a href="...">...</a>
                    string replacement = "<a href=\"$1\" style=\"word-break: break-word;\">$1</a>";

                    var htmlText = Regex.Replace(
                        html.ToStringNewLineOnAttributes()
                            .Replace("(pagebreak)", "<div style=\"page-break-before: always;\"></div> ")
                            .Replace("#x200e", "")
                            .Replace("&;", ""),
                        pattern,
                        replacement,
                        RegexOptions.IgnoreCase
                    );

                    htmlText = htmlText.Replace("&lt;", "<").Replace("&gt;", ">");

                    File.WriteAllText(caminhoHtml, htmlText);
                }
            }
        }
        public static void convertHtmlToPdf(string caminhoPdf, string caminhoHtml, string orientation = "portrait", string zoomFactor = "1.3")
        {
            String html = IncluirBody(File.ReadAllText(caminhoHtml));

            var pdfBytes = getPdfBytesFromHtmlString(html, orientation, zoomFactor);
            File.WriteAllBytes(caminhoPdf, pdfBytes);
        }
        public static void convertHtmlToPdfLandscape(string caminhoPdf, string caminhoHtml)
        {
            String html = IncluirBody(File.ReadAllText(caminhoHtml));

            var pdfBytes = getPdfBytesFromHtmlString(html, "landscape");
            File.WriteAllBytes(caminhoPdf, pdfBytes);
        }

        public static byte[] getPdfBytesFromHtmlString(string html, string orientation = "portrait", string zoomFactor = "1.0")
        {
            return Pdf.From(html)
                  .WithGlobalSetting("size.paperSize", "A4")
                  .WithGlobalSetting("orientation", orientation)
                  .WithObjectSetting("web.printMediaType", "true")
                  .WithObjectSetting("load.zoomFactor", zoomFactor)
                  .WithObjectSetting("web.defaultEncoding", "utf-8")
                  .WithObjectSetting("imageDPI", "1")
                  .WithMargins(1.Centimeters())
                .Content();
        }
        public static byte[] GetPdfBytesFromHtmlStringMTA(string html, string orientation = "portrait")
        {

            HtmlToPdf converter = new HtmlToPdf();

            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = orientation.ToLower() == "portrait" ? PdfPageOrientation.Portrait : PdfPageOrientation.Landscape;

            //converter.Options.WebPageWidth = (int)(1024 * 1.1); zoom factor
            //converter.Options.DefaultEncoding = "utf-8";
            //converter.Options.PdfCompressionLevel = 1;
            //converter.Options.MarginLeft = -10;
            //converter.Options.MarginRight = -10;
            //converter.Options.MarginTop = -10;
            //converter.Options.MarginBottom = -10;

            PdfDocument doc = converter.ConvertHtmlString(html);

            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                doc.Save(ms);
                pdfBytes = ms.ToArray();
                doc.Close();
            }

            return pdfBytes;
        }
        public static string getHtmlFromExternalSource(string url)
        {
            string html;
            try
            {
                var client = new RestClient(url);

                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("charset", "utf-8");
                request.AddHeader("cache-control", "no-cache");

                html = client.Execute(request)?.Content;
            }
            catch (Exception ex)
            {
                CONSTANTES.log4Net.ErrorFormat("Mensagem da exception: {0} \r\n Mensagem da InnerException: {1}\r\n StackTrace:{2}",
                                                ex.Message, ex?.InnerException?.Message, ex.StackTrace);
                html = string.Empty;
            }
            return html;
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
            foreach (Xceed.Words.NET.Cell cell in document.Tables[0].Rows[row].Cells)
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
            foreach (Xceed.Words.NET.Row row in document.Tables[0].Rows)
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

        public static byte[] gerarBoletoPDFPersonalizado(string boletoHTML, int width, int height)
        {
            var converter = new NReco.PdfGenerator.HtmlToPdfConverter();
            // Set the page size to 80mm width and custom height (e.g., 297mm for A4 height or another height)
            converter.PageWidth = width;  // Width in millimeters
            converter.PageHeight = height; // Height in millimeters (you can adjust this based on your content)

            // Optional: You can also adjust margins if needed
            converter.Margins.Top = 0;  // Top margin in millimeters
            converter.Margins.Bottom = 0; // Bottom margin in millimeters
            converter.Margins.Left = 0;   // Left margin in millimeters
            converter.Margins.Right = 0;  // Right margin in millimeters;
            byte[] boletoPDF = converter.GeneratePdf(boletoHTML);

            return boletoPDF;
        }
    }
}
