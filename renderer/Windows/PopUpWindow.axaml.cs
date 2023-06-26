using Avalonia.Controls;
using Avalonia.Interactivity;
using Discout.Utils;
using System;

namespace SquareSmash.renderer.Windows
{
    public partial class PopUpWindow : Window
    {
        private readonly Action<int, string> Callback;
        private readonly int Score;

        public PopUpWindow()
        {
            Score = 0;
            Callback = (int n, string s) => { };
            InitializeComponent();
            TextInput.Text = "";
        }

        public PopUpWindow(int score, Action<int, string> callback)
        {
            Callback = callback;
            Score = score;
            InitializeComponent();
            TextInput.Text = "";
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            SoundUtils.PlaySound(/*SoundUtils.CLICK_SOUND*/ null);
            if (TextInput.Text!.Length is >= 1 and <= 10)
            {
                Callback.Invoke(Score, TextInput.Text);
                Close();
                return;
            }
            TextInput.Text = "must be between 1 & 10 chars";
        }
    }
}
