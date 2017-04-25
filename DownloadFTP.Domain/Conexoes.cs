namespace DownloadFTP.Domain
{
    public class Conexoes
    {
        public int IdConexao { get; set; }
        public string EnderecoEntrada { get; set; }
        public string PastaLocal { get; set; }
        public string EnderecoSaida { get; set; }
        public int MoverPastaLocal { get; set; }
        public string PastaTemporaria { get; set; }
        public int Ativo { get; set; }
        public int Prioridade { get; set; }
        public string Identificacao { get; set; }
        public int Sftp { get; set; }
        public int UploadSftp { get; set; }
        public int Rbauto { get; set; }
    }
}
