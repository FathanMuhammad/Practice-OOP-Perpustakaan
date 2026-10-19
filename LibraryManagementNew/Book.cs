namespace LibraryManagementNew;

// Class Book (Inheritance dari LibraryItem, Implements IRenewable):
// Merepresentasikan bahan pustaka berjenis Buku.
// Buku memiliki Author, bisa diperpanjang sesuai MaxRenewals.

public class Book : LibraryItem, IRenewable
{
    // Property khusus Book
    public string Author { get; set; }

    // Property dari IRenewable: Batas maksimal perpanjangan
    public int MaxRenewals { get; private set; }

    // Constructor
    public Book(string code, string title, int publishYear, string author)
        : base(code, title, publishYear)
    {
        Author = author;
        MaxRenewals = 2; // Buku bisa diperpanjang maksimal 2 kali
    }

    // Override Abstract: Denda per hari untuk Buku
    public override int FinePerDay()
    {
        return 1000; // Denda buku Rp 1.000 per hari
    }

    // Implementasi IRenewable: Proses perpanjangan
    public bool Renew()
    {
        if (renewalCount >= MaxRenewals)
        {
            Console.WriteLine($"[Gagal] Buku [{Code}] sudah mencapai batas perpanjangan ({MaxRenewals}x).");
            return false;
        }

        renewalCount++;
        Console.WriteLine($"[Berhasil] Perpanjangan ke-{renewalCount} untuk Buku [{Code}] {Title}.");
        return true;
    }
    
}
