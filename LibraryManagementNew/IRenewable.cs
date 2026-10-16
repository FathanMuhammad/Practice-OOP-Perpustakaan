namespace LibraryManagementNew;

/// <summary>
/// Interface IRenewable:
/// Mendefinisikan kontrak untuk bahan pustaka yang dapat diperpanjang masa pinjamnya.
/// Diimplementasikan oleh Book dan Magazine (DVD tidak bisa diperpanjang).
/// </summary>
public interface IRenewable
{
    // Property: Batas maksimal perpanjangan
    int MaxRenewals { get; }

    // Method: Proses perpanjangan, return true jika berhasil
    bool Renew();
}
