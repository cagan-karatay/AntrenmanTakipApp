using SQLite;

namespace AntrenmanTakipApp.Models
{

    public class Hareket
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Bolge { get; set; }


        public string Ad { get; set; }
    }
}