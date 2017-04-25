using DownloadFTP.Core.Helpers;
using DownloadFTP.Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace DownloadFTP.Core.Data.Repositories.Impl
{
    public class DistribuidorRepository
    {
        public List<Distribuidor> Consultar()
        {

            List<Distribuidor> lstDistribuidor = new List<Distribuidor>();

            using (DbHelper db = new DbHelper())
            {
                try
                {
                    DataTable leitura = db.ExecuteReader("SP_ListarDistribuidores", CommandType.StoredProcedure);

                    foreach (DataRow row in leitura.Rows)
                    {
                        Distribuidor distribuidor = new Distribuidor();

                        distribuidor.Id = int.Parse(row["Id"].ToString());
                        distribuidor.Cnpj = row["Cnpj"].ToString();
                        distribuidor.CnpjFmt = row["CnpjFmt"].ToString();
                        distribuidor.Descricao = row["Descricao"].ToString();

                        lstDistribuidor.Add(distribuidor);
                    }

                }
                catch (Exception ex)
                {
                    db.RollbackTransaction();

                    throw ex;
                }
            }

            return lstDistribuidor;
        }
    }
}
