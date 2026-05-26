using AntrenmanTakipApp.Data;

namespace AntrenmanTakipApp;

public partial class HareketlerPage : ContentPage
{
    public string GecerliBolge { get; set; }

    public HareketlerPage(string bolgeAdi)
    {
        InitializeComponent();

        GecerliBolge = bolgeAdi;
        Title = bolgeAdi;
    }
    // SAYFA EKRANDA HER GÖRÜNDÜĞÜNDE OTOMATİK ÇALIŞAN FONKSİYON
    protected override async void OnAppearing()
    {
        base.OnAppearing();


        var db = new DatabaseService();


        var tumHareketler = await db.GetHareketlerAsync();


        var buBolgeninHareketleri = tumHareketler.Where(h => h.Bolge == GecerliBolge).ToList();


        HareketlerListesi.ItemsSource = buBolgeninHareketleri;
    }
    private async void OnEkleClicked(object sender, EventArgs e)
    {

        await Navigation.PushAsync(new HareketEklePage(GecerliBolge));
    }


    private async void OnHareketTapped(object? sender, EventArgs e)
    {
        var tiklananKutu = (Border)sender!;


        tiklananKutu.BackgroundColor = Color.FromArgb("#4e535b");
        await tiklananKutu.ScaleTo(0.95, 100);

        await Task.Delay(50);


        tiklananKutu.BackgroundColor = Color.FromArgb("#36393e");
        await tiklananKutu.ScaleTo(1, 100);
    }
}