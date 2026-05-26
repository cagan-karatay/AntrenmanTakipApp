using AntrenmanTakipApp.Data;   // Veritabanı köprümüzü kullanmak için
using AntrenmanTakipApp.Models; // Hareket şablonumuzu kullanmak için
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace AntrenmanTakipApp;

public partial class HareketEklePage : ContentPage
{
    // Hangi bölgeye hareket eklediğimizi hafızada tutacak değişken
    string seciliBolge;

    public HareketEklePage(string bolgeAdi)
    {
        InitializeComponent();
        seciliBolge = bolgeAdi; // Önceki sayfadan (örneğin Back) gelen ismi hafızaya al
    }

    private async void OnKaydetClicked(object sender, EventArgs e)
    {
        // 1. KONTROL: Kullanıcı kutuyu boş mu bıraktı?
        if (string.IsNullOrWhiteSpace(HareketAdiEntry.Text))
        {
            await DisplayAlert("Hata", "Lütfen bir hareket adı girin.", "Tamam");
            return; // Boşsa işlemi iptal et
        }


        var yeniHareket = new Hareket
        {
            Bolge = seciliBolge,      
            Ad = HareketAdiEntry.Text  
        };

        var db = new DatabaseService();
        await db.KaydetHareketAsync(yeniHareket);

        var toast = Toast.Make("Hareket başarıyla eklendi", ToastDuration.Short, 14);
        await toast.Show();

        await Navigation.PopAsync();
    }
}