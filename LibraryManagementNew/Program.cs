namespace LibraryManagementNew;

internal class Program
{
    static Library perpustakaan = new Library();
    static bool isRunning = true;

    static void Main(string[] args)
    {
        SafeClear();
        SeedInitialData();
        Console.WriteLine("\nTekan Enter untuk melanjutkan ke menu utama...");
        Console.ReadLine();
        SafeClear();

        while (isRunning)
        {
            DisplayMenu();
            int choice = ReadIntInRange("Pilih menu (0-8): ", 0, 8);
            Console.WriteLine();
            HandleMenu(choice);

            if (isRunning)
            {
                Console.WriteLine("\nTekan Enter untuk kembali ke menu utama...");
                Console.ReadLine();
                SafeClear();
            }
        }
    }

    static void SafeClear()
    {
        try
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }
        }
        catch (IOException)
        {
            // Abaikan jika konsol dalam mode redirected / subshell
        }
    }

    static void DisplayMenu()
    {
        Console.WriteLine("\n==========================================");
        Console.WriteLine("    SISTEM MANAJEMEN PERPUSTAKAAN KAMPUS  ");
        Console.WriteLine("==========================================");
        Console.WriteLine("1. Tampilkan Daftar Bahan Pustaka");
        Console.WriteLine("2. Tampilkan Daftar Anggota");
        Console.WriteLine("3. Pinjam Bahan Pustaka");
        Console.WriteLine("4. Perpanjang Peminjaman");
        Console.WriteLine("5. Kembalikan Bahan Pustaka");
        Console.WriteLine("6. Tampilkan Riwayat Peminjaman Anggota");
        Console.WriteLine("7. Tampilkan Laporan Harian");
        Console.WriteLine("8. Tambah Bahan Pustaka Baru");
        Console.WriteLine("0. Keluar Program");
    }

    static void HandleMenu(int choice)
    {
        switch (choice)
        {
            case 1: perpustakaan.PrintItems(); break;
            case 2: DisplayMembers(); break;
            case 3: ProcessBorrow(); break;
            case 4: ProcessRenew(); break;
            case 5: ProcessReturn(); break;
            case 6: ProcessMemberHistory(); break;
            case 7: perpustakaan.PrintDailyReport(); break;
            case 8: ProcessAddItem(); break;
            case 0:
                isRunning = false;
                Console.WriteLine("Terima kasih telah menggunakan Sistem Perpustakaan.");
                break;
        }
    }

    // =============================================
    //  FITUR: DAFTAR ANGGOTA
    // =============================================
    static void DisplayMembers()
    {
        Console.WriteLine("=== DAFTAR ANGGOTA PERPUSTAKAAN ===");
        var allMembers = perpustakaan.GetAllMembers();
        if (allMembers.Count == 0)
        {
            Console.WriteLine("Belum ada data anggota terdaftar.");
            return;
        }

        foreach (var m in allMembers)
        {
            string tipe = m.Premium ? "Premium" : "Reguler";
            Console.WriteLine($"[{m.MemberNumber}] {m.Name} | Tipe: {tipe} | Kuota Pinjam: {m.BorrowedCount}/{m.BorrowLimit} item");
        }
    }

    // =============================================
    //  FITUR: PINJAM BAHAN PUSTAKA
    // =============================================
    static void ProcessBorrow()
    {
        Console.WriteLine("--- PEMINJAMAN BAHAN PUSTAKA ---");
        string memberId = ReadNonEmptyString("Masukkan Nomor Anggota: ");

        Member? member = FindMember(memberId);
        if (member == null)
        {
            Console.WriteLine("[Gagal] Anggota dengan nomor tersebut tidak ditemukan.");
            return;
        }

        string itemCode = ReadNonEmptyString("Masukkan Kode Bahan Pustaka: ");
        try
        {
            perpustakaan.Borrow(member, itemCode);
        }
        catch (ItemNotAvailableException ex)
        {
            Console.WriteLine($"[Ditolak] {ex.Message}");
            Console.Write("Apakah Anda ingin masuk ke antrean peminjaman untuk bahan pustaka ini? (y/n): ");
            string? ans = Console.ReadLine();
            if (ans?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true)
            {
                perpustakaan.EnqueueBorrower(member, itemCode);
            }
        }
        catch (BorrowLimitExceededException ex)
        {
            Console.WriteLine($"[Ditolak] {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Gagal] {ex.Message}");
        }
    }

    // =============================================
    //  FITUR: PERPANJANG PEMINJAMAN (IRenewable)
    // =============================================
    static void ProcessRenew()
    {
        Console.WriteLine("--- PERPANJANG PEMINJAMAN ---");
        string itemCode = ReadNonEmptyString("Masukkan Kode Bahan Pustaka yang akan diperpanjang: ");

        LibraryItem? item = perpustakaan.FindItem(itemCode);
        if (item == null)
        {
            Console.WriteLine($"[Gagal] Bahan pustaka dengan kode '{itemCode}' tidak ditemukan.");
            return;
        }

        if (item.Borrower == null)
        {
            Console.WriteLine("[Gagal] Bahan pustaka ini sedang tidak dipinjam.");
            return;
        }

        if (item is not IRenewable renewable)
        {
            Console.WriteLine("[Ditolak] Bahan pustaka jenis ini tidak bisa diperpanjang.");
            return;
        }

        renewable.Renew();
    }

    // =============================================
    //  FITUR: KEMBALIKAN BAHAN PUSTAKA
    // =============================================
    static void ProcessReturn()
    {
        Console.WriteLine("--- PENGEMBALIAN BAHAN PUSTAKA ---");
        string itemCode = ReadNonEmptyString("Masukkan Kode Bahan Pustaka: ");
        int lateDays = ReadNonNegativeInt("Masukkan jumlah hari keterlambatan: ");

        perpustakaan.Return(itemCode, lateDays);
    }

    // =============================================
    //  FITUR: RIWAYAT PEMINJAMAN ANGGOTA
    // =============================================
    static void ProcessMemberHistory()
    {
        Console.WriteLine("--- RIWAYAT PEMINJAMAN ANGGOTA ---");
        string memberId = ReadNonEmptyString("Masukkan Nomor Anggota: ");

        Member? member = FindMember(memberId);
        if (member == null)
        {
            Console.WriteLine("[Gagal] Anggota dengan nomor tersebut tidak ditemukan.");
            return;
        }

        member.PrintHistory();
    }

    // =============================================
    //  FITUR: TAMBAH BAHAN PUSTAKA BARU
    // =============================================
    static void ProcessAddItem()
    {
        Console.WriteLine("--- TAMBAH BAHAN PUSTAKA BARU ---");
        Console.WriteLine("Pilih Jenis:");
        Console.WriteLine("1. Buku (Denda Rp 1.000/hari, Maks. Perpanjang 2x)");
        Console.WriteLine("2. Majalah (Denda Rp 500/hari, Maks. Perpanjang 1x)");
        Console.WriteLine("3. DVD (Denda Rp 3.000/hari, Tidak Bisa Diperpanjang)");

        int jenis = ReadIntInRange("Pilihan (1-3): ", 1, 3);
        string code = ReadNonEmptyString("Masukkan Kode: ");
        string title = ReadNonEmptyString("Masukkan Judul: ");

        Console.Write($"Masukkan Tahun Terbit (1900-{DateTime.Now.Year}): ");
        string? inputYear = Console.ReadLine();
        if (!int.TryParse(inputYear, out int year))
        {
            Console.WriteLine("[Ditolak] Input tahun harus berupa angka bulat. Pemasukan bahan pustaka dibatalkan.");
            return;
        }

        LibraryItem? newItem = null;
        try
        {
            // Validasi tahun menggunakan ArgumentOutOfRangeException sesuai ketentuan SeriesC
            if (!LibraryItem.IsValidYear(year))
            {
                throw new ArgumentOutOfRangeException(nameof(year),
                    $"Tahun terbit ({year}) tidak valid! Harus antara 1900 sampai {DateTime.Now.Year}.");
            }

            if (jenis == 1)
            {
                string author = ReadNonEmptyString("Masukkan Nama Pengarang: ");
                newItem = new Book(code, title, year, author);
            }
            else if (jenis == 2)
            {
                string edition = ReadNonEmptyString("Masukkan Edisi: ");
                newItem = new Magazine(code, title, year, edition);
            }
            else if (jenis == 3)
            {
                int duration = ReadPositiveInt("Masukkan Durasi (menit): ");
                newItem = new Dvd(code, title, year, duration);
            }

            if (newItem != null)
            {
                perpustakaan.AddItem(newItem);
                Console.WriteLine($"[Berhasil] Bahan pustaka [{code}] {title} berhasil ditambahkan.");
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"[Ditolak] {ex.Message}");
            Console.WriteLine("Pemasukan bahan pustaka dibatalkan.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Gagal] {ex.Message}");
        }
    }

    // =============================================
    //  HELPER: Mencari Member dari daftar
    // =============================================
    static Member? FindMember(string memberId)
    {
        return perpustakaan.FindMember(memberId);
    }

    // =============================================
    //  DATA AWAL (Seed)
    // =============================================
    static void SeedInitialData()
    {
        // Bahan Pustaka Awal
        perpustakaan.AddItem(new Book("1", "Pemrograman C#", 2023, "John Doe"));
        perpustakaan.AddItem(new Magazine("2", "National Geographic", 2024, "Edisi Januari"));
        perpustakaan.AddItem(new Dvd("3", "Belajar .NET", 2023, 90));

        // Anggota Awal
        perpustakaan.AddMember(new Member("1", "Irza", false));     // Reguler (Batas 2)
        perpustakaan.AddMember(new Member("2", "Fathan", true));     // Premium (Batas 4)

        Console.WriteLine($"\n[Info] Data awal berhasil dimuat: {LibraryItem.TotalCreated} bahan pustaka, {perpustakaan.GetAllMembers().Count} anggota.");
    }

    // =============================================
    //  VALIDASI INPUT
    // =============================================
    static int ReadIntInRange(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int result) && result >= min && result <= max)
                return result;
            Console.WriteLine($"[Error] Masukkan angka yang valid ({min}-{max}).");
        }
    }

    static string ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();
            Console.WriteLine("[Error] Input tidak boleh kosong.");
        }
    }

    static int ReadNonNegativeInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int result) && result >= 0)
                return result;
            Console.WriteLine("[Error] Masukkan angka non-negatif (>= 0).");
        }
    }

    static int ReadPositiveInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int result) && result > 0)
                return result;
            Console.WriteLine("[Error] Masukkan angka positif (> 0).");
        }
    }
}
