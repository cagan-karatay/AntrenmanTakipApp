using AntrenmanTakipApp.Data;
using AntrenmanTakipApp.Models;

namespace AntrenmanTakipApp;

public partial class HareketEklePage : ContentPage
{
    private string _gecerliBolge;

    // 🌟 A'dan Z'ye Sıralı Hazır Hareket Kütüphanesi
    // 🌟 A'dan Z'ye Devasa Spor Veritabanı (Fitness Ansiklopedisi)
    private List<string> _tumHareketler = new List<string>
    {
        // GÖĞÜS (Chest)
        "Bench Press", "Incline Bench Press", "Decline Bench Press", "Dumbbell Bench Press",
        "Incline Dumbbell Press", "Decline Dumbbell Press", "Dumbbell Fly", "Incline Dumbbell Fly",
        "Cable Crossover", "Low Cable Crossover", "High Cable Crossover", "Pec Deck Fly",
        "Machine Chest Press", "Chest Dip", "Push-up (Şınav)", "Diamond Push-up", "Decline Push-up", "Pullover",

        // SIRT (Back)
        "Pull-up (Barfiks)", "Chin-up", "Lat Pulldown (Wide Grip)", "Lat Pulldown (Close Grip)",
        "Lat Pulldown (Reverse Grip)", "Barbell Row", "Pendlay Row", "Yates Row", "Dumbbell Row",
        "T-Bar Row", "Seated Cable Row", "Straight Arm Pulldown", "Face Pull", "Deadlift",
        "Rack Pull", "Good Morning", "Hyperextension (Ters Mekik)",

        // OMUZ (Shoulders)
        "Overhead Press", "Military Press", "Dumbbell Shoulder Press", "Arnold Press",
        "Machine Shoulder Press", "Lateral Raise", "Cable Lateral Raise", "Machine Lateral Raise",
        "Front Raise (Dumbbell)", "Front Raise (Barbell)", "Front Raise (Cable)",
        "Reverse Pec Deck Fly", "Rear Delt Dumbbell Fly", "Upright Row", "Barbell Shrug", "Dumbbell Shrug",

        // BİSEPS (Biceps)
        "Barbell Curl", "Dumbbell Curl", "Hammer Curl", "EZ Bar Curl", "Preacher Curl",
        "Concentration Curl", "Cable Curl", "Incline Dumbbell Curl", "Reverse Curl", "Spider Curl",

        // TRİSEPS (Triceps)
        "Pushdown (Rope/Halat)", "Pushdown (Straight Bar/Düz Bar)", "Pushdown (V-Bar)",
        "Skullcrusher", "Overhead Tricep Extension", "Cable Tricep Extension", "Tricep Kickback",
        "Close-Grip Bench Press", "Tricep Dips", "Bench Dips",

        // BACAK & KALÇA (Legs & Glutes)
        "Squat", "Front Squat", "Sumo Squat", "Goblet Squat", "Hack Squat",
        "Leg Press", "Lunges", "Walking Lunges", "Bulgarian Split Squat", "Step-up",
        "Leg Extension", "Leg Curl (Seated)", "Leg Curl (Lying)",
        "Romanian Deadlift (RDL)", "Stiff-Leg Deadlift", "Hip Thrust", "Glute Bridge",
        "Calf Raise (Standing)", "Calf Raise (Seated)", "Donkey Calf Raise",

        // KARIN (Core & Abs)
        "Crunch", "Decline Crunch", "Cable Crunch", "Bicycle Crunch", "Sit-up (Mekik)",
        "Plank", "Side Plank", "Hanging Leg Raise", "Lying Leg Raise", "Russian Twist",
        "Ab Wheel Rollout", "Woodchopper",

        // TAM VÜCUT & DİĞER (Full Body / Functional)
        "Burpee", "Kettlebell Swing", "Farmer's Walk", "Power Clean", "Thruster", "Box Jump"

    }.OrderBy(h => h).ToList(); // .OrderBy(h => h) sayesinde hepsi otomatik olarak A'dan Z'ye sıralanır!
    public HareketEklePage(string bolgeAdi)
    {
        InitializeComponent();
        _gecerliBolge = bolgeAdi;

        // Sayfa ilk açıldığında hazır hareketler listesini doldurur
        HazirHareketlerListesi.ItemsSource = _tumHareketler;
    }

    // 🌟 SEN YAZDIKÇA ÇALIŞAN ANLIK FİLTRELEME MOTORU
    private void OnHareketTextChanged(object sender, TextChangedEventArgs e)
    {
        var arananKelime = e.NewTextValue?.ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(arananKelime))
        {
            // Arama kutusu boşsa tüm listeyi göster
            HazirHareketlerListesi.ItemsSource = _tumHareketler;
        }
        else
        {
            // Yazılan kelimeyi içeren hareketleri anında süz
            var filtrelenmisListe = _tumHareketler
                .Where(h => h.ToLowerInvariant().Contains(arananKelime))
                .ToList();

            HazirHareketlerListesi.ItemsSource = filtrelenmisListe;
        }
    }

    // 🌟 HAZIR HAREKETE TIKLANDIĞINDA YUKARIYA AKTARMA MANTIĞI
    private void OnHazirHareketSelected(object sender, SelectionChangedEventArgs e)
    {
        var secilenHareket = e.CurrentSelection.FirstOrDefault() as string;

        if (!string.IsNullOrEmpty(secilenHareket))
        {
            // 1. Tıklanan hazır hareketi metin kutusuna yazdırır
            HareketAdiEntry.Text = secilenHareket;

            // 2. Seçimi temizler (Aynı harekete tekrar basılabilsin diye)
            ((CollectionView)sender).SelectedItem = null;

            // 3. Odağı kutudan çekerek klavyeyi kapatır (Daha temiz UX)
            HareketAdiEntry.Unfocus();
        }
    }

    private async void OnKaydetClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(HareketAdiEntry.Text))
        {
            await DisplayAlert("Hata", "Lütfen hareketin adını girin.", "Tamam");
            return;
        }

        var yeniHareket = new Hareket
        {
            Ad = HareketAdiEntry.Text,
            Bolge = _gecerliBolge,
            AktifMi = true
        };

        var db = new DatabaseService();
        await db.KaydetVeyaGuncelleHareketAsync(yeniHareket);

        await Navigation.PopAsync();
    }
}