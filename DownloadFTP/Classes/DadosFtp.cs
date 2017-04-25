using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections;
using System.Configuration;
using Renci.SshNet;
using DownloadFTP.Domain;
using DownloadFTP.Core.Business.Services.Impl;

namespace DownloadFTP.Classes
{
    public class DadosFtp
    {
        public String usuario = "";
        public String senha = "";
        public String host = "";
        public String pastaServidor = "";

        private readonly DistribuidorService _appDistribuidor = new DistribuidorService();
        private readonly ArquivoService _appArquivo = new ArquivoService();

        public void limpaEndereco(String pasta)
        {
            String dadosFtp = pasta.Substring(6).Replace('/', '\\');

            this.usuario = dadosFtp.Substring(0, dadosFtp.IndexOf(":"));

            dadosFtp = dadosFtp.Substring(usuario.Length + 1);           // corta
            this.senha = dadosFtp.Substring(0, dadosFtp.IndexOf("@"));

            dadosFtp = dadosFtp.Substring(senha.Length + 1);           // corta

            if (dadosFtp.Contains("\\"))
            {
                this.host = dadosFtp.Substring(0, dadosFtp.IndexOf("\\"));
                dadosFtp = dadosFtp.Substring(host.Length + 1);           // corta
                this.pastaServidor = dadosFtp;   // sobrou apenas a pasta
            }
            else
            {
                this.host = dadosFtp;
                this.pastaServidor = "";
            }

        }

