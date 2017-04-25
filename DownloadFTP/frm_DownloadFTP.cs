using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using DownloadFTP.Classes;
using System.Configuration;
using DownloadFTP.Domain;
using Core.Business.Services.Impl;

// Mauricio cassiano de seles
// Download FTP 
// 24/04/2014

namespace DownloadFTP
{
    public partial class frm_DownloadFTP : Form
    {

        #region Variaveis Globais

        bool Processamento = false;
        string criaBackup = ConfigurationManager.AppSettings["PastaBackup"].ToString();
        int tipoapp;
        Conexoes conection = new Conexoes();
        

        private readonly ConexaoService _appConexao = new ConexaoService();

        #endregion

        #region Metodos

        public void inicializaSistema()
        {
            this.Processamento = true;

            try
            {
                //DadosBd objDados = new DadosBd();
                List<Conexoes> listaConexoes = new List<Conexoes>();
                listaConexoes = this._appConexao.Listar();

                foreach (var itemLista in listaConexoes)
                {
                     conection = itemLista;
                    if (itemLista.Ativo == 1)
                    {
                        DadosFtp FtpEnt = new DadosFtp();
                        DadosFtp FtpSai = new DadosFtp();
                        ArrayList ArquivosParaDeletar = new ArrayList();
                        ArquivosParaDeletar = null;
                        Boolean ArquivosLocal = false;

                        if (itemLista.MoverPastaLocal == 1)
                        {
                            FtpSai.limpaEndereco(itemLista.EnderecoSaida);
                        }

                        // Cria diretorios caso não existam
                        CriaDiretorios(itemLista);

                        // Incializa o Download do FTP
                        if (itemLista.EnderecoEntrada.StartsWith("ftp"))
                        {
                            FtpEnt.limpaEndereco(itemLista.EnderecoEntrada);

                            if (itemLista.Sftp == 0)
                                ArquivosParaDeletar = FtpEnt.Download(FtpEnt.host, criaBackup + "\\DownloadFTP_PastaTemporaria\\" + itemLista.PastaTemporaria, FtpEnt.pastaServidor, FtpEnt.usuario, FtpEnt.senha);
                            else
                                ArquivosParaDeletar = FtpEnt.DownloadSftp(FtpEnt.host, criaBackup + "\\DownloadFTP_PastaTemporaria\\" + itemLista.PastaTemporaria, FtpEnt.pastaServidor, FtpEnt.usuario, FtpEnt.senha);
                        }
                        else
                        {
                            //METODO DE SOBRECARGA
                            ArquivosLocal = FtpEnt.Download(itemLista.EnderecoEntrada, criaBackup + "\\DownloadFTP_PastaTemporaria\\" + itemLista.PastaTemporaria);
                        }

                        if (ArquivosParaDeletar != null || ArquivosLocal == true)
                        {

                            if (itemLista.EnderecoEntrada.StartsWith("ftp"))
                            {
                                // Deleta do FTP apenas os arquivos que foram baixados
                                if (itemLista.Sftp == 0)
                                {
                                    FtpEnt.DeletarFTP(FtpEnt.host, FtpEnt.pastaServidor, ArquivosParaDeletar, FtpEnt.usuario, FtpEnt.senha);
                                }
                                else
                                    FtpEnt.DeletarSFTP(FtpEnt.host, FtpEnt.pastaServidor, ArquivosParaDeletar, FtpEnt.usuario, FtpEnt.senha);

                                // Efetua o Backup
                                if (tipoapp == 1)
                                    FtpEnt.backup(criaBackup + "\\DownloadFTP_PastaTemporaria\\" + itemLista.PastaTemporaria, FtpEnt.host, FtpEnt.pastaServidor, itemLista.PastaLocal, itemLista.Identificacao, itemLista.MoverPastaLocal, itemLista.EnderecoSaida, tipoapp);
                                else
                                    FtpEnt.backupEdi(criaBackup + "\\DownloadFTP_PastaTemporaria\\" + itemLista.PastaTemporaria, FtpEnt.host, FtpEnt.pastaServidor, itemLista.PastaLocal, itemLista.Identificacao, itemLista.MoverPastaLocal, itemLista.EnderecoSaida, tipoapp, itemLista.Rbauto);
                            }
                            else
                            {
                                if (tipoapp == 1)
                                    FtpEnt.backup(criaBackup + "\\DownloadFTP_PastaTemporaria\\" + itemLista.PastaTemporaria, itemLista.EnderecoEntrada, itemLista.PastaLocal, itemLista.Identificacao, itemLista.MoverPastaLocal, itemLista.EnderecoSaida, tipoapp);
                                else
                                    FtpEnt.backupEdi(criaBackup + "\\DownloadFTP_PastaTemporaria\\" + itemLista.PastaTemporaria, itemLista.EnderecoEntrada, itemLista.PastaLocal, itemLista.Identificacao, itemLista.MoverPastaLocal, itemLista.EnderecoSaida, tipoapp, itemLista.Rbauto);
                            }

                            if (itemLista.MoverPastaLocal == 1)
                            {
                                if (itemLista.UploadSftp == 0)
                                    FtpEnt.Upload(FtpSai.host, itemLista.PastaLocal, FtpSai.pastaServidor, FtpSai.usuario, FtpSai.senha, itemLista.Identificacao, tipoapp);

                                else
                                    FtpEnt.UploadSftp(FtpSai.host, itemLista.PastaLocal, FtpSai.pastaServidor, FtpSai.usuario, FtpSai.senha, itemLista.Identificacao, tipoapp);
                            }
                        }

                        else
                        {
                            if (!Directory.Exists(criaBackup + "\\DownloadFTP_Backups\\Erros\\" + itemLista.Identificacao))
                            {
                                //Criamos um com o nome da identificacao

                                Directory.CreateDirectory(criaBackup + "\\DownloadFTP_Backups\\Erros\\" + itemLista.Identificacao);
                            }

                            string log = criaBackup + "\\DownloadFTP_Backups\\Erros\\" + itemLista.Identificacao + "\\Erro_log_" + DateTime.Now.ToShortDateString().ToString().Replace("/", "-") + ".txt";

                            if (!System.IO.File.Exists(log))
                            {
                                using (System.IO.File.Create(log)) { }
                            }

                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(log, true))
                            {

                                if (itemLista.EnderecoEntrada.StartsWith("ftp"))
                                {

                                    file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " Erro ao tentar acessar Endereço: " + FtpEnt.host + "\\" + FtpEnt.pastaServidor + " Usuario: " + FtpEnt.usuario);

                                }
                                else
                                {
                                    file.WriteLine(DateTime.Now.ToString().Replace("/", "-") + " Erro ao tentar acessar Endereço: " + itemLista.EnderecoEntrada);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

                MessageBox.Show("Verificar as informações da conexão " + conection.Identificacao + " " + conection.EnderecoEntrada + " " + conection.EnderecoSaida);

            }
            this.Processamento = false;

        }

        public void AtualizaForm()
        {
            CaixaDeTexto.AppendText("Aplicação em execução... " + DateTime.Now + " \n\n");
            CaixaDeTexto.Refresh();
        }

        private void CarregaListView()
        {
            try
            {
                List<Conexoes> conexoesList = new List<Conexoes>();
                conexoesList = this._appConexao.Listar();

                this.cb_Listar.DataSource = conexoesList;
                this.cb_Listar.DisplayMember = "identificacao";
                this.cb_Listar.ValueMember = "idconexao";

                foreach (var item in conexoesList)
                {
                    ListViewItem listaItem = new ListViewItem();
                    listaItem.Text = item.IdConexao.ToString();
                    listaItem.SubItems.Add(item.PastaLocal);
                    listaItem.SubItems.Add(item.Identificacao);

                    if (item.Ativo == 1)
                        listaItem.SubItems.Add("Sim");
                    else
                        listaItem.SubItems.Add("Não");

                    if (item.Prioridade == 1)
                        listaItem.SubItems.Add("Sim");
                    else
                        listaItem.SubItems.Add("Não");

                    listConexao.Items.Add(listaItem);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public void limparTela()
        {
            textUpload.Text = "";
            textDownload.Text = "";
            textPastaLocal.Text = "";
            textTemp.Text = "";
            textId.Text = "";
            textEditDownload.Text = "";
            textEditUpload.Text = "";
            textEditPasta.Text = "";
            rbt_AtivoSim.Checked = false;
            rbt_PrioridadeSim.Checked = false;
            rbt_EditMoverSim.Checked = false;
            rbt_AtivoNao.Checked = false;
            rbt_PrioridadeNao.Checked = false;
            rbt_EditMoverNao.Checked = false;
            EditSftp_Nao.Checked = false;
            EditSftp_Sim.Checked = false;
            SFTP_Nao.Checked = true;
            UploadSftp_Nao.Checked = true;
            rbt_Nao.Checked = true;
            EditUploadSftp_Sim.Checked = false;
            EditUploadSftp_Nao.Checked = false;
            check_RBAuto_Edit.Checked = false;
            check_RBAuto.Checked = false;
            btn_Atualizar.Enabled = false;
            cb_Listar.DataSource = null;
            cb_Listar.Items.Clear();
            listConexao.Items.Clear();


        }

        public void CriaDiretorios(Conexoes conexao)
        {

            if (!conexao.EnderecoEntrada.StartsWith("ftp"))
            {
                if (!Directory.Exists(conexao.EnderecoEntrada))
                {
                    //Criamos um diretorio com o nome da de entrada
                    Directory.CreateDirectory(conexao.EnderecoEntrada);
                }
            }

            if (!Directory.Exists(conexao.PastaLocal))
            {
                //Criamos um diretorio com o nome da pastalocal
                Directory.CreateDirectory(conexao.PastaLocal);
            }

            if (!Directory.Exists(criaBackup + "\\DownloadFTP_PastaTemporaria\\" + conexao.PastaTemporaria))
            {
                //Criamos um diretorio com o nome da pastalocal
                Directory.CreateDirectory(criaBackup + "\\DownloadFTP_PastaTemporaria\\" + conexao.PastaTemporaria);
            }

            //Criamos um diretorio com o nome da pasta para Backup

            if (!Directory.Exists(criaBackup + "\\DownloadFTP_Backups\\" + conexao.Identificacao))
            {

                Directory.CreateDirectory(criaBackup + "\\DownloadFTP_Backups\\" + conexao.Identificacao);
            }

            if (!Directory.Exists(criaBackup + "\\DownloadFTP_Backups\\" + conexao.Identificacao + "\\logs"))
            {
                Directory.CreateDirectory(criaBackup + "\\DownloadFTP_Backups\\" + conexao.Identificacao + "\\logs");
            }
        }

        #endregion

        #region Eventos do formulario

        public frm_DownloadFTP()
            {
                InitializeComponent();
        }

        private void btConectar_Click(object sender, EventArgs e)
        {
            errorProv.Clear();
            if (!String.IsNullOrEmpty(textTempo.Text) && (!String.IsNullOrEmpty(TipoAplicacao.Text)))
            {

                if (TipoAplicacao.Text.Equals("V") || TipoAplicacao.Text.Equals("E"))
                {

                    limparTela();
                    CarregaListView();

                    btConectar.Enabled = false;
                    btnDesc.Enabled = true;
                    groupAuto.Enabled = false;
                    groupEdit.Enabled = false;
                    btn_Excluir.Enabled = false;

                    tipoapp = TipoAplicacao.Text.Equals("V") ? 1 : 2;

                    lb_aplicacao.Text = TipoAplicacao.Text.Equals("V") ? "ViaLivre" : "Epedido";

                    lb_aplicacao.ForeColor = TipoAplicacao.Text.Equals("V") ? Color.LimeGreen : Color.Black;

                    tabPage1.BackColor = TipoAplicacao.Text.Equals("V") ? Color.WhiteSmoke : Color.DarkSeaGreen;

                    timer2.Interval = (int.Parse(textTempo.Text) * 60000);
                    CaixaDeTexto.AppendText("Iniciando a aplicação... " + DateTime.Now + " \n\n");
                    timer1.Start();
                    timer2.Start();
                    textTempo.Enabled = false;
                    TipoAplicacao.Enabled = false;

                }
                else errorProv.SetError(TipoAplicacao, "Informe o tipo da aplicação");

            }
            else
            {

                if (string.IsNullOrEmpty(textTempo.Text))
                {

                    errorProv.SetError(textTempo, "Informe o tempo de varredura");
                }

                if (string.IsNullOrEmpty(TipoAplicacao.Text))
                {

                    errorProv.SetError(TipoAplicacao, "Informe o tipo da aplicação");
                }


            }

        }

        private void btnDesc_Click(object sender, EventArgs e)
        {
            textTempo.Enabled = true;
            btConectar.Enabled = true;
            btnDesc.Enabled = false;
            groupAuto.Enabled = true;
            groupEdit.Enabled = true;
            TipoAplicacao.Enabled = true;
            lb_aplicacao.Text = "";
            tabPage1.BackColor = Color.WhiteSmoke;
            timer1.Stop();
            timer2.Stop();
            CaixaDeTexto.AppendText("Aguarde... - " + DateTime.Now + " \n\n");
            Thread.Sleep(10000);
            CaixaDeTexto.AppendText("Aplicação parada - " + DateTime.Now + " \n\n");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            AtualizaForm();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (this.Processamento == false)
            {
                this.CaixaDeTexto.Clear();
                CaixaDeTexto.AppendText("Iniciando varredura de arquivos FTP - " + DateTime.Now + " \n\n");
                Thread IniciaThread = new Thread(inicializaSistema);
                IniciaThread.Start();
            }
        }

        private void frm_DownloadFTP_Load(object sender, EventArgs e)
        {
            CarregaListView();
        }

        private void bt_Gravar_Click(object sender, EventArgs e)
        {
            try
            {
                errorProv.Clear();
                if (!String.IsNullOrEmpty(textDownload.Text) && !String.IsNullOrEmpty(textPastaLocal.Text) && !String.IsNullOrEmpty(textTemp.Text) && !String.IsNullOrEmpty(textId.Text))
                {
                    if ((!String.IsNullOrEmpty(textUpload.Text) && rbt_Sim.Checked) || (String.IsNullOrEmpty(textUpload.Text) && rbt_Nao.Checked) || (!String.IsNullOrEmpty(textUpload.Text) && rbt_Nao.Checked))
                    {
                        Conexoes conexao = new Conexoes();

                        conexao.EnderecoEntrada = textDownload.Text;
                        conexao.PastaLocal = textPastaLocal.Text;
                        conexao.EnderecoSaida = textUpload.Text;
                        conexao.Prioridade = 0;
                        conexao.Ativo = 1;
                        conexao.PastaTemporaria = textTemp.Text;
                        conexao.Identificacao = textId.Text;
                        conexao.MoverPastaLocal = rbt_Sim.Checked ? 1 : 0;
                        conexao.Sftp = SFTP_Sim.Checked ? 1 : 0;
                        conexao.UploadSftp = UploadSftp_Sim.Checked ? 1 : 0;
                        conexao.Rbauto = check_RBAuto.Checked ? 1 : 0;

                        this._appConexao.Gravar(conexao);

                        MessageBox.Show("Registrado com sucesso");

                        limparTela();

                        CarregaListView();
                    }
                    else
                        if (string.IsNullOrEmpty(textUpload.Text))
                            errorProv.SetError(textUpload, "Informe o caminho para upload");
                }
                else
                {
                    if (string.IsNullOrEmpty(textDownload.Text))
                        errorProv.SetError(textDownload, "Informe o caminho para download");

                    if (string.IsNullOrEmpty(textPastaLocal.Text))
                        errorProv.SetError(textPastaLocal, "Informe o caminho da pasta local");

                    if (string.IsNullOrEmpty(textTemp.Text))
                        errorProv.SetError(textTemp, "Informe o caminho da pasta temporaria");

                    if (string.IsNullOrEmpty(textId.Text))
                        errorProv.SetError(textId, "Informe uma identificação");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu erro ao gravar " + ex.Message);
            }
        }

        private void bt_Pesquisar_Click(object sender, EventArgs e)
        {
            try
            {
                Conexoes conexao = new Conexoes();
                conexao = this._appConexao.Pesquisar(Convert.ToInt32(cb_Listar.SelectedValue));

                if (conexao != null)
                {
                    textEditDownload.Text = conexao.EnderecoEntrada;
                    textEditUpload.Text = conexao.EnderecoSaida;
                    textEditPasta.Text = conexao.PastaLocal;

                    if (conexao.MoverPastaLocal == 1)
                        rbt_EditMoverSim.Checked = true;
                    else
                        rbt_EditMoverNao.Checked = true;

                    if (conexao.Ativo == 1)
                        rbt_AtivoSim.Checked = true;
                    else
                        rbt_AtivoNao.Checked = true;

                    if (conexao.Prioridade == 1)
                        rbt_PrioridadeSim.Checked = true;
                    else
                        rbt_PrioridadeNao.Checked = true;

                    if (conexao.Sftp == 1)
                        EditSftp_Sim.Checked = true;
                    else
                        EditSftp_Nao.Checked = true; ;

                    if (conexao.UploadSftp == 1)
                        EditUploadSftp_Sim.Checked = true;
                    else
                        EditUploadSftp_Nao.Checked = true;

                    if (conexao.Rbauto == 1)
                        check_RBAuto_Edit.Checked = true;
                    else
                        check_RBAuto_Edit.Checked = false;
  
                    btn_Atualizar.Enabled = true;
                }
                else
                {
                    textEditDownload.Text = "";
                    textEditUpload.Text = "";
                    textEditPasta.Text = "";
                    rbt_AtivoSim.Checked = false;
                    rbt_PrioridadeSim.Checked = false;
                    btn_Atualizar.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu erro ao consultar " + ex.Message);
            }
        }

        private void bt_Atualizar_Click(object sender, EventArgs e)
        {
            try
            {
                errorProv.Clear();
                if (!String.IsNullOrEmpty(textEditDownload.Text) && !String.IsNullOrEmpty(textEditPasta.Text))
                {
                    if ((!String.IsNullOrEmpty(textEditUpload.Text) && rbt_EditMoverSim.Checked) || (String.IsNullOrEmpty(textEditUpload.Text) && rbt_EditMoverNao.Checked) || (!String.IsNullOrEmpty(textEditUpload.Text) && rbt_EditMoverNao.Checked))
                    {
                        Conexoes conexao = new Conexoes();

                        conexao.Ativo = rbt_AtivoSim.Checked ? 1 : 0;
                        conexao.Prioridade = rbt_PrioridadeSim.Checked ? 1 : 0;
                        conexao.MoverPastaLocal = rbt_EditMoverSim.Checked ? 1 : 0;
                        conexao.Sftp = EditSftp_Sim.Checked ? 1 : 0;
                        conexao.UploadSftp = EditUploadSftp_Sim.Checked ? 1 : 0;
                        conexao.EnderecoEntrada = textEditDownload.Text;
                        conexao.EnderecoSaida = textEditUpload.Text;
                        conexao.PastaLocal = textEditPasta.Text;
                        conexao.IdConexao = Convert.ToInt32(this.cb_Listar.SelectedValue);
                        conexao.Rbauto = check_RBAuto_Edit.Checked ? 1 : 0;

                        this._appConexao.Atualizar(conexao);

                        MessageBox.Show("Atualizado com sucesso");

                        limparTela();
                        CarregaListView();
                    }
                    else
                        if (string.IsNullOrEmpty(textEditUpload.Text))
                            errorProv.SetError(textEditUpload, "Informe o caminho para upload");
                }
                else
                {
                    if (string.IsNullOrEmpty(textEditDownload.Text))
                        errorProv.SetError(textEditDownload, "Informe o caminho para download");

                    if (string.IsNullOrEmpty(textEditPasta.Text))
                        errorProv.SetError(textEditPasta, "Informe o caminho da pasta local");
                }
            }
            catch (Exception exe)
            {
                string log = exe.ToString();
                MessageBox.Show("Erro ao atualizar", exe.Message);
            }
        }

        private void btn_Excluir_Click(object sender, EventArgs e)
        {
            int Codigo = 0;

            try
            {
                if (listConexao.SelectedItems.Count > 0)
                {
                    Codigo = Convert.ToInt32(listConexao.SelectedItems[0].Text);

                    if (MessageBox.Show("Deseja excluir este registro (" + listConexao.SelectedItems[0].SubItems[2].Text + ") ?", "Excluir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        this._appConexao.Excluir(Codigo);
                        limparTela();
                        CarregaListView();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possivel excluir o registro selecionado. ", ex.Message);
            }
        }

        private void listConexao_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(listConexao.SelectedItems[0].Text) > 0)
                {
                    if (btnDesc.Enabled == false)
                    {
                        btn_Excluir.Enabled = true;
                    }
                }
            }
            catch (Exception)
            {
                btn_Excluir.Enabled = false;
            }
        }

        private void cb_Listar_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn_Atualizar.Enabled = false;
        }
        #endregion
    }
}

