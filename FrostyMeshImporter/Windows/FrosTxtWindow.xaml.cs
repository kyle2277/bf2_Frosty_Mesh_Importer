using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using Frosty.Controls;
using FrostySdk.Managers;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using FrosTxtCore;

namespace FrostyMeshImporter.Windows
{
    /// <summary>
    /// Interaction logic for FrosTxt.xaml
    /// </summary>
    public partial class FrosTxtWindow : FrostyDockableWindow
    {
        // Enumaration of all localization types
        public enum Localizations
        {
            English,
            BrazilianPortuguese,
            French,
            German,
            Italian,
            Japanese,
            Polish,
            Russian,
            Spanish,
            SpanishMex,
            TraditionalChinese,
            WorstCase
        }
        public LocalizationMerger lm;
        public Localizations language;
        public FrosTxtWindow(Localizations language, string baseFile)
        {
            this.language = language;
            lm = new LocalizationMerger(baseFile);
            SetItems(lm.GetGenericModifiedFiles());
            InitializeComponent();
        }

        internal void SetItems(List<object> files)
        {
            CheckCount(files);
            fileListBox.ItemsSource = files;
            FileSelection_Changed(this);
        }

        private void CheckCount(List<object> files)
        {
            moveUpButton.IsEnabled = true;
            moveDownButton.IsEnabled = true;
            removeButton.IsEnabled = true;
            mergeButton.IsEnabled = true;
            if (files.Count <= 1)
            {
                moveUpButton.IsEnabled = false;
                moveDownButton.IsEnabled = false;
                if (files.Count == 0)
                {
                    removeButton.IsEnabled = false;
                    mergeButton.IsEnabled = false;
                }
            }
        }

        private void FileSelection_Changed(object sender, RoutedEventArgs e = null)
        {
            object selected = null;
            List<object> files = GetSelectedAndList(out selected);
            if (selected == null)
            {
                moveUpButton.IsEnabled = false;
                moveDownButton.IsEnabled = false;
                removeButton.IsEnabled = false;
                return;
            }
            CheckCount(files);
            if (files.IndexOf(selected) == 0)
            {
                moveUpButton.IsEnabled = false;
            }
            else if (files.IndexOf(selected) == files.Count - 1)
            {
                moveDownButton.IsEnabled = false;
            }
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            object selected = null;
            List<object> files = GetSelectedAndList(out selected);
            if (selected == null)
            {
                return;
            }
            int index = files.IndexOf(selected);
            int newIndex = index - 1;
            if (newIndex >= 0)
            {
                files[index] = files[newIndex];
                files[newIndex] = selected;
            }
            SetItems(files);
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            object selected = null;
            List<object> files = GetSelectedAndList(out selected);
            if (selected == null)
            {
                return;
            }
            int index = files.IndexOf(selected);
            int newIndex = index + 1;
            if (newIndex < files.Count)
            {
                files[index] = files[newIndex];
                files[newIndex] = selected;
            }
            SetItems(files);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            object selected;
            List<object> files = GetSelectedAndList(out selected);
            if (selected == null)
            {
                return;
            }
            int index = files.IndexOf(selected);
            files.Remove(selected);
            lm.RemoveModifiedFile((ModifiedLocalizationFile)selected);
            // Decrement selected index if deleted was at end of the list
            if (index > 0 && index == files.Count)
            {
                index -= 1;
            }
            SetItems(files);
            if (files.Count > 0)
            {
                fileListBox.SelectedItem = fileListBox.Items[index];
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            // Configure dialog
            ofd.Title = "Select localization file to import";
            ofd.Filter = "Binary chunk (.chunk)|*.chunk";
            ofd.RestoreDirectory = true;
            ofd.DereferenceLinks = true;
            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.Multiselect = true;
            if (ofd.ShowDialog(this) == true && ofd.FileNames.Length > 0)
            {
                foreach (string newFilePath in ofd.FileNames)
                {
                    await Task.Run(() =>
                    {
                        lm.AddModifiedFile(newFilePath);
                    });
                }
                string endStatus = "Imported " + ofd.FileNames.Length + " files";
            }
            SetItems(lm.GetGenericModifiedFiles());
        }

        private void MergeButton_Click(object sender, RoutedEventArgs e)
        {
            // save file dialog
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Choose save file location";
            sfd.FileName = "FrosTxt_output.chunk";
            sfd.Filter = "Binary chunk (.chunk)|*.chunk";
            sfd.RestoreDirectory = true;
            sfd.DereferenceLinks = true;
            sfd.AddExtension = true;
            if (sfd.ShowDialog(this) != true || sfd.FileName.Length <= 0)
            {
                return;
            }
            List<ModifiedLocalizationFile> files = lm.GetModifiedFiles();
            if (files.Count == 0)
            {
                return;
            }
            // Merge localization files and write to disk
            lm.MergeFiles(sfd.FileName);
            string file = sfd.FileName;
            string finalStatus = "Merged file saved as " + System.IO.Path.GetFileName(sfd.FileName);
        }

        private List<object> GetSelectedAndList(out object selected)
        {
            selected = fileListBox.SelectedItem;
            IEnumerable<object> itemsSource = (IEnumerable<object>)fileListBox.ItemsSource;
            return itemsSource.ToList();
        }

        private void OutputButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            // Configure dialog
            ofd.Title = "Choose save file location";
            ofd.Filter = "Binary chunk (.chunk)|*.chunk";
            ofd.DereferenceLinks = true;
            ofd.AddExtension = true;
            ofd.CheckFileExists = false;
            ofd.Multiselect = false;
            ofd.FileName = System.IO.Path.GetFileName(lm.outPath);
            ofd.ShowDialog(this);
            lm.outPath = ofd.FileName;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = new bool?(false);
            this.Close();
        }

        private void SelectBaseButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void FrostyDockableWindow_FrostyLoaded(object sender, EventArgs e)
        {

        }
    }
}
