using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
using AntrenmanTakipApp.Data;
using AntrenmanTakipApp.Models;

namespace AntrenmanTakipApp;

public partial class GrafikView : ContentView
{
    private string _aktifSekme = "Gunluk";
    private DateTime _aktifTarih = DateTime.Today;
    private GrafikTimelineItem _seciliZaman;

    private DatabaseService _db = new DatabaseService();

    public class GrafikTimelineItem : BindableObject
    {
        public string Text { get; set; }
        public DateTime HedefTarih { get; set; }
        public bool IsNavigationBtn { get; set; }
        public string NavDirection { get; set; }

        public double Width { get; set; } = 60;
        public int FontSize { get; set; } = 14;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(BorderColor));
                OnPropertyChanged(nameof(BgColor));
                OnPropertyChanged(nameof(TextColor));
            }
        }

        public Color BorderColor => (IsSelected && !IsNavigationBtn) ? Color.FromArgb("#22ff00") : Colors.Transparent;
        public Color BgColor => IsNavigationBtn ? Color.FromArgb("#1e1e1e") : (IsSelected ? Color.FromArgb("#4e535b") : Color.FromArgb("#282b30"));
        public Color TextColor => (IsSelected || IsNavigationBtn) ? Colors.White : Color.FromArgb("#94a3b8");
    }

    public GrafikView()
    {
        InitializeComponent();
        TimelineGuncelle();
        DinamikGrafikCizAsync();
    }

    private void OnSekmeTapped(object sender, TappedEventArgs e)
    {
        var secilen = e.Parameter.ToString();
        if (_aktifSekme == secilen) return;

        _aktifSekme = secilen;
        _aktifTarih = DateTime.Today;

        BtnGunlukBorder.BackgroundColor = Colors.Transparent;
        BtnAylikBorder.BackgroundColor = Colors.Transparent;
        BtnYillikBorder.BackgroundColor = Colors.Transparent;
        LblGunluk.TextColor = Color.FromArgb("#94a3b8");
        LblAylik.TextColor = Color.FromArgb("#94a3b8");
        LblYillik.TextColor = Color.FromArgb("#94a3b8");

        if (secilen == "Gunluk") { BtnGunlukBorder.BackgroundColor = Color.FromArgb("#4e535b"); LblGunluk.TextColor = Colors.White; }
        else if (secilen == "Aylik") { BtnAylikBorder.BackgroundColor = Color.FromArgb("#4e535b"); LblAylik.TextColor = Colors.White; }
        else if (secilen == "Yillik") { BtnYillikBorder.BackgroundColor = Color.FromArgb("#4e535b"); LblYillik.TextColor = Colors.White; }

        TimelineGuncelle();
        DinamikGrafikCizAsync();
    }

    private void TimelineGuncelle()
    {
        var timeline = new ObservableCollection<GrafikTimelineItem>();
        var today = DateTime.Today;

        if (_aktifSekme == "Gunluk")
        {
            ZamanBaslikLabel.Text = _aktifTarih.ToString("yyyy - MMMM", new System.Globalization.CultureInfo("tr-TR"));
            timeline.Add(new GrafikTimelineItem { Text = "Önceki\nAy", IsNavigationBtn = true, NavDirection = "Back", Width = 80, FontSize = 12 });

            int gunSayisi = DateTime.DaysInMonth(_aktifTarih.Year, _aktifTarih.Month);
            if (_aktifTarih.Year == today.Year && _aktifTarih.Month == today.Month)
                gunSayisi = today.Day;

            for (int i = 1; i <= gunSayisi; i++)
            {
                bool isToday = (_aktifTarih.Year == today.Year && _aktifTarih.Month == today.Month && i == today.Day);
                timeline.Add(new GrafikTimelineItem
                {
                    Text = isToday ? "Bugün" : i.ToString(),
                    HedefTarih = new DateTime(_aktifTarih.Year, _aktifTarih.Month, i),
                    Width = 60,
                    IsSelected = (i == gunSayisi)
                });
            }

            if (_aktifTarih.Year < today.Year || (_aktifTarih.Year == today.Year && _aktifTarih.Month < today.Month))
                timeline.Add(new GrafikTimelineItem { Text = "Sonraki\nAy", IsNavigationBtn = true, NavDirection = "Forward", Width = 80, FontSize = 12 });
        }
        else if (_aktifSekme == "Aylik")
        {
            ZamanBaslikLabel.Text = _aktifTarih.ToString("yyyy");
            timeline.Add(new GrafikTimelineItem { Text = "Önceki\nYıl", IsNavigationBtn = true, NavDirection = "Back", Width = 80, FontSize = 12 });

            int aySayisi = 12;
            if (_aktifTarih.Year == today.Year)
                aySayisi = today.Month;

            string[] aylar = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

            for (int i = 1; i <= aySayisi; i++)
            {
                bool isThisMonth = (_aktifTarih.Year == today.Year && i == today.Month);
                timeline.Add(new GrafikTimelineItem
                {
                    Text = isThisMonth ? "Bu Ay" : aylar[i - 1],
                    HedefTarih = new DateTime(_aktifTarih.Year, i, 1),
                    Width = 100,
                    IsSelected = (i == aySayisi)
                });
            }

            if (_aktifTarih.Year < today.Year)
                timeline.Add(new GrafikTimelineItem { Text = "Sonraki\nYıl", IsNavigationBtn = true, NavDirection = "Forward", Width = 80, FontSize = 12 });
        }
        else if (_aktifSekme == "Yillik")
        {
            ZamanBaslikLabel.Text = "Tüm Yıllar";
            for (int i = today.Year - 4; i <= today.Year; i++)
            {
                timeline.Add(new GrafikTimelineItem
                {
                    Text = (i == today.Year) ? "Bu Yıl" : i.ToString(),
                    HedefTarih = new DateTime(i, 1, 1),
                    Width = 100,
                    IsSelected = (i == today.Year)
                });
            }
        }

        TimelineListesi.ItemsSource = timeline;

        Dispatcher.Dispatch(() =>
        {
            TimelineListesi.ScrollTo(timeline.Last(), position: ScrollToPosition.End, animate: false);
        });

        _seciliZaman = timeline.LastOrDefault(t => !t.IsNavigationBtn);
    }

    private async void OnTimelineItemTapped(object sender, TappedEventArgs e)
    {
        var border = (Border)sender;
        var selectedItem = border.BindingContext as GrafikTimelineItem;
        if (selectedItem == null) return;

        await border.ScaleToAsync(0.9, 50);
        await border.ScaleToAsync(1, 50);

        if (selectedItem.IsNavigationBtn)
        {
            if (_aktifSekme == "Gunluk")
                _aktifTarih = selectedItem.NavDirection == "Back" ? _aktifTarih.AddMonths(-1) : _aktifTarih.AddMonths(1);
            else if (_aktifSekme == "Aylik")
                _aktifTarih = selectedItem.NavDirection == "Back" ? _aktifTarih.AddYears(-1) : _aktifTarih.AddYears(1);

            TimelineGuncelle();
            DinamikGrafikCizAsync();
            return;
        }

        if (_seciliZaman != null) _seciliZaman.IsSelected = false;
        selectedItem.IsSelected = true;
        _seciliZaman = selectedItem;

        DinamikGrafikCizAsync();
    }

    private async void DinamikGrafikCizAsync()
    {
        AnaGrafikKonteyneri.Children.Clear();

        if (_aktifSekme == "Gunluk") await GunlukGrafikleriCizAsync();
        else if (_aktifSekme == "Aylik") await AylikGrafikleriCizAsync();
        else if (_aktifSekme == "Yillik") await YillikGrafikleriCizAsync();
    }

    // ========================================================
    // 🌟 1. GÜNLÜK DASHBOARD
    // ========================================================
    private async Task GunlukGrafikleriCizAsync()
    {
        DateTime seciliTarih = _seciliZaman != null ? _seciliZaman.HedefTarih : _aktifTarih;
        var gunlukVeri = await _db.GetTumGecmisByDateAsync(seciliTarih);

        if (gunlukVeri == null || gunlukVeri.Count == 0)
        {
            AnaGrafikKonteyneri.Children.Add(new Label { Text = "Bu tarihte hiç antrenman kaydı yok.", TextColor = Color.FromArgb("#94a3b8"), HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 50, 0, 0) });
            return;
        }

        var prSet = gunlukVeri.OrderByDescending(x => x.Agirlik).FirstOrDefault();
        string gununRekoru = "Yok";
        if (prSet != null)
        {
            var prHareket = await _db.GetHareketByIdAsync(prSet.HareketId);
            gununRekoru = $"{(prHareket != null ? prHareket.Ad : "Bilinmiyor")} - {prSet.Agirlik} KG";
        }

        var hacimListesi = new List<(string Ad, double Hacim)>();
        foreach (var grup in gunlukVeri.GroupBy(x => x.HareketId))
        {
            var hareket = await _db.GetHareketByIdAsync(grup.Key);
            hacimListesi.Add((hareket != null ? hareket.Ad : "Bilinmiyor", grup.Sum(x => x.Agirlik * x.Tekrar)));
        }

        var bolgeYuzdeListesi = new List<(string Ad, double Yuzde)>();
        double toplamHacim = gunlukVeri.Sum(x => x.Agirlik * x.Tekrar);
        foreach (var grup in gunlukVeri.GroupBy(x => x.Bolge))
        {
            bolgeYuzdeListesi.Add((grup.Key, Math.Round((grup.Sum(x => x.Agirlik * x.Tekrar) / toplamHacim) * 100, 1)));
        }

        var hareketSayisiListesi = new List<(string Ad, double Sayi)>();
        foreach (var grup in gunlukVeri.GroupBy(x => x.Bolge))
        {
            int farkliHareketSayisi = grup.Select(x => x.HareketId).Distinct().Count();
            hareketSayisiListesi.Add((grup.Key, farkliHareketSayisi));
        }

        AnaGrafikKonteyneri.Children.Add(OlusturPRKarti("GÜNÜN REKORU (PR)", gununRekoru, "👑", "#ffaa00"));
        AnaGrafikKonteyneri.Children.Add(OlusturDikeyBarGrafik("HAREKETLERE GÖRE TOPLAM HACİM (KG)", hacimListesi, "#22ff00"));
        AnaGrafikKonteyneri.Children.Add(OlusturYatayBarGrafik("KAS BÖLGESİ DAĞILIMI", bolgeYuzdeListesi, "#ff0055", "%"));
        AnaGrafikKonteyneri.Children.Add(OlusturYatayBarGrafik("BÖLGEYE GÖRE HAREKET SAYISI", hareketSayisiListesi, "#00e5ff", "Hrk"));
    }

    // ========================================================
    // 🌟 2. AYLIK DASHBOARD (Kendi Tasarımımız Dropdown Eklendi)
    // ========================================================
    private async Task AylikGrafikleriCizAsync()
    {
        DateTime seciliTarih = _seciliZaman != null ? _seciliZaman.HedefTarih : _aktifTarih;
        var aylikVeri = await _db.GetTumGecmisByMonthAsync(seciliTarih.Year, seciliTarih.Month);

        if (aylikVeri == null || aylikVeri.Count == 0)
        {
            AnaGrafikKonteyneri.Children.Add(new Label { Text = "Bu ay hiç antrenman kaydı yok.", TextColor = Color.FromArgb("#94a3b8"), HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 50, 0, 0) });
            return;
        }

        int idmanGunuSayisi = aylikVeri.Select(x => x.Tarih.Date).Distinct().Count();
        double toplamAylikHacim = aylikVeri.Sum(x => x.Agirlik * x.Tekrar);
        AnaGrafikKonteyneri.Children.Add(OlusturPRKarti("AYLIK ÖZET", $"{idmanGunuSayisi} Gün | Toplam {toplamAylikHacim:N0} KG", "🗓️", "#ffaa00"));

        var trendListesi = new List<(string Ad, double Deger)>();
        int gunSiniri = (seciliTarih.Year == DateTime.Today.Year && seciliTarih.Month == DateTime.Today.Month) ? DateTime.Today.Day : DateTime.DaysInMonth(seciliTarih.Year, seciliTarih.Month);

        for (int i = 1; i <= gunSiniri; i++)
        {
            double gunlukHacim = aylikVeri.Where(x => x.Tarih.Day == i).Sum(x => x.Agirlik * x.Tekrar);
            trendListesi.Add(($"{i}. Gün", gunlukHacim));
        }
        AnaGrafikKonteyneri.Children.Add(OlusturDikeyBarGrafik("GÜNLÜK TOPLAM HACİM TRENDİ", trendListesi, "#22ff00"));

        var bolgeDengeListesi = new List<(string Ad, double Tonaj)>();
        foreach (var grup in aylikVeri.GroupBy(x => x.Bolge))
        {
            double tonaj = Math.Round(grup.Sum(x => x.Agirlik * x.Tekrar) / 1000.0, 1);
            bolgeDengeListesi.Add((grup.Key, tonaj));
        }
        AnaGrafikKonteyneri.Children.Add(OlusturYatayBarGrafik("BÖLGESEL DENGE RADARI (TON)", bolgeDengeListesi, "#ff0055", "Ton"));

        var hareketSayisiListesi = new List<(string Ad, double Sayi)>();
        foreach (var grup in aylikVeri.GroupBy(x => x.Bolge))
        {
            int farkliHareketSayisi = grup.Select(x => x.HareketId).Distinct().Count();
            hareketSayisiListesi.Add((grup.Key, farkliHareketSayisi));
        }
        AnaGrafikKonteyneri.Children.Add(OlusturYatayBarGrafik("AYLIK BÖLGEYE GÖRE HAREKET ÇEŞİTLİLİĞİ", hareketSayisiListesi, "#00e5ff", "Hrk"));

        // 🌟 ETKİLEŞİMLİ KENDİ YAPIMIMIZ (CUSTOM) AÇILIR MENÜ VE GELİŞİM GRAFİĞİ 🌟
        var benzersizHareketIdleri = aylikVeri.Select(x => x.HareketId).Distinct().ToList();
        var hareketSozlugu = new Dictionary<string, int>();

        foreach (var id in benzersizHareketIdleri)
        {
            var h = await _db.GetHareketByIdAsync(id);
            if (h != null) hareketSozlugu[h.Ad] = id;
        }

        var anaDropdownKonteyneri = new VerticalStackLayout { Margin = new Thickness(0, 20, 0, 0) };
        anaDropdownKonteyneri.Children.Add(new Label { Text = "GELİŞİMİNİ GÖRMEK İSTEDİĞİNİZ HAREKET", TextColor = Color.FromArgb("#94a3b8"), FontSize = 12, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, 0, 10) });

        var dropdownBaslikYazisi = new Label { Text = "Hareket Seçiniz ▼", TextColor = Colors.White, FontSize = 16, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center };
        var dropdownBaslikBorder = new Border { BackgroundColor = Color.FromArgb("#36393e"), Stroke = Color.FromArgb("#ffaa00"), StrokeThickness = 1, Padding = 15, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Content = dropdownBaslikYazisi };

        var seceneklerStack = new VerticalStackLayout { Spacing = 5 };
        var dropdownAcilirListe = new Border { BackgroundColor = Color.FromArgb("#282b30"), Stroke = Color.FromArgb("#ffaa00"), StrokeThickness = 1, Padding = 5, Margin = new Thickness(15, -5, 15, 0), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 10, 10) }, IsVisible = false, Content = seceneklerStack };

        var seciliHareketKonteyneri = new VerticalStackLayout { Margin = new Thickness(0, 15, 0, 0) };

        // Aç/Kapat Butonu
        dropdownBaslikBorder.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => dropdownAcilirListe.IsVisible = !dropdownAcilirListe.IsVisible)
        });

        // Seçim İşlemi (Action)
        Action<string> HareketiCizdir = (secilenHareketAdi) =>
        {
            dropdownBaslikYazisi.Text = secilenHareketAdi + " ▼";
            dropdownBaslikYazisi.TextColor = Color.FromArgb("#ffaa00");
            dropdownAcilirListe.IsVisible = false;
            seciliHareketKonteyneri.Children.Clear();

            int secilenId = hareketSozlugu[secilenHareketAdi];
            var oHareketinVerileri = aylikVeri.Where(x => x.HareketId == secilenId).OrderBy(x => x.Tarih).ToList();

            var hareketGelisimListesi = new List<(string Ad, double Deger)>();
            foreach (var gunluk in oHareketinVerileri.GroupBy(x => x.Tarih.Day))
            {
                hareketGelisimListesi.Add(($"{gunluk.Key}. Gün", gunluk.Max(x => x.Agirlik)));
            }

            // DİKKAT: Yeni eklediğimiz "ustteDegerGoster = true" parametresi sayesinde kilolar barların tepesine yazılacak!
            var grafik = OlusturDikeyBarGrafik($"{secilenHareketAdi.ToUpper()} AĞIRLIK GELİŞİMİ", hareketGelisimListesi, "#ffaa00", ustteDegerGoster: true);
            seciliHareketKonteyneri.Children.Add(grafik);
        };

        // Listeyi Doldur
        foreach (var hareket in hareketSozlugu.Keys)
        {
            var secenekBorder = new Border { BackgroundColor = Colors.Transparent, StrokeThickness = 0, Padding = 10 };
            var secenekYazi = new Label { Text = hareket, TextColor = Color.FromArgb("#94a3b8"), FontSize = 14, HorizontalOptions = LayoutOptions.Center };
            secenekBorder.Content = secenekYazi;

            secenekBorder.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => HareketiCizdir(hareket))
            });

            seceneklerStack.Children.Add(secenekBorder);
        }

        anaDropdownKonteyneri.Children.Add(dropdownBaslikBorder);
        anaDropdownKonteyneri.Children.Add(dropdownAcilirListe);

        AnaGrafikKonteyneri.Children.Add(anaDropdownKonteyneri);
        AnaGrafikKonteyneri.Children.Add(seciliHareketKonteyneri);

        // Sayfa açıldığında ilk hareketi otomatik olarak seç ve listele
        if (hareketSozlugu.Keys.Any())
        {
            HareketiCizdir(hareketSozlugu.Keys.First());
        }
    }

    // ========================================================
    // 🌟 3. YILLIK DASHBOARD
    // ========================================================
    private async Task YillikGrafikleriCizAsync()
    {
        DateTime seciliTarih = _seciliZaman != null ? _seciliZaman.HedefTarih : _aktifTarih;
        var yillikVeri = await _db.GetTumGecmisByYearAsync(seciliTarih.Year);

        if (yillikVeri == null || yillikVeri.Count == 0)
        {
            AnaGrafikKonteyneri.Children.Add(new Label { Text = "Bu yıl hiç antrenman kaydı yok.", TextColor = Color.FromArgb("#94a3b8"), HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 50, 0, 0) });
            return;
        }

        string[] aylar = { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };
        int aySiniri = (seciliTarih.Year == DateTime.Today.Year) ? DateTime.Today.Month : 12;

        var tonajListesi = new List<(string Ad, double Deger)>();
        for (int i = 1; i <= aySiniri; i++)
        {
            double aylikTon = Math.Round(yillikVeri.Where(x => x.Tarih.Month == i).Sum(x => x.Agirlik * x.Tekrar) / 1000.0, 1);
            tonajListesi.Add((aylar[i - 1], aylikTon));
        }
        AnaGrafikKonteyneri.Children.Add(OlusturDikeyBarGrafik("AYLIK TOPLAM TONAJ (TON)", tonajListesi, "#22ff00"));

        var gunSayisiListesi = new List<(string Ad, double Deger)>();
        for (int i = 1; i <= aySiniri; i++)
        {
            int gunSayisi = yillikVeri.Where(x => x.Tarih.Month == i).Select(x => x.Tarih.Date).Distinct().Count();
            gunSayisiListesi.Add((aylar[i - 1], gunSayisi));
        }
        AnaGrafikKonteyneri.Children.Add(OlusturDikeyBarGrafik("AYLIK ANTRENMAN GÜNÜ SAYISI", gunSayisiListesi, "#00e5ff"));

        AnaGrafikKonteyneri.Children.Add(new Label { Text = "🌟 YILIN KİŞİSEL REKORLARI (PR DUVARI) 🌟", TextColor = Color.FromArgb("#ffaa00"), FontSize = 14, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 20, 0, 10) });

        var hareketGruplari = yillikVeri.GroupBy(x => x.HareketId).ToList();
        var prListesi = new List<(string Ad, int MaxAgirlik)>();

        foreach (var grup in hareketGruplari)
        {
            var hareket = await _db.GetHareketByIdAsync(grup.Key);
            if (hareket != null)
            {
                int max = grup.Max(x => x.Agirlik);
                prListesi.Add((hareket.Ad, max));
            }
        }

        foreach (var pr in prListesi.OrderByDescending(x => x.MaxAgirlik).Take(4))
        {
            AnaGrafikKonteyneri.Children.Add(OlusturPRKarti(pr.Ad.ToUpper(), $"{pr.MaxAgirlik} KG", "🏆", "#22ff00"));
        }
    }

    // --- WIDGET OLUŞTURUCULAR ---

    private Border OlusturPRKarti(string ustBaslik, string anaMetin, string ikon, string renkKodu)
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#36393e"),
            StrokeThickness = 0,
            Padding = 15,
            StrokeShape = new RoundRectangle { CornerRadius = 15 },
            Content = new HorizontalStackLayout
            {
                Spacing = 15,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = ikon, FontSize = 30, VerticalOptions = LayoutOptions.Center },
                    new VerticalStackLayout
                    {
                        Children =
                        {
                            new Label { Text = ustBaslik, TextColor = Color.FromArgb(renkKodu), FontSize = 12, FontAttributes = FontAttributes.Bold },
                            new Label { Text = anaMetin, TextColor = Colors.White, FontSize = 18, FontAttributes = FontAttributes.Bold }
                        }
                    }
                }
            }
        };
    }

    // 🌟 DEĞİŞTİRİLDİ: "ustteDegerGoster" parametresi eklendi. Barın üstüne ağırlıkları yazar!
    private Border OlusturDikeyBarGrafik(string baslik, List<(string Ad, double Deger)> veriler, string temaRengi, bool ustteDegerGoster = false)
    {
        var kutu = new Border { BackgroundColor = Color.FromArgb("#36393e"), StrokeThickness = 0, Padding = 15, StrokeShape = new RoundRectangle { CornerRadius = 15 } };
        var anaGrid = new Grid { RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) } };

        anaGrid.Children.Add(new Label { Text = baslik, TextColor = Color.FromArgb("#94a3b8"), FontSize = 12, HorizontalOptions = LayoutOptions.Center });

        var cizimAlani = new HorizontalStackLayout { Spacing = 15, Margin = new Thickness(0, 20, 0, 0) };
        var yatayKaydirma = new ScrollView { Orientation = ScrollOrientation.Horizontal, HorizontalScrollBarVisibility = ScrollBarVisibility.Never, Content = cizimAlani };

        anaGrid.Add(yatayKaydirma, 0, 1);
        kutu.Content = anaGrid;

        double maxDeger = veriler.Any() ? veriler.Max(v => v.Deger) : 1;
        if (maxDeger == 0) maxDeger = 1;

        double maxKapasite = 200;

        for (int i = 0; i < veriler.Count; i++)
        {
            var veri = veriler[i];
            double hedefYukseklik = (veri.Deger / maxDeger) * maxKapasite;
            if (veri.Deger == 0) hedefYukseklik = 2;

            var sutun = new VerticalStackLayout { WidthRequest = 70, Spacing = 8, VerticalOptions = LayoutOptions.End };

            // Yükseklik değeri alanları kurtarmak için biraz daha pay bırakıldı
            var barKonteyner = new ContentView { HeightRequest = maxKapasite + 20, VerticalOptions = LayoutOptions.End };

            // Barın ve üstündeki değerin birlikte yükseleceği akıllı konteyner
            var barVeDegerKonteyneri = new VerticalStackLayout { VerticalOptions = LayoutOptions.End, Spacing = 5 };

            // Üstteki Değer (KG)
            var degerLabel = new Label
            {
                Text = $"{veri.Deger} KG",
                TextColor = Colors.White,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                Opacity = 0
            };

            var bar = new Border
            {
                HeightRequest = 0,
                WidthRequest = 45,
                Opacity = 0,
                BackgroundColor = Color.FromArgb(temaRengi),
                VerticalOptions = LayoutOptions.End,
                StrokeThickness = 0,
                HorizontalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8, 8, 0, 0) },
                Shadow = new Shadow { Brush = Color.FromArgb(temaRengi), Radius = 10, Opacity = 0.5f }
            };

            // İsteniyorsa değeri üste ekle
            if (ustteDegerGoster) barVeDegerKonteyneri.Children.Add(degerLabel);
            barVeDegerKonteyneri.Children.Add(bar);

            barKonteyner.Content = barVeDegerKonteyneri;

            var etiket = new Label
            {
                Text = veri.Ad,
                TextColor = Color.FromArgb("#94a3b8"),
                FontSize = 10,
                MaxLines = 3,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalTextAlignment = TextAlignment.Center,
                HeightRequest = 40,
                VerticalTextAlignment = TextAlignment.Start
            };

            sutun.Children.Add(barKonteyner);
            sutun.Children.Add(etiket);
            cizimAlani.Children.Add(sutun);

            DikeyAnimasyonBaslat(bar, hedefYukseklik, i * 30, ustteDegerGoster ? degerLabel : null);
        }

        return kutu;
    }

    private Border OlusturYatayBarGrafik(string baslik, List<(string Ad, double Deger)> veriler, string temaRengi, string birim)
    {
        var kutu = new Border { BackgroundColor = Color.FromArgb("#36393e"), StrokeThickness = 0, Padding = 15, StrokeShape = new RoundRectangle { CornerRadius = 15 } };
        var stack = new VerticalStackLayout { Spacing = 15 };
        stack.Children.Add(new Label { Text = baslik, TextColor = Color.FromArgb("#94a3b8"), FontSize = 12, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, 0, 10) });

        double maxReferans = (birim == "%") ? 100.0 : (veriler.Any() ? veriler.Max(v => v.Deger) : 1);
        if (maxReferans == 0) maxReferans = 1;

        for (int i = 0; i < veriler.Count; i++)
        {
            var veri = veriler[i];

            var satir = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 15
            };

            satir.Children.Add(new Label { Text = veri.Ad, TextColor = Colors.White, FontSize = 12, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start });

            var barArkaPlan = new Border { BackgroundColor = Color.FromArgb("#282b30"), HeightRequest = 12, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 6 }, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Fill };

            var barDolu = new Border
            {
                BackgroundColor = Color.FromArgb(temaRengi),
                WidthRequest = 0,
                HorizontalOptions = LayoutOptions.Start,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                Opacity = 0,
                Shadow = new Shadow { Brush = Color.FromArgb(temaRengi), Radius = 10, Opacity = 0.5f }
            };

            barArkaPlan.Content = barDolu;

            string etiketMetni = (birim == "%") ? $"%{veri.Deger}" : $"{veri.Deger} {birim}";

            var sagEtiket = new Label { Text = etiketMetni, TextColor = Color.FromArgb(temaRengi), FontSize = 12, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };

            satir.Add(barArkaPlan, 1, 0);
            satir.Add(sagEtiket, 2, 0);

            stack.Children.Add(satir);

            int gecikmeMs = i * 150;
            barArkaPlan.SizeChanged += (s, e) =>
            {
                if (barArkaPlan.Width > 0 && barDolu.WidthRequest == 0)
                {
                    double hedefGenislik = (veri.Deger / maxReferans) * barArkaPlan.Width;
                    YatayAnimasyonBaslat(barDolu, hedefGenislik, gecikmeMs);
                }
            };
        }

        kutu.Content = stack;
        return kutu;
    }

    // Animasyon motoruna, ağırlık değerini (Label) barlarla birlikte yumuşakça gösterme özelliği eklendi
    private async void DikeyAnimasyonBaslat(Border bar, double hedefYukseklik, int gecikmeMs, Label degerLabel = null)
    {
        await Task.Delay(gecikmeMs);
        bar.Opacity = 1;
        bar.Animate("DikeyBarY", new Animation(v => bar.HeightRequest = v, 0, hedefYukseklik), length: 400, easing: Easing.SpringOut);

        if (degerLabel != null)
        {
            _ = degerLabel.FadeTo(1, 400); // Rakamlar zarifçe belirsin
        }
    }

    private async void YatayAnimasyonBaslat(Border bar, double hedefGenislik, int gecikmeMs)
    {
        await Task.Delay(gecikmeMs);
        bar.Opacity = 1;
        bar.Animate("YatayBarX", new Animation(v => bar.WidthRequest = v, 0, hedefGenislik), length: 400, easing: Easing.CubicOut);
    }
}