namespace LibraryManagementNew;

// Abstract Class LibraryItem:
// Class induk untuk semua jenis bahan pustaka (Book, Magazine, Dvd).
// Menerapkan konsep Abstraction, Encapsulation, dan Polymorphism.

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

    // Fitur Tambahan (LINQ & Manajemen Jatuh Tempo)
    public int BorrowCount { get; private set; } = 0;       // Berapa kali bahan pustaka ini pernah dipinjam
    public DateTime? BorrowDate { get; private set; }       // Waktu saat dipinjam
    public DateTime? DueDate { get; private set; }          // Batas akhir pengembalian (jatuh tempo)

    // Property kalkulasi keterlambatan
    public bool IsOverdue => Status == "Dipinjam" && DueDate.HasValue && DateTime.Now > DueDate.Value;
    public int OverdueDays => IsOverdue && DueDate.HasValue ? Math.Max(1, (DateTime.Now.Date - DueDate.Value.Date).Days) : 0;

    // Queue (Collection FIFO): Antrian anggota yang ingin meminjam saat item sedang dipinjam
    private Queue<Member> _waitingQueue = new Queue<Member>();

    // Property: Jumlah anggota yang sedang mengantri
    public int WaitingCount => _waitingQueue.Count;

    // Constructor (Protected karena abstract class hanya dipanggil oleh child class)
    protected LibraryItem(string code, string title, int publishYear)
    {
        Code = code;
        Title = title;
        Status = "Tersedia";
        Borrower = null;
        renewalCount = 0;
        BorrowCount = 0;
        BorrowDate = null;
        DueDate = null;

        // Validasi tahun terbit: throw exception jika di luar batas (menggantikan pesan validasi)
        if (!IsValidYear(publishYear))
        {
            throw new ArgumentOutOfRangeException(nameof(publishYear),
                $"Tahun terbit ({publishYear}) tidak valid! Harus antara {MIN_YEAR} sampai {DateTime.Now.Year}.");
        }

        PublishYear = publishYear;

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

    // Method: Tambah frekuensi peminjaman manual (berguna untuk seeding data riwayat)
    public void AddBorrowCount(int count)
    {
        if (count > 0)
        {
            BorrowCount += count;
        }
    }

    // Method: Perpanjang tanggal jatuh tempo
    public void ExtendDueDate(int days = 7)
    {
        if (DueDate.HasValue)
        {
            DueDate = DueDate.Value.AddDays(days);
            Console.WriteLine($"[Info] Tanggal jatuh tempo diperpanjang hingga: {DueDate.Value:dd/MM/yyyy}");
        }
    }

    // Method: Proses peminjaman bahan pustaka
    // Melempar ItemNotAvailableException jika item sedang dipinjam (throw)
    public bool Borrow(Member member, DateTime? dueDate = null)
    {
        if (Status != "Tersedia")
        {
            throw new ItemNotAvailableException(Code,
                $"Bahan pustaka [{Code}] {Title} sedang dipinjam oleh {Borrower?.Name}.");
        }

        Status = "Dipinjam";
        Borrower = member;
        renewalCount = 0;
        BorrowCount++;
        BorrowDate = DateTime.Now;
        DueDate = dueDate ?? DateTime.Now.AddDays(7); // Default 7 hari masa pinjam

        Console.WriteLine($"[Berhasil] {member.Name} meminjam [{Code}] {Title} (Jatuh tempo: {DueDate.Value:dd/MM/yyyy}).");
        return true;
    }

    // Method: Menambahkan anggota ke antrian pemesanan (Queue)
    public void Enqueue(Member member)
    {
        _waitingQueue.Enqueue(member);
        Console.WriteLine($"[Antrian] {member.Name} masuk antrian untuk [{Code}] {Title}. Posisi antrian: {_waitingQueue.Count}");
    }

    // Method: Proses pengembalian bahan pustaka
    // Jika ada anggota di antrian (Queue), otomatis pinjamkan ke anggota terdepan (Dequeue)
    public Member? Return()
    {
        Console.WriteLine($"[Info] Bahan pustaka [{Code}] {Title} telah dikembalikan.");
        Status = "Tersedia";
        Borrower = null;
        BorrowDate = null;
        DueDate = null;
        renewalCount = 0;

        // Cek antrian: jika ada anggota yang menunggu, otomatis pinjamkan
        if (_waitingQueue.Count > 0)
        {
            Member nextMember = _waitingQueue.Dequeue();
            Console.WriteLine($"[Antrian] {nextMember.Name} (antrian terdepan) otomatis mendapatkan [{Code}] {Title}.");
            Borrow(nextMember);
            return nextMember; // Kembalikan member yang otomatis mendapat item
        }

        return null; // Tidak ada yang mengantri
    }

    // Virtual Method: Cetak informasi bahan pustaka (bisa di-override oleh child class)
    public virtual void Info()
    {
        string statusInfo;
        if (Status == "Tersedia")
        {
            statusInfo = "Tersedia";
        }
        else
        {
            string overdueText = IsOverdue ? $" [TERLAMBAT {OverdueDays} HARI]" : "";
            string dueText = DueDate.HasValue ? $" (Jatuh Tempo: {DueDate.Value:dd/MM/yyyy}{overdueText})" : "";
            statusInfo = $"Dipinjam oleh {Borrower?.Name}{dueText}";
        }

        string detailInfo = this switch
        {
            Book b => $" | Pengarang: {b.Author}",
            Magazine m => $" | Edisi: {m.Edition}",
            _ => ""
        };

        string queueInfo = _waitingQueue.Count > 0
            ? $" | Antrian: {_waitingQueue.Count} orang"
            : "";

        Console.WriteLine($"[{Code}] {Title} | {GetType().Name}{detailInfo} | Tahun: {PublishYear} | Dipinjam: {BorrowCount}x | Status: {statusInfo}{queueInfo}");
    }
}
