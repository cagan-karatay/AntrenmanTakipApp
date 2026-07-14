using SQLite;

namespace AntrenmanTakipApp.Models
{
    public class Hareket
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Bolge { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public bool AktifMi { get; set; } = true;
    }
}