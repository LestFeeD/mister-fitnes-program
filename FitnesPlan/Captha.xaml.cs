using EasyCaptcha.Wpf;
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
using System.Windows.Shapes;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для Captha.xaml
    /// </summary>
    public partial class Captha : Window
    {
        private string generatedCaptcha; // Хранит сгенерированную капчу
        private int captchaAttempts = 0;
        private const int MaxCaptchaAttempts = 3;
        public event Action CaptchaVerificationFailed;

        public Captha()
        {
            InitializeComponent();
            GenerateCaptcha();
        }
        private void GenerateCaptcha()
        {
            MyCaptcha.CreateCaptcha(Captcha.LetterOption.Alphanumeric, 5);
            generatedCaptcha = MyCaptcha.CaptchaText;

            string[] letter = { "a", "b", "c", "d", "e", "f" };
            Random r = new Random();
            int hex1 = r.Next(0, 9);
            int hex2 = r.Next(0, 9);
            int hex3 = r.Next(0, 9);
            string hex4 = letter[r.Next(0, 5)];
            string hex5 = letter[r.Next(0, 5)];
            string hex6 = letter[r.Next(0, 5)];
            string hex = $"{hex1}{hex4}{hex2}{hex5}{hex3}{hex6}";
            MyCaptcha.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString($"#{hex}"));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string enteredCaptcha = tb.Text;

            if (enteredCaptcha == generatedCaptcha)
            {
                MessageBox.Show("Капча введена верно!");
                this.DialogResult = true;
            }
            else
            {
                captchaAttempts++;
                MessageBox.Show("Неверная капча. Попробуйте снова.");

                if (captchaAttempts >= MaxCaptchaAttempts)
                {
                    CaptchaVerificationFailed?.Invoke(); 
                    this.DialogResult = false;
                }
            }
        }
    }
}
