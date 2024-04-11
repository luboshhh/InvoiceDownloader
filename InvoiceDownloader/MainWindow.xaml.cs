using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;



// Jelikož odjíždím na dovolenou, musím program odevzdat dřív. Podařilo se mi program zprovoznit zatím pouze pro Gmail. Spoustu hodin jsem se snažil přijít na to, proč to nefunguje a nejspíš knihovna, kterou jsem si vybral nepodporuje Seznam. V kódu jsem ale možnost pro Seznam maily nechal, protože ji budu pravděpodobně předělávat mimo projekt s jinou knihovnou pro reálné využití. 

namespace InvoiceDownloader
{
   
    public partial class MainWindow : Window
    {
        private const string AccountsFilePath = "accounts.txt";
       
       

        public static SelectedAccount selectedAccount { get; set; }
        public bool IsAccountSelected { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadAccounts();
            InitializeSelectedAccount();
            
            CreateFiles();


        }




        private void LoadAccounts()
        {
            try
            {
                
                if (!File.Exists(AccountsFilePath))
                    using (StreamWriter writer = new StreamWriter(AccountsFilePath))
                    {
                        
                    }

              


                
                string[] lines = File.ReadAllLines(AccountsFilePath);

                
                for (int i = 0; i < lines.Length; i++)
                {
                    
                    if (i >= EmailGrid.Children.Count)
                        break;

                    
                    if (EmailGrid.Children[i+1] is TextBlock textBlock)
                    {
                        textBlock.Text = lines[i].Split('_')[0];
                        Grid.SetRow(textBlock, i+1);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání e-mailů: {ex.Message}");
            }
        }
        private void InitializeSelectedAccount()
        {
            selectedAccount = new SelectedAccount
            {
                Email = null,
                Password = null,
                Provider = null,
                Store = null,
                Date = null
            };
        }

        private void CreateFiles()
        {
            string[] folderName = { "Nike", "About You", "LVR", "Adidas" };
            string folderPath;

            foreach (string folder in folderName)
            {
                folderPath = Path.Combine("Invoices", folder);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            string email = textBlock.Text;
           

           
            string[] lines = File.ReadAllLines(AccountsFilePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split('_');
                if (parts[0] == email)
                {
                    
                    selectedAccount.Email = parts[0];
                    selectedAccount.Password = parts[1];
                    selectedAccount.Provider = parts[2];
                    MessageBox.Show($"Selected Account: {selectedAccount.Email}");
                    IsAccountSelected = true;
                    return;
                }
            }

            MessageBox.Show("Email not found in accounts.txt");
        }
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Button removeButton = sender as Button;
            TextBlock emailTextBlock = removeButton.CommandParameter as TextBlock;

            if (emailTextBlock != null)
            {
                string emailToRemove = emailTextBlock.Text;
                EmailGrid.Children.Remove(emailTextBlock); // Remove TextBlock
               
                RemoveAccountFromFile(emailToRemove);

                // Move up displayed emails by one TextBlock
                foreach (var child in EmailGrid.Children)
                {
                    if (child is TextBlock textBlock && Grid.GetRow(textBlock) > Grid.GetRow(emailTextBlock))
                    {
                        int newRow = Grid.GetRow(textBlock) - 1;
                        Grid.SetRow(textBlock, newRow);

                        // Update CommandParameter for remove button
                        Button correspondingButton = GetRemoveButtonForTextBlock(newRow);
                        if (correspondingButton != null)
                        {
                            correspondingButton.CommandParameter = textBlock;
                        }
                    }
                }
            }
        }

        
        private Button GetRemoveButtonForTextBlock(int row)
        {
            foreach (var child in EmailGrid.Children)
            {
                if (child is Button removeButton && Grid.GetRow(removeButton) == row)
                {
                    return removeButton;
                }
            }
            return null;
        }


        private void RemoveAccountFromFile(string email)
        {
            try
            {
                List<string> lines = new List<string>(File.ReadAllLines(AccountsFilePath));
                lines.RemoveAll(line => line.StartsWith(email));
                File.WriteAllLines(AccountsFilePath, lines);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing account from 'accounts.txt': {ex.Message}");
            }
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            
                textBox.FontWeight = FontWeights.Normal;
                if (textBox.Text == "Email" || textBox.Text == "Heslo")
                {
                    textBox.Text = "";
                }
            
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
           
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.FontWeight = FontWeights.Bold;
                    if (textBox.Name == "emailTextBox")
                    {
                        textBox.Text = "Email";
                    }
                    else if (textBox.Name == "passwordTextBox")
                    {
                        textBox.Text = "Heslo";
                    
                    }
                }
            
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string email = emailTextBox.Text;
            string password = passwordTextBox.Text;

            string filePath = "accounts.txt";

            

            
            if (email.Contains("@"))
            {
                string[] providerOptions = { "seznam", "gmail" };
                foreach (string provider in providerOptions)
                {
                    if (email.Contains(provider))
                    {
                       
                        NewAccount account = new NewAccount
                        {
                            Email = email,
                            Password = password,
                            Provider = provider
                        };
                        
                        string accountString = $"{account.Email}_{account.Password}_{account.Provider}";
                        

                        emailTextBox.Text = "Email";
                        emailTextBox.FontWeight = FontWeights.Bold;
                        passwordTextBox.Text = "Heslo";
                        passwordTextBox.FontWeight = FontWeights.Bold;

                       
                        AddAccountToFile(email, password, provider);

                        
                        LoadAccounts();

                        return;
                    }
                }
                
            }

            MessageBox.Show("Neplatný e-mail. E-mail musí obsahovat '@' a buď 'seznam' nebo 'gmail'.");
        }

        //private void WriteAccountToFile(string accountString)
        //{
           

        //    try
        //    {
        //        // Přidat účet do souboru
        //        using (StreamWriter writer = new StreamWriter(AccountsFilePath, true))
        //        {
        //            writer.Write(accountString);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Nastala chyba při zápisu do souboru 'accounts.txt': {ex.Message}");
        //    }
        //}

        private void AddAccountToFile(string email, string password, string provider)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(AccountsFilePath, true))
                {
                    writer.WriteLine($"{email}_{password}_{provider}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při zápisu do souboru 'accounts.txt': {ex.Message}");
            }
        }

        private void StoreButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if (IsAccountSelected)
                {
                    selectedAccount.Store = button.Content.ToString();
                    Window1 newWindow = new Window1(selectedAccount);
                    newWindow.ShowDialog();

                    
;                    
                }
                else
                {
                    MessageBox.Show("Nejprve kliknutím vyberte email");
                }
            }
        }
    }





    public class NewAccount
    {
        public string Email { get; set; }    
        public string Password { get; set; }

        public string Provider { get; set; }
    }

    public class SelectedAccount
    {
    public string Email { get; set; }
    public string Password { get; set; }

    public string Provider { get; set; }
        public string Store { get; set; }
        public string Date { get; set; }
    }

}
