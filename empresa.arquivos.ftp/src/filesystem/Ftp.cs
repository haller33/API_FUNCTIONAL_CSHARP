using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Configuration;
using Empresa.integracao.ftp.log;

namespace Empresa.integracao.ftp.filesystem
{
    public class Ftp
    {
        private const string USUARIO = "anonymous";
        private const string SENHA = "something@gmail.com";
        /// private static string diretorioLocal = Path.GetTempPath() + "IMPORTACAO_ARQUIVO\\";
        private static bool EnableSsl = false;

        //public static List<CaminhosFtpDto> Acesso()
        //{
        //    string arquivos = "";
        //    List<CaminhosFtpDto> lista = new List<CaminhosFtpDto>();
        //    //List<string> parceiros = Parceiros.ListaParceiros();
        //    List<string> parceiros = ListarParceiros();

        //    for (int i = 0; parceiros.Count > i; i++)
        //    {
        //        arquivos = arquivos + AcessarFtp(parceiros[i]);
        //        if (!String.IsNullOrEmpty(arquivos))
        //        {
        //            lista.Add(
        //                new CaminhosFtpDto()
        //                {
        //                    Parceiro = parceiros[i],
        //                    Arquivos = arquivos.Split(',')
        //                });
        //            arquivos = String.Empty;
        //        }
        //    }
        //    return lista;
        //}

