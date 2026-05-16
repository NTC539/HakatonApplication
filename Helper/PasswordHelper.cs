using System.Windows;
using System.Windows.Controls;

namespace HakatonApplication.Helper
{
    public static class PasswordHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordHelper),
                new FrameworkPropertyMetadata(string.Empty, OnPasswordChanged));

        public static void SetPassword(DependencyObject obj, string value) => obj.SetValue(PasswordProperty, value);
        public static string GetPassword(DependencyObject obj) => (string)obj.GetValue(PasswordProperty);

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox pb && pb.Password != (string)e.NewValue)
                pb.Password = (string)e.NewValue;
        }
    }
}