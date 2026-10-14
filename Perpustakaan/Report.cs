using System;

namespace LibraryManagement;

/*
 * REPORT CLASS:
 * Bertanggung jawab eksklusif untuk menampilkan laporan, katalog bahan pustaka,
 * daftar anggota terdaftar, serta rekapitulasi operasional harian.
 */
internal static class Report
{
    // Case 1: Tampilkan Daftar Bahan Pustaka
    public static void ShowAllItems(LibrarySystem system)
    {
        system.PrintAllItems();
    }

    // Case 2: Tampilkan Daftar Anggota
    public static void ShowAllMembers(LibrarySystem system)
    {
        system.PrintAllMembers();
    }

    // Case 6: Tampilkan Laporan Harian
    public static void ShowDailyReport(LibrarySystem system)
    {
        system.PrintDailyReport();
    }
}
