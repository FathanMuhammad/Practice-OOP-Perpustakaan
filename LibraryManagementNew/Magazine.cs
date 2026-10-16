namespace LibraryManagementNew;

/// <summary>
/// Class Magazine (Inheritance dari LibraryItem, Implements IRenewable):
/// Merepresentasikan bahan pustaka berjenis Majalah.
/// Majalah memiliki Edition, bisa diperpanjang sesuai MaxRenewals.
/// </summary>
public class Magazine : LibraryItem, IRenewable
{
    // Property khusus Magazine
    public string Edition { get; set; }

    // Property dari IRenewable: Batas maksimal perpanjangan
    public int MaxRenewals { get; private set; }

    // Constructor
    public Magazine(string code, string title, int publishYear, string edition)
        : base(code, title, publishYear)
    {
        Edition = edition;
        MaxRenewals = 1; // Majalah bisa diperpanjang maksimal 1 kali
    }

    // Override Abstract: Denda per hari untuk Majalah
    public override int FinePerDay()
    {
        return 500; // Denda majalah Rp 500 per hari
    }

    // Implementasi IRenewable: Proses perpanjangan
    public bool Renew()
    {
        if (renewalCount >= MaxRenewals)
        {
            Console.WriteLine($"[Gagal] Majalah [{Code}] sudah mencapai batas perpanjangan ({MaxRenewals}x).");
            return false;
        }

        renewalCount++;
        Console.WriteLine($"[Berhasil] Perpanjangan ke-{renewalCount} untuk Majalah [{Code}] {Title}.");
        return true;
    }

    // Override Virtual: Info() menampilkan informasi tambahan edisi (Edition)
    public override void Info()
    {
        string statusInfo = Status == "Tersedia"
            ? "Tersedia"
            : $"Dipinjam oleh {Borrower?.Name}";
        Console.WriteLine($"[{Code}] {Title} | {GetType().Name} | Edisi: {Edition} | Tahun: {PublishYear} | Status: {statusInfo}");
    }
}
