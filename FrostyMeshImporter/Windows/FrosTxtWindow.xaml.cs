// FrosTxtWindow.xaml.cs - FrostyMeshImporter
// Contributors:
//      Copyright (C) 2021  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Forms;
using Frosty.Controls;
using FrostyEditor;
using FrostySdk.Managers;
using FrostySdk.Ebx;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using FrosTxtCore;
using static FrostyMeshImporter.Program;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace FrostyMeshImporter.Windows
{
    /// <summary>
    /// Interaction logic for FrosTxt.xaml
    /// </summary>
    public partial class FrosTxtWindow : FrostyDockableWindow
    {
        public Localizations language;
        public LocalizationMerger lm;
        internal FrosTxtWindow(FrosTxtObj fObj)
        {
            InitializeComponent();
            this.lm = fObj.lm;
            this.language = fObj.language;
            this.Title += " - " + language.ToString();
            baseComboBox.ItemsSource = Enum.GetNames(typeof(Localizations));
            baseComboBox.SelectedItem = language.ToString();
            List<object> files = lm.GetGenericModifiedFiles();
            SetItems(lm.GetGenericModifiedFiles());
            baseComboBox.SelectionChanged += BaseComboBox_SelectionChanged;
        }

        public void SetItems(List<object> files)
        {

            mergeStatus.Visibility = Visibility.Hidden;
            mergeButton.IsEnabled = true;
            saveButton.IsEnabled = true;
            if (lm.IsMergeValid())
            {
                mergeStatus.Visibility = Visibility.Visible;
                mergeButton.IsEnabled = false;
            }
            LocalizationFile dummyFile = new LocalizationFile(null);
            dummyFile.name = language.ToString() + " (base)";
            files.Insert(0, dummyFile);
            CheckCount(files);
            fileListBox.ItemsSource = files;
            FileSelection_Changed(this);
        }

        public void BaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Close();
            SwitchFrosTxtProfile(baseComboBox.SelectedItem.ToString());
        }

        private void CheckCount(List<object> files)
        {
            moveUpButton.IsEnabled = true;
            moveDownButton.IsEnabled = true;
            removeButton.IsEnabled = true;
            if (files.Count <= 2)
            {
                moveUpButton.IsEnabled = false;
                moveDownButton.IsEnabled = false;
                if (files.Count == 1)
                {
                    removeButton.IsEnabled = false;
                    mergeButton.IsEnabled = false;
                    saveButton.IsEnabled = false;
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
            int index = files.IndexOf(selected);
            if(index == 0)
            {
                moveUpButton.IsEnabled = false;
                moveDownButton.IsEnabled = false;
                removeButton.IsEnabled = false;
            }
            else if (files.IndexOf(selected) == 1)
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
            lm.FileMoveUp((ModifiedLocalizationFile)selected);
            SetItems(lm.GetGenericModifiedFiles());
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            object selected = null;
            List<object> files = GetSelectedAndList(out selected);
            if (selected == null)
            {
                return;
            }
            lm.FileMoveDown((ModifiedLocalizationFile)selected);
            SetItems(lm.GetGenericModifiedFiles());
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            object selected = null;
            List<object> files = GetSelectedAndList(out selected);
            if (selected == null)
            {
                return;
            }
            int index = files.IndexOf(selected);
            lm.RemoveModifiedFile((ModifiedLocalizationFile)selected);
            // Decrement selected index if deleted was at end of the list
            SetItems(lm.GetGenericModifiedFiles());
            files = GetSelectedAndList(out selected);
            if (index > 0 && index == files.Count)
            {
                index -= 1;
            }
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
                FrostyTask.Begin($"Importing localization");
                await Task.Run(() =>
                {
                    foreach (string newFilePath in ofd.FileNames)
                    {
                        lm.AddModifiedFile(newFilePath);
                    }
                });
                FrostyTask.End();
            }
            SetItems(lm.GetGenericModifiedFiles());
        }

        private void MergeButton_Click(object sender, RoutedEventArgs e)
        {
            List<ModifiedLocalizationFile> files = lm.GetModifiedFiles();
            if (files.Count == 0)
            {
                return;
            }
            // Merge localization files and write to chunk
            bool result = MergeCurrentProfile();
            if(!result)
            {
                return;
            }
            SetItems(lm.GetGenericModifiedFiles());
            App.Logger.Log($"Merged {this.language} localization files");
            //string finalStatus = "Merged file saved as " + System.IO.Path.GetFileName(sfd.FileName);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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
            // Merge localization files and save to disk
            lm.MergeFiles(sfd.FileName);
            App.Logger.Log($"Merged and saved {this.language} localization file to {sfd.SafeFileName}");
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
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
