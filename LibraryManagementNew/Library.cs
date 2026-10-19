using System.Diagnostics;

namespace LibraryManagementNew;

// Class Library:
// Mengelola bahan pustaka menggunakan Generic Repository<LibraryItem>,
// mengelola anggota menggunakan List dan Dictionary,
// serta antrean peminjaman menggunakan Queue.

// Record untuk menyimpan riwayat denda yang telah dibayar
public record FineRecord(string MemberNumber, string MemberName, string ItemCode, string ItemTitle, int LateDays, int Amount);

public class Library
{
    // Penyimpanan bahan pustaka memakai Repository<T> dengan constraint where T : LibraryItem
    private readonly Repository<LibraryItem> itemRepository = new();

    // Daftar anggota memakai List
    private readonly List<Member> members = new();

    // Pencarian cepat anggota via Dictionary
    private readonly Dictionary<string, Member> membersByNumber = new(StringComparer.OrdinalIgnoreCase);


    // Akumulasi total denda harian perpustakaan
    private int todayTotalFine = 0;

    // Riwayat transaksi denda (untuk keperluan reporting & query LINQ)
    private readonly List<FineRecord> fineHistory = new();

    public IReadOnlyList<FineRecord> GetFineHistory() => fineHistory;

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

    // Mengambil seluruh koleksi bahan pustaka memakai List
    public IReadOnlyList<LibraryItem> GetAllItems() => itemRepository.GetAll();

    // Mengambil daftar seluruh anggota (List)
    public IReadOnlyList<Member> GetAllMembers() => members;

    // Meminjam bahan pustaka
    public bool Borrow(Member member, string code, DateTime? dueDate = null)
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

        // 2. Meminjam bahan pustaka yang sedang dipinjam melempar ItemNotAvailableException (dilempar oleh item.Borrow)
        bool success = item.Borrow(member, dueDate);
        if (success)
        {
            member.AddLoan(item);
            return true;
        }

