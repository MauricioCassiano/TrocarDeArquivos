using DownloadFTP.Core.Data.Repositories.Impl;
using DownloadFTP.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Business.Services.Impl
{
    public class ConexaoService
    {

        private readonly ConexaoRepository _context;

        #region Construtor

        public ConexaoService()
        {
            this._context = new ConexaoRepository();
        }

        #endregion

        public void Gravar(Conexoes conexao)
        {
            this._context.Gravar(conexao);
        }

        public Conexoes Pesquisar(int id)
        {
            return this._context.Pesquisar(id);
        }

        public void Atualizar(Conexoes conexao) 
        {
            this._context.Atualizar(conexao);
        }

        public void Excluir(int id)
        {
            this._context.Excluir(id);
        }

        public List<Conexoes> Listar()
        {
            return this._context.Listar();
        }
    }
}
