using System;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;

namespace Cipherpad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string DefaultExtension = ".cipher";
        const string FileFilter = "CipherPad file|*" + DefaultExtension;

        string openFileName;
        string savedPassphrase;
        bool unsavedChanges;


        public MainWindow()
        {
            InitializeComponent();

            UpdateTitle();
        }

        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save(false);
        }

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Save(true);
        }

        private void PlainTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(unsavedChanges == false)
            {
                unsavedChanges = true;
                UpdateTitle();
            } 
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UpdateTitle()
        {
            Title = "CipherPad" + (openFileName != null ? (" - " + openFileName + (unsavedChanges ? "*" : "")) : "");
        }

        private void Save(bool withDialog)
        {
            try
            {
                string newFileName = withDialog ? null : openFileName;

                if (newFileName == null) // Only ask for a file name if withDialog is true, or the current file is new
                {
                    var dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.DefaultExt = DefaultExtension;
                    dlg.Filter = FileFilter;

                    if (dlg.ShowDialog() == true)
                    {
                        newFileName = dlg.FileName;
                    }
                }

                if (newFileName != null) // Don't go on if the user had to choose a file name and didn't
                {
                    var passphrase = withDialog ? null : savedPassphrase;

                    if (passphrase == null)
                    {
                        var dlg = new ChoosePasswordModal();

                        dlg.Owner = this;

                        if (dlg.ShowDialog() == true)
                        {
                            passphrase = dlg.Passphrase;
                        }
                    }

                    if (passphrase != null)
                    {
                        using (var file = File.Create(newFileName))
                        {
                            Exception unknownException = null;

                            string plainText = PlainTextBox.Text;

                            new LoadingModal(() =>
                            {
                                try
                                {
                                    using (var crypto = Crypto.OpenEncryptionStream(passphrase, file))
                                    {
                                        using (var writer = new StreamWriter(crypto))
                                        {
                                            writer.Write(plainText);

                                            writer.Flush();
                                        }
                                    }
                                }
                                catch(Exception ex)
                                {
                                    unknownException = ex;
                                }
                            }, "Encrypting...", this).ShowDialog();
                            
                            if(unknownException != null)
                            {
                                throw unknownException;
                            }
                        }

                        openFileName = newFileName;
                        savedPassphrase = passphrase;
                        unsavedChanges = false;

                        UpdateTitle();

                        MessageBox.Show("The file was saved.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);

                MessageBox.Show("The file could not be saved.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool PromptUnsavedChanges()
        {
            if (unsavedChanges)
            {
                var result = MessageBox.Show("If you continue, you will lose all unsaved changes.", "Unsaved changes", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.OK)
                {
                    unsavedChanges = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !PromptUnsavedChanges();
        }

        private void Open()
        {
            if (PromptUnsavedChanges())
            {
                var fileDlg = new Microsoft.Win32.OpenFileDialog();

                fileDlg.DefaultExt = DefaultExtension;
                fileDlg.Filter = FileFilter;

                if (fileDlg.ShowDialog() == true)
                {
                    bool invalidPassphrase = false;

                    try
                    {
                        using (var file = File.OpenRead(fileDlg.FileName))
                        {
                            CryptoStream crypto = null;

                            string passphrase = null;

                            while (true) // Keep showing a dialog until the password is correct
                            {
                                var passDlg = new PasswordModal(invalidPassphrase);

                                passDlg.Owner = this;

                                if (passDlg.ShowDialog() == true)
                                {
                                    Exception unknownException = null;

                                    new LoadingModal(() =>
                                    {
                                        try
                                        {
                                            crypto = Crypto.OpenDecryptionStream(passDlg.Password, file);
                                            passphrase = passDlg.Password;
                                        }
                                        catch (Exception ex)
                                        {
                                            if (ex is IncorrectPasswordException)
                                            {
                                                invalidPassphrase = true;
                                                file.Seek(0, SeekOrigin.Begin);
                                            }
                                            else
                                            {
                                                unknownException = ex;
                                            }
                                        }
                                    }, "Verifying...", this).ShowDialog();

                                    if (crypto != null)
                                    {
                                        break;
                                    }

                                    if(unknownException != null)
                                    {
                                        throw unknownException;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (crypto != null)
                            {
                                // Finally, we can decrypt the text
                                using (crypto)
                                {
                                    using (var reader = new StreamReader(crypto))
                                    {
                                        Exception unknownException = null;
                                        string plainText = null;

                                        new LoadingModal(() =>
                                        {
                                            try
                                            {
                                                plainText = reader.ReadToEnd();
                                            }
                                            catch(Exception ex)
                                            {
                                                unknownException = ex;
                                            }
                                        }, "Decrypting...", this).ShowDialog();

                                        if (unknownException != null)
                                        {
                                            throw unknownException;
                                        }
                                        else
                                        {
                                            PlainTextBox.Text = plainText;
                                        }
                                    }

                                    savedPassphrase = passphrase;
                                    unsavedChanges = false;
                                    openFileName = fileDlg.FileName;

                                    UpdateTitle();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);

                        MessageBox.Show("The file could not be opened.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
