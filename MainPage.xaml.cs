
namespace AntrenmanTakipApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }
        private async void OnBolgeTapped(object? sender, EventArgs e)
        {
            var tiklananKutu = (Border)sender!;
            string secilenKategori = tiklananKutu.ClassId;


            tiklananKutu.BackgroundColor = Color.FromArgb("#424549");

            await tiklananKutu.ScaleTo(0.95, 100);

            await Task.Delay(50);

            tiklananKutu.BackgroundColor = Color.FromArgb("#36393e");

            await tiklananKutu.ScaleTo(1, 100);

            if (secilenKategori == "Biceps")
            {

            }
            else if (secilenKategori == "Triceps")
            {

            }
            else if (secilenKategori == "Gogus")
            {

            }

            await Navigation.PushAsync(new HareketlerPage(secilenKategori));
        }
    }
}
