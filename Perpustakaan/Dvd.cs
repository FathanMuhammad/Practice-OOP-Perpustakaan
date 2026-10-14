using LibraryManagement;

namespace LibraryManagement
{
    public class Dvd : LibraryItem
    {
        public Dvd(string code, string title, int publicationYear)
            : base(code, title, publicationYear)
        {
        }

        public override string GetItemType()
        {
            return "DVD";
        }

        public override int GetDailyFine()
        {
            return 3000; // Denda Dvd 3.000 per hari
        }

        public override int GetMaxRenewals()
        {
            return 0; // Dvd tidak bisa diperpanjang
        }
    }
}