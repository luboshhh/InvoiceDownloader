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
using System.Windows.Shapes;

namespace InvoiceDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string AccountsFilePath = "accounts.txt";
        public static SelectedAccount selectedAccount { get; set; }
        public bool IsAccountSelected { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadAccounts();
            //DataContext = this;
            InitializeSelectedAccount();
        }




        private void LoadAccounts()
        {
            try
            {
                // Pokud soubor neexistuje, není třeba provádět načítání
                if (!File.Exists(AccountsFilePath))
                    using (StreamWriter writer = new StreamWriter(AccountsFilePath))
                    {
                        // Zde můžete přidat výchozí obsah souboru, pokud je to žádoucí
                    }

                // Načíst e-maily ze souboru
                string[] lines = File.ReadAllLines(AccountsFilePath);

                // Projít každý řádek a zobrazit e-mail v odpovídajícím TextBlocku
                for (int i = 0; i < lines.Length; i++)
                {
                    // Pokud index přesahuje počet TextBlocků, přestat načítat
                    if (i >= EmailGrid.Children.Count)
                        break;

                    // Získat TextBlock pro e-mail
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
                Provider = null
            };
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            string email = textBlock.Text;
           

            // Search for the email in the accounts.txt file
            string[] lines = File.ReadAllLines(AccountsFilePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split('_');
                if (parts[0] == email)
                {
                    // Update SelectedAccount
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

        // Helper method to get the remove button corresponding to the given row
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

            

            // Zkontrolovat, jestli se v mailu nachází "@", "seznam" nebo "gmail", a "."
            if (email.Contains("@"))
            {
                string[] providerOptions = { "seznam", "gmail" };
                foreach (string provider in providerOptions)
                {
                    if (email.Contains(provider))
                    {
                        // Nastavit provider podle nalezeného řetězce
                        NewAccount account = new NewAccount
                        {
                            Email = email,
                            Password = password,
                            Provider = provider
                        };
                        // Provádět další akce s vytvořeným účtem
                        // Zapsat účet do souboru
                        string accountString = $"{account.Email}_{account.Password}_{account.Provider}";
                        //WriteAccountToFile(accountString);

                        emailTextBox.Text = "Email";
                        emailTextBox.FontWeight = FontWeights.Bold;
                        passwordTextBox.Text = "Heslo";
                        passwordTextBox.FontWeight = FontWeights.Bold;

                        //AddAccountToList(email);

                        // Přidat e-mail do souboru
                        AddAccountToFile(email, password, provider);

                        // Načíst e-maily znovu pro zobrazení aktualizovaných údajů
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
                    Task newTask = new Task
                    {
                        Email = selectedAccount.Email,
                        Password = selectedAccount.Password,
                        Provider = selectedAccount.Provider,
                        Store = button.Content.ToString()
                    };

                    Window1 newTaskWindow = new Window1(newTask);
                    newTaskWindow.ShowDialog();

                    // Handle any further actions after the new window is closed
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
     }

    public class Task : SelectedAccount
    {
        public string Store { get; set; }
        public string date { get; set; }

        
    }
}
