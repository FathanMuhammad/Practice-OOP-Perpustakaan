namespace LibraryManagementNew;

public class Member
{
    // Properties sesuai Class Diagram
    public string MemberNumber { get; private set; }
    public string Name { get; set; }
    public bool Premium { get; set; }

    // Read-only Property: Kuota pinjam (Premium: 4, Reguler: 2)
    public int BorrowLimit => Premium ? 4 : 2;

    // Attributes sesuai Class Diagram
    private LibraryItem[] borrowedItems;
    private int borrowedCount;

    private string[] history;
    private int historyCount;

    // Constructor
    public Member(string memberNumber, string name, bool premium)
    {
        MemberNumber = memberNumber;
        Name = name;
        Premium = premium;

        // Sesuai diagram: borrowedItems : LibraryItem[4]
        borrowedItems = new LibraryItem[4];
        borrowedCount = 0;

        // Sesuai diagram: history : string[10]
        history = new string[10];
        historyCount = 0;
    }

    // Mengecek apakah member masih memiliki kuota pinjam
    public bool CanBorrow()
    {
        return borrowedCount < BorrowLimit;
    }

    // Menambahkan item ke daftar pinjaman aktif dan riwayat
    public void AddLoan(LibraryItem item)
    {
        if (item == null) return;

        if (!CanBorrow())
        {
            Console.WriteLine($"[Peringatan] Batas maksimal peminjaman anggota {Name} ({BorrowLimit} item) telah tercapai.");
            return;
        }

        borrowedItems[borrowedCount] = item;
        borrowedCount++;

        // Simpan ke riwayat peminjaman jika slot history masih tersedia
        if (historyCount < history.Length)
        {
            history[historyCount] = $"[{item.Code}] {item.Title}";
            historyCount++;
        }
    }

    // Menghapus item dari daftar pinjaman aktif saat dikembalikan (Array Shifting)
    public void RemoveLoan(LibraryItem item)
    {
        if (item == null || borrowedCount == 0) return;

        int foundIndex = -1;
        for (int i = 0; i < borrowedCount; i++)
        {
            if (borrowedItems[i] == item || borrowedItems[i]?.Code == item.Code)
            {
                foundIndex = i;
                break;
            }
        }

        if (foundIndex == -1) return;

        // Geser elemen array ke kiri untuk menutup celah
        for (int i = foundIndex; i < borrowedCount - 1; i++)
        {
            borrowedItems[i] = borrowedItems[i + 1];
        }

        borrowedItems[borrowedCount - 1] = null!;
        borrowedCount--;
    }

    // Mencetak riwayat peminjaman anggota
    public void PrintHistory()
    {
        Console.WriteLine($"\nRiwayat Peminjaman : {Name} ({MemberNumber}) [{(Premium ? "Premium" : "Reguler")}]");
        if (historyCount == 0)
        {
            Console.WriteLine("- Belum ada riwayat peminjaman.");
            return;
        }

        for (int i = 0; i < historyCount; i++)
        {
            Console.WriteLine($"{i + 1}. {history[i]}");
        }
    }
}
