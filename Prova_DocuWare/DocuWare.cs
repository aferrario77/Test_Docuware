using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using DocuWare.Platform.ServerClient;
using DocuWare.Services.Http.Client;

namespace Prova_DocuWare
{

  public class DocuWareClass
  {

    private Int32 conteggio = 0;
    private bool _Busy = false;

    public bool Busy
    {
      get
      {
        return _Busy;
      }
      set
      {
        Busy = _Busy;
      }
    }

    #region Connection

    public ServiceConnection Connect(Uri url, string user, string psw, out string errore)
    {
      try
      {
        errore = "";
        return ServiceConnection.Create(url, user, psw);
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        return null;
      }
    }

    public ServiceConnection ConnectWithUserAgent(Uri url, string user, string psw, out string errore)
    {
      try
      {
        errore = "";
        return ServiceConnection.Create(url, user, psw,
            userAgent: new System.Net.Http.Headers.ProductInfoHeaderValue[] {
                    new System.Net.Http.Headers.ProductInfoHeaderValue("DocuWare+.NET+API+Test+Client", "1.0")
            });
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        return null;
      }
    }

    public static ServiceConnection ConnectWithOrg(Uri url, string user, string psw, string organization, out string errore)
    {
      try
      {
        errore = "";
        return ServiceConnection.Create(url, user, psw, organization); //  organization: "Peters Engineering");
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        return null;
      }
    }

    public ServiceConnection ConnectWithCaching(Uri url, string user, string psw, out string errore)
    {
      try
      {
        errore = "";
        var handler = new WebRequestHandler() { CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default) };
        return ServiceConnection.Create(url, user, psw, httpClientHandler: handler);
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        return null;
      }
    }

    public ServiceConnection ConnectWithNTLM(Uri url, string user, string psw, out string errore)
    {
      try
      {
        errore = "";
        return ServiceConnection.CreateWithWindowsAuthentication(url, user, psw);
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        return null;
      }
    }

    public ServiceConnection ConnectWithDefaultUser(Uri url, out string errore)
    {
      try
      {
        errore = "";
        return ServiceConnection.CreateWithWindowsAuthentication(url, System.Net.CredentialCache.DefaultCredentials);
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        return null;
      }

    }

    public ServiceConnection ConnectWithDisplayLanguageAndFormatCulture(Uri url, string nazione, string user, string psw, out string errore)
    {
      try
      {
        // Create WebRequestHandler with cookie container
        var handler = new WebRequestHandler()
        {
          CookieContainer = new CookieContainer(),
          UseCookies = true,
        };
        //Set format culture to German
        handler.CookieContainer.Add(new Cookie("DWFormatCulture", nazione, "/", "localhost")); // de
        //Set display language to German
        handler.CookieContainer.Add(new Cookie("DWLanguage", nazione, "/", "localhost"));  // de
        errore = "";
        return ServiceConnection.Create(url, user, psw, httpClientHandler: handler);
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        return null;
      }
    }
    #endregion

    public bool InfoOrganizzazione(ref ServiceConnection Connection, out string Organizzazione, out string Guid, out string errore)
    {
      try
      {
        _Busy = true;
        if (Connection == null)
        {
          errore = "...";
          Organizzazione = "";
          Guid = "";
          _Busy = false;
          return false;
        }
        else
        {
          errore = "";
          Organizzazione = Connection.Organizations[0].Name.ToString(); // shows how you can access the active organization          
          var pippo = Connection.Organizations[0].GetFileCabinetsFromFilecabinetsRelation().FileCabinet;
          Guid = pippo[0].Id.ToString();
          _Busy = false;
          return true;
        }
      }
      catch (Exception ex)
      {
        _Busy = false;
        Organizzazione = "";
        Guid = "";
        errore = ex.ToString();
        return false;
      }
    }

    private string ChangeDataFormat(string valore)
    {
      try
      {
        DateTime enteredDate = DateTime.Parse(valore);
        valore = enteredDate.ToString("MM/dd/yyyy");
        return valore;
      }
      catch (Exception ex)
      {
        return valore;
      }
    }

    #region CabinetsAndFiles

    public bool GetFileCabinets(ref ServiceConnection Connection, bool WebBasket, out string errore,out List<FileCabinet> ListFileCabinet, out string nome, out string IDcabinet)
    {
      try
      {        
        var fileCabinets = Connection.Organizations[0].GetFileCabinetsFromFilecabinetsRelation().FileCabinet;
        
        //List<FileCabinet> tmpList = new List<FileCabinet>(); // Creating a new list of
        //foreach (var fc in fileCabinets)
        //{          
        //  if (fc.IsBasket == WebBasket ) // aggiunge solo Cabient or webBasket files
        //    tmpList.Add(fc);  // f.IsBasket -> TRUE web basket , FALSE File Cabinet          
        //}        
        ListFileCabinet = null;        
        errore = "";
        IDcabinet = fileCabinets[0].Id.ToString(); // 
        nome = fileCabinets[0].Name.ToString();
        return true;
      }
      catch(Exception ex)
      {
        errore = ex.ToString();
        ListFileCabinet = null;
        IDcabinet = "";
        nome = "";
        return false;
      }
    }

