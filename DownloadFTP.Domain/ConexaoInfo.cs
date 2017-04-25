using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadFTP.Domain
{
    public class ConexaoInfo
    {
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public string Host { get; set; }
        public string DiretorioServidor { get; set; }
    }
}
