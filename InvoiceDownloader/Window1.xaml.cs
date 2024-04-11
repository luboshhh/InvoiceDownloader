using MailKit.Net.Imap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InvoiceDownloader
{
    
    public partial class Window1 : Window
    {
        private SelectedAccount selectedAccount;
       
        public Window1(SelectedAccount account)
        {
            InitializeComponent();
            selectedAccount = account;

            storeButton.Content = selectedAccount.Store;






        }


        public  async void AllMailsButton_Click(object sender, RoutedEventArgs e)
        {
           
                Button button = sender as Button;

            selectedAccount.Date = "all";
               
                    EmailDownloader downloader = new EmailDownloader();
                    await downloader.DownloadInvoices(selectedAccount);
                
               
        }


        private void DatePickerFromDay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerFromDay.SelectedDate != null && DatePickerFrom.SelectedDate != null)
            {
               
                DatePickerFromDay.SelectedDate = null;
            }
            else
            {
                
                Potvrdit.IsEnabled = DatePickerFromDay.SelectedDate != null || DatePickerFrom.SelectedDate != null;
            }
        }

        private void DatePickerFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerFrom.SelectedDate != null && DatePickerFromDay.SelectedDate != null)
            {
                
                DatePickerFromDay.SelectedDate = null;
            }
            else
            {
                
                Potvrdit.IsEnabled = DatePickerFromDay.SelectedDate != null || DatePickerFrom.SelectedDate != null;
            }
        }

        private async void Potvrdit_Click(object sender, RoutedEventArgs e)
        {
            
            if (DatePickerFromDay.SelectedDate != null)
            {
               
                string realDate = DatePickerFromDay.SelectedDate.Value.ToString("yyyy_MM_dd");

                string formattedDate = realDate + "_FromDay";

                
                selectedAccount.Date = formattedDate;

                EmailDownloader downloader = new EmailDownloader();
                await downloader.DownloadInvoices(selectedAccount);


            }
            else if (DatePickerFrom.SelectedDate != null)
            {
               
                string realDate = DatePickerFrom.SelectedDate.Value.ToString("yyyy_MM_dd");

                string formattedDate = realDate + "_From";

               
                selectedAccount.Date = formattedDate;

                EmailDownloader downloader = new EmailDownloader();
                await downloader.DownloadInvoices(selectedAccount);
            }
        }

        private void Zpet_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            selectedAccount = new SelectedAccount();
        }


    }
}
