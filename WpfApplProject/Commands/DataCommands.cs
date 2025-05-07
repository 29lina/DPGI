using System.Windows.Input;

namespace WpfApplProject.Commands
{
    public static class DataCommands
    {
        public static RoutedCommand Undo { get; }
        public static RoutedCommand New { get; }
        public static RoutedCommand Replace { get; }
        public static RoutedCommand Save { get; }
        public static RoutedCommand Find { get; }
        public static RoutedCommand Delete { get; }

        static DataCommands()
        {
            Undo = new RoutedCommand("Undo", typeof(DataCommands),
                        new InputGestureCollection { new KeyGesture(Key.Z, ModifierKeys.Control) });
            New = new RoutedCommand("New", typeof(DataCommands),
                        new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Control) });
            Replace = new RoutedCommand("Replace", typeof(DataCommands),
                        new InputGestureCollection { new KeyGesture(Key.R, ModifierKeys.Control) });
            Save = new RoutedCommand("Save", typeof(DataCommands),
                        new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) });
            Find = new RoutedCommand("Find", typeof(DataCommands),
                        new InputGestureCollection { new KeyGesture(Key.F, ModifierKeys.Control) });
            Delete = new RoutedCommand("Delete", typeof(DataCommands),
                        new InputGestureCollection { new KeyGesture(Key.D, ModifierKeys.Control) });
        }
    }
}