        public static string AcessarFtp(string caminho, string extensao)
        {
            string arquivos = "";
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(RetornarUrl(caminho) + "*." + extensao);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(USUARIO, SENHA);
                request.EnableSsl = EnableSsl;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream, true))
                        {
                            string todosDiretoriosString = reader.ReadToEnd();
                            string[] linha = todosDiretoriosString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                            arquivos = String.Join(",", linha.Where(i => !string.IsNullOrWhiteSpace(i)));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, ":ERROR:: Problema Em AcessarFTP ::ERROR");
                CentralLog.LogInfo(RetornarUrl(caminho) + "*." + extensao);
            }

            return arquivos;
        }

        public static string[] LerArquivo(string path)
        {
            try
            {
                var uri = RetornarUrl(path);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(USUARIO, SENHA);
                request.EnableSsl = EnableSsl;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                var lines = new List<string>();

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream, true))
                        {
                            while (reader.Peek() >= 0)
                            {
                                lines.Add(reader.ReadLine());
                            }

                        }
                    }
                }
                return lines.ToArray();
            }
            catch (Exception ex)
            {
                CentralLog.LogError(ex, ":ERROR:: Problema Em LerArquivo ::ERROR");
                CentralLog.LogInfo(RetornarUrl(path));
                return new string[] { };
            }
        }

        public static bool FileExist(string path)
        {
            bool exists = false;
            var uri = RetornarUrl(path);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(USUARIO, SENHA);
            request.EnableSsl = EnableSsl;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            try
            {
               var response = (FtpWebResponse)request.GetResponse();
                exists = response.StatusCode == FtpStatusCode.OpeningData;
                if (response != null)
                    response.Close();
            }
            catch (Exception e)
            {
                exists = false;
                CentralLog.LogError(e, ":ERROR:: Problema Em FileExist ::ERROR");
            } finally
            {

            }

            return exists;
        }

        public static void Download(string caminho, string arquivo, string diretorioLocal)
        {
            if (!Directory.Exists(diretorioLocal))
            {
                Directory.CreateDirectory(diretorioLocal);
            }

            try
            {
                // var urlof = HttpUtility.UrlEncode("FTP/PDA/dados_de_beneficiarios_por_operadora/sib_ativos.zip");
                // FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://191.238.223.106:21/" + urlof);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(RetornarUrl(caminho) + arquivo);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(USUARIO, SENHA);
                request.EnableSsl = EnableSsl;

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream rs = response.GetResponseStream())
                    {
                        using (FileStream ws = new FileStream(diretorioLocal + "\\" + arquivo, FileMode.Create))
                        // using (FileStream ws = new FileStream("sib_ativos.zip", FileMode.Create))
                        {
                            byte[] buffer = new byte[2048];
                            int bytesRead = rs.Read(buffer, 0, buffer.Length);

                            while (bytesRead > 0)
                            {
                                ws.Write(buffer, 0, bytesRead);
                                bytesRead = rs.Read(buffer, 0, buffer.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, ":ERROR:: Problema Em Download ::ERROR");
                CentralLog.LogInfo(RetornarUrl(caminho) + arquivo);
            }
        }

        public static void Rename(string arquivo, int idArquivo, string caminho, string extensao)
        {
            Stream ftpStream = null;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(RetornarUrl(caminho) + arquivo);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.RenameTo = Path.ChangeExtension(arquivo, "." + extensao);
                request.UsePassive = false;
                request.UseBinary = true;
                request.KeepAlive = true;
                request.Proxy = null;
                request.Credentials = new NetworkCredential(USUARIO, SENHA);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                ftpStream = response.GetResponseStream();
                ftpStream.Close();
                response.Close();
            }
            catch (Exception e)
            {
                if (ftpStream != null)
                {
                    ftpStream.Close();
                    ftpStream.Dispose();
                }
                CentralLog.LogError(e, ":ERROR:: Problema Em Rename ::ERROR");
            }
        }

        public static void Move(string path, string newPath)
        {
            try
            {
                var uri = RetornarUrl(path);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.Credentials = new NetworkCredential(USUARIO, SENHA);
                request.RenameTo = newPath;
                request.EnableSsl = EnableSsl;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, ":ERROR:: Problema Em Move ::ERROR");
            }
        }

        public static void Move(string arquivo, string diretorio, int idArquivo, string caminho)
        {
            try
            {
                string nomeDestino = arquivo.Split('.')[0] + (DateTime.Now.Hour.ToString() + DateTime.Now.TimeOfDay.ToString()).Replace(":", "").Replace(".", "");
                string caminhoNovo = "../" + diretorio + "/" + nomeDestino;
                var uri = RetornarUrl(caminho) + arquivo;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.Credentials = new NetworkCredential(USUARIO, SENHA);
                request.RenameTo = caminhoNovo.Trim();
                request.EnableSsl = EnableSsl;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, ":ERROR:: Problema Em Move ::ERROR");
            }
        }

        public static void Delete(string arquivo, int idArquivo, string caminho)
        {
            try
            {

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(RetornarUrl(caminho) + arquivo);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(USUARIO, SENHA);
                request.UsePassive = false;
                request.Proxy = null;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();

            }
            catch (Exception e)
            {
                CentralLog.LogError(e, ":ERROR:: Problema Em Delete ::ERROR");
            }
        }

        public static void UpLoad(string arquivo, int idArquivo, string caminho, string diretorioLocal)
        {
            try
            {
                FileInfo fileInf = new FileInfo(diretorioLocal + arquivo);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(RetornarUrl(caminho) + arquivo);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(USUARIO, SENHA);
                request.EnableSsl = EnableSsl;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                Stream responseStream = request.GetRequestStream();
                byte[] buffer = new byte[2048];

                FileStream fs = fileInf.OpenRead();
                try
                {
                    int readCount = fs.Read(buffer, 0, buffer.Length);
                    while (readCount > 0)
                    {
                        responseStream.Write(buffer, 0, readCount);
                        readCount = fs.Read(buffer, 0, buffer.Length);
                    }
                }
                finally
                {
                    fs.Close();
                    responseStream.Close();
                }
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, ":ERROR:: Problema Em Upload ::ERROR");
            }
        }

        private static string RetornarUrl(string caminho)
        {
            StringBuilder sb = new StringBuilder();
            string server = ConfigurationManager.AppSettings["ftpServer"];
            // sb.Append("ftp://ftp.dadosabertos.ans.gov.br:21/");
            sb.Append(server);
            sb.Append(caminho);
            return sb.ToString();
        }

        //public static List<string> ListarParceiros()
        //{
        //    List<string> parceiros = new List<string>();
        //    try
        //    {
        //        FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftpweb.Empresa.com.br:21/parceiros/parceiros.csv");
        //        request.Method = WebRequestMethods.Ftp.DownloadFile;
        //        request.Credentials = new NetworkCredential(USUARIO, SENHA);
        //        request.EnableSsl = true;
        //        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

        //        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        //        {
        //            using (var stream = response.GetResponseStream())
        //            {
        //                using (var reader = new StreamReader(stream, true))
        //                {
        //                    string[] allLines = reader.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    parceiros = allLines.ToList();

        //                    reader.Close();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //    }

        //    return parceiros;
        //}
    }
}