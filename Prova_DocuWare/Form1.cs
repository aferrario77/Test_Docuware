using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Windows.Forms;
using System.Drawing;
using DocuWare.Platform.ServerClient;
using DocuWare.Services.Http.Client;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.IO;
using System.Xml.Linq;
using DocuWare.Services.Http;

namespace Prova_DocuWare
{
  public partial class FrmMain : Form
  {
    DocuWareClass Test = new DocuWareClass();
    public ServiceConnection DocuConn;

    public FrmMain()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void btn_connect_Click(object sender, EventArgs e)
    {
      if (Test.Busy == true) return;
      string message = "";
      string user = txt_user.Text;
      string psw = txt_psw.Text;
      Uri uri = new Uri(txt_uri.Text);
      DocuConn = Test.ConnectWithUserAgent(uri, user, psw, out message);
      if (DocuConn == null)
      {
        lbl_esito_conn.BackColor = Color.Red;
        lbl_esito_conn.ForeColor = Color.WhiteSmoke;
        MessageBox.Show(message, "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else
      {
        lbl_esito_conn.BackColor = Color.ForestGreen;
        lbl_esito_conn.ForeColor = Color.WhiteSmoke;
        lbl_esito_conn.Text = "Connesso!";
      }
    }

    private void btn_org_Click(object sender, EventArgs e)
    {
      if (Test.Busy == true) return;
      if (Test.InfoOrganizzazione(ref DocuConn, out string nome, out string guid, out string errore) == false)
      {
        lbl_esito_org.BackColor = Color.Red;
        lbl_esito_org.ForeColor = Color.WhiteSmoke;
        lbl_id.BackColor = Color.Red;
        lbl_id.ForeColor = Color.WhiteSmoke;
        lbl_esito_org.Text = "";
        lbl_id.Text = "";
        MessageBox.Show(errore, "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else
      {
        lbl_id.Text = guid;
        lbl_id.BackColor = Color.ForestGreen;
        lbl_id.ForeColor = Color.WhiteSmoke;
        lbl_esito_org.BackColor = Color.ForestGreen;
        lbl_esito_org.ForeColor = Color.WhiteSmoke;
        lbl_esito_org.Text = nome;
      }
    }

    private void btn_cab_Click(object sender, EventArgs e)
    {
      if (Test.Busy == true) return;


      if (Test.GetFileCabinets(ref DocuConn, false, out string errore, out List<FileCabinet> ListFileCabinet, out string nome, out string ID) == false)
      {
        lbl_esito_cab.BackColor = Color.Red;
        lbl_esito_cab.ForeColor = Color.WhiteSmoke;
        //lbl_id.Text = "";
        lbl_esito_cab.Text = "";
        MessageBox.Show(errore, "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else

        lbl_esito_cab.BackColor = Color.ForestGreen;
      lbl_esito_cab.ForeColor = Color.WhiteSmoke;
      //lbl_id.Text = ID;
      lbl_esito_cab.Text = nome;
    }

    private void btn_bas_Click(object sender, EventArgs e)
    {
      if (Test.Busy == true) return;
      if (Test.GetWebBasketDefault(ref DocuConn, out string errore, out List<FileCabinet> ListFileCabinet) == false)
      {
        lbl_esito_bas.BackColor = Color.Red;
        lbl_esito_bas.ForeColor = Color.WhiteSmoke;
        MessageBox.Show(errore, "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else
      if (ListFileCabinet.Count == 0)
      {
        lbl_esito_bas.BackColor = Color.Red;
        lbl_esito_bas.ForeColor = Color.WhiteSmoke;
        lbl_esito_bas.Text = "Nessun elemento trovato!";
      }
      else
      {
        lbl_esito_bas.BackColor = Color.ForestGreen;
        lbl_esito_bas.ForeColor = Color.WhiteSmoke;
        lbl_esito_bas.Text = ListFileCabinet[0].Name.ToString();
      }
    }

    private void btn_files_Click(object sender, EventArgs e)
    {      
      // added
      if (Test.Busy == true) return;
      DocumentsQueryResult MakeFilter;
      MakeFilter = Test.DocFilterValues(ref DocuConn, "", "", "", "", "", "", 1000, "DWSTOREDATETIME", out string errore);
      if (MakeFilter == null)
      {
        if (errore == "") // nessun errore ma nessun file trovato!
        {
          list_files.Items.Clear();
          list_files.BackColor = Color.Gainsboro;
          list_files.ForeColor = Color.Black;
        }
        else // errore!
        {
          list_files.Items.Clear();
          list_files.BackColor = Color.Red;
          list_files.ForeColor = Color.WhiteSmoke;
          MessageBox.Show(errore, "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      else
      {
        list_files.Items.Clear();
        foreach (var d in MakeFilter.Items)
        {
          string azienda = d["AZIENDA"].Item.ToString();
          string numero = d["NUMERO"].Item.ToString();
          string data = d["DATA"].Item.ToString();
          list_files.BackColor = Color.Gainsboro;
          list_files.ForeColor = Color.Black;
          string docValue = d.Id.ToString() + " - " + azienda + " - Num: " + numero + " - Data: " + ChangeDataFormat(data, false);
          list_files.Items.Add(docValue);
        }
      }

    }


    private void timer_Tick(object sender, EventArgs e)
    {
      try
      {
        if (Test.Busy == true)
          this.Cursor = Cursors.WaitCursor;
        else
          this.Cursor = Cursors.Default;
      }
      catch (Exception ex)
      {
        //
      }
    }

    private void btn_acc_Click(object sender, EventArgs e)
    {
      //
      if (Test.Busy == true) return;
      DocumentsQueryResult MakeFilter;
      MakeFilter = Test.DocFilterValues(ref DocuConn, txt_az_da.Text, txt_az_a.Text, txt_nu_da.Text, txt_nu_a.Text, txt_data_da.Text, txt_data_a.Text, 1000, "DWSTOREDATETIME", out string errore);
      if (MakeFilter == null)
      {
        if (errore == "") // nessun errore ma nessun file trovato!
        {
          list_sort.Items.Clear();
          list_sort.BackColor = Color.Gainsboro;
          list_sort.ForeColor = Color.Black;
        }
        else // errore!
        {
          list_sort.Items.Clear();
          list_sort.BackColor = Color.Red;
          list_sort.ForeColor = Color.WhiteSmoke;
          MessageBox.Show(errore, "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      else
      {
        list_sort.Items.Clear();
        foreach (var d in MakeFilter.Items)
        {
          string azienda = d["AZIENDA"].Item.ToString();
          string numero = d["NUMERO"].Item.ToString();
          string data = d["DATA"].Item.ToString();
          list_sort.BackColor = Color.Gainsboro;
          list_sort.ForeColor = Color.Black;
          string docValue = d.Id.ToString() + " - " + azienda + " - Num: " + numero + " - Data: " + ChangeDataFormat(data, false);
          list_sort.Items.Add(docValue);
        }
      }

    }


    private void button1_Click(object sender, EventArgs e)
    {

    }

    public void GetAllDocuments(DocumentsQueryResult queryResult, List<Document> documents)
    {
      documents.AddRange(queryResult.Items);
      if (queryResult.NextRelationLink != null)
      {
        GetAllDocuments(queryResult.GetDocumentsQueryResultFromNextRelationAsync().Result, documents);
      }
    }

    private void button1_ClientSizeChanged(object sender, EventArgs e)
    {

    }

    private void txt_new_data_MouseLeave(object sender, EventArgs e)
    {

    }

    private string ChangeDataFormat(string valore, bool DocuWareUpdate) // DocuWareUpdate -> true in scrittura!
    {
      try
      {
        DateTime enteredDate = DateTime.Parse(valore);
        if (DocuWareUpdate == false)
          valore = enteredDate.ToString("dd/MM/yyyy");
        else
          valore = enteredDate.ToString("MM/dd/yyyy");
        return valore;
      }
      catch (Exception ex)
      {
        return "";
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {


    }

    private void btn_upd_Click(object sender, EventArgs e)
    {
      if (list_sort.Items.Count > 1 || list_sort.Items.Count < 1)
      {
        MessageBox.Show("Impossibile aggiornare una lista con più di un elemento!", "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      DocumentsQueryResult MakeFilter;
      MakeFilter = Test.DocFilterValues(ref DocuConn, txt_az_da.Text, txt_az_a.Text, txt_nu_da.Text, txt_nu_a.Text, txt_data_da.Text, txt_data_a.Text, 1000, "DWSTOREDATETIME", out string errore);
      List<Document> result = new List<Document>();
      GetAllDocuments(MakeFilter, result);
      foreach (Document document in result)
      {
        // legge campi da inserire in DB, memorizzando i valori già inseriti!
        string azienda = document["AZIENDA"].Item.ToString();
        string numero = document["NUMERO"].Item.ToString();
        string data = document["DATA"].Item.ToString();
        // se inserito sovrascrivo il nuovo valore
        if (txt_new_az.Text.Trim() != "") azienda = txt_new_az.Text;
        if (txt_new_num.Text.Trim() != "") numero = txt_new_num.Text;
        if (txt_new_data.Text.Trim() != "") data = txt_new_data.Text;
        var fields = new DocumentIndexFields()
        {
          Field = new List<DocumentIndexField>()
                {
                    DocumentIndexField.Create("NUMERO", numero),
                    DocumentIndexField.Create("DATA", ChangeDataFormat(data,true)),
                    DocumentIndexField.Create("AZIENDA", azienda)
                }
        };
        bool res = Test.UpdateDocsIndexFields(document, fields, out string errore2);
        if (res == false)
        {
          list_sort.BackColor = Color.Red;
          list_sort.ForeColor = Color.WhiteSmoke;
        }
        else
        {
          list_sort.BackColor = Color.ForestGreen;
          list_sort.ForeColor = Color.WhiteSmoke;
        }
      }
    }

    private void btn_path_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog openFileDialog = new OpenFileDialog())
      {
        openFileDialog.InitialDirectory = @System.Reflection.Assembly.GetEntryAssembly().Location;
        openFileDialog.Filter = "All files (*.*)|*.*";
        openFileDialog.FilterIndex = 0;
        openFileDialog.RestoreDirectory = true;
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
          txt_file_path.Text = openFileDialog.FileName;
        }
      }
    }

    private void btn_new_Click(object sender, EventArgs e)
    {
      string azienda = txt_new_az.Text.Trim();
      string numero = txt_new_num.Text.Trim();
      string data = txt_new_data.Text.Trim();
      string path = @txt_file_path.Text;
      if ((azienda == "") || (numero == "") || (data == "") || (path == ""))
      {
        MessageBox.Show("Impossibile aggiornare, compilare tutti i campi!", "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      try
      {
        Document indexData = new Document()
        {
          Fields = new List<DocumentIndexField>()
                {
                    DocumentIndexField.Create("AZIENDA", azienda),
                    DocumentIndexField.Create("NUMERO", numero),
                    DocumentIndexField.Create("DATA", ChangeDataFormat(data,true)),
                }
        };        
        FileInfo filepath = new FileInfo(path);
        FileCabinet fileCabinet = Test.GetFileCabinet(ref DocuConn, lbl_id.Text, out string errore);
        Document uploadedDocument = Test.InsertNewDoc(fileCabinet, indexData, filepath, out errore);

        MessageBox.Show("Documento creato con successo!", "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Impossibile creare il documento! Errore: " + ex.ToString(), "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void button1_Click_1(object sender, EventArgs e)
    {

    }

    private void btn_del_Click(object sender, EventArgs e)
    {
      if (list_sort.SelectedIndex == -1) return;
      string id = list_sort.SelectedItem.ToString();
      char divisorio = '-';
      string[] values = id.Split(divisorio);      
      var fileCabinets = Test.GetFileCabinet(ref DocuConn, lbl_id.Text, out string errore);
      DocuWare.Platform.ServerClient.Dialog search = fileCabinets.GetDialogFromCustomSearchRelation();
      Document documentToDelete = search.GetDialogFromSelfRelation().GetDocumentsResult(new DialogExpression()
      {
        Start = 0,
        Count = 1,
        Condition = new List<DialogExpressionCondition>(new List<DialogExpressionCondition>
        {
            new DialogExpressionCondition
            {
                Value = new List<string>() {values[0]},
                DBName = "DWDOCID"
            }
        }),
        Operation = DialogExpressionOperation.And
      }).Items.FirstOrDefault() ?? throw new InvalidOperationException();
      string esito = documentToDelete.DeleteSelfRelation().ToString().ToUpper();
      if (esito == "OK")
        MessageBox.Show("Documento con ID " + id + " eliminato con successo!", "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Information);
      else
        MessageBox.Show("Errore durante l'eliminazione del file con ID " + id + " !", "ATTENZIONE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void button1_Click_2(object sender, EventArgs e)
    {
      if (list_sort.SelectedIndex == -1) return;
      string id = list_sort.SelectedItem.ToString();
      char divisorio = '-';
      string[] values = id.Split(divisorio);      
      var fileCabinets = Test.GetFileCabinet(ref DocuConn, lbl_id.Text, out string errore);
      DocuWare.Platform.ServerClient.Dialog search = fileCabinets.GetDialogFromCustomSearchRelation();
      Document documentoToDownload = search.GetDialogFromSelfRelation().GetDocumentsResult(new DialogExpression()
      {
        Start = 0,
        Count = 1,
        Condition = new List<DialogExpressionCondition>(new List<DialogExpressionCondition>
        {
            new DialogExpressionCondition
            {
                Value = new List<string>() {values[0]},
                DBName = "DWDOCID"
            }
        }),
        Operation = DialogExpressionOperation.And
      }).Items.FirstOrDefault() ?? throw new InvalidOperationException();


      if (documentoToDownload.FileDownloadRelationLink == null)
        documentoToDownload = documentoToDownload.GetDocumentFromSelfRelation();

      var downloadResponse = documentoToDownload.PostToFileDownloadRelationForStreamAsync(
          new FileDownload()
          {
            TargetFileType = FileDownloadType.Auto
          }).Result;

      var contentHeaders = downloadResponse.ContentHeaders;

      System.IO.Stream Stream = downloadResponse.Content;
      long? ContentLength = contentHeaders.ContentLength;
      string ContentType = contentHeaders.ContentType.MediaType;
      string FileName = downloadResponse.GetFileName();
    }
  }
 }

