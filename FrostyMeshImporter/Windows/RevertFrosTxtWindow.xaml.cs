// RevertFrosTxtWindow.Xaml.cs - FrostyMeshImporter
// Contributors:
//      Copyright (C) 2021  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using System.Collections.Generic;
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
using Frosty.Controls;
using FrosTxtCore;
using static FrostyMeshImporter.Program;

namespace FrostyMeshImporter.Windows
{
    /// <summary>
    /// Interaction logic for RevertFrosTxtWindow.xaml
    /// </summary>
    public partial class RevertFrosTxtWindow : FrostyDockableWindow
    {
        internal RevertFrosTxtWindow(List<FrosTxtObj> modifiedLocalizations)
        {
            InitializeComponent();
            profileSelect.ItemsSource = modifiedLocalizations;
        }

        // On revert button click
        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            if (profileSelect.SelectedItems.Count == 0)
            {
                FrostyMessageBox.Show($"Select at least one localization to revert.", Program.IMPORTER_MESSAGE, MessageBoxButton.OK);
                return;
            }
            foreach (FrosTxtObj profile in profileSelect.SelectedItems.OfType<FrosTxtObj>())
            {
                string language = profile.ToString();
                FrosTxtObj toRevert = GetFrosTxtProfile(language);
                if (toRevert != null)
                {
                    RevertProfile(toRevert);
                }
            }
            this.DialogResult = new bool?(true);
            this.Close();
        }

        // On cancel button click
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = new bool?(false);
            this.Close();
        }

        private void FrostyDockableWindow_FrostyLoaded(object sender, EventArgs e)
        {

        }
    }
}
