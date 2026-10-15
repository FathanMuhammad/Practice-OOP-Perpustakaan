using System;

namespace LibraryManagement;

/*
 * LIBRARY APP CLASS:
 * Berperan sebagai Application Controller / Orchestrator.
 * Bertanggung jawab mengelola siklus hidup aplikasi (lifecycle),
 * inisialisasi data awal, rendering menu, dan mendelegasikan aksi
 * ke class-class spesifik (Report, LoanFeature, ItemFeature).
 */
internal class LibraryApp
{
    private readonly LibrarySystem system;
    private bool isRunning;

    public LibraryApp()
    {
        system = new LibrarySystem();
        isRunning = true;
    }

    public void Run()
    {
        SeedInitialData();

        while (isRunning)
        {
            DisplayMenu();

            int choice = Validation.ReadIntInRange("Pilih menu (0-8): ", 0, 8);
            Console.WriteLine();

            switch (choice)
            {
                case 1:
                    Report.ShowAllItems(system);
                    break;

                case 2:
                    Report.ShowAllMembers(system);
                    break;

                case 3:
                    LoanFeature.ProcessBorrow(system);
                    break;

                case 4:
                    LoanFeature.ProcessRenew(system);
                    break;

                case 5:
                    LoanFeature.ProcessReturn(system);
                    break;

                case 6:
                    Report.ShowDailyReport(system);
                    break;

                case 7:
                    LoanFeature.ProcessMemberHistory(system);
                    break;

                case 8:
                    ItemFeature.ProcessAddNewItem(system);
                    break;

                case 0:
                    isRunning = false;
                    Console.WriteLine("Terima kasih telah menggunakan Sistem Perpustakaan.");
                    break;
            }
        }
    }

    private void DisplayMenu()
    {
        Console.WriteLine("\n==========================================");
        Console.WriteLine("    SISTEM MANAJEMEN PERPUSTAKAAN KAMPUS  ");
        Console.WriteLine("==========================================");
        Console.WriteLine("1. Tampilkan Daftar Bahan Pustaka");
        Console.WriteLine("2. Tampilkan Daftar Anggota");
        Console.WriteLine("3. Pinjam Bahan Pustaka");
        Console.WriteLine("4. Perpanjang Peminjaman");
        Console.WriteLine("5. Kembalikan Bahan Pustaka");
        Console.WriteLine("6. Tampilkan Laporan Harian");
        Console.WriteLine("7. Tampilkan Riwayat Peminjaman Anggota");
        Console.WriteLine("8. Tambah Bahan Pustaka Baru");
        Console.WriteLine("0. Keluar Program");
    }

    private void SeedInitialData()
    {
        try
        {
            // Bahan Pustaka Awal (Bagian A)
            system.AddItem(new Book("1", "Pemrograman C# untuk Pemula", 2023));
            system.AddItem(new Magazine("2", "Majalah Info Teknologi", 2024));
            system.AddItem(new Dvd("3", "Tutorial Desain Sistem OOP", 2022));

            // Anggota Awal (Bagian B)
            system.AddMember(new Member("1", "Andi Reguler", false));
            system.AddMember(new Member("2", "Siti Premium", true));
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Info] Gagal memuat data dummy awal: " + ex.Message);
        }
    }
}