    public bool GetWebBasketDefault (ref ServiceConnection Connection, out string errore, out List<FileCabinet> ListFileCabinet)
    {
      try
      {
        var fileCabinets = Connection.Organizations[0].GetFileCabinetsFromFilecabinetsRelation().FileCabinet;
        List<FileCabinet> tmpList = new List<FileCabinet>(); // Creating a new list of        
        foreach (var fc in fileCabinets)
        {
          if (fc.IsBasket == true && fc.Default == true) // aggiunge solo il default Web 
            tmpList.Add(fc);         
        }
        ListFileCabinet = tmpList;
        errore = "";
        return true;
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        ListFileCabinet = null;
        return false;
      }
    }

    public List<Document> ListAllDocuments(ref ServiceConnection conn, Int32 IDDOC , string fileCabinetId,  out string errore, int? count = 999999)
    {
      conteggio = 0;
      try
      {
        DocumentsQueryResult queryResult = conn.GetFromDocumentsForDocumentsQueryResultAsync(
            fileCabinetId,            
            count: count
            )
            .Result;
        List<Document> result = new List<Document>();   
        GetAllDocuments(queryResult, result);

        foreach (Document document in result)
        {
          Console.WriteLine("Document {0} created at {1}", document.Id, document.CreatedAt);
        }

        errore = "";
        return result;
      }
      catch (Exception ex)
      {        
        errore = ex.ToString();
        return null;
      }
    }

    public void GetAllDocuments(DocumentsQueryResult queryResult, List<Document> documents)
    {      
      documents.AddRange(queryResult.Items);
      if (queryResult.NextRelationLink != null)
      {
        GetAllDocuments(queryResult.GetDocumentsQueryResultFromNextRelationAsync().Result, documents);
      }   
    }

    public bool UpdateDocsIndexFields(Document Documento, DocumentIndexFields Campi , out string errore)
    {
      try
      {
        Documento.PutToFieldsRelationForDocumentIndexFields(Campi);
        errore = "";
        return true;
      }
      catch(Exception ex)
      {
        errore = ex.ToString();
        return false;
      }
    }

    public DocuWare.Platform.ServerClient.FileCabinet GetFileCabinet(ref ServiceConnection conn, string idConn , out string errore)
    {
      try
      {
        var esito = conn.GetFileCabinet(idConn);
        errore = "";
        return esito;
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        return null;
      }
    }

    public Document InsertNewDoc(FileCabinet FCabinet, Document Values, FileInfo filepath, out string errore)
    {
      try
      {
        errore = "";
        return FCabinet.UploadDocument(Values, filepath);
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        return null;
      }
    }

    public DocumentsQueryResult DocFilterValues(ref ServiceConnection conn, string aziendaDa, string aziendaA, string numeroDa, string numeroA, string dataDa, string dataA, int MaxItems, string SortField, out string errore)
    {
      try
      {
        //
        if (aziendaDa == "") aziendaDa = "A";
        if (aziendaA == "") aziendaA = "Z";
        if (numeroDa == "") numeroDa = "1";
        if (numeroA == "") numeroA = "999999999";
        if (dataDa == "") dataDa = "#01/01/1900#";
        if (dataA == "") dataA = "#12/31/2099#";
        //

        var prova = conn.Organizations[0].GetDialogInfosFromDialogsRelation();
        var dialog = prova.Dialog[0].GetDialogFromSelfRelation();

        var q = new DialogExpression()
        {
          Operation = DialogExpressionOperation.And,
          Condition = new List<DialogExpressionCondition>()
              {
                DialogExpressionCondition.Create("Azienda", aziendaDa, aziendaA ),
                DialogExpressionCondition.Create("Numero", numeroDa, numeroA ) ,
                DialogExpressionCondition.Create("Data", ChangeDataFormat(dataDa), ChangeDataFormat(dataA))
          },
          Count = MaxItems,
          SortOrder = new List<SortedField>
              {
                  SortedField.Create(SortField, SortDirection.Desc) // DWSTOREDATETIME
              }
        };
        
        var queryResult = dialog.GetDocumentsResult(q);
        

        errore = "";
        return queryResult;
      }
      catch (Exception ex)
      {
        errore = ex.ToString();
        return null;
      }
    }

    #endregion
  }
}
