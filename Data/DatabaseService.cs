using SQLite;
using AntrenmanTakipApp.Models; // Kendi oluşturduğumuz Hareket sınıfını kullanabilmek için

namespace AntrenmanTakipApp.Data
{
    public class DatabaseService
    {
        // SQLite bağlantımızı tutacak olan ana değişken
        private SQLiteAsyncConnection _db;

        // 1. VERİTABANI KURULUMU (İnşaat Aşaması)
        // Bu fonksiyon her çalıştığında veritabanının var olup olmadığını kontrol eder.
        async Task Init()
        {
            // Eğer bağlantı zaten kuruluysa tekrar kurmaya çalışma, doğrudan geri dön
            if (_db != null)
                return;

            // Telefonun (Android/iOS) kendi güvenli hafıza klasörünün yolunu bulur
            // Ve oraya "Antrenman.db3" adında bir veritabanı dosyası yolu oluşturur
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Antrenman.db3");

            // O yola veritabanı dosyasını bağla
            _db = new SQLiteAsyncConnection(databasePath);

            // Hareket modelimizi okuyup SQL tablosunu OLUŞTUR. 
            // (Eğer tablo daha önceden oluşturulmuşsa hiçbir şeyi silmez, hata vermez, es geçer)
            await _db.CreateTableAsync<Hareket>();
        }

        // 2. VERİLERİ OKUMA (Listeleme)
        // Veritabanındaki tüm hareketleri bir Liste (List) olarak bize geri döndürür
        public async Task<List<Hareket>> GetHareketlerAsync()
        {
            await Init(); 

            return await _db.Table<Hareket>().ToListAsync();
        }

        public async Task<int> KaydetHareketAsync(Hareket hareket)
        {
            await Init(); 


            return await _db.InsertAsync(hareket);
        }
    }
}