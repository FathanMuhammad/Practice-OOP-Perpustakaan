namespace LibraryManagementNew;

/// <summary>
/// Class Dvd (Inheritance dari LibraryItem):
/// Merepresentasikan bahan pustaka berjenis DVD.
/// DVD memiliki DurationMinutes, TIDAK mengimplementasikan IRenewable (tidak bisa diperpanjang).
/// DVD meng-override Info() untuk menampilkan durasi.
/// </summary>
public class Dvd : LibraryItem
{
    // Property khusus Dvd
    public int DurationMinutes { get; set; }

    // Constructor
    public Dvd(string code, string title, int publishYear, int durationMinutes)
        : base(code, title, publishYear)
    {
        DurationMinutes = durationMinutes;
    }

    // Override Abstract: Denda per hari untuk DVD
    public override int FinePerDay()
    {
        return 3000; // Denda DVD Rp 3.000 per hari
    }

    // Override Virtual: Info() menampilkan informasi tambahan durasi
    public override void Info()
    {
        string statusInfo = Status == "Tersedia"
            ? "Tersedia"
            : $"Dipinjam oleh {Borrower?.Name}";
        Console.WriteLine($"[{Code}] {Title} | Durasi: {DurationMinutes} menit | Tahun: {PublishYear} | Status: {statusInfo}");
    }
}
