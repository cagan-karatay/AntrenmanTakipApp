using SQLite;
using System;

namespace AntrenmanTakipApp.Models
{
    public class AntrenmanGecmisi
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int HareketId { get; set; }
        public string Bolge { get; set; } = string.Empty;
        public int SetSayisi { get; set; }
        public int Tekrar { get; set; }
        public int Agirlik { get; set; }
        public DateTime Tarih { get; set; }
    }
}