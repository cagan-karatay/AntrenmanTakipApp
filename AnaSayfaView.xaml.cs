using AntrenmanTakipApp.Data;
using AntrenmanTakipApp.Models;

namespace AntrenmanTakipApp;

public partial class AnaSayfaView : ContentView
{
    private DatabaseService _db = new DatabaseService();
    private Hareket? _aktifHareket; // YENİ: Nullable yapıldı.

    public AnaSayfaView()
    {
        InitializeComponent();
        VerileriYukle();
    }

    public async void VerileriYukle()
    {
        var ozetler = await _db.GetBugunkuHareketOzetiAsync();

        if (ozetler == null || ozetler.Count == 0)
        {
            AktifBolumu.IsVisible = false;
            GecmisBolumu.IsVisible = false;
            BosDurumLabel.IsVisible = true;
            return;
        }

        BosDurumLabel.IsVisible = false;

        // 1. En son yapılan hareketi "AKTİF" karta koy
        var aktifOzet = ozetler.First();
        _aktifHareket = aktifOzet.Hareket;
        AktifAdLabel.Text = aktifOzet.Ad;
        AktifAgirliklarLabel.Text = aktifOzet.AgirliklarString;

        // Aktif kartı yukarıdan kayarak (fade) getir
        AktifBolumu.Opacity = 0;
        AktifBolumu.TranslationY = -20;
        AktifBolumu.IsVisible = true;
        _ = AktifBolumu.FadeToAsync(1, 400, Easing.CubicOut);
        _ = AktifBolumu.TranslateToAsync(0, 0, 400, Easing.SpringOut);

        // 2. Geriye kalanları "Bugünkü Hareketler" listesine at
        var gecmisOzetler = ozetler.Skip(1).ToList();
        if (gecmisOzetler.Count > 0)
        {
            BindableLayout.SetItemsSource(BugunkuHareketlerListesi, gecmisOzetler);
            GecmisBolumu.IsVisible = true;
        }
        else
        {
            GecmisBolumu.IsVisible = false;
        }
    }

    // --- AKTİF KART BUTONLARI ---
    private async void OnAktifDevamEtClicked(object sender, EventArgs e)
    {
        await ButonAnimasyonu((Button)sender);
        if (Application.Current?.Windows.Count > 0 && _aktifHareket != null)
        {
            await Application.Current.Windows[0].Page!.Navigation.PushAsync(new AntrenmanGecmisiPage(_aktifHareket));
        }
    }

    private async void OnAktifDuzenleClicked(object sender, EventArgs e)
    {
        await ButonAnimasyonu((Button)sender);
        if (Application.Current?.Windows.Count > 0 && _aktifHareket != null)
        {
            await Application.Current.Windows[0].Page!.Navigation.PushAsync(new AntrenmanGecmisiPage(_aktifHareket));
        }
    }

    // --- LİSTE KART BUTONLARI ---
    private async void OnListeDevamEtClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        await ButonAnimasyonu(btn);
        var hareket = btn.CommandParameter as Hareket;

        if (Application.Current?.Windows.Count > 0 && hareket != null)
        {
            await Application.Current.Windows[0].Page!.Navigation.PushAsync(new AntrenmanGecmisiPage(hareket));
        }
    }

    private async void OnListeDuzenleClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        await ButonAnimasyonu(btn);
        var hareket = btn.CommandParameter as Hareket;

        if (Application.Current?.Windows.Count > 0 && hareket != null)
        {
            await Application.Current.Windows[0].Page!.Navigation.PushAsync(new AntrenmanGecmisiPage(hareket));
        }
    }

    // Ortak Buton Tıklama Animasyonu
    private async Task ButonAnimasyonu(Button btn)
    {
        await Task.WhenAll(btn.FadeToAsync(0.5, 100), btn.ScaleToAsync(0.9, 100));
        await Task.WhenAll(btn.FadeToAsync(1, 100), btn.ScaleToAsync(1, 100));
    }
}