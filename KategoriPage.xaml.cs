namespace AntrenmanTakipApp;

public partial class KategoriPage : ContentPage
{
    public KategoriPage()
    {
        InitializeComponent();

        // Uygulama okurken kutuları baştan gizliyoruz (Animasyon hazırlığı)
        foreach (var eleman in AnaListe.Children)
        {
            if (eleman is VisualElement view)
            {
                view.Opacity = 0;
                view.TranslationY = -50;
            }
        }
    }

    // YENİ: Artık otomatik değil, dışarıdan çağrılabilen özel bir animasyon motoru
    public async void AnimasyonlariBaslat()
    {
        // Ekrana çizilmesi için ufak bir esneme payı veriyoruz
        await Task.Delay(50);

        foreach (var eleman in AnaListe.Children)
        {
            if (eleman is VisualElement view)
            {
                _ = view.FadeToAsync(1, 400, Easing.CubicOut);
                _ = view.TranslateToAsync(0, 0, 400, Easing.SpringOut);

                await Task.Delay(50);
            }
        }
    }

    private async void OnBolgeTapped(object sender, EventArgs e)
    {
        var tiklananKutu = (Border)sender;
        var secilenBolge = tiklananKutu.ClassId;

        // Tıklama animasyonu
        tiklananKutu.BackgroundColor = Color.FromArgb("#4e535b");
        await tiklananKutu.ScaleToAsync(0.95, 100);
        await Task.Delay(50);
        tiklananKutu.BackgroundColor = Color.FromArgb("#36393e");
        await tiklananKutu.ScaleToAsync(1, 100);

        // ✅ DÜZELTİLDİ: Global navigasyon kullanılarak HareketlerPage açılıyor
        if (Application.Current?.MainPage?.Navigation != null)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new HareketlerPage(secilenBolge));
        }
    }
}