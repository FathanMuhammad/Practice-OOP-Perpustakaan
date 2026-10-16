namespace LibraryManagementNew;

/// <summary>
/// Class Library:
/// Mengelola bahan pustaka menggunakan Generic Repository<LibraryItem>,
/// mengelola anggota menggunakan List dan Dictionary,
/// serta antrean peminjaman menggunakan Queue.
/// </summary>
public class Library
{
    // Penyimpanan bahan pustaka memakai Repository<T> dengan constraint where T : LibraryItem
    private readonly Repository<LibraryItem> itemRepository = new();

    // Daftar anggota memakai List
    private readonly List<Member> members = new();

    // Pencarian cepat anggota via Dictionary
    private readonly Dictionary<string, Member> membersByNumber = new(StringComparer.OrdinalIgnoreCase);

    // Antrean peminjaman per kode bahan pustaka menggunakan Queue
    private readonly Dictionary<string, Queue<Member>> reservationQueues = new(StringComparer.OrdinalIgnoreCase);

    // Akumulasi total denda harian perpustakaan
    private int todayTotalFine = 0;

    public Library()
    {
        todayTotalFine = 0;
    }

    // Registrasi bahan pustaka ke Repository
    public void AddItem(LibraryItem item)
    {
        itemRepository.Add(item);
    }

    // Registrasi anggota ke List dan Dictionary
    public void AddMember(Member member)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (membersByNumber.ContainsKey(member.MemberNumber))
        {
            throw new InvalidOperationException($"Anggota dengan nomor '{member.MemberNumber}' sudah terdaftar.");
        }

