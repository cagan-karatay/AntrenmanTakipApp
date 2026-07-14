using AntrenmanTakipApp.Data;
using AntrenmanTakipApp.Models;

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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        HareketlerListesi.Opacity = 0;

        var db = new DatabaseService();
        var tumHareketler = await db.GetHareketlerAsync();
        var buBolgeninHareketleri = tumHareketler.Where(h => h.Bolge == GecerliBolge).ToList();

        BindableLayout.SetItemsSource(HareketlerListesi, buBolgeninHareketleri);

        await Task.Delay(50);

        foreach (var eleman in HareketlerListesi.Children)
        {
            if (eleman is VisualElement view)
            {
                view.Opacity = 0;
                view.TranslationY = -40;
            }
        }

        HareketlerListesi.Opacity = 1;

        foreach (var eleman in HareketlerListesi.Children)
        {
            if (eleman is VisualElement view)
            {
                _ = view.FadeToAsync(1, 350, Easing.CubicOut);
                _ = view.TranslateToAsync(0, 0, 350, Easing.SpringOut);

                await Task.Delay(40);
            }
        }
    }

    private async void OnEkleClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HareketEklePage(GecerliBolge));
    }

    private async void OnHareketTapped(object? sender, EventArgs e)
    {
        var tiklananKutu = (Border)sender!;

        // YENİ: Tıklanan kutunun içindeki hareket verisini yakalıyoruz
        var secilenHareket = (Hareket)tiklananKutu.BindingContext;

        tiklananKutu.BackgroundColor = Color.FromArgb("#4e535b");
        await tiklananKutu.ScaleToAsync(0.95, 100);
        await Task.Delay(50);
        tiklananKutu.BackgroundColor = Color.FromArgb("#36393e");
        await tiklananKutu.ScaleToAsync(1, 100);

        // YENİ: Yakaladığımız hareketi detay sayfasına gönderiyoruz
        if (Application.Current?.MainPage?.Navigation != null)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AntrenmanGecmisiPage(secilenHareket));
        }
    }

    private async void OnSilInvoked(object sender, EventArgs e)
    {
        var swipeItem = (SwipeItemView)sender;
        var silinecekHareket = (Hareket)swipeItem.CommandParameter;

        bool eminMisiniz = await DisplayAlertAsync("Uyarı", $"{silinecekHareket.Ad} hareketini silmek istediğinize emin misiniz?", "Evet, Sil", "İptal");

        if (eminMisiniz)
        {
            var db = new DatabaseService();
            await db.AkilliSilHareketAsync(silinecekHareket);
            OnAppearing();
        }
    }
}