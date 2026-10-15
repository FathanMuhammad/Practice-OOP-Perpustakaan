using System;

namespace LibraryManagement;

/*
 * LOAN FEATURE CLASS:
 * Bertanggung jawab eksklusif untuk menangani alur transaksi peminjaman,
 * perpanjangan waktu pinjam, pengembalian bahan pustaka, dan riwayat peminjaman anggota.
 */
internal static class LoanFeature
{
    // Case 3: Pinjam Bahan Pustaka
    public static void ProcessBorrow(LibrarySystem system)
    {
        Console.WriteLine("\n--- PEMINJAMAN BAHAN PUSTAKA ---");
        string memberId = Validation.ReadNonEmptyString("Masukkan Nomor Anggota (contoh: 1): ");
        string itemCode = Validation.ReadNonEmptyString("Masukkan Kode Bahan Pustaka (contoh: 1, B001): ");
        int loanDays = Validation.ReadPositiveInt("Masukkan masa pinjam (hari): ");

        system.BorrowItem(memberId, itemCode, loanDays);
    }

    // Case 4: Perpanjang Peminjaman
    public static void ProcessRenew(LibrarySystem system)
    {
        Console.WriteLine("\n--- PERPANJANG PEMINJAMAN ---");
        string itemCode = Validation.ReadNonEmptyString("Masukkan Kode Bahan Pustaka yang akan diperpanjang: ");
        int extraDays = Validation.ReadPositiveInt("Perpanjang berapa hari?: ");

        system.RenewItem(itemCode, extraDays);
    }

    // Case 5: Kembalikan Bahan Pustaka
    public static void ProcessReturn(LibrarySystem system)
    {
        Console.WriteLine("\n--- PENGEMBALIAN BAHAN PUSTAKA ---");
        string memberId = Validation.ReadNonEmptyString("Masukkan Nomor Anggota: ");
        string itemCode = Validation.ReadNonEmptyString("Masukkan Kode Bahan Pustaka: ");
        int returnDay = Validation.ReadPositiveInt("Dikembalikan pada hari ke berapa?: ");

        system.ReturnItem(memberId, itemCode, returnDay);
    }

    // Case 7: Tampilkan Riwayat Peminjaman Anggota
    public static void ProcessMemberHistory(LibrarySystem system)
    {
        Console.WriteLine("\n--- RIWAYAT PEMINJAMAN ANGGOTA ---");
        string memberId = Validation.ReadNonEmptyString("Masukkan Nomor Anggota yang ingin dilihat riwayatnya: ");

        Member? member = system.FindMember(memberId);
        if (member != null)
        {
            member.ShowLoanHistory();
        }
        else
        {
            Console.WriteLine("[Gagal] Anggota dengan ID tersebut tidak ditemukan.");
        }
    }
}
