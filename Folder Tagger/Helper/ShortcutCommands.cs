using System.Windows.Input;

namespace Folder_Tagger
{
    public static class ShortcutCommands
    {
        public static RoutedCommand AddOneFolder = new RoutedCommand("AddOneFolder", typeof(MainWindow), new InputGestureCollection(new InputGesture[] 
        {
            new KeyGesture(Key.O, ModifierKeys.Control)
        }));

        public static RoutedCommand AddManyFolders = new RoutedCommand("AddOneFolder", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.P, ModifierKeys.Control)
        }));

        public static RoutedCommand OpenFolder = new RoutedCommand("AddOneFolder", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.W, ModifierKeys.Control)
        }));

        public static RoutedCommand OpenInMangareader = new RoutedCommand("AddOneFolder", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.Q, ModifierKeys.Control)
        }));

        public static RoutedCommand AddTag = new RoutedCommand("AddOneFolder", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.E, ModifierKeys.Control)
        }));

        public static RoutedCommand EditArtist = new RoutedCommand("AddOneFolder", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.D, ModifierKeys.Control)
        }));

        public static RoutedCommand EditGroup = new RoutedCommand("AddOneFolder", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.F, ModifierKeys.Control)
        }));

        public static RoutedCommand EditTag = new RoutedCommand("AddOneFolder", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.S, ModifierKeys.Control)
        }));

        public static RoutedCommand RemoveTag = new RoutedCommand("AddOneFolder", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.R, ModifierKeys.Control)
        }));
    }
}
