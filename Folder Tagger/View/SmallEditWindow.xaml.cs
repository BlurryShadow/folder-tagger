﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class SmallEditWindow : Window
    {
        private readonly string type;
        private readonly List<string> locations;
        private readonly List<string> agList;
        private readonly int numberCharactersToActiveSuggestion = 3;

        private readonly FolderController fc = new FolderController();
        public SmallEditWindow(string type, List<string> locations)
        {
            InitializeComponent();
            this.type = type;
            this.locations = locations;
            agList = (type == "Artist") ? fc.GetArtists() : fc.GetGroups();
            Title = "Edit " + type;

            if (locations.Count == 1)
                tbInput.Text = (type == "Artist") ?
                    fc.GetArtists(locations[0]).ElementAtOrDefault(0) :
                    fc.GetGroups(locations[0]).ElementAtOrDefault(0);

            //If not popup will open everytime a new edit artist / group starts
            popupAutoComplete.IsOpen = false;
            Loaded += (sender, e) => { tbInput.Focus(); tbInput.SelectAll(); };
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
            listboxAutoComplete.PreviewMouseUp += (sender, e) => ApplySuggestion();
        }

        protected override void OnLocationChanged(EventArgs e) //Make popup stick with textbox
        {
            popupAutoComplete.HorizontalOffset += 1;
            popupAutoComplete.HorizontalOffset -= 1;
            base.OnLocationChanged(e);
        }

        private void ApplySuggestion()
        {
            tbInput.Text = listboxAutoComplete.SelectedItem as string;
            tbInput.Focus();
            tbInput.CaretIndex = tbInput.Text.Length;
            popupAutoComplete.IsOpen = false;
        }

        private void TextBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbInput.Text))
            {
                string currentInput = tbInput.Text.ToLower().Trim();
                if (currentInput.Length < numberCharactersToActiveSuggestion)
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

        private void TextBoxInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Jump to suggestion box when press Down or Up Key
            if (popupAutoComplete.IsOpen)
            {
                if (e.Key == Key.Down)
                    listboxAutoComplete.SelectedIndex = 0;
                if (e.Key == Key.Up)
                    listboxAutoComplete.SelectedIndex = listboxAutoComplete.Items.Count - 1;
                if (e.Key == Key.Down || e.Key == Key.Up)
                {
                    var item = (ListBoxItem)listboxAutoComplete.ItemContainerGenerator.ContainerFromItem(listboxAutoComplete.SelectedItem);
                    item.Focus();
                    //Prevent focus from jumping to next element
                    e.Handled = true;
                }
            }
        }

        private void ListBoxAutoComplete_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Jump back to TextBox when press Up Key at first element
            if (e.Key == Key.Up && listboxAutoComplete.SelectedIndex == 0)
                tbInput.Focus();

            //Return to first element when pressed at last one
            //Trying to mimic S Key default behavior
            if (e.Key == Key.Down && listboxAutoComplete.SelectedIndex == listboxAutoComplete.Items.Count - 1)
            {
                var item = (ListBoxItem)listboxAutoComplete.ItemContainerGenerator.ContainerFromItem(listboxAutoComplete.Items[0]);
                item.Focus();
                e.Handled = true;
            }

            //Disable 'S' key default event
            //Which is the same as Down key
            //But return to first element when pressed at last one
            if (e.Key == Key.S)
                e.Handled = true;

            if (e.Key == Key.Enter)
            {
                ApplySuggestion();
                //Update Button is default
                //Cancel Enter Key Down event to prevent premature trigger
                e.Handled = true;
            }
        }

        private void ButtonUpdate_Clicked(object sender, RoutedEventArgs e)
        {
            string input = tbInput.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(input))
                input = null;
            
            foreach (string location in locations)
                if (type == "Artist")
                    fc.UpdateArtist(location, input);
                else
                    fc.UpdateGroup(location, input);
            Close();
        }
    }
}
