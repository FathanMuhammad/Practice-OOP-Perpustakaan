using LibraryManagement;

namespace LibraryManagement
{
    public class Book : LibraryItem
    {
        public Book(string code, string title, int publicationYear)
            : base(code, title, publicationYear)
        {
        }

        public override string GetItemType()
        {
            return "Buku";
        }

        public override int GetDailyFine()
        {
            return 1000; // Denda buku 1.000 per hari
        }

        public override int GetMaxRenewals()
        {
            return 2; // Buku bisa diperpanjang maksimal 2 kali
        }
    }
}