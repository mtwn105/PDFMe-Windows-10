using SQLite.Net.Attributes;

namespace PDF_Me_Universal
{
    [Table("DownloadList")]

    public class Downloads
    {
        [AutoIncrement, PrimaryKey]
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
        public string Size { get; set; }

    }
}
