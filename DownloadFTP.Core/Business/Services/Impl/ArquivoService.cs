using DownloadFTP.Core.Data.Repositories.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadFTP.Core.Business.Services.Impl
{
    public class ArquivoService
    {

        private readonly ArquivoRepository _context;

        public ArquivoService()
        {
            this._context = new ArquivoRepository();
        }

        public void LogarArquivos(ArrayList Arquivos, int status, string Identificacao) 
        {
            this._context.LogarArquivos(Arquivos, status, Identificacao);
        }
    }
}
