namespace LibraryManagementNew;

// Class Member:
// Merepresentasikan anggota perpustakaan (Reguler / Premium).
// Menyimpan daftar pinjaman aktif dan riwayat menggunakan List.

public class Member
{
    // Properties sesuai Class Diagram
    public string MemberNumber { get; private set; }
    public string Name { get; set; }
    public bool Premium { get; set; }

    // Read-only Property: Kuota pinjam (Premium: 4, Reguler: 2)
    public int BorrowLimit => Premium ? 4 : 2;

    // Koleksi memakai List sesuai kebutuhan arsitektur baru
    private readonly List<LibraryItem> borrowedItems;
    private readonly List<string> history;

    public IReadOnlyList<LibraryItem> BorrowedItems => borrowedItems;
    public int BorrowedCount => borrowedItems.Count;

    // Constructor
    public Member(string memberNumber, string name, bool premium)
    {
        MemberNumber = memberNumber;
        Name = name;
        Premium = premium;

        borrowedItems = new List<LibraryItem>();
        history = new List<string>();
    }

    // Mengecek apakah member masih memiliki kuota pinjam
    public bool CanBorrow()
    {
        return borrowedItems.Count < BorrowLimit;
    }

    // Menambahkan item ke daftar pinjaman aktif dan riwayat
    public void AddLoan(LibraryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Jika melebihi batas pinjam, lempar BorrowLimitExceededException
        if (!CanBorrow())
        {
            throw new BorrowLimitExceededException(MemberNumber, BorrowLimit);
        }

        borrowedItems.Add(item);
        history.Add($"[{item.Code}] {item.Title}");
    }

    // Menghapus item dari daftar pinjaman aktif saat dikembalikan
    public void RemoveLoan(LibraryItem item)
    {
        if (item == null) return;
        borrowedItems.Remove(item);
    }

    // Mencetak riwayat peminjaman anggota
    public void PrintHistory()
    {
        Console.WriteLine($"\nRiwayat Peminjaman : {Name} ({MemberNumber}) [{(Premium ? "Premium" : "Reguler")}]");
        if (history.Count == 0)
        {
            Console.WriteLine("- Belum ada riwayat peminjaman.");
            return;
        }

        for (int i = 0; i < history.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {history[i]}");
        }
    }
}
