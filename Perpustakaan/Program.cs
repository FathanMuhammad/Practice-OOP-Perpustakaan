using System;

namespace LibraryManagement;

internal class Program
{
    static void Main(string[] args)
    {
        LibrarySystem system = new LibrarySystem();
        bool isRunning = true;

        // 1. Masukkan Data Dummy (Bahan Pustaka & Anggota) untuk Pengujian Awal
        try
        {
            // Bahan Pustaka (Bagian A)
            system.AddItem(new Book("1", "Pemrograman C# untuk Pemula", 2023));
            system.AddItem(new Magazine("2", "Majalah Info Teknologi", 2024));
            system.AddItem(new Dvd("3", "Tutorial Desain Sistem OOP", 2022));

            // Anggota (Bagian B)
            system.AddMember(new Member("1", "Andi Reguler", false));
            system.AddMember(new Member("2", "Siti Premium", true));
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Info] Gagal memuat data dummy: " + ex.Message);
        }

        // 2. Menu Utama Interaktif
        while (isRunning)
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
            Console.Write("Pilih menu (0-8): ");

            string inputMenu = Console.ReadLine();

            // Validasi Error Handling menggunakan int.TryParse
            if (!int.TryParse(inputMenu, out int pilihan))
            {
                Console.WriteLine("\n[Error] Mohon masukkan angka yang valid (0-8).");
                continue;
            }

            Console.WriteLine();

            switch (pilihan)
            {
                case 1:
                    system.PrintAllItems();
                    break;

                case 2:
                    system.PrintAllMembers();
                    break;

                case 3:
                    Console.Write("Masukkan Nomor Anggota (contoh: M01): ");
                    string idPinjam = Console.ReadLine();
                    Console.Write("Masukkan Kode Bahan Pustaka (contoh: B001): ");
                    string kodePinjam = Console.ReadLine();
                    system.BorrowItem(idPinjam, kodePinjam);
                    break;

                case 4:
                    Console.Write("Masukkan Kode Bahan Pustaka yang akan diperpanjang: ");
                    string kodePerpanjang = Console.ReadLine();
                    system.RenewItem(kodePerpanjang);
                    break;

                case 5:
                    Console.Write("Masukkan Nomor Anggota (contoh: M01): ");
                    string idKembali = Console.ReadLine();
                    Console.Write("Masukkan Kode Bahan Pustaka: ");
                    string kodeKembali = Console.ReadLine();

                    Console.Write("Masukkan jumlah hari keterlambatan: ");
                    string inputTelat = Console.ReadLine();

                    if (int.TryParse(inputTelat, out int hariTelat))
                    {
                        system.ReturnItem(idKembali, kodeKembali, hariTelat);
                    }
                    else
                    {
                        Console.WriteLine("[Error] Jumlah hari keterlambatan harus berupa angka.");
                    }
                    break;

                case 6:
                    system.PrintDailyReport();
                    break;

                case 7:
                    Console.Write("Masukkan Nomor Anggota yang ingin dilihat riwayatnya: ");
                    string idRiwayat = Console.ReadLine();

                    Member m = system.FindMember(idRiwayat);
                    if (m != null)
                    {
                        m.ShowLoanHistory();
                    }
                    else
                    {
                        Console.WriteLine("Gagal: Anggota dengan ID tersebut tidak ditemukan.");
                    }
                    break;

                case 8:
                    Console.WriteLine("Pilih Jenis Bahan Pustaka:");
                    Console.WriteLine("1. Buku");
                    Console.WriteLine("2. Majalah");
                    Console.WriteLine("3. DVD");
                    Console.Write("Pilihan (1-3): ");
                    string jenisInput = Console.ReadLine();

                    Console.Write("Masukkan Kode (unik): ");
                    string kodeBaru = Console.ReadLine();
                    Console.Write("Masukkan Judul: ");
                    string judulBaru = Console.ReadLine();
                    Console.Write("Masukkan Tahun Terbit (1900-" + DateTime.Now.Year + "): ");

                    if (int.TryParse(Console.ReadLine(), out int tahunBaru))
                    {
                        if (jenisInput == "1")
                            system.AddItem(new Book(kodeBaru, judulBaru, tahunBaru));
                        else if (jenisInput == "2")
                            system.AddItem(new Magazine(kodeBaru, judulBaru, tahunBaru));
                        else if (jenisInput == "3")
                            system.AddItem(new Dvd(kodeBaru, judulBaru, tahunBaru));
                        else
                            Console.WriteLine("[Error] Jenis bahan pustaka tidak valid.");
                    }
                    else
                    {
                        Console.WriteLine("[Error] Tahun terbit harus berupa angka.");
                    }
                    break;

                case 0:
                    isRunning = false;
                    Console.WriteLine("Terima kasih telah menggunakan Sistem Perpustakaan.");
                    break;

                default:
                    Console.WriteLine("[Error] Pilihan menu tidak tersedia. Silakan pilih 0-8.");
                    break;
            }
        }
    }
}