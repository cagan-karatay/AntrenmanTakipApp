using SQLite;
using AntrenmanTakipApp.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AntrenmanTakipApp.Data
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _db;

        async Task Init()
        {
            if (_db != null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Antrenman.db3");
            _db = new SQLiteAsyncConnection(databasePath);

            await _db.CreateTableAsync<Hareket>();
            await _db.CreateTableAsync<AntrenmanGecmisi>(); // Geçmiş tablosunu da sisteme tanıttık
        }

        public async Task<List<Hareket>> GetHareketlerAsync()
        {
            await Init();
            // Yalnızca Aktif olanları (Silinmemişleri) listele
            return await _db!.Table<Hareket>().Where(h => h.AktifMi == true).ToListAsync();
        }

        public async Task<int> KaydetHareketAsync(Hareket hareket)
        {
            await Init();
            return await _db!.InsertAsync(hareket);
        }

        // YENİ EKLENEN: HareketEklePage sayfasının aradığı fonksiyon
        public async Task KaydetVeyaGuncelleHareketAsync(Hareket hareket)
        {
            await Init();

            // Eğer Id 0 değilse daha önce kaydedilmiş demektir (Güncelle), 0 ise yepyeni harekettir (Ekle)
            if (hareket.Id != 0)
                await _db!.UpdateAsync(hareket);
            else
                await _db!.InsertAsync(hareket);
        }

        // AKILLI SİLME ALGORİTMASI
        public async Task AkilliSilHareketAsync(Hareket hareket)
        {
            await Init();

            // 1. KONTROL: Bu hareketin Antrenman Geçmişi tablosunda bir kaydı var mı?
            var gecmisKaydiVarMi = await _db!.Table<AntrenmanGecmisi>()
                                            .Where(g => g.HareketId == hareket.Id)
                                            .CountAsync();

            if (gecmisKaydiVarMi > 0)
            {
                // GEÇMİŞİ VAR: Gerçekten silersek veriler çöker. Yumuşak silme (Arşivleme) yapıyoruz.
                hareket.AktifMi = false;
                await _db.UpdateAsync(hareket);
            }
            else
            {
                // GEÇMİŞİ YOK: Kullanıcı yanlışlıkla eklemiş. Veritabanında yer kaplamasın, KALICI SİL!
                await _db.DeleteAsync(hareket);
            }
        }
        public async Task<List<AntrenmanGecmisi>> GetAntrenmanGecmisiAsync(int hareketId)
        {
            await Init();
            // Geçmiş setleri tarihe göre en yeniden eskiye sıralayarak getirir
            return await _db!.Table<AntrenmanGecmisi>().Where(g => g.HareketId == hareketId).OrderByDescending(g => g.Tarih).ToListAsync();
        }

        public async Task<int> KaydetGecmisAsync(AntrenmanGecmisi gecmis)
        {
            await Init();
            return await _db!.InsertAsync(gecmis);
        }
        // 1. Bu hareket için girilen en son ağırlığı getirir
        public async Task<int> GetSonAgirlikAsync(int hareketId)
        {
            await Init();
            var sonKayit = await _db!.Table<AntrenmanGecmisi>()
                                     .Where(g => g.HareketId == hareketId)
                                     .OrderByDescending(g => g.Tarih)
                                     .FirstOrDefaultAsync();
            return sonKayit?.Agirlik ?? 0;
        }

        // 2. Bugün bu hareketten kaç set yapıldığını sayar (Sıradaki seti belirlemek için)
        public async Task<int> GetBugunkuSetSayisiAsync(int hareketId)
        {
            await Init();
            var bugun = DateTime.Today;
            return await _db!.Table<AntrenmanGecmisi>()
                             .Where(g => g.HareketId == hareketId && g.Tarih >= bugun)
                             .CountAsync();
        }

        // 3. Sadece BUGÜN girilen setleri getirir
        public async Task<List<AntrenmanGecmisi>> GetBugunkuGecmisAsync(int hareketId)
        {
            await Init();
            var bugun = DateTime.Today;
            return await _db!.Table<AntrenmanGecmisi>()
                             .Where(g => g.HareketId == hareketId && g.Tarih >= bugun)
                             .OrderByDescending(g => g.Tarih)
                             .ToListAsync();
        }
        // 1. Yeni Veri Modeli: Ana ekrandaki kartları besleyecek özel sınıf
        public class GunlukHareketOzet
        {
            public Hareket Hareket { get; set; } = new();
            public string Ad { get; set; } = string.Empty;
            public string AgirliklarString { get; set; } = string.Empty;
            public DateTime SonIslemTarihi { get; set; }
        }

        // 2. Ana ekran için bugünkü hareketleri analiz eden zeki fonksiyon
        public async Task<List<GunlukHareketOzet>> GetBugunkuHareketOzetiAsync()
        {
            await Init();
            var bugun = DateTime.Today;

            // Bugün girilen tüm setleri çek
            var bugunkuSetler = await _db!.Table<AntrenmanGecmisi>()
                                         .Where(g => g.Tarih >= bugun)
                                         .ToListAsync();

            var hareketIds = bugunkuSetler.Select(s => s.HareketId).Distinct().ToList();
            var ozetListesi = new List<GunlukHareketOzet>();

            foreach (var id in hareketIds)
            {
                var hareket = await _db.Table<Hareket>().Where(h => h.Id == id).FirstOrDefaultAsync();
                if (hareket == null) continue;

                // Bu hareketin bugünkü setlerini tarihe göre sırala
                var setler = bugunkuSetler.Where(s => s.HareketId == id).OrderBy(s => s.Tarih).ToList();
                var agirliklar = setler.Select(s => s.Agirlik.ToString()).ToList();

                // 4 setten fazlaysa sonuna "..." koy, değilse normal birleştir (Örn: 8/10/10)
                string agirlikStr = agirliklar.Count > 4
                    ? string.Join("/", agirliklar.Take(4)) + "..."
                    : string.Join("/", agirliklar);

                ozetListesi.Add(new GunlukHareketOzet
                {
                    Hareket = hareket,
                    Ad = hareket.Ad,
                    AgirliklarString = agirlikStr,
                    SonIslemTarihi = setler.Last().Tarih
                });
            }

            // En son işlem yapılan en üstte (Aktif) olacak şekilde sırala
            return ozetListesi.OrderByDescending(x => x.SonIslemTarihi).ToList();
        }

        // 3. Devam Et butonu için son seti (Kilo + Tekrar) tam getiren fonksiyon
        public async Task<(int Agirlik, int Tekrar)> GetSonSetVerileriAsync(int hareketId)
        {
            await Init();
            var sonKayit = await _db!.Table<AntrenmanGecmisi>()
                                     .Where(g => g.HareketId == hareketId)
                                     .OrderByDescending(g => g.Tarih)
                                     .FirstOrDefaultAsync();

            return sonKayit != null ? (sonKayit.Agirlik, sonKayit.Tekrar) : (0, 0);
        }
        // YENİ: Sadece istenilen seti kalıcı olarak siler
        public async Task SilGecmisSetAsync(AntrenmanGecmisi set)
        {
            await Init();
            await _db!.DeleteAsync(set);
        }
        // YENİ: İstenilen spesifik bir güne ait setleri getirir
        public async Task<List<AntrenmanGecmisi>> GetGecmisByDateAsync(int hareketId, DateTime date)
        {
            await Init();
            // O günün başlangıcı (00:00) ve bitişi (23:59) arasındaki verileri filtreler
            var start = date.Date;
            var end = start.AddDays(1);

            return await _db!.Table<AntrenmanGecmisi>()
                             .Where(g => g.HareketId == hareketId && g.Tarih >= start && g.Tarih < end)
                             .OrderBy(g => g.Tarih)
                             .ToListAsync();
        }
        // Grafikler için o günkü TÜM antrenman geçmişini çeker
        public async Task<List<AntrenmanGecmisi>> GetTumGecmisByDateAsync(DateTime tarih)
        {
            await Init();
            var tumKayitlar = await _db.Table<AntrenmanGecmisi>().ToListAsync();
            // Sadece seçilen güne ait olanları filtrele
            return tumKayitlar.Where(g => g.Tarih.Date == tarih.Date).ToList();
        }

        // ID'ye bakarak hareketin adını bulmamızı sağlar
        public async Task<Hareket> GetHareketByIdAsync(int id)
        {
            await Init();
            return await _db.Table<Hareket>().FirstOrDefaultAsync(h => h.Id == id);
        }
        // Grafikler için O AYKİ tüm antrenman geçmişini çeker
        public async Task<List<AntrenmanGecmisi>> GetTumGecmisByMonthAsync(int yil, int ay)
        {
            await Init();
            var tumKayitlar = await _db.Table<AntrenmanGecmisi>().ToListAsync();
            // Sadece seçilen yıla ve aya ait olanları filtrele
            return tumKayitlar.Where(g => g.Tarih.Year == yil && g.Tarih.Month == ay).ToList();
        }
        // Yıllık grafikler için o yıla ait tüm antrenman geçmişini çeker
        public async Task<List<AntrenmanGecmisi>> GetTumGecmisByYearAsync(int yil)
        {
            await Init();
            var tumKayitlar = await _db.Table<AntrenmanGecmisi>().ToListAsync();
            return tumKayitlar.Where(g => g.Tarih.Year == yil).ToList();
        }
    }
}