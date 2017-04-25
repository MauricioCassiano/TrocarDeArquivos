using DownloadFTP.Core.Helpers;
using DownloadFTP.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadFTP.Core.Data.Repositories.Impl
{
    public class ArquivoRepository
    {
        public void LogarArquivos(ArrayList Arquivos, int status, string Identificacao)
        {
            using (DbHelper db = new DbHelper())
            {
                foreach (ArquivoLog arq in Arquivos)
                {
                    try
                    {
                        Dictionary<string, object> dic0 = new Dictionary<string, object>();
                        dic0.Add("@Arquivo", arq.Nome);

                        arq.Pedido = arq.Pedido == null ? "0" : arq.Pedido;

                        dic0.Add("@Pedido", arq.Pedido);
                        dic0.Add("@Status", status);
                        dic0.Add("@Identificacao", Identificacao);

                        db.BeginTransaction();

                        db.ExecuteReader("SP_LogarArquivos", CommandType.StoredProcedure, dic0);

                        db.EndTransaction();
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
}
