namespace LibraryManagementNew;

public class Library
{
    // Attributes sesuai Class Diagram
    private LibraryItem[] items;
    private Member[] members;
    private int todayTotalFine;

    // Helper counters untuk manajemen array murni
    private int itemCount;
    private int memberCount;

    // Constructor
    public Library(int itemCapacity = 100, int memberCapacity = 50)
    {
        items = new LibraryItem[itemCapacity];
        members = new Member[memberCapacity];
        todayTotalFine = 0;
        itemCount = 0;
        memberCount = 0;
    }

    // Helper method untuk menambahkan item dan member ke dalam sistem perpustakaan
    public void AddItem(LibraryItem item)
    {
        if (itemCount < items.Length)
        {
            items[itemCount] = item;
            itemCount++;
        }
        else
        {
            Console.WriteLine("[Error] Kapasitas penyimpanan bahan pustaka penuh.");
        }
    }

    public void AddMember(Member member)
    {
        if (memberCount < members.Length)
        {
            members[memberCount] = member;
            memberCount++;
        }
        else
        {
            Console.WriteLine("[Error] Kapasitas pendaftaran anggota penuh.");
        }
    }

    // Mencari bahan pustaka berdasarkan kode
    public LibraryItem? FindItem(string code)
    {
        for (int i = 0; i < itemCount; i++)
        {
            if (items[i] != null && items[i].Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return items[i];
            }
        }
        return null;
    }

    // Memproses peminjaman bahan pustaka oleh anggota
    public bool Borrow(Member member, string code)
    {
        if (member == null)
        {
            Console.WriteLine("[Gagal] Data anggota tidak valid.");
            return false;
        }

        LibraryItem? item = FindItem(code);
        if (item == null)
        {
            Console.WriteLine($"[Gagal] Bahan pustaka dengan kode '{code}' tidak ditemukan.");
            return false;
        }

        if (!member.CanBorrow())
        {
            Console.WriteLine($"[Ditolak] Kuota peminjaman untuk {member.Name} sudah maksimal ({member.BorrowLimit} item).");
            return false;
        }

        // Panggil method Borrow pada LibraryItem (akan mengubah status dan Borrower)
        bool success = item.Borrow(member);
        if (!success)
        {
            Console.WriteLine($"[Ditolak] Bahan pustaka [{item.Code}] {item.Title} sedang tidak tersedia.");
            return false;
        }

        // Tambahkan ke daftar pinjaman aktif anggota
        member.AddLoan(item);
        Console.WriteLine($"[Berhasil] {member.Name} berhasil meminjam [{item.Code}] {item.Title}.");
        return true;
    }

    // Memproses pengembalian bahan pustaka dan mengembalikan denda yang harus dibayar
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

        // Hitung denda melalui method CalculateFine pada LibraryItem
        int fine = lateDays > 0 ? item.CalculateFine(lateDays, borrower.Premium) : 0;
        todayTotalFine += fine;

        // Lepas relasi peminjaman pada member dan item
        borrower.RemoveLoan(item);
        item.Return();

        Console.WriteLine($"[Berhasil] [{item.Code}] {item.Title} telah berhasil dikembalikan oleh {borrower.Name}.");
        if (fine > 0)
        {
            Console.WriteLine($"[Denda] Terlambat {lateDays} hari. Denda yang dibayarkan: Rp {fine:N0}");
        }

        return fine;
    }

    // Mencetak seluruh daftar bahan pustaka menggunakan method Info() pada LibraryItem
    public void PrintItems()
    {
        Console.WriteLine("\n=== DAFTAR BAHAN PUSTAKA PERPUSTAKAAN ===");
        if (itemCount == 0)
        {
            Console.WriteLine("Belum ada data bahan pustaka.");
            return;
        }

        for (int i = 0; i < itemCount; i++)
        {
            items[i]?.Info();
        }
    }

    // Mencetak laporan harian perpustakaan
    public void PrintDailyReport()
    {
        int borrowedCount = 0;
        for (int i = 0; i < itemCount; i++)
        {
            if (items[i] != null && items[i].Borrower != null)
            {
                borrowedCount++;
            }
        }

        Console.WriteLine("\n==========================================");
        Console.WriteLine("        LAPORAN HARIAN PERPUSTAKAAN       ");
        Console.WriteLine("==========================================");
        Console.WriteLine($"Total Bahan Pustaka  : {itemCount}");
        Console.WriteLine($"Total Sedang Dipinjam: {borrowedCount}");
        Console.WriteLine($"Total Denda Hari Ini : Rp {todayTotalFine:N0}");
        Console.WriteLine("==========================================");
    }
}