        members.Add(member);
        membersByNumber[member.MemberNumber] = member;
    }

    // Pencarian bahan pustaka lewat kode memakai Dictionary di dalam Repository
    public LibraryItem? FindItem(string code)
    {
        return itemRepository.GetByCode(code);
    }

    // Pencarian anggota lewat nomor anggota memakai Dictionary
    public Member? FindMember(string memberNumber)
    {
        if (string.IsNullOrWhiteSpace(memberNumber)) return null;
        membersByNumber.TryGetValue(memberNumber, out var member);
        return member;
    }

    // Mengambil daftar seluruh anggota (List)
    public IReadOnlyList<Member> GetAllMembers() => members;

    // Meminjam bahan pustaka
    public bool Borrow(Member member, string code)
    {
        ArgumentNullException.ThrowIfNull(member);

        LibraryItem? item = FindItem(code);
        if (item == null)
        {
            Console.WriteLine($"[Gagal] Bahan pustaka dengan kode '{code}' tidak ditemukan.");
            return false;
        }

        // 1. Melebihi batas pinjam melempar BorrowLimitExceededException
        if (!member.CanBorrow())
        {
            throw new BorrowLimitExceededException(member.MemberNumber, member.BorrowLimit);
        }

        // 2. Jika bahan pustaka sedang dipinjam
        if (item.Status != "Tersedia" || item.Borrower != null)
        {
            Console.WriteLine($"[Ditolak] Bahan pustaka [{item.Code}] '{item.Title}' sedang tidak tersedia (sedang dipinjam oleh {item.Borrower?.Name}).");
            return false;
        }

        // Proses peminjaman
        bool success = item.Borrow(member);
        if (success)
        {
            member.AddLoan(item);
            return true;
        }

        return false;
    }

    // Memasukkan anggota ke dalam antrean peminjaman (Queue) untuk bahan pustaka yang sedang dipinjam
    public void EnqueueBorrower(Member member, string code)
    {
        ArgumentNullException.ThrowIfNull(member);

        LibraryItem? item = FindItem(code);
        if (item == null)
        {
            Console.WriteLine($"[Gagal] Bahan pustaka dengan kode '{code}' tidak ditemukan.");
            return;
        }

        if (item.Status == "Tersedia")
        {
            Console.WriteLine($"[Info] Bahan pustaka [{item.Code}] sedang tersedia, silakan langsung dipinjam tanpa antre.");
            return;
        }

        if (item.Borrower == member)
        {
            Console.WriteLine($"[Peringatan] Anda saat ini sedang meminjam bahan pustaka ini.");
            return;
        }

        if (!reservationQueues.TryGetValue(code, out var queue))
        {
            queue = new Queue<Member>();
            reservationQueues[code] = queue;
        }

        if (queue.Contains(member))
        {
            Console.WriteLine($"[Peringatan] Anggota {member.Name} sudah berada di dalam antrean untuk bahan pustaka ini.");
            return;
        }

        queue.Enqueue(member);
        Console.WriteLine($"[Antrean Berhasil] {member.Name} masuk antrean ke-{queue.Count} untuk [{item.Code}] {item.Title}.");
    }

    // Mengembalikan antrean peminjaman untuk bahan pustaka tertentu
    public Queue<Member>? GetReservationQueue(string code)
    {
        reservationQueues.TryGetValue(code, out var queue);
        return queue;
    }

    // Mengembalikan bahan pustaka, menghitung denda, dan otomatis mengalihkan ke antrean terdepan
    public int Return(string code, int lateDays)
    {
        LibraryItem? item = FindItem(code);
        if (item == null)
        {
            Console.WriteLine($"[Gagal] Bahan pustaka dengan kode '{code}' tidak ditemukan.");
            return 0;
        }

        Member? borrower = item.Borrower;
        if (borrower == null)
        {
            Console.WriteLine($"[Gagal] Bahan pustaka [{item.Code}] {item.Title} tidak tercatat sedang dipinjam.");
            return 0;
        }

        // Hitung denda
        int fine = lateDays > 0 ? item.CalculateFine(lateDays, borrower.Premium) : 0;
        todayTotalFine += fine;

        // Lepaskan pinjaman dari anggota yang mengembalikan
        borrower.RemoveLoan(item);
        item.Return();

        Console.WriteLine($"[Berhasil] [{item.Code}] {item.Title} telah berhasil dikembalikan oleh {borrower.Name}.");
        if (fine > 0)
        {
            Console.WriteLine($"[Denda] Terlambat {lateDays} hari. Denda yang dibayarkan: Rp {fine:N0}");
        }

        // OTOMATISASI ANTREAN (Queue):
        // Saat dikembalikan, anggota terdepan di antrian otomatis mendapatkannya
        ProcessNextInQueue(item);

        return fine;
    }

    // Logika pemrosesan antrean terdepan
    private void ProcessNextInQueue(LibraryItem item)
    {
        if (reservationQueues.TryGetValue(item.Code, out var queue) && queue.Count > 0)
        {
            while (queue.Count > 0)
            {
                Member nextMember = queue.Dequeue();

                // Pastikan anggota di antrean masih memiliki kuota pinjam
                if (nextMember.CanBorrow())
                {
                    item.Borrow(nextMember);
                    nextMember.AddLoan(item);
                    Console.WriteLine($"\n>>> [ANTREAN OTOMATIS] Bahan pustaka [{item.Code}] {item.Title} otomatis langsung dipinjamkan ke {nextMember.Name} (No: {nextMember.MemberNumber}) dari antrean terdepan!");
                    break;
                }
                else
                {
                    Console.WriteLine($"\n[Info Antrean] Anggota {nextMember.Name} (No: {nextMember.MemberNumber}) dilewati karena kuota pinjamannya saat ini penuh.");
                }
            }
        }
    }

    // Mencetak seluruh daftar bahan pustaka beserta info antreannya jika ada
    public void PrintItems()
    {
        Console.WriteLine("\n=== DAFTAR BAHAN PUSTAKA PERPUSTAKAAN ===");
        var allItems = itemRepository.GetAll();
        if (allItems.Count == 0)
        {
            Console.WriteLine("Belum ada data bahan pustaka.");
            return;
        }

        foreach (var item in allItems)
        {
            item.Info();
            if (reservationQueues.TryGetValue(item.Code, out var queue) && queue.Count > 0)
            {
                Console.WriteLine($"   └── [Antrean]: {queue.Count} anggota sedang mengantre.");
            }
        }
    }

    // Mencetak laporan harian perpustakaan (Laporan peminjaman & antrean lengkap)
    public void PrintDailyReport()
    {
        int borrowedCount = 0;
        var allItems = itemRepository.GetAll();

        Console.WriteLine("\n==========================================");
        Console.WriteLine("        LAPORAN HARIAN PERPUSTAKAAN       ");
        Console.WriteLine("==========================================");
        Console.WriteLine($"Total Bahan Pustaka  : {itemRepository.Count}");
        Console.WriteLine($"Total Anggota        : {members.Count}");
        Console.WriteLine($"Total Denda Hari Ini : Rp {todayTotalFine:N0}");
        Console.WriteLine("------------------------------------------");
        Console.WriteLine("Rincian Bahan Pustaka yang Sedang Dipinjam:");

        bool anyBorrowed = false;
        foreach (var item in allItems)
        {
            if (item.Borrower != null)
            {
                anyBorrowed = true;
                borrowedCount++;
                string queueInfo = "";
                if (reservationQueues.TryGetValue(item.Code, out var queue) && queue.Count > 0)
                {
                    queueInfo = $" | [Antrean: {queue.Count} orang]";
                }
                Console.WriteLine($" • [{item.Code}] {item.Title} -> Dipinjam oleh {item.Borrower.Name} ({item.Borrower.MemberNumber}){queueInfo}");
            }
        }

        if (!anyBorrowed)
        {
            Console.WriteLine(" • Tidak ada bahan pustaka yang sedang dipinjam.");
        }

        Console.WriteLine("------------------------------------------");
        Console.WriteLine($"Total Sedang Dipinjam: {borrowedCount}");
        Console.WriteLine("==========================================");
    }
}