        return false;
    }

    // Overload Borrow dengan durasi hari
    public bool Borrow(Member member, string code, int loanDays)
    {
        return Borrow(member, code, DateTime.Now.AddDays(loanDays));
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

        // Menambahkan ke antrean pada bahan pustaka
        item.Enqueue(member);
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

        // Jika user tidak memasukkan hari keterlambatan manual (0) namun item tercatat IsOverdue di sistem
        if (lateDays <= 0 && item.IsOverdue)
        {
            lateDays = item.OverdueDays;
            Console.WriteLine($"[Info] Otomatis menghitung keterlambatan berdasarkan tanggal jatuh tempo ({lateDays} hari).");
        }

        // Hitung denda
        int fine = lateDays > 0 ? item.CalculateFine(lateDays, borrower.Premium) : 0;
        todayTotalFine += fine;

        // Lepaskan pinjaman dari anggota yang mengembalikan
        borrower.RemoveLoan(item);

        // Panggil Return pada item (mengembalikan Member terdepan di antrian jika ada)
        Member? nextBorrower = item.Return();

        Console.WriteLine($"[Berhasil] [{item.Code}] {item.Title} telah berhasil dikembalikan oleh {borrower.Name}.");
        if (fine > 0)
        {
            borrower.AddFine(fine);
            fineHistory.Add(new FineRecord(borrower.MemberNumber, borrower.Name, item.Code, item.Title, lateDays, fine));
            Console.WriteLine($"[Denda] Terlambat {lateDays} hari. Denda yang dibayarkan: Rp {fine:N0}");
        }

        // Jika ada anggota dari antrean yang otomatis mendapatkan item:
        if (nextBorrower != null)
        {
            nextBorrower.AddLoan(item);
        }

        return fine;
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
        }
    }

    #region Bagian B - Fitur LINQ (Poin 4 & 5)
    // Poin 4: 3 bahan pustaka paling sering dipinjam
    public void PrintTopBorrowedItems(int count = 3)
    {
        Console.WriteLine($"\n=== TOP {count} BAHAN PUSTAKA PALING SERING DIPINJAM ===");

        var topItems = itemRepository.GetAll()
            .Where(item => item.BorrowCount > 0)
            .OrderByDescending(item => item.BorrowCount)
            .ThenBy(item => item.Title)
            .Take(count)
            .ToList();

        if (topItems.Count == 0)
        {
            Console.WriteLine("Belum ada bahan pustaka yang pernah dipinjam.");
            return;
        }

        for (int i = 0; i < topItems.Count; i++)
        {
            var item = topItems[i];
            string jenis = item switch
            {
                Book => "Buku",
                Magazine => "Majalah",
                Dvd => "DVD",
                _ => item.GetType().Name
            };

            Console.WriteLine($"{i + 1}. [{item.Code}] {item.Title} ({jenis}) - Dipinjam {item.BorrowCount}x");
        }
    }

    // Poin 5: Anggota yang sedang terlambat mengembalikan
    public void PrintOverdueMembers()
    {
        Console.WriteLine("\n=== DAFTAR ANGGOTA YANG SEDANG TERLAMBAT MENGEMBALIKAN ===");

        // Ambil anggota yang memiliki pinjaman lewat jatuh tempo, urutkan dari denda terbanyak
        var overdueMembers = members
            .Where(m => m.BorrowedItems.Any(i => i.IsOverdue))
            .OrderByDescending(m => m.BorrowedItems.Where(i => i.IsOverdue).Sum(i => i.CalculateFine(i.OverdueDays, m.Premium)))
            .ToList();

        if (overdueMembers.Count == 0)
        {
            Console.WriteLine("Tidak ada anggota yang sedang terlambat mengembalikan bahan pustaka.");
            return;
        }

        int no = 1;
        foreach (var member in overdueMembers)
        {
            var lateItems = member.BorrowedItems.Where(i => i.IsOverdue).ToList();
            int totalFine = lateItems.Sum(i => i.CalculateFine(i.OverdueDays, member.Premium));
            string tipe = member.Premium ? "Premium" : "Reguler";

            Console.WriteLine($"\n{no++}. [{member.MemberNumber}] {member.Name} ({tipe})");
            foreach (var item in lateItems)
            {
                int fine = item.CalculateFine(item.OverdueDays, member.Premium);
                Console.WriteLine($"   - [{item.Code}] {item.Title} | Jatuh Tempo: {item.DueDate:dd/MM/yyyy} | Terlambat: {item.OverdueDays} hari | Denda: Rp {fine:N0}");
            }
            Console.WriteLine($"   Total Denda: Rp {totalFine:N0}");
        }
    }
    #endregion

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

        // LINQ Count untuk anggota terlambat
        int overdueMemberCount = members.Count(m => m.BorrowedItems.Any(i => i.IsOverdue));
        Console.WriteLine($"Anggota Terlambat    : {overdueMemberCount} orang");
        Console.WriteLine("------------------------------------------");
        Console.WriteLine("Rincian Bahan Pustaka yang Sedang Dipinjam:");

        bool anyBorrowed = false;
        foreach (var item in allItems)
        {
            if (item.Borrower != null)
            {
                anyBorrowed = true;
                borrowedCount++;
                string queueInfo = item.WaitingCount > 0 ? $" | [Antrean: {item.WaitingCount} orang]" : "";
                string overdueWarning = item.IsOverdue ? $" [TERLAMBAT {item.OverdueDays} HARI]" : "";
                Console.WriteLine($" • [{item.Code}] {item.Title} -> Dipinjam oleh {item.Borrower.Name} ({item.Borrower.MemberNumber}){queueInfo}{overdueWarning}");
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

    #region Bagian B - Fitur Asynchronous
    // Pengingat jatuh tempo besok dikirim bersamaan dengan Task.WhenAll
    public async Task SendDueDateRemindersAsync()
    {
        Console.WriteLine("\n=== PENGIRIMAN PENGINGAT JATUH TEMPO (BESOK) ===");

        DateTime tomorrow = DateTime.Now.Date.AddDays(1);

        // Ambil anggota yang memiliki pinjaman dengan tanggal jatuh tempo besok
        var targetMembers = members
            .Where(m => m.BorrowedItems.Any(item => item.DueDate.HasValue && item.DueDate.Value.Date == tomorrow))
            .Select(m => new
            {
                Member = m,
                Items = m.BorrowedItems.Where(item => item.DueDate.HasValue && item.DueDate.Value.Date == tomorrow).ToList()
            })
            .ToList();

        if (targetMembers.Count == 0)
        {
            Console.WriteLine($"Tidak ada anggota yang memiliki pinjaman jatuh tempo besok ({tomorrow:dd/MM/yyyy}).");
            return;
        }

        Console.WriteLine($"Ditemukan {targetMembers.Count} anggota dengan pinjaman jatuh tempo besok ({tomorrow:dd/MM/yyyy}).");
        Console.WriteLine("Mengirim pengingat secara bersamaan menggunakan Task.WhenAll...\n");

        var stopwatch = Stopwatch.StartNew();

        // Buat list Task pengiriman pengingat
        var reminderTasks = targetMembers.Select(target => SendSingleReminderAsync(target.Member, target.Items));

        // Tunggu semua pengiriman selesai secara bersamaan
        await Task.WhenAll(reminderTasks);

        stopwatch.Stop();
        Console.WriteLine($"\n[Selesai] Semua pengingat berhasil dikirim dalam {stopwatch.ElapsedMilliseconds} ms.");
    }

    private async Task SendSingleReminderAsync(Member member, List<LibraryItem> items)
    {
        string itemTitles = string.Join(", ", items.Select(i => $"[{i.Code}] {i.Title}"));
        Console.WriteLine($"[Kirim] Mengirim pengingat ke {member.Name} ({member.MemberNumber}) untuk {itemTitles}...");

        // Simulasi pengiriman pesan via network
        await Task.Delay(1000);

        Console.WriteLine($"[Terkirim] Notifikasi sukses diterima oleh {member.Name} ({member.MemberNumber}).");
    }

    // Pengecekan seluruh bahan pustaka (Stock Opname) dengan CancellationToken
    public async Task RunStockOpnameAsync(CancellationToken cancellationToken)
    {
        var allItems = itemRepository.GetAll();
        Console.WriteLine($"\nMemulai Stock Opname untuk total {allItems.Count} bahan pustaka...");

        int count = 0;
        for (int i = 0; i < allItems.Count; i++)
        {
            // Cek pembatalan sebelum memproses item
            cancellationToken.ThrowIfCancellationRequested();

            var item = allItems[i];
            Console.WriteLine($"[{i + 1}/{allItems.Count}] Memeriksa: [{item.Code}] {item.Title} | Status: {item.Status}");
            count++;

            // Simulasi waktu pengecekan fisik per item
            await Task.Delay(300, cancellationToken);
        }

        Console.WriteLine($"\n[Selesai] Stock Opname selesai! Seluruh {count} bahan pustaka berhasil diverifikasi.");
    }
    #endregion
}
