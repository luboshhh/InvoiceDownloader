
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Windows;

namespace InvoiceDownloader
{
    public class EmailDownloader
    {
        private Dictionary<string, int> downloadedInvoiceCounts = new Dictionary<string, int>();

        public async Task DownloadInvoices(SelectedAccount selectedAccount)
        {
            var email = selectedAccount.Email;
            var password = selectedAccount.Password;
            var provider = selectedAccount.Provider;
            var store = selectedAccount.Store;
            var date = selectedAccount.Date;

            string invoicesDirectory = Path.Combine("Invoices", store);

            using (var client = new ImapClient())
            {
                try
                {
                    client.CheckCertificateRevocation = false;

                    if (provider == "seznam")
                    {

                        await client.ConnectAsync("imap.seznam.cz", 993, SecureSocketOptions.Auto); 
                    }
                    else if (provider == "gmail")
                    {
                        await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.Auto); 
                    } 

                    await client.AuthenticateAsync(email, password);

                    var inbox = await client.GetFolderAsync("INBOX");
                    await inbox.OpenAsync(FolderAccess.ReadOnly);
                
                    SearchQuery query = null;





                    if (!string.IsNullOrEmpty(date) && date.EndsWith("_FromDay"))
                    {
                        var fromDate = DateTime.ParseExact(date.Replace("_FromDay", ""), "yyyy_MM_dd", null);

                        if (store == "Nike")
                        {
                            query = SearchQuery.SubjectContains("Zde je tvůj přehled plateb za objednávku").And(SearchQuery.SentOn(fromDate));
                        }
                        else if (store == "LVR")
                        {
                            query = SearchQuery.SubjectContains("Shipment Confirmation").And(SearchQuery.SentOn(fromDate));
                        }

                        else if (store == "About You")
                        {
                            query = SearchQuery.SubjectContains("Informace o doručení a platbě").And(SearchQuery.SentOn(fromDate));
                        }

                        else if (store == "Adidas")
                        {
                            query = SearchQuery.SubjectContains("Objednávka").And(SearchQuery.SentOn(fromDate));
                        }
                        
                    }


                    else if (!string.IsNullOrEmpty(date) && date.EndsWith("_From"))
                    {
                        var fromDate = DateTime.ParseExact(date.Replace("_From", ""), "yyyy_MM_dd", null);

                        if (store == "Nike")
                        {
                            query = SearchQuery.SubjectContains("Zde je tvůj přehled plateb za objednávku").And(SearchQuery.DeliveredAfter(fromDate));
                        }
                        else if (store == "LVR")
                        {
                            query = SearchQuery.SubjectContains("Shipment Confirmation").And(SearchQuery.DeliveredAfter(fromDate));
                        }

                        else if (store == "About You")
                        {
                            query = SearchQuery.SubjectContains("Informace o doručení a platbě").And(SearchQuery.DeliveredAfter(fromDate));
                        }

                        else if (store == "Adidas")
                        {
                            query = SearchQuery.SubjectContains("Objednávka").And(SearchQuery.DeliveredAfter(fromDate));
                        }
                        
                    }



                    else 
                    {
                        if (store == "Nike")
                        {
                            query = SearchQuery.SubjectContains("Zde je tvůj přehled plateb za objednávku");
                        }
                        else if (store == "LVR")
                        {
                            query = SearchQuery.SubjectContains("Shipment Confirmation");
                        }

                        else if (store == "About You")
                        {
                            query = SearchQuery.SubjectContains("Informace o doručení a platbě");
                        }

                        else if (store == "Adidas")
                        {
                            query = SearchQuery.SubjectContains("Objednávka");

                        }

                    }













                    var results = await inbox.SearchAsync(query);

                    foreach (var uid in results)
                    {
                        var message = await inbox.GetMessageAsync(uid);


                        


                        var attachments = message.Attachments.OfType<MimePart>().Where(x => x.FileName.EndsWith(".pdf"));

                        if (store == "LVR")
                        {
                            attachments = message.Attachments.OfType<MimePart>();
                        }

                        foreach (var attachment in attachments)
                        {
                            var dateMail = message.Date.UtcDateTime.ToString("yyyy_MM_dd");
                            string baseFileName = $"{store}_{dateMail}";

                            int invoiceCount = 1;
                            if (downloadedInvoiceCounts.ContainsKey(baseFileName))
                            {
                               
                                invoiceCount = downloadedInvoiceCounts[baseFileName] + 1;
                            }

                           
                            downloadedInvoiceCounts[baseFileName] = invoiceCount;

                            
                            string fileName = $"{baseFileName}_{invoiceCount}.pdf";

                           
                            string filePath = Path.Combine(invoicesDirectory, fileName);
                            using (var stream = File.Create(filePath))
                            {
                                await attachment.Content.DecodeToAsync(stream);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                    MessageBox.Show($"Chyba při stahování faktur: {ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    
                    await client.DisconnectAsync(true);
                }
            }
        }
    }

}
