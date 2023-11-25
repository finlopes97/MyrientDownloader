using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Ookii.Dialogs.Wpf;

namespace MyrientDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            filesListBox.Items.Clear();
            string url = urlTextBox.Text;
            var client = new HttpClient();

            int downloadLimit = 0;
            if(!int.TryParse(downloadLimitBox.Text, out downloadLimit)) 
            {
                MessageBox.Show("Invalid download limit. Please enter a valid number.");
                return;
            }

            try
            {
                var response = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                var links = doc.DocumentNode.SelectNodes("//td[@class='link']/a[contains(@href, '.zip')]");

                if (links != null)
                {
                    int downloadCount = 0;
                    foreach(var link in links)
                    {
                        if(downloadLimit != -1 && downloadCount >= downloadLimit)
                        {
                            break;
                        }

                        var downloadLink = link.GetAttributeValue("href", string.Empty);
                        if(!Uri.IsWellFormedUriString(downloadLink, UriKind.Absolute))
                        {
                            downloadLink = new Uri(new Uri(url), downloadLink).AbsoluteUri;
                        }

                        var fileBytes = await client.GetByteArrayAsync(downloadLink);
                        var fileName = System.IO.Path.GetFileName(downloadLink);
                        fileName = Uri.UnescapeDataString(fileName);
                        
                        var directoryPath = fileTextBox.Text;
                        if(!System.IO.Directory.Exists(directoryPath))
                        {
                            System.IO.Directory.CreateDirectory(directoryPath);
                        }

                        var filePath = System.IO.Path.Combine(directoryPath, fileName);
                        await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                        filesListBox.Items.Add($"{downloadCount} - {fileName} has been downloaded.");
                        downloadCount++;
                    }
                } else
                {
                    MessageBox.Show("No .zip files found on the page.");
                }
            } catch (Exception ex) 
            {
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if(dialog.ShowDialog().GetValueOrDefault())
            {
                string selectedFolderPath = dialog.SelectedPath;
                fileTextBox.Text = selectedFolderPath;
            }
        }
    }
}