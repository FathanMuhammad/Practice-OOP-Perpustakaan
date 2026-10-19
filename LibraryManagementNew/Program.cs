namespace LibraryManagementNew;

internal class Program
{
    static Library perpustakaan = new Library();
    static bool isRunning = true;

    static async Task Main(string[] args)
    {
        {
            SafeClear();

            SeedInitialData();
            Console.WriteLine("\nTekan Enter untuk melanjutkan ke menu utama...");
            Console.ReadLine();
            SafeClear();

            while (isRunning)
            {
                DisplayMenu();
                int choice = ReadIntInRange("Pilih menu (0-15): ", 0, 15);
                Console.WriteLine();
                await HandleMenu(choice);

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
            Console.WriteLine("9. [LINQ] Bahan Pustaka Tersedia (Urut Judul)");
            Console.WriteLine("10. [LINQ] Jumlah Bahan Pustaka per Jenis");
            Console.WriteLine("11. [LINQ] Total Denda per Anggota");
            Console.WriteLine("12. [LINQ] 3 Bahan Pustaka Paling Sering Dipinjam");
            Console.WriteLine("13. [LINQ] Anggota yang Sedang Terlambat Mengembalikan");
            Console.WriteLine("14. [ASYNC] Kirim Pengingat Jatuh Tempo (Task.WhenAll)");
            Console.WriteLine("15. [ASYNC] Stock Opname Bahan Pustaka (CancellationToken)");
            Console.WriteLine("0. Keluar Program");
        }

        static async Task HandleMenu(int choice)
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
                case 9: LinqQuery.TampilkanBahanPustakaTersediaUrutJudul(perpustakaan); break;
                case 10: LinqQuery.TampilkanJumlahBahanPustakaPerJenis(perpustakaan); break;
                case 11: LinqQuery.TampilkanTotalDendaPerAnggota(perpustakaan); break;
                case 12: perpustakaan.PrintTopBorrowedItems(3); break;
                case 13: perpustakaan.PrintOverdueMembers(); break;
                case 14: await AsyncService.KirimPengingatJatuhTempoAsync(perpustakaan); break;
                case 15: await AsyncService.StockOpnameAsync(perpustakaan); break;
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
            int loanDays = ReadOptionalPositiveInt("Masukkan masa pinjam (hari, tekan Enter untuk default 7 hari): ", 7);
            try
            {
                perpustakaan.Borrow(member, itemCode, loanDays);
            }
            catch (BorrowLimitExceededException ex)
            {
                Console.WriteLine($"[Ditolak] {ex.Message}");
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
            int? year = ReadPublicationYear($"Masukkan Tahun Terbit (1900-{DateTime.Now.Year}): ");
            if (!year.HasValue)
            {
                return; // Batalkan dan kembali ke menu utama
            }

            LibraryItem? newItem = null;

            try
            {
                if (jenis == 1)
                {
                    string author = ReadNonEmptyString("Masukkan Nama Pengarang: ");
                    newItem = new Book(code, title, year.Value, author);
                }
                else if (jenis == 2)
                {
                    string edition = ReadNonEmptyString("Masukkan Edisi: ");
                    newItem = new Magazine(code, title, year.Value, edition);
                }
                else if (jenis == 3)
                {
                    int duration = ReadPositiveInt("Masukkan Durasi (menit): ");
                    newItem = new Dvd(code, title, year.Value, duration);
                }

                if (newItem != null)
                {
                    perpustakaan.AddItem(newItem);
                    Console.WriteLine($"[Berhasil] Bahan pustaka [{code}] {title} berhasil ditambahkan.");
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"[Error] {ex.Message}");
                Console.WriteLine("[Batal] Proses penambahan bahan pustaka dibatalkan, kembali ke menu utama.");
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
            // ==========================================
            // BAHAN PUSTAKA AWAL
            // ==========================================

            // Buku
            perpustakaan.AddItem(new Book("B001", "Pemrograman C#", 2023, "John Doe"));
            perpustakaan.AddItem(new Book("B002", "Dasar-Dasar OOP", 2022, "Robert Martin"));
            perpustakaan.AddItem(new Book("B003", "Clean Code", 2008, "Robert C. Martin"));
            perpustakaan.AddItem(new Book("B004", "Design Patterns", 1994, "Erich Gamma"));
            perpustakaan.AddItem(new Book("B005", "The Pragmatic Programmer", 1999, "Andrew Hunt"));
            perpustakaan.AddItem(new Book("B006", "Introduction to Algorithms", 2022, "Thomas H. Cormen"));
            perpustakaan.AddItem(new Book("B007", "Head First Java", 2021, "Kathy Sierra"));
            perpustakaan.AddItem(new Book("B008", "Database System Concepts", 2019, "Abraham Silberschatz"));
            perpustakaan.AddItem(new Book("B009", "Learning SQL", 2020, "Alan Beaulieu"));
            perpustakaan.AddItem(new Book("B010", "Web Development with ASP.NET", 2024, "Mark J. Price"));

            // Majalah
            perpustakaan.AddItem(new Magazine("M001", "National Geographic", 2024, "Edisi Januari"));
            perpustakaan.AddItem(new Magazine("M002", "National Geographic", 2024, "Edisi Februari"));
            perpustakaan.AddItem(new Magazine("M003", "National Geographic", 2024, "Edisi Maret"));
            perpustakaan.AddItem(new Magazine("M004", "Tech Monthly", 2023, "Edisi April"));
            perpustakaan.AddItem(new Magazine("M005", "Tech Monthly", 2023, "Edisi Mei"));
            perpustakaan.AddItem(new Magazine("M006", "Computer World", 2024, "Edisi Juni"));
            perpustakaan.AddItem(new Magazine("M007", "Science Today", 2022, "Edisi Juli"));
            perpustakaan.AddItem(new Magazine("M008", "Digital Innovation", 2024, "Edisi Agustus"));
            perpustakaan.AddItem(new Magazine("M009", "Developer Weekly", 2025, "Edisi September"));
            perpustakaan.AddItem(new Magazine("M010", "Technology Review", 2025, "Edisi Oktober"));

            // DVD
            perpustakaan.AddItem(new Dvd("D001", "Belajar .NET", 2023, 90));
            perpustakaan.AddItem(new Dvd("D002", "C# untuk Pemula", 2022, 120));
            perpustakaan.AddItem(new Dvd("D003", "Database Fundamentals", 2021, 85));
            perpustakaan.AddItem(new Dvd("D004", "Web Development Basics", 2024, 100));
            perpustakaan.AddItem(new Dvd("D005", "Algoritma dan Struktur Data", 2020, 110));
            perpustakaan.AddItem(new Dvd("D006", "Object-Oriented Programming", 2023, 95));
            perpustakaan.AddItem(new Dvd("D007", "Computer Networking", 2022, 80));
            perpustakaan.AddItem(new Dvd("D008", "Operating System Concepts", 2021, 105));
            perpustakaan.AddItem(new Dvd("D009", "Software Engineering", 2024, 115));
            perpustakaan.AddItem(new Dvd("D010", "Cloud Computing Introduction", 2025, 90));


            // ==========================================
            // ANGGOTA AWAL
            // ==========================================

            // Anggota Reguler (Batas 2 peminjaman)
            perpustakaan.AddMember(new Member("1", "Irza", false));
            perpustakaan.AddMember(new Member("2", "Andi", false));
            perpustakaan.AddMember(new Member("3", "Budi", false));
            perpustakaan.AddMember(new Member("4", "Citra", false));
            perpustakaan.AddMember(new Member("5", "Dewi", false));
            perpustakaan.AddMember(new Member("6", "Eko", false));

            // Anggota Premium (Batas 4 peminjaman)
            perpustakaan.AddMember(new Member("7", "Fathan", true));
            perpustakaan.AddMember(new Member("8", "Gilang", true));
            perpustakaan.AddMember(new Member("9", "Hana", true));
            perpustakaan.AddMember(new Member("10", "Intan", true));


            // ==========================================
            // INFORMASI DATA AWAL
            // ==========================================

            Console.WriteLine(
                $"\n[Info] Data awal berhasil dimuat: " +
                $"{LibraryItem.TotalCreated} bahan pustaka, " +
                $"{perpustakaan.GetAllMembers().Count} anggota."
            );

            // Panggil simulasi transaksi awal khusus Bagian B (terpisah untuk mencegah conflict saat merge)
            SeedBagianBTransactions();
        }

        // =============================================
        //  HELPER: SIMULASI TRANSAKSI AWAL (BAGIAN B)
        // =============================================
        static void SeedBagianBTransactions()
        {
            // 1. Tambah riwayat frekuensi peminjaman masa lalu
            perpustakaan.FindItem("B001")?.AddBorrowCount(4); // Pemrograman C# (+1 saat dipinjam = 5x)
            perpustakaan.FindItem("B003")?.AddBorrowCount(4); // Clean Code (4x)
            perpustakaan.FindItem("D001")?.AddBorrowCount(2); // Belajar .NET (+1 saat dipinjam = 3x)
            perpustakaan.FindItem("M001")?.AddBorrowCount(2); // National Geographic (2x)
            perpustakaan.FindItem("B002")?.AddBorrowCount(1); // Dasar-Dasar OOP (1x)

            // 2. Simulasi Pinjaman Aktif & Keterlambatan
            // Andi (Reguler) meminjam B001, jatuh tempo 3 hari yang lalu (Terlambat 3 hari)
            var andi = perpustakaan.FindMember("2");
            if (andi != null)
            {
                perpustakaan.Borrow(andi, "B001", DateTime.Now.AddDays(-3));
            }

            // Budi (Reguler) meminjam D001, jatuh tempo 5 hari yang lalu (Terlambat 5 hari)
            var budi = perpustakaan.FindMember("3");
            if (budi != null)
            {
                perpustakaan.Borrow(budi, "D001", DateTime.Now.AddDays(-5));
            }

            // Citra (Reguler) meminjam M002, jatuh tempo 2 hari yang lalu (Terlambat 2 hari)
            var citra = perpustakaan.FindMember("4");
            if (citra != null)
            {
                perpustakaan.Borrow(citra, "M002", DateTime.Now.AddDays(-2));
            }

            // Fathan (Premium) meminjam B004, jatuh tempo 5 hari ke depan (Masih aktif, tidak terlambat)
            var fathan = perpustakaan.FindMember("7");
            if (fathan != null)
            {
                perpustakaan.Borrow(fathan, "B004", DateTime.Now.AddDays(5));
            }

            // 3. Simulasi Pinjaman yang Jatuh Tempo Besok (Pengujian Fitur Asinkron / Task.WhenAll)
            // Dewi (Reguler) meminjam B002, jatuh tempo besok (H+1)
            var dewi = perpustakaan.FindMember("5");
            if (dewi != null)
            {
                perpustakaan.Borrow(dewi, "B002", DateTime.Now.AddDays(1));
            }

            // Eko (Reguler) meminjam M001, jatuh tempo besok (H+1)
            var eko = perpustakaan.FindMember("6");
            if (eko != null)
            {
                perpustakaan.Borrow(eko, "M001", DateTime.Now.AddDays(1));
            }

            // Gilang (Premium) meminjam D002, jatuh tempo besok (H+1)
            var gilang = perpustakaan.FindMember("8");
            if (gilang != null)
            {
                perpustakaan.Borrow(gilang, "D002", DateTime.Now.AddDays(1));
            }
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

        static int ReadOptionalPositiveInt(string prompt, int defaultValue)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;
            if (int.TryParse(input, out int result) && result > 0)
                return result;
            Console.WriteLine($"[Info] Input tidak valid, menggunakan default {defaultValue} hari.");
            return defaultValue;
        }

        static int? ReadPublicationYear(string prompt)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int year))
            {
                if (year < 1900 || year > DateTime.Now.Year)
                {
                    Console.WriteLine($"[Error] Tahun terbit ({year}) tidak valid! Harus antara 1900 sampai {DateTime.Now.Year}.");
                    Console.WriteLine("[Batal] Proses penambahan bahan pustaka dibatalkan, kembali ke menu utama.");
                    return null;
                }
                return year;
            }

            Console.WriteLine("[Error] Input tahun terbit harus berupa angka.");
            Console.WriteLine("[Batal] Proses penambahan bahan pustaka dibatalkan, kembali ke menu utama.");
            return null;
        }
    }
}
