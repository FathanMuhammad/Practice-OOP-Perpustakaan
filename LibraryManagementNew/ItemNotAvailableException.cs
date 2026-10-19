namespace LibraryManagementNew;

// Custom Exception: Dilempar ketika bahan pustaka yang ingin dipinjam sedang tidak tersedia (sedang dipinjam anggota lain).
// Turunan dari Exception (Inheritance).

public class ItemNotAvailableException : Exception
{
    // Property tambahan: Kode bahan pustaka yang gagal dipinjam
    //public string ItemCode { get; }

    // Constructor: Menerima kode item dan pesan error, lalu diteruskan ke base Exception
    public ItemNotAvailableException(string itemCode, string message)
        : base(message)
    {
        //ItemCode = itemCode;
    }
}
