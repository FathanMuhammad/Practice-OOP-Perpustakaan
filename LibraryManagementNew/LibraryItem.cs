namespace LibraryManagementNew;

/// <summary>
/// Abstract Class LibraryItem:
/// Class induk untuk semua jenis bahan pustaka (Book, Magazine, Dvd).
/// Menerapkan konsep Abstraction, Encapsulation, dan Polymorphism.
/// </summary>
public abstract class LibraryItem
{
    // Static Field: Menghitung total item yang pernah dibuat (garis bawah = static)
    public static int TotalCreated { get; private set; } = 0;

    // Konstanta validasi tahun
    private static int MIN_YEAR = 1900;

    // C# Properties (Encapsulation)
    public string Code { get; private set; }        // Kode unik bahan pustaka (private set)
    public string Title { get; set; }                // Judul bahan pustaka
    public int PublishYear { get; private set; }     // Tahun terbit (validasi 1900 s/d tahun ini)
    public string Status { get; private set; }       // Status: "Tersedia" atau "Dipinjam" (private set)
    public Member? Borrower { get; private set; }    // Referensi ke Member yang meminjam (private set)
    protected int renewalCount;                      // Jumlah perpanjangan yang sudah dilakukan (protected)

    // Constructor (Protected karena abstract class hanya dipanggil oleh child class)
    protected LibraryItem(string code, string title, int publishYear)
    {
        Code = code;
        Title = title;
        Status = "Tersedia";
        Borrower = null;
        renewalCount = 0;

        // Validasi tahun terbit saat objek dibuat
        if (IsValidYear(publishYear))
        {
            PublishYear = publishYear;
        }
        else
        {
            Console.WriteLine($"[Validasi Gagal] Tahun terbit ({publishYear}) tidak valid! Harus antara {MIN_YEAR} sampai {DateTime.Now.Year}.");
            PublishYear = DateTime.Now.Year; // Default fallback
        }

        // Increment total item yang pernah dibuat
        TotalCreated++;
    }

    // Static Method: Validasi tahun terbit (1900 s/d tahun ini)
    public static bool IsValidYear(int year)
    {
        int currentYear = DateTime.Now.Year;
        return year >= MIN_YEAR && year <= currentYear;
    }

    // Abstract Method: Denda per hari (wajib di-override oleh child class)
    // Miring di class diagram = abstract
    public abstract int FinePerDay();

    // Method: Hitung total denda berdasarkan jumlah hari terlambat
    public int CalculateFine(int days)
    {
        if (days <= 0) return 0;
        return days * FinePerDay();
    }

    // Method Overload: Hitung denda dengan diskon premium (50%)
    public int CalculateFine(int days, bool premium)
    {
        int baseFine = CalculateFine(days);
        if (premium)
        {
            return baseFine / 2; // Diskon 50% untuk member premium
        }
        return baseFine;
    }

    // Method: Proses peminjaman bahan pustaka
    public bool Borrow(Member member)
    {
        if (Status != "Tersedia")
        {
            Console.WriteLine($"[Gagal] Bahan pustaka [{Code}] sedang dipinjam oleh {Borrower?.Name}.");
            return false;
        }

        Status = "Dipinjam";
        Borrower = member;
        renewalCount = 0;
        Console.WriteLine($"[Berhasil] {member.Name} meminjam [{Code}] {Title}.");
        return true;
    }

    // Method: Proses pengembalian bahan pustaka
    public void Return()
    {
        Console.WriteLine($"[Info] Bahan pustaka [{Code}] {Title} telah dikembalikan.");
        Status = "Tersedia";
        Borrower = null;
        renewalCount = 0;
    }

    // Virtual Method: Cetak informasi bahan pustaka (bisa di-override oleh child class)
    public virtual void Info()
    {
        string statusInfo = Status == "Tersedia"
            ? "Tersedia"
            : $"Dipinjam oleh {Borrower?.Name}";
        Console.WriteLine($"[{Code}] {Title} | Tahun: {PublishYear} | Status: {statusInfo}");
    }
}
