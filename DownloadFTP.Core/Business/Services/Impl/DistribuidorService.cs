using DownloadFTP.Core.Data.Repositories.Impl;
using DownloadFTP.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadFTP.Core.Business.Services.Impl
{
    public class DistribuidorService
    {

        private readonly DistribuidorRepository _context;

        #region Construtor

        public DistribuidorService()
        {
            this._context = new DistribuidorRepository();
        }

        #endregion

        #region Metodos
        
        public List<Distribuidor> Listar()
        {
            return this._context.Consultar();
        }

        #endregion
    }
}
