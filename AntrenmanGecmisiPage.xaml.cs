using AntrenmanTakipApp.Data;
using AntrenmanTakipApp.Models;
using System.Collections.ObjectModel;

namespace AntrenmanTakipApp;

public partial class AntrenmanGecmisiPage : ContentPage
{
    private Hareket _secilenHareket;
    private DatabaseService _db = new DatabaseService();

    private DateTime _aktifAy = DateTime.Today; // Varsayılan olarak bu ay

    private SetUIModel _suAnAcikSet = null;
    private VerticalStackLayout _suAnAcikLayout = null;

    private readonly Color _neutralBgColor = Color.FromArgb("#4e535b");
    private readonly Color _neutralTextColor = Colors.White;

    private TimelineItem _seciliGun;

    public class TimelineItem : BindableObject
    {
        public string Text { get; set; }
        public DateTime Tarih { get; set; }
        public bool IsToday { get; set; }
        public bool IsPreviousMonth { get; set; }

        public int FontSize => IsPreviousMonth ? 11 : 14;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BorderColor));
                OnPropertyChanged(nameof(BgColor));
                OnPropertyChanged(nameof(TextColor));
            }
        }

        public Color BorderColor => IsToday ? Color.FromArgb("#22ff00") : (IsSelected ? Color.FromArgb("#ff6a00") : Colors.Transparent);
        public Color BgColor => IsPreviousMonth ? Color.FromArgb("#1e1e1e") : (IsSelected ? Color.FromArgb("#4e535b") : Color.FromArgb("#36393e"));
        public Color TextColor => (IsToday || IsSelected) ? Colors.White : Color.FromArgb("#94a3b8");
    }

    public class SetUIModel : BindableObject
    {
        public AntrenmanGecmisi Gecmis { get; set; }

        private bool _editModuAcik;
        public bool EditModuAcik
        {
            get => _editModuAcik;
            set
            {
                _editModuAcik = value;
                OnPropertyChanged();
            }
        }
    }

    public AntrenmanGecmisiPage(Hareket hareket)
    {
        InitializeComponent();
        _secilenHareket = hareket;
        this.Title = hareket.Ad;

        BaslangicVerileriniGetir();
        TimelineOlustur();
    }

    private async void BaslangicVerileriniGetir()
    {
        var sonVeri = await _db.GetSonSetVerileriAsync(_secilenHareket.Id);
        KgEntry.Text = sonVeri.Agirlik > 0 ? sonVeri.Agirlik.ToString() : string.Empty;
        TekrarEntry.Text = sonVeri.Tekrar > 0 ? sonVeri.Tekrar.ToString() : string.Empty;
    }

    private void TimelineOlustur()
    {
        var timeline = new ObservableCollection<TimelineItem>();
        var today = DateTime.Today;

        // Varsayılan olarak o ayın toplam gün sayısını al (Örn: 30 veya 31)
        int gunSayisi = DateTime.DaysInMonth(_aktifAy.Year, _aktifAy.Month);

        // MANTIK DÜZELTMESİ: Eğer içinde bulunduğumuz ay ve yıldaysak, geleceği çizme!
        // Gün sayısını "Bugün" olarak sınırla. (Örn: 3 Haziransak sadece 3'e kadar çizer)
        if (_aktifAy.Year == today.Year && _aktifAy.Month == today.Month)
        {
            gunSayisi = today.Day;
        }

        // 1. Önceki Ay Butonu
        timeline.Add(new TimelineItem { Text = "Önceki\nAy", IsPreviousMonth = true });

        // 2. Günleri oluştur
        for (int i = 1; i <= gunSayisi; i++)
        {
            var date = new DateTime(_aktifAy.Year, _aktifAy.Month, i);
            bool isToday = (date == today);

            var item = new TimelineItem
            {
                Text = isToday ? "Bugün" : i.ToString(),
                Tarih = date,
                IsToday = isToday,
                // Ekran ilk açıldığında veya ay değiştiğinde seçili günü ayarla
                IsSelected = (_seciliGun == null && isToday) || (date.Date == _seciliGun?.Tarih.Date)
            };

            timeline.Add(item);
            if (item.IsSelected) _seciliGun = item; // Seçili referansı güncelle
        }

        // 3. Sonraki Ay Butonu (Sadece geçmiş aylara bakıyorsak çıkar)
        if (_aktifAy.Year < today.Year || (_aktifAy.Year == today.Year && _aktifAy.Month < today.Month))
        {
            timeline.Add(new TimelineItem { Text = "Sonraki\nAy", IsPreviousMonth = true });
        }

        TimelineListesi.ItemsSource = timeline;

        var culture = new System.Globalization.CultureInfo("tr-TR");
        AyLabel.Text = _aktifAy.ToString("MMMM", culture);

        // Ekran çizildiğinde otomatik olarak seçili güne (veya en sağdaki Bugüne) kaydır
        Dispatcher.Dispatch(() =>
        {
            if (_seciliGun != null)
                TimelineListesi.ScrollTo(_seciliGun, position: ScrollToPosition.End, animate: false);
        });

        // Verileri seçili gün için yükle
        VerileriYukleByDate(_seciliGun != null ? _seciliGun.Tarih : today);
    }

    private async void VerileriYukleByDate(DateTime date)
    {
        _suAnAcikSet = null;
        _suAnAcikLayout = null;

        var gecmis = await _db.GetGecmisByDateAsync(_secilenHareket.Id, date);
        var uiListe = gecmis.Select(g => new SetUIModel { Gecmis = g, EditModuAcik = false }).ToList();
        GecmisListesi.ItemsSource = uiListe;
    }

    private async void OnTimelineItemTapped(object sender, TappedEventArgs e)
    {
        var border = (Border)sender;
        var selectedItem = border.BindingContext as TimelineItem;

        if (selectedItem == null) return;

        // ÖNCEKİ AY BUTONU MANTIĞI
        if (selectedItem.Text.Contains("Önceki"))
        {
            _aktifAy = _aktifAy.AddMonths(-1);
            TimelineOlustur(); // Yeni ayın günlerini çiz
            return;
        }

        // SONRAKİ AY BUTONU MANTIĞI
        if (selectedItem.Text.Contains("Sonraki"))
        {
            _aktifAy = _aktifAy.AddMonths(1);
            TimelineOlustur();
            return;
        }

        // Normal Gün Seçimi
        await border.ScaleToAsync(0.9, 50);

        if (_seciliGun != null) _seciliGun.IsSelected = false;

        selectedItem.IsSelected = true;
        _seciliGun = selectedItem;

        VerileriYukleByDate(selectedItem.Tarih);

        await border.ScaleToAsync(1, 50);
    }

    // DÜZELTİLDİ: Gün değiştikçe setlerin şelale gibi pürüzsüz süzülme efekti (Serileştirildi)
    // 1. GÜNCELLENDİ: Bölgeler ekranındaki efsanevi yukarıdan aşağı yaylanarak dizilme efekti
    // 1. KUSURSUZ ŞELALE: Kartlar artık aynı anda değil, sıra sıra yaylanarak inecek!
    // 1. KUSURSUZ ŞELALE: Kartlar doğuştan gizlidir, sırası gelince sahneye atlarlar!
    private async void OnSetCardLoaded(object sender, EventArgs e)
    {
        var view = sender as VisualElement;
        var model = view?.BindingContext as SetUIModel;

        if (view != null && model != null)
        {
            // SİHİRLİ FORMÜL: Set sayısına göre bekleme süresi hesapla
            int gecikmeMilisaniyesi = (model.Gecmis.SetSayisi - 1) * 100;

            if (gecikmeMilisaniyesi > 0)
            {
                await Task.Delay(gecikmeMilisaniyesi); // Sırası gelene kadar bekle
            }

            // Sırası gelen kart yaylanarak ekrana iner ve görünür olur
            _ = view.FadeTo(1, 350, Easing.CubicOut);
            _ = view.TranslateTo(0, 0, 350, Easing.SpringOut);
        }
    }

    private void OnSetTapped(object sender, TappedEventArgs e)
    {
        var border = (Border)sender;
        var tappedSet = border.BindingContext as SetUIModel;
        if (tappedSet == null) return;

        if (border.Content is VerticalStackLayout mainStack && mainStack.Children.Count > 1)
        {
            if (mainStack.Children[1] is VerticalStackLayout deleteSection)
            {
                if (_suAnAcikSet != null && _suAnAcikSet != tappedSet && _suAnAcikLayout != null)
                {
                    AnimateDeleteSection(_suAnAcikLayout, false);
                    _suAnAcikSet.EditModuAcik = false;
                }

                tappedSet.EditModuAcik = !tappedSet.EditModuAcik;

                if (tappedSet.EditModuAcik)
                {
                    AnimateDeleteSection(deleteSection, true);
                    _suAnAcikSet = tappedSet;
                    _suAnAcikLayout = deleteSection;
                }
                else
                {
                    AnimateDeleteSection(deleteSection, false);
                    _suAnAcikSet = null;
                    _suAnAcikLayout = null;
                }
            }
        }
    }

    // 🌟 DÜZELTİLDİ: 'duration' parametresi yerine 'length' kullanılarak hata çözüldü
    // 🌟 MATRİX AKICILIĞINDAKİ ESNETME MOTORU (PÜRÜZSÜZLEŞTİRİLDİ) 🌟
    // 🌟 KESİN ÇÖZÜM: HeightRequest animasyonu kaldırıldı, yerine Fade ve Translate eklendi.
    // Artık Android listenin boyunu kitlemeyecek ve buton asla yarım kalmayacak!
    private async void AnimateDeleteSection(VerticalStackLayout layout, bool isOpen)
    {
        // Eski animasyonları temizle
        layout.AbortAnimation("CollapseExpand");
        layout.CancelAnimations(); // 🌟 1. HATA ÇÖZÜLDÜ: Belirsizlik yaratmamak için doğrudan obje üzerinden çağırdık.

        if (isOpen)
        {
            // Önce görünür yap (Android çerçevenin boyunu hemen hesaplasın)
            layout.IsVisible = true;
            layout.Opacity = 0;
            layout.TranslationY = -15;

            // 🌟 2. HATA ÇÖZÜLDÜ: TranslateTo(X, Y, Süre, Easing) formatına uygun olarak baştaki 0 eklendi.
            _ = layout.TranslateTo(0, 0, 200, Easing.CubicOut);
            await layout.FadeTo(1, 200, Easing.CubicOut);
        }
        else
        {
            // Kapanırken yukarı doğru kayarak kaybolur
            _ = layout.TranslateTo(0, -15, 200, Easing.CubicIn);
            await layout.FadeTo(0, 200, Easing.CubicIn);

            // İşlem bitince tamamen gizle ve pozisyonu sıfırla
            layout.IsVisible = false;
            layout.TranslationY = 0;
        }
    }

    private async void OnSetSilClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        var silinecekSet = btn.CommandParameter as AntrenmanGecmisi;

        if (silinecekSet == null) return;

        btn.BackgroundColor = Color.FromArgb("#ef4444");
        btn.TextColor = Colors.White;
        await btn.ScaleToAsync(0.9, 100);
        btn.BackgroundColor = Colors.Transparent;
        btn.TextColor = Color.FromArgb("#ef4444");
        await btn.ScaleToAsync(1, 100);

        bool onay = await DisplayAlert("Seti Sil", $"{silinecekSet.SetSayisi}. Seti silmek istediğine emin misin?", "Evet, Sil", "İptal");

        if (onay)
        {
            await _db.SilGecmisSetAsync(silinecekSet);
            VerileriYukleByDate(_seciliGun != null ? _seciliGun.Tarih : DateTime.Today);
        }
    }

    private async void OnKgPlusClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        btn.BackgroundColor = Colors.Transparent;
        btn.BorderWidth = 2;
        btn.BorderColor = Color.FromArgb("#22ff00");
        await btn.ScaleToAsync(0.9, 100);

        int current = int.TryParse(KgEntry.Text, out int val) ? val : 0;
        KgEntry.Text = (current + 1).ToString();

        await Task.Delay(50);

        btn.BackgroundColor = _neutralBgColor;
        btn.BorderWidth = 0;
        await btn.ScaleToAsync(1, 100);
    }

    private async void OnKgMinusClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        btn.BackgroundColor = Colors.Transparent;
        btn.BorderWidth = 2;
        btn.BorderColor = Color.FromArgb("#ef4444");
        await btn.ScaleToAsync(0.9, 100);

        int current = int.TryParse(KgEntry.Text, out int val) ? val : 0;
        if (current > 0) KgEntry.Text = (current - 1).ToString();

        await Task.Delay(50);

        btn.BackgroundColor = _neutralBgColor;
        btn.BorderWidth = 0;
        await btn.ScaleToAsync(1, 100);
    }

    private async void OnRepSelectClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        await btn.ScaleToAsync(0.9, 100);
        TekrarEntry.Text = btn.Text;
        await btn.ScaleToAsync(1, 100);
    }

    private async void OnKaydetClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        await Task.WhenAll(btn.FadeToAsync(0.5, 100), btn.ScaleToAsync(0.95, 100));
        await Task.Delay(50);
        await Task.WhenAll(btn.FadeToAsync(1, 100), btn.ScaleToAsync(1, 100));

        if (string.IsNullOrWhiteSpace(KgEntry.Text) || string.IsNullOrWhiteSpace(TekrarEntry.Text)) return;

        // MANTIK DÜZELTMESİ 1: Hedef Tarih
        // Kaydedilen verinin tarihi artık zorla 'Şu An' değil, Timeline'da seçili olan gün!
        DateTime hedefTarih = _seciliGun != null ? _seciliGun.Tarih : DateTime.Today;
        if (hedefTarih.Date == DateTime.Today)
            hedefTarih = DateTime.Now; // Bugünse saati de tut
        else
            hedefTarih = hedefTarih.Date.Add(DateTime.Now.TimeOfDay); // Geçmişse saati uydur

        // MANTIK DÜZELTMESİ 2: Set Sırası
        // Set numarasını sadece "bugün"den çekmek yerine, ekranda o an açık olan listeden sayıyoruz.
        // Böylece geçmiş bir güne set eklerken sıralama (1. Set, 2. Set) kusursuz işler.
        var ekrandakiSetler = GecmisListesi.ItemsSource as List<SetUIModel>;
        int yeniSetNumarasi = (ekrandakiSetler?.Count ?? 0) + 1;

        var yeniSet = new AntrenmanGecmisi
        {
            HareketId = _secilenHareket.Id,
            Bolge = _secilenHareket.Bolge,
            SetSayisi = yeniSetNumarasi,
            Tekrar = int.Parse(TekrarEntry.Text),
            Agirlik = int.Parse(KgEntry.Text),
            Tarih = hedefTarih // Seçili günün tarihi veritabanına gidiyor!
        };

        await _db.KaydetGecmisAsync(yeniSet);
        TekrarEntry.Text = string.Empty;

        // MANTIK DÜZELTMESİ 3: Ekranı Tazeleme
        // Sistemi zorla "Bugün" topuna fırlatmayı kaldırdık. 
        // Sadece bulunduğun seçili günün listesini tazeler.
        if (_seciliGun != null)
        {
            VerileriYukleByDate(_seciliGun.Tarih);
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        btn.BackgroundColor = Colors.Transparent;
        btn.BorderWidth = 2;
        btn.BorderColor = Color.FromArgb("#22ff00");
        await btn.ScaleToAsync(0.8, 100);
        await Task.Delay(50);
        btn.BackgroundColor = _neutralBgColor;
        btn.BorderWidth = 0;
        await btn.ScaleToAsync(1, 100);

        if (AnaSayfaWrapper.Current != null)
        {
            AnaSayfaWrapper.Current.AnaEkranaDon();
        }
        await Navigation.PopToRootAsync();
    }
}