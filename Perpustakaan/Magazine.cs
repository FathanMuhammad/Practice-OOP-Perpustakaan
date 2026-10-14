using LibraryManagement;

namespace LibraryManagement;

public class Magazine : LibraryItem
{
    public Magazine(string code, string title, int publicationYear)
                : base(code, title, publicationYear)
    {
    }

    public override string GetItemType()
    {
        return "Majalah";
    }

    public override int GetDailyFine()
    {
        return 500; // Denda majalah 500 per hari
    }

    public override int GetMaxRenewals()
    {
        return 1; // Majalah bisa diperpanjang maksimal 1 kali
    }
}
