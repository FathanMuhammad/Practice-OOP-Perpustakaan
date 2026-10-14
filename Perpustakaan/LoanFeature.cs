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
        string idPinjam = Validation.ReadNonEmptyString("Masukkan Nomor Anggota (contoh: 1, M01): ");
        string kodePinjam = Validation.ReadNonEmptyString("Masukkan Kode Bahan Pustaka (contoh: 1, B001): ");

        system.BorrowItem(idPinjam, kodePinjam);
    }

    // Case 4: Perpanjang Peminjaman
    public static void ProcessRenew(LibrarySystem system)
    {
        Console.WriteLine("\n--- PERPANJANG PEMINJAMAN ---");
        string kodePerpanjang = Validation.ReadNonEmptyString("Masukkan Kode Bahan Pustaka yang akan diperpanjang: ");

        system.RenewItem(kodePerpanjang);
    }

    // Case 5: Kembalikan Bahan Pustaka
    public static void ProcessReturn(LibrarySystem system)
    {
        Console.WriteLine("\n--- PENGEMBALIAN BAHAN PUSTAKA ---");
        string idKembali = Validation.ReadNonEmptyString("Masukkan Nomor Anggota: ");
        string kodeKembali = Validation.ReadNonEmptyString("Masukkan Kode Bahan Pustaka: ");
        int hariTelat = Validation.ReadNonNegativeInt("Masukkan jumlah hari keterlambatan: ");

        system.ReturnItem(idKembali, kodeKembali, hariTelat);
    }

    // Case 7: Tampilkan Riwayat Peminjaman Anggota
    public static void ProcessMemberHistory(LibrarySystem system)
    {
        Console.WriteLine("\n--- RIWAYAT PEMINJAMAN ANGGOTA ---");
        string idRiwayat = Validation.ReadNonEmptyString("Masukkan Nomor Anggota yang ingin dilihat riwayatnya: ");

        Member member = system.FindMember(idRiwayat);
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
