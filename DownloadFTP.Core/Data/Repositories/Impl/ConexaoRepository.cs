using DownloadFTP.Core.Helpers;
using DownloadFTP.Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace DownloadFTP.Core.Data.Repositories.Impl
{
    public class ConexaoRepository
    {
        public void Gravar(Conexoes conexao)
        {
            using (DbHelper db = new DbHelper())
            {
                try
                {
                    Dictionary<string, object> dic0 = new Dictionary<string, object>();
                    dic0.Add("@EnderecoEntrada", conexao.EnderecoEntrada);
                    dic0.Add("@PastaLocal", conexao.PastaLocal);
                    dic0.Add("@EnderecoSaida", conexao.EnderecoSaida);
                    dic0.Add("@MoverPastaLocal", conexao.MoverPastaLocal);
                    dic0.Add("@Prioridade", conexao.Prioridade);
                    dic0.Add("@Ativo", conexao.Ativo);
                    dic0.Add("@PastaTemporaria", conexao.PastaTemporaria);
                    dic0.Add("@IdentificacaoTask", conexao.Identificacao);
                    dic0.Add("@Sftp", conexao.Sftp);
                    dic0.Add("@UploadSftp", conexao.UploadSftp);
                    dic0.Add("@Rbauto", conexao.Rbauto);

                    db.BeginTransaction();

                    db.ExecuteReader("IncluirConexao", CommandType.StoredProcedure, dic0);

                    db.EndTransaction();
                }
                catch (Exception ex)
                {
                    db.RollbackTransaction();

                    throw ex;
                }

            }
        }

        public List<Conexoes> Listar()
        {
            List<Conexoes> lstClientes = new List<Conexoes>();

            using (DbHelper db = new DbHelper())
            {
                try
                {
                    DataTable leitura = db.ExecuteReader("SelecionaConexoes", CommandType.StoredProcedure);

                    foreach (DataRow row in leitura.Rows)
                    {
                        Conexoes conexao = new Conexoes();
                        conexao.IdConexao = int.Parse(row["IdConexao"].ToString());
                        conexao.EnderecoEntrada = row["EnderecoEntrada"].ToString();
                        conexao.PastaLocal = row["PastaLocal"].ToString();
                        conexao.EnderecoSaida = row["EnderecoSaida"].ToString();
                        conexao.MoverPastaLocal = int.Parse(row["MoverPastaLocal"].ToString());
                        conexao.PastaTemporaria = row["PastaTemporaria"].ToString();
                        conexao.Identificacao = row["Identificacao"].ToString();
                        conexao.Ativo = int.Parse(row["Ativo"].ToString());
                        conexao.Prioridade = int.Parse(row["Prioridade"].ToString());
                        conexao.Sftp = int.Parse(row["Sftp"].ToString());
                        conexao.UploadSftp = int.Parse(row["Upload_Sftp_Ftp"].ToString());
                        conexao.Rbauto = int.Parse(row["Rbauto"].ToString());

                        lstClientes.Add(conexao);
                    }
                }
                catch (Exception ex)
                {
                    db.RollbackTransaction();

                    throw ex;
                }
            }

            return lstClientes;
        }

        public Conexoes Pesquisar(int IdConexao)
        {
            Conexoes conexao = new Conexoes();

            using (DbHelper db = new DbHelper())
            {
                try
                {
                    Dictionary<string, object> dic0 = new Dictionary<string, object>();

                    dic0.Add("@Idconexao", IdConexao);

                    DataTable leitura = db.ExecuteReader("PesquisaConexao", CommandType.StoredProcedure, dic0);

                    if (leitura.Rows != null && leitura.Rows.Count > 0)
                    {
                        var row = leitura.Rows[0];

                        conexao.IdConexao = int.Parse(row["IdConexao"].ToString());
                        conexao.EnderecoEntrada = row["EnderecoEntrada"].ToString();
                        conexao.PastaLocal = row["PastaLocal"].ToString();
                        conexao.EnderecoSaida = row["EnderecoSaida"].ToString();
                        conexao.MoverPastaLocal = int.Parse(row["MoverPastaLocal"].ToString());
                        conexao.PastaTemporaria = row["PastaTemporaria"].ToString();
                        conexao.Identificacao = row["Identificacao"].ToString();
                        conexao.Ativo = int.Parse(row["Ativo"].ToString());
                        conexao.Prioridade = int.Parse(row["Prioridade"].ToString());
                        conexao.Sftp = int.Parse(row["Sftp"].ToString());
                        conexao.UploadSftp = int.Parse(row["Upload_Sftp_Ftp"].ToString());
                        conexao.Rbauto = int.Parse(row["rbauto"].ToString());
                    }

                }
                catch (Exception ex)
                {
                    db.RollbackTransaction();

                    throw ex;
                }
            }
            return conexao;
        }

        public void Atualizar(Conexoes conexao)
        {
            using (DbHelper db = new DbHelper())
            {
                try
                {
                    Dictionary<string, object> dic0 = new Dictionary<string, object>();

                    dic0.Add("@Idconexao", conexao.IdConexao);
                    dic0.Add("@EnderecoEntrada", conexao.EnderecoEntrada);
                    dic0.Add("@PastaLocal", conexao.PastaLocal);
                    dic0.Add("@EnderecoSaida", conexao.EnderecoSaida);
                    dic0.Add("@MoverPastaLocal", conexao.MoverPastaLocal);
                    dic0.Add("@Prioridade", conexao.Prioridade);
                    dic0.Add("@Ativo", conexao.Ativo);
                    dic0.Add("@Sftp", conexao.Sftp);
                    dic0.Add("@UploadSftp", conexao.UploadSftp);
                    dic0.Add("@Rbauto", conexao.Rbauto);

                    db.BeginTransaction();

                    db.ExecuteReader("AtualizarConexao", CommandType.StoredProcedure, dic0);

                    db.EndTransaction();
                }
                catch (Exception ex)
                {
                    db.RollbackTransaction();

                    throw ex;
                }
            }
        }

        public void Excluir(int IdConexao)
        {
            using (DbHelper db = new DbHelper())
            {
                try
                {
                    Dictionary<string, object> dic0 = new Dictionary<string, object>();

                    Conexoes conexao = new Conexoes();

                    dic0.Add("@Idconexao", IdConexao);

                    DataTable leitura = db.ExecuteReader("Deletaconexao", CommandType.StoredProcedure, dic0);

                }
                catch (Exception ex)
                {
                    db.RollbackTransaction();

                    throw ex;
                }
            }
        }
    }
}
