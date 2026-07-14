namespace AntrenmanTakipApp;

public partial class AnaSayfaWrapper : ContentPage
{
    // 🌟 Uygulamanın her yerinden bu sayfaya ulaşmamızı sağlayacak telsiz
    public static AnaSayfaWrapper Current { get; private set; }

    public AnaSayfaWrapper()
    {
        InitializeComponent();

        // Bu sayfa açıldığı an kendini telsize kaydeder
        Current = this;

        // Uygulama ilk açıldığında boş ana sayfayı gösteriyoruz.
        GosterBosAnaSayfa();
    }

    // Kullanıcı alt sayfalardan ana sayfaya her geri döndüğünde burası çalışır.
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (AnaIcerik.Content is AnaSayfaView anaSayfaView)
        {
            anaSayfaView.VerileriYukle();
        }
    }

    // Telsizden emir geldiğinde ortadaki menüyü anında değiştirecek TEK FONKSİYON
    public void AnaEkranaDon()
    {
        this.Title = "Ana Sayfa";
        var anaSayfaView = new AnaSayfaView();
        AnaIcerik.Content = anaSayfaView;
    }

    private async void OnSolMenuClicked(object sender, EventArgs e)
    {
        this.Title = "Hareket Ekle";

        var button = (Border)sender;

        button.BackgroundColor = Color.FromArgb("#22ff00");
        button.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#22ff00")),
            Offset = new Point(0, 0),
            Radius = 40,
            Opacity = 0.6f
        };

        await button.ScaleToAsync(0.9, 100);
        await Task.Delay(50);

        button.BackgroundColor = Colors.Transparent;
        button.Shadow = null;

        await button.ScaleToAsync(1, 100);

        var kategoriSayfasi = new KategoriPage();
        AnaIcerik.Content = kategoriSayfasi.Content;
        kategoriSayfasi.AnimasyonlariBaslat();
    }

    private async void OnOrtaMenuClicked(object sender, EventArgs e)
    {
        var button = (Border)sender;
        button.BackgroundColor = Color.FromArgb("#22ff00");

        if (button.Shadow != null)
            button.Shadow.Opacity = 1.0f;

        await button.ScaleToAsync(0.9, 100);
        await Task.Delay(50);

        if (button.Shadow != null)
            button.Shadow.Opacity = 0.6f;

        await button.ScaleToAsync(1, 100);

        AnaEkranaDon();
    }

    private async void OnSagMenuClicked(object sender, EventArgs e)
    {
        this.Title = "Grafikler";

        var button = (Border)sender;

        // 1. Parlama Efekti (Yeşil yanar ve gölge eklenir)
        button.BackgroundColor = Color.FromArgb("#22ff00");
        button.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#22ff00")),
            Offset = new Point(0, 0),
            Radius = 40,
            Opacity = 0.6f
        };

        // 2. Basılma Hissi (Küçülme)
        await button.ScaleToAsync(0.9, 100);
        await Task.Delay(50);

        // 3. Eski Haline (Şeffaf) Dönme (Eksik olan kısım burasıydı!)
        button.BackgroundColor = Colors.Transparent;
        button.Shadow = null;

        // 4. Büyüme
        await button.ScaleToAsync(1, 100);

        // Yeni yaptığımız GrafikView'i yüklüyoruz
        AnaIcerik.Content = new GrafikView();
    }

    private void GosterBosAnaSayfa()
    {
        this.Title = "Ana Sayfa";

        // Dinamik kart sistemini orta ekrana yüklüyoruz.
        var anaSayfaView = new AnaSayfaView();
        AnaIcerik.Content = anaSayfaView;
    }
}