        public string[] GetListaArquivos(string _ftpIPServidor, string _caminhoFTP, string _user, string _pass)
        {
            string[] downloadArquivos;
            StringBuilder resultado = new StringBuilder();
            FtpWebRequest requisicaoFTP;
            WebResponse response;
            try
            {
                requisicaoFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + _ftpIPServidor + "//" + _caminhoFTP + "/"));
                requisicaoFTP.UseBinary = true;
                requisicaoFTP.Credentials = new NetworkCredential(_user, _pass);
                requisicaoFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                response = requisicaoFTP.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    resultado.Append(line);
                    resultado.Append("\n");
                    line = reader.ReadLine();
                }
                if (resultado.ToString() != "")
                {
                    resultado.Remove(resultado.ToString().LastIndexOf('\n'), 1);
                    reader.Close();
                    response.Close();
                    return resultado.ToString().Split('\n');
                }
                else
                {
                    reader.Close();
                    response.Close();
                    return downloadArquivos = null;
                }
            }
            catch (Exception)
            {
                downloadArquivos = null;
                return downloadArquivos;
            }
        }

        public void DeletarFTP(string _ftpIPServidor, string _caminhoFTP, ArrayList listaDeletados, string _user, string _pass)
        {

            try
            {

                foreach (string arq in listaDeletados)
                {

                    string ftpfullpath = "ftp://" + _ftpIPServidor + "//" + _caminhoFTP + "/" + arq;
                    var requestFileDelete = (FtpWebRequest)WebRequest.Create(ftpfullpath);

                    try
                    {
                        requestFileDelete.Credentials = new NetworkCredential(_user, _pass);
                        requestFileDelete.Method = WebRequestMethods.Ftp.DeleteFile;
                        requestFileDelete.KeepAlive = true;
                        var responseFileDelete = (FtpWebResponse)requestFileDelete.GetResponse();
                        if (responseFileDelete != null) responseFileDelete.Close();


                    }
                    catch (Exception)
                    {

                    }
                }

            }
            catch (Exception ex)
            {
                string log = ex.ToString();
            }
        }

        public void Upload(string _ftpIPServidor, string _diretorioAtual, string _caminhoFTP, string _user, string _pass, string Identificacao, int tipoApp)
        {
            string strPastaBackup = ConfigurationManager.AppSettings["PastaBackup"].ToString();

            ArrayList listaEnviados = new ArrayList();

            try
            {
                //string[] arquivosLocal = Directory.GetFiles(_diretorioAtual, "*.*");

                DirectoryInfo dir = new DirectoryInfo(_diretorioAtual);

                foreach (FileInfo arq in dir.GetFiles("*.*"))
                {
                    ArquivoLog ArquivoInfo = new ArquivoLog();
                    try
                    {
                        //FileInfo _arquivoInfo = new FileInfo(arq);
                        string ftpfullpath = "ftp://" + _ftpIPServidor + "//" + _caminhoFTP + "/" + arq.Name;
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpfullpath);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.Credentials = new NetworkCredential(_user, _pass);
                        request.KeepAlive = true;
                        byte[] bytes = File.ReadAllBytes(arq.FullName);
                        request.ContentLength = bytes.Length;
                        Stream requestStream = request.GetRequestStream();
                        requestStream.Write(bytes, 0, bytes.Length);
                        requestStream.Close();

                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                        response.Close();


                        //ftpfullpath = "ftp://" + _ftpIPServidor + "//Processados//" + arq.Name;
                        //FtpWebRequest request2 = (FtpWebRequest)WebRequest.Create(ftpfullpath);
                        //request2.Method = WebRequestMethods.Ftp.UploadFile;
                        //request2.Credentials = new NetworkCredential(_user, _pass);
                        //request2.KeepAlive = true;
                        //request2.ContentLength = bytes.Length;
                        //Stream requestStream2 = request2.GetRequestStream();
                        //requestStream2.Write(bytes, 0, bytes.Length);
                        //requestStream2.Close();

                        //FtpWebResponse response2 = (FtpWebResponse)request2.GetResponse();
                        //response2.Close();


                        ArquivoInfo.Nome = arq.Name;
                        ArquivoInfo.NomeCompleto = arq.FullName;

                        if (tipoApp == 2)
                        {

                            StreamReader ler = new StreamReader(arq.FullName);
                            string linha;
                            if ((linha = ler.ReadLine()) != null)
                            {
                                if (linha.Substring(0, 1).Trim() == "1")
                                {
                                    ArquivoInfo.Pedido = linha.Substring(15, 8).Trim();
                                }
                            }
                            ler.Close();

                        }

                        listaEnviados.Add(ArquivoInfo);
                        System.IO.File.Delete(arq.FullName);
                    }
                    catch (Exception)
                    {

                    }
                }

                foreach (ArquivoLog arq in listaEnviados)
                {
                    try
                    {

                        string log = strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\logs" + "\\LogEnviado_" + DateTime.Now.ToShortDateString().ToString().Replace("/", "-") + ".txt";

                        if (!System.IO.File.Exists(log))
                        {
                            using (System.IO.File.Create(log)) { }
                        }

                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(log, true))
                        {
                            file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " Enviando arquivo de " + _diretorioAtual + "\\" + arq.Nome + " Para " + _ftpIPServidor + "\\" + _caminhoFTP + " Tranferido com sucesso ... ");
                        }


                        System.IO.File.Delete(arq.NomeCompleto);

                    }
                    catch (Exception)
                    {

                    }
                }

                _appArquivo.LogarArquivos(listaEnviados, 2, Identificacao);
            }
            catch (Exception)
            {

            }
        }

        // SOBRECARGA DE METODOS
        #region Metodos de sobrecarga

        public ArrayList Download(string _ftpIPServidor, string _caminhoArquivo, string _caminhoFTP, string _user, string _pass)
        {
            ArrayList listaDeletados = new ArrayList();
            string[] arquivos = GetListaArquivos(_ftpIPServidor, _caminhoFTP, _user, _pass);
            string ftpfullpath = "ftp://" + _ftpIPServidor + "//" + _caminhoFTP + "/";
            WebClient request = new WebClient();

            try
            {
                if (arquivos != null)
                {
                    request.Credentials = new NetworkCredential(_user, _pass);
                    foreach (string arq in arquivos)
                    {
                        try
                        {
                            byte[] fileData = request.DownloadData(ftpfullpath + arq);
                            FileStream file = File.Create(_caminhoArquivo + "\\" + arq);
                            file.Write(fileData, 0, fileData.Length);
                            file.Close();
                            listaDeletados.Add(arq);
                        }
                        catch (Exception)
                        {

                        }

                    }
                    return listaDeletados;
                }
                else
                {
                    listaDeletados.Add("");
                    return listaDeletados;
                }
            }

            catch (Exception)
            {
                return listaDeletados = null;
            }

        }

        public Boolean Download(string diretorioEntrada, string _caminhoArquivo)
        {

            try
            {
                DirectoryInfo dir = new DirectoryInfo(diretorioEntrada);

                foreach (FileInfo arquivo in dir.GetFiles("*.*"))
                {
                    if (!File.Exists((_caminhoArquivo + "\\" + arquivo.Name)))
                        File.Move(arquivo.FullName, _caminhoArquivo + "\\" + arquivo.Name);
                }
                return true;
            }

            catch (Exception ex)
            {
                MessageBox.Show("ERRO :" + ex);
                return false;
            }

        }


        // TIPO VIA LIVRE 

        #region METODOS TIPO EPEDIDO

        public void backupEdi(string DiretorioAtual, string _ftpIPServidor, string _caminhoFTP, string DiretorioFinal, string Identificacao, int MoverLocal, string EnderecoSaida, int tipoApp, int rbAuto)
        {
            string strPastaBackup = ConfigurationManager.AppSettings["PastaBackup"].ToString();

            IEnumerable<Distribuidor> listaDistribuidores;

            listaDistribuidores = _appDistribuidor.Listar();

            DirectoryInfo dir = new DirectoryInfo(DiretorioAtual);

            ArrayList listaEnviados = new ArrayList();

            string logTrasfer = strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\logs" + "\\LogTrasfer.txt";

            if (File.Exists(logTrasfer))
            {
                File.Delete(logTrasfer);

                using (File.Create(logTrasfer)) { }
            }

            int QuantidadeArquivos = dir.GetFiles("*.*").Length;

            using (StreamWriter file = new StreamWriter(logTrasfer, true))
            {
                file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " - " + " Arquivos identificados em " + _ftpIPServidor + "\\" + _caminhoFTP + ": " + QuantidadeArquivos);
            }

            foreach (FileInfo arquivo in dir.GetFiles("*.*"))
            {
                ArquivoLog ArquivoInfo = new ArquivoLog();
                string cnpj = "";
                bool mudarDiretorio = false;
                bool rAuto = false;
                bool bAuto = false;

                try
                {
                    if (File.Exists((strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\" + arquivo.Name)))
                    {

                        File.Copy(arquivo.FullName, strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\Alt_" + DateTime.Now.ToShortTimeString().ToString().Replace(":", "") + arquivo.Name);

                        if (Directory.Exists(EnderecoSaida) && (MoverLocal == 0))
                        {
                            File.Copy(arquivo.FullName, EnderecoSaida + "\\" + arquivo.Name);
                        }

                        ArquivoInfo.Nome = arquivo.Name;


                        if (tipoApp == 2)
                        {
                            StreamReader ler = new StreamReader(arquivo.FullName);
                            string linha;

                            while ((linha = ler.ReadLine()) != null)
                            {
                                if (linha.Substring(0, 2).Trim() == "01")
                                    ArquivoInfo.Pedido = linha.Substring(15, 8).Trim();

                                if (rbAuto == 1)
                                {
                                    if (linha.Substring(0, 2).Trim() == "02")
                                    {
                                        cnpj = linha.Substring(40, 14).Trim();

                                        var listDistTemp = listaDistribuidores.Where(x => x.Cnpj == cnpj);

                                        if (listDistTemp.Count() > 0)
                                        {
                                            mudarDiretorio = true;
                                        }
                                    }
                                    if (mudarDiretorio)
                                    {
                                        if (linha.Substring(0, 2).Trim() == "03")
                                        {
                                            if (linha.Substring(2, 2).Trim() == "02")
                                                rAuto = true;
                                            else if (linha.Substring(2, 2).Trim() == "04")
                                                bAuto = true;
                                        }
                                    }
                                }
                            }
                            ler.Close();
                        }

                        if (mudarDiretorio)
                        {

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "RAuto\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "RAuto\\" + cnpj);
                            }

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "BAuto\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "BAuto\\" + cnpj);
                            }

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "Rejeitados\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                if (rAuto == false && bAuto == false)
                                    Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "Rejeitados\\" + cnpj);
                            }

                            if (rAuto)
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\RAuto\\" + cnpj + "\\" + arquivo.Name);
                            else if (bAuto)
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\BAuto\\" + cnpj + "\\" + arquivo.Name);
                            else
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\Rejeitados\\" + cnpj + "\\" + arquivo.Name);
                        }

                        else if (!File.Exists((DiretorioFinal + "\\" + arquivo.Name)))
                            File.Move(arquivo.FullName, DiretorioFinal + "\\" + arquivo.Name);
                    }
                    else
                    {
                        File.Copy(arquivo.FullName, strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\" + arquivo.Name);

                        if (Directory.Exists(EnderecoSaida) && (MoverLocal == 0))
                        {
                            File.Copy(arquivo.FullName, EnderecoSaida + "\\" + arquivo.Name);
                        }

                        ArquivoInfo.Nome = arquivo.Name;


                        if (tipoApp == 2)
                        {
                            StreamReader ler = new StreamReader(arquivo.FullName);
                            string linha;

                            while ((linha = ler.ReadLine()) != null)
                            {
                                if (linha.Substring(0, 2).Trim() == "01")
                                    ArquivoInfo.Pedido = linha.Substring(15, 8).Trim();

                                if (rbAuto == 1)
                                {
                                    if (linha.Substring(0, 2).Trim() == "02")
                                    {
                                        cnpj = linha.Substring(40, 14).Trim();

                                        var listDistTemp = listaDistribuidores.Where(x => x.Cnpj == cnpj);

                                        if (listDistTemp.Count() > 0)
                                        {
                                            mudarDiretorio = true;
                                        }
                                    }
                                    if (mudarDiretorio)
                                    {
                                        if (linha.Substring(0, 2).Trim() == "03")
                                        {
                                            if (linha.Substring(2, 2).Trim() == "02")
                                                rAuto = true;
                                            else if (linha.Substring(2, 2).Trim() == "04")
                                                bAuto = true;
                                        }
                                    }
                                }
                            }
                            ler.Close();
                        }

                        if (mudarDiretorio)
                        {

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "RAuto\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "RAuto\\" + cnpj);
                            }

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "BAuto\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "BAuto\\" + cnpj);
                            }

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "Rejeitados\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                if (rAuto == false && bAuto == false)
                                    Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "Rejeitados\\" + cnpj);
                            }

                            if (rAuto)
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\RAuto\\" + cnpj + "\\" + arquivo.Name);
                            else if (bAuto)
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\BAuto\\" + cnpj + "\\" + arquivo.Name);
                            else
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\Rejeitados\\" + cnpj + "\\" + arquivo.Name);
                        }

                        else if (!File.Exists((DiretorioFinal + "\\" + arquivo.Name)))
                            File.Move(arquivo.FullName, DiretorioFinal + "\\" + arquivo.Name);
                    }

                    string log = strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\logs" + "\\LogRecebido_" + DateTime.Now.ToShortDateString().ToString().Replace("/", "-") + ".txt";

                    if (!System.IO.File.Exists(log))
                    {
                        using (System.IO.File.Create(log)) { }
                    }

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(log, true))
                    {
                        file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " Movendo arquivo de " + _ftpIPServidor + "\\" + _caminhoFTP + "\\" + arquivo.Name + " Para " + strPastaBackup + "\\Backups\\" + Identificacao + " Tranferido com sucesso ... ");
                    }

                    listaEnviados.Add(ArquivoInfo);

                }
                catch (System.IO.IOException e)
                {
                    string log = e.ToString();

                }
            }

            _appArquivo.LogarArquivos(listaEnviados, 1, Identificacao);

        }

        public void backupEdi(string DiretorioAtual, string DiretorioEntrada, string DiretorioFinal, string Identificacao, int MoverLocal, string EnderecoSaida, int tipoApp, int rbAuto)
        {
            string strPastaBackup = ConfigurationManager.AppSettings["PastaBackup"].ToString();

            IEnumerable<Distribuidor> listaDistribuidores;

            listaDistribuidores = _appDistribuidor.Listar();

            DirectoryInfo dir = new DirectoryInfo(DiretorioAtual);

            ArrayList listaEnviados = new ArrayList();

            string logTrasfer = strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\logs" + "\\LogTrasfer.txt";

            if (File.Exists(logTrasfer))
            {
                File.Delete(logTrasfer);

                using (File.Create(logTrasfer)) { }
            }

            int QuantidadeArquivos = dir.GetFiles("*.*").Length;

            using (StreamWriter file = new StreamWriter(logTrasfer, true))
            {
                file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " - " + " Arquivos identificados em " + DiretorioFinal + ": " + QuantidadeArquivos);
            }

            foreach (FileInfo arquivo in dir.GetFiles("*.*"))
            {
                ArquivoLog ArquivoInfo = new ArquivoLog();
                string cnpj = "";
                bool mudarDiretorio = false;
                bool rAuto = false;
                bool bAuto = false;

                try
                {

                    if (File.Exists((strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\" + arquivo.Name)))
                    {
                        File.Copy(arquivo.FullName, strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\Alt_" + DateTime.Now.ToShortTimeString().ToString().Replace(":", "") + arquivo.Name);


                        // está regra se aplica para diretorios LOCAIS
                        //se ouver um diretorio valido é feito uma copia para o Endereco de saida
                        if (Directory.Exists(EnderecoSaida) && (MoverLocal == 0))
                        {
                            File.Copy(arquivo.FullName, EnderecoSaida + "\\" + arquivo.Name);
                        }

                        ArquivoInfo.Nome = arquivo.Name;

                        if (tipoApp == 2)
                        {
                            StreamReader ler = new StreamReader(arquivo.FullName);
                            string linha;

                            while ((linha = ler.ReadLine()) != null)
                            {
                                if (linha.Substring(0, 2).Trim() == "01")
                                    ArquivoInfo.Pedido = linha.Substring(15, 8).Trim();

                                if (rbAuto == 1)
                                {
                                    if (linha.Substring(0, 2).Trim() == "02")
                                    {
                                        cnpj = linha.Substring(40, 14).Trim();

                                        var listDistTemp = listaDistribuidores.Where(x => x.Cnpj == cnpj);

                                        if (listDistTemp.Count() > 0)
                                        {
                                            mudarDiretorio = true;
                                        }
                                    }
                                    if (mudarDiretorio)
                                    {
                                        if (linha.Substring(0, 2).Trim() == "03")
                                        {
                                            if (linha.Substring(2, 2).Trim() == "02")
                                                rAuto = true;
                                            else if (linha.Substring(2, 2).Trim() == "04")
                                                bAuto = true;
                                        }
                                    }
                                }
                            }
                            ler.Close();
                        }

                        if (mudarDiretorio)
                        {

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "RAuto\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "RAuto\\" + cnpj);
                            }

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "BAuto\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "BAuto\\" + cnpj);
                            }

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "Rejeitados\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                if (rAuto == false && bAuto == false)
                                    Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "Rejeitados\\" + cnpj);
                            }

                            if (rAuto)
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\RAuto\\" + cnpj + "\\" + arquivo.Name);
                            else if (bAuto)
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\BAuto\\" + cnpj + "\\" + arquivo.Name);
                            else
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\Rejeitados\\" + cnpj + "\\" + arquivo.Name);
                        }

                        else if (!File.Exists((DiretorioFinal + "\\" + arquivo.Name)))
                            File.Move(arquivo.FullName, DiretorioFinal + "\\" + arquivo.Name);

                    }
                    else
                    {
                        File.Copy(arquivo.FullName, strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\" + arquivo.Name);

                        // está regra se aplica para diretorios LOCAIS
                        if (Directory.Exists(EnderecoSaida) && (MoverLocal == 0))
                        {
                            File.Copy(arquivo.FullName, EnderecoSaida + "\\" + arquivo.Name);
                        }

                        ArquivoInfo.Nome = arquivo.Name;


                        if (tipoApp == 2)
                        {
                            StreamReader ler = new StreamReader(arquivo.FullName);
                            string linha;

                            while ((linha = ler.ReadLine()) != null)
                            {
                                if (linha.Substring(0, 2).Trim() == "01")
                                    ArquivoInfo.Pedido = linha.Substring(15, 8).Trim();

                                if (rbAuto == 1)
                                {
                                    if (linha.Substring(0, 2).Trim() == "02")
                                    {
                                        cnpj = linha.Substring(40, 14).Trim();

                                        var listDistTemp = listaDistribuidores.Where(x => x.Cnpj == cnpj);

                                        if (listDistTemp.Count() > 0)
                                        {
                                            mudarDiretorio = true;
                                        }
                                    }
                                    if (mudarDiretorio)
                                    {
                                        if (linha.Substring(0, 2).Trim() == "03")
                                        {
                                            if (linha.Substring(2, 2).Trim() == "02")
                                                rAuto = true;
                                            else if (linha.Substring(2, 2).Trim() == "04")
                                                bAuto = true;
                                        }
                                    }
                                }
                            }
                            ler.Close();
                        }

                        if (mudarDiretorio)
                        {

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "RAuto\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "RAuto\\" + cnpj);
                            }

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "BAuto\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "BAuto\\" + cnpj);
                            }

                            if (!Directory.Exists(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "Rejeitados\\" + cnpj))
                            {
                                //Criamos um diretorio com o nome da pastalocal
                                if (rAuto == false && bAuto == false)
                                    Directory.CreateDirectory(strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\" + "Rejeitados\\" + cnpj);
                            }

                            if (rAuto)
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\RAuto\\" + cnpj + "\\" + arquivo.Name);
                            else if (bAuto)
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\BAuto\\" + cnpj + "\\" + arquivo.Name);
                            else
                                File.Move(arquivo.FullName, strPastaBackup + "\\DownloadFTP_DiretoriosLocais\\Rejeitados\\" + cnpj + "\\" + arquivo.Name);
                        }
                        else if (!File.Exists((DiretorioFinal + "\\" + arquivo.Name)))
                            File.Move(arquivo.FullName, DiretorioFinal + "\\" + arquivo.Name);
                    }

                    string log = strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\logs" + "\\LogRecebido_" + DateTime.Now.ToShortDateString().ToString().Replace("/", "-") + ".txt";

                    if (!System.IO.File.Exists(log))
                    {
                        using (System.IO.File.Create(log)) { }
                    }

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(log, true))
                    {
                        file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " Movendo arquivo de " + DiretorioEntrada + "\\" + arquivo.Name + " Para " + strPastaBackup + "\\Backups\\" + Identificacao + " Tranferido com sucesso ... ");
                    }

                    listaEnviados.Add(ArquivoInfo);
                }
                catch (System.IO.IOException e)
                {
                    string log = e.ToString();

                }
            }

            _appArquivo.LogarArquivos(listaEnviados, 1, Identificacao);
        }

        #endregion

        #region  METODOS TIPO VIALIVRE

        public void backup(string DiretorioAtual, string _ftpIPServidor, string _caminhoFTP, string DiretorioFinal, string Identificacao, int MoverLocal, string EnderecoSaida, int tipoApp)
        {
            string strPastaBackup = ConfigurationManager.AppSettings["PastaBackup"].ToString();

            DirectoryInfo dir = new DirectoryInfo(DiretorioAtual);

            ArrayList listaEnviados = new ArrayList();

            string logTrasfer = strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\logs" + "\\LogTrasfer.txt";

            if (File.Exists(logTrasfer))
            {
                File.Delete(logTrasfer);

                using (File.Create(logTrasfer)) { }
            }

            int QuantidadeArquivos = dir.GetFiles("*.*").Length;

            using (StreamWriter file = new StreamWriter(logTrasfer, true))
            {
                file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " - " + " Arquivos identificados em " + _ftpIPServidor + "\\" + _caminhoFTP + ": " + QuantidadeArquivos);
            }

            foreach (FileInfo arquivo in dir.GetFiles("*.*"))
            {
                ArquivoLog ArquivoInfo = new ArquivoLog();

                try
                {
                    if (File.Exists((strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\" + arquivo.Name)))
                    {

                        File.Copy(arquivo.FullName, strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\Alt_" + DateTime.Now.ToShortTimeString().ToString().Replace(":", "") + arquivo.Name);

                        if (Directory.Exists(EnderecoSaida) && (MoverLocal == 0))
                        {
                            File.Copy(arquivo.FullName, EnderecoSaida + "\\" + arquivo.Name);
                        }

                        ArquivoInfo.Nome = arquivo.Name;


                        File.Move(arquivo.FullName, DiretorioFinal + "\\" + arquivo.Name);
                    }
                    else
                    {
                        File.Copy(arquivo.FullName, strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\" + arquivo.Name);

                        if (Directory.Exists(EnderecoSaida) && (MoverLocal == 0))
                        {
                            File.Copy(arquivo.FullName, EnderecoSaida + "\\" + arquivo.Name);
                        }

                        ArquivoInfo.Nome = arquivo.Name;

                        File.Move(arquivo.FullName, DiretorioFinal + "\\" + arquivo.Name);
                    }

                    string log = strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\logs" + "\\LogRecebido_" + DateTime.Now.ToShortDateString().ToString().Replace("/", "-") + ".txt";

                    if (!System.IO.File.Exists(log))
                    {
                        using (System.IO.File.Create(log)) { }
                    }

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(log, true))
                    {
                        file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " Movendo arquivo de " + _ftpIPServidor + "\\" + _caminhoFTP + "\\" + arquivo.Name + " Para " + strPastaBackup + "\\Backups\\" + Identificacao + " Tranferido com sucesso ... ");
                    }

                    listaEnviados.Add(ArquivoInfo);

                }
                catch (System.IO.IOException e)
                {
                    string log = e.ToString();

                }
            }

            _appArquivo.LogarArquivos(listaEnviados, 1, Identificacao);
        }

        public void backup(string DiretorioAtual, string DiretorioEntrada, string DiretorioFinal, string Identificacao, int MoverLocal, string EnderecoSaida, int tipoApp)
        {
            string strPastaBackup = ConfigurationManager.AppSettings["PastaBackup"].ToString();

            DirectoryInfo dir = new DirectoryInfo(DiretorioAtual);

            ArrayList listaEnviados = new ArrayList();

            string logTrasfer = strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\logs" + "\\LogTrasfer.txt";

            if (File.Exists(logTrasfer))
            {
                File.Delete(logTrasfer);

                using (File.Create(logTrasfer)) { }
            }

            int QuantidadeArquivos = dir.GetFiles("*.*").Length;

            using (StreamWriter file = new StreamWriter(logTrasfer, true))
            {
                file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " - " + " Arquivos identificados em " + DiretorioFinal + ": " + QuantidadeArquivos);
            }

            foreach (FileInfo arquivo in dir.GetFiles("*.*"))
            {

                ArquivoLog ArquivoInfo = new ArquivoLog();
                try
                {

                    if (File.Exists((strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\" + arquivo.Name)))
                    {
                        File.Copy(arquivo.FullName, strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\Alt_" + DateTime.Now.ToShortTimeString().ToString().Replace(":", "") + arquivo.Name);


                        // está regra se aplica para diretorios LOCAIS
                        //se ouver um diretorio valido é feito uma copia para o Endereco de saida
                        if (Directory.Exists(EnderecoSaida) && (MoverLocal == 0))
                        {
                            File.Copy(arquivo.FullName, EnderecoSaida + "\\" + arquivo.Name);
                        }

                        ArquivoInfo.Nome = arquivo.Name;

                        if (tipoApp == 2)
                        {

                            StreamReader ler = new StreamReader(arquivo.FullName);
                            string linha;
                            if ((linha = ler.ReadLine()) != null)
                            {
                                if (linha.Substring(0, 1).Trim() == "1")
                                {
                                    ArquivoInfo.Pedido = linha.Substring(15, 8).Trim();

                                }
                            }
                            ler.Close();

                        }

                        File.Move(arquivo.FullName, DiretorioFinal + "\\" + arquivo.Name);

                    }
                    else
                    {
                        File.Copy(arquivo.FullName, strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\" + arquivo.Name);

                        // está regra se aplica para diretorios LOCAIS
                        if (Directory.Exists(EnderecoSaida) && (MoverLocal == 0))
                        {
                            File.Copy(arquivo.FullName, EnderecoSaida + "\\" + arquivo.Name);
                        }

                        ArquivoInfo.Nome = arquivo.Name;

                        if (tipoApp == 2)
                        {
                            StreamReader ler = new StreamReader(arquivo.FullName);
                            string linha;
                            if ((linha = ler.ReadLine()) != null)
                            {
                                if (linha.Substring(0, 1).Trim() == "1")
                                {
                                    ArquivoInfo.Pedido = linha.Substring(15, 8).Trim();

                                }
                            }
                            ler.Close();

                        }

                        File.Move(arquivo.FullName, DiretorioFinal + "\\" + arquivo.Name);
                    }

                    string log = strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\logs" + "\\LogRecebido_" + DateTime.Now.ToShortDateString().ToString().Replace("/", "-") + ".txt";

                    if (!System.IO.File.Exists(log))
                    {
                        using (System.IO.File.Create(log)) { }
                    }

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(log, true))
                    {
                        file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " Movendo arquivo de " + DiretorioEntrada + "\\" + arquivo.Name + " Para " + strPastaBackup + "\\Backups\\" + Identificacao + " Tranferido com sucesso ... ");
                    }

                    listaEnviados.Add(ArquivoInfo);
                }
                catch (System.IO.IOException e)
                {
                    string log = e.ToString();

                }
            }

            _appArquivo.LogarArquivos(listaEnviados, 1, Identificacao);
        }

        #endregion

        // METODOS SFTP
        public ArrayList DownloadSftp(string _ftpIPServidor, string _caminhoArquivo, string _caminhoFTP, string _user, string _pass)
        {
            ArrayList listaDeletados = new ArrayList();
            try
            {
                using (SftpClient sftp = new SftpClient(_ftpIPServidor, 22, _user, _pass))
                {
                    string DiretorioSFTP = ("//" + _caminhoFTP.Replace("\\", "//"));

                    sftp.Connect();
                    sftp.ChangeDirectory(DiretorioSFTP); // subdiretorio do ftp
                    string downloadedFileName = "";
                    string localPath = _caminhoArquivo + "\\"; //local para salvar o arquivo


                    var listDirectory = sftp.ListDirectory(DiretorioSFTP); // subdiretorio do ftp

                    try
                    {
                        foreach (var fi in listDirectory)
                        {
                            if (fi.Name.Length > 2)
                            {
                                downloadedFileName = fi.Name;

                                if (sftp.Exists(downloadedFileName))
                                {
                                    using (var file = File.OpenWrite(localPath + downloadedFileName))
                                    {
                                        sftp.DownloadFile(downloadedFileName, file);
                                        listaDeletados.Add(downloadedFileName);
                                    }
                                }
                            }

                        }
                        sftp.Disconnect();
                        return listaDeletados;
                    }

                    catch (Exception e)
                    {
                        string ex = e.Message;

                        return listaDeletados = null;
                    }

                }
            }
            catch (Exception)
            {
                return listaDeletados = null;

            }

            //ArrayList listaDeletados = new ArrayList();
            //string[] arquivos = GetListaArquivos(_ftpIPServidor, _caminhoFTP, _user, _pass);
            //string ftpfullpath = "ftp://" + _ftpIPServidor + "//" + _caminhoFTP + "/";
            //FtpWebRequest requisicaoFTP;
            //try
            //{

            //    if (arquivos != null)
            //    {

            //        foreach (string arq in arquivos)
            //        {
            //            try
            //            {


            //                requisicaoFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpfullpath + arq));
            //                requisicaoFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            //                requisicaoFTP.UseBinary = true;
            //                requisicaoFTP.Credentials = new NetworkCredential(_user, _pass);
            //                requisicaoFTP.KeepAlive = false;
            //                FtpWebResponse response = (FtpWebResponse)requisicaoFTP.GetResponse();
            //                Stream ftpStream = response.GetResponseStream();
            //                long cl = response.ContentLength;
            //                int bufferSize = 2048;
            //                int lerContador;
            //                byte[] buffer = new byte[bufferSize];

            //                lerContador = ftpStream.Read(buffer, 0, bufferSize);

            //                FileStream outputStream = new FileStream(_caminhoArquivo + "\\" + arq, FileMode.Create);
            //                while (lerContador > 0)
            //                {
            //                    outputStream.Write(buffer, 0, lerContador);
            //                    lerContador = ftpStream.Read(buffer, 0, bufferSize);
            //                }

            //                ftpStream.Close();
            //                outputStream.Close();
            //                response.Close();
            //                listaDeletados.Add(arq);
            //            }
            //            catch (Exception ex)
            //            {
            //                string erro = ex.Message;
            //            }

            //        }
            //        return listaDeletados;
            //    }
            //    else
            //    {
            //        listaDeletados.Add("");
            //        return listaDeletados;
            //    }
            //}

            //catch (Exception)
            //{
            //    return listaDeletados = null;
            //}

        }

        public void DeletarSFTP(string _ftpIPServidor, string _caminhoFTP, ArrayList listaDeletados, string _user, string _pass)
        {

            try
            {
                using (SftpClient sftp = new SftpClient(_ftpIPServidor, 22, _user, _pass))
                {
                    string DiretorioSFTP = ("//" + _caminhoFTP.Replace("\\", "//"));

                    sftp.Connect();
                    sftp.ChangeDirectory(DiretorioSFTP); // subdiretorio do ftp

                    try
                    {
                        foreach (string arquivo in listaDeletados)
                        {
                            if (sftp.Exists(arquivo))
                            {
                                sftp.DeleteFile(arquivo);
                            }

                        }
                        sftp.Disconnect();

                    }

                    catch (Exception e)
                    {
                        string ex = e.Message;
                        sftp.Disconnect();
                    }

                }

            }
            catch (Exception ex)
            {
                string log = ex.ToString();
            }
        }

        public void UploadSftp(string _ftpIPServidor, string _diretorioAtual, string _caminhoFTP, string _user, string _pass, string Identificacao, int tipoApp)
        {
            string strPastaBackup = ConfigurationManager.AppSettings["PastaBackup"].ToString();
            ArrayList listaEnviados = new ArrayList();
            try
            {
                using (SftpClient client = new SftpClient(_ftpIPServidor, 22, _user, _pass))
                {

                    DirectoryInfo dir = new DirectoryInfo(_diretorioAtual);

                    string DiretorioSFTP = ("//" + _caminhoFTP.Replace("\\", "//"));

                    client.Connect();

                    client.ChangeDirectory(DiretorioSFTP);
                    foreach (FileInfo arq in dir.GetFiles("*.*"))
                    {
                        ArquivoLog ArquivoInfo = new ArquivoLog();

                        try
                        {
                            using (var fileStream = new FileStream(_diretorioAtual + "\\" + arq.Name, FileMode.Open))
                            {
                                client.BufferSize = 4 * 1024; // bypass Payload error large files
                                client.UploadFile(fileStream, arq.Name);

                                fileStream.Close();

                                ArquivoInfo.Nome = arq.Name;
                                ArquivoInfo.NomeCompleto = arq.FullName;

                                if (tipoApp == 2)
                                {

                                    StreamReader ler = new StreamReader(arq.FullName);
                                    string linha;
                                    if ((linha = ler.ReadLine()) != null)
                                    {
                                        if (linha.Substring(0, 1).Trim() == "1")
                                        {
                                            ArquivoInfo.Pedido = linha.Substring(15, 8).Trim();

                                        }
                                    }
                                    ler.Close();

                                }

                                listaEnviados.Add(ArquivoInfo);
                                System.IO.File.Delete(_diretorioAtual + "\\" + arq.Name);
                            }

                        }
                        catch (Exception e)
                        {
                            string erro = e.Message;
                        }

                    }
                    client.Disconnect();

                    foreach (ArquivoLog arq in listaEnviados)
                    {
                        try
                        {

                            string log = strPastaBackup + "\\DownloadFTP_Backups\\" + Identificacao + "\\logs" + "\\LogEnviado_" + DateTime.Now.ToShortDateString().ToString().Replace("/", "-") + ".txt";

                            if (!System.IO.File.Exists(log))
                            {
                                using (System.IO.File.Create(log)) { }
                            }

                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(log, true))
                            {
                                file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " Enviando arquivo de " + _diretorioAtual + "\\" + arq.Nome + " Para " + _ftpIPServidor + "\\" + _caminhoFTP + " Tranferido com sucesso ... ");
                            }

                        }
                        catch (Exception)
                        {

                        }
                    }

                    _appArquivo.LogarArquivos(listaEnviados, 2, Identificacao);
                }
            }
            catch (Exception e)
            {
                string erro = e.Message;
            }
        }

        #endregion

    }


}
