using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class SmallEditWindow : Window
    {
        private readonly string type = "";
        private readonly List<string> locationList;
        private readonly List<string> agList = new List<string>();
        FolderController fc = new FolderController();
        public SmallEditWindow(string type, List<string> locationList)
        {
            InitializeComponent();
            this.type = type;
            this.locationList = locationList;
            agList = type == "Artist" ? fc.GetArtistList("all") : fc.GetGroupList("all");
            Title = "Edit " + type;

            if (locationList.Count > 1)
                tbInput.Text = "";
            else
            {
                switch (type)
                {
                    case "Artist":
                        tbInput.Text = fc.GetArtistList(locationList[0]).ElementAtOrDefault(0);
                        break;
                    case "Group":
                        tbInput.Text = fc.GetGroupList(locationList[0]).ElementAtOrDefault(0);
                        break;
                }
            }
            popupAutoComplete.IsOpen = false;

            Loaded += (sender, e) => { tbInput.Focus(); tbInput.SelectAll(); };
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        protected override void OnLocationChanged(EventArgs e) //Make popup stick with textbox
        {
            popupAutoComplete.HorizontalOffset += 1;
            popupAutoComplete.HorizontalOffset -= 1;
            base.OnLocationChanged(e);
        }

        private void TextBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbInput.Text))
            {
                string currentInput = tbInput.Text.ToLower().Trim();
                if (currentInput.Length < 3)
                {
                    popupAutoComplete.IsOpen = false;
                    return;
                }

                var suggestedInput = agList.Where(ag => ag.Contains(currentInput)).ToList();
                int suggestedInputCount = suggestedInput.Count;
                if (suggestedInputCount == 0)
                {
                    popupAutoComplete.IsOpen = false;
                    return;
                }

                popupAutoComplete.Height = suggestedInputCount > 10 ? 250 : suggestedInputCount * 25 + 10;
                listboxAutoComplete.ItemsSource = suggestedInput;
                popupAutoComplete.IsOpen = true;
            }
        }

        private void TextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && popupAutoComplete.IsOpen)
            {
                listboxAutoComplete.SelectedIndex = 0;
                var item = (ListBoxItem)listboxAutoComplete.ItemContainerGenerator.ContainerFromItem(listboxAutoComplete.SelectedItem);
                item.Focus();
                e.Handled = true;
            }
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && listboxAutoComplete.SelectedIndex == 0)
                tbInput.Focus();

            if (e.Key == Key.S) //Disable 'S' key default event
                e.Handled = true;

            if (e.Key == Key.Enter)
            {
                tbInput.Text = listboxAutoComplete.SelectedItem as string;
                tbInput.Focus();
                tbInput.CaretIndex = tbInput.Text.Length;
                popupAutoComplete.IsOpen = false;
                e.Handled = true;
            }
        }

        private void ButtonUpdate_Clicked(object sender, RoutedEventArgs e)
        {
            string input = tbInput.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(input)) input = null;
            
            foreach (string location in locationList)
                switch (type)
                {
                    case "Artist":
                        fc.UpdateArtist(location, input);
                        break;
                    case "Group":
                        fc.UpdateGroup(location, input);
                        break;
                }

            Close();
        }
    }
}
