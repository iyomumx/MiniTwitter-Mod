using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MiniTwitter.Controls
{
    /// <summary>
    /// ShortcutKeyBox.xaml の相互作用ロジック
    /// </summary>
    public partial class ShortcutKeyBox : UserControl
    {
        public ShortcutKeyBox()
        {
            InitializeComponent();
        }

        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(Key), typeof(ShortcutKeyBox), new PropertyMetadata(Key.None, KeyPropertyChanged));

        public ModifierKeys ModifierKeys
        {
            get { return (ModifierKeys)GetValue(ModifierKeysProperty); }
            set { SetValue(ModifierKeysProperty, value); }
        }

        public static readonly DependencyProperty ModifierKeysProperty =
            DependencyProperty.Register("ModifierKeys", typeof(ModifierKeys), typeof(ShortcutKeyBox), new PropertyMetadata(ModifierKeys.None, ModifierKeysPropertyChanged));

        private static void KeyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ShortcutKeyBox)sender).OnKeyPropertyChanged((Key)e.NewValue);
        }

        private static void ModifierKeysPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ShortcutKeyBox)sender).OnModifierKeysPropertyChanged((ModifierKeys)e.NewValue);
        }

        private void OnKeyPropertyChanged(Key key)
        {
            if (IsShortcutKey(key) || (IsAlphaNumKey(key) && ModifierKeys != ModifierKeys.None))
            {
                TextBox.Text = GetModifierString(ModifierKeys) + GetKeyString(key);
            }
            else
            {
                TextBox.Text = string.Empty;
            }
        }

        private void OnModifierKeysPropertyChanged(ModifierKeys modifierKey)
        {
            if (IsShortcutKey(Key) || (IsAlphaNumKey(Key) && modifierKey != ModifierKeys.None))
            {
                TextBox.Text = GetModifierString(modifierKey) + GetKeyString(Key);
            }
            else
            {
                TextBox.Text = string.Empty;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsShortcutKey(e.Key) || (IsAlphaNumKey(e.Key) && Keyboard.Modifiers != ModifierKeys.None))
            {
                Key = e.Key;
                ModifierKeys = Keyboard.Modifiers;
            }
            else
            {
                Key = Key.None;
                ModifierKeys = ModifierKeys.None;
            }
            e.Handled = true;
        }

        private static string GetKeyString(Key key)
        {
            switch (key)
            {
                case Key.Return:
                    return "Enter";
                case Key.Prior:
                    return "PageUp";
                case Key.Escape:
                    return "Esc";
                default:
                    if (key >= Key.D0 && key <= Key.D9)
                    {
                        return (key - Key.D0).ToString();
                    }
                    break;
            }
            return Enum.GetName(typeof(Key), key);
        }

        private static string GetModifierString(ModifierKeys modifierKeys)
        {
            string text = string.Empty;
            if ((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control)
            {
                text += "Ctrl + ";
            }
            if ((modifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                text += "Shift + ";
            }
            if ((modifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                text += "Alt + ";
            }
            return text;
        }

        private static bool IsShortcutKey(Key key)
        {
            if ((key >= Key.F1 && key <= Key.F24) ||
                (key >= Key.PageUp && key <= Key.Down) ||
                key == Key.Tab ||
                key == Key.Enter ||
                key == Key.Escape ||
                key == Key.Space)
            {
                return true;
            }
            return false;
        }

        private static bool IsAlphaNumKey(Key key)
        {
            return (key >= Key.D0 && key <= Key.Z);
        }
    }
}
