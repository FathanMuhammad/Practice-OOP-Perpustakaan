using System;

namespace LibraryManagement;

/*
 * VALIDATION CLASS:
 * Bertanggung jawab eksklusif untuk menangani pembacaan input dari console
 * beserta validasi tipe data, batas nilai, dan penanganan kesalahan (error handling).
 */
internal static class Validation
{
    // Membaca integer dengan rentang tertentu (contoh: pilihan menu 0-8, jenis item 1-3)
    public static int ReadIntInRange(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int result) && result >= min && result <= max)
            {
                return result;
            }

            Console.WriteLine($"\n[Error] Mohon masukkan angka yang valid ({min}-{max}).");
        }
    }

    // Membaca teks dan memastikan tidak bernilai null atau spasi kosong
    public static string ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.WriteLine("[Error] Input tidak boleh kosong. Silakan ulangi.");
        }
    }

    // Membaca bilangan bulat non-negatif (contoh: hari keterlambatan >= 0)
    public static int ReadNonNegativeInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int result) && result >= 0)
            {
                return result;
            }

            Console.WriteLine("[Error] Mohon masukkan angka bilangan bulat non-negatif (>= 0).");
        }
    }

    // Membaca tahun terbit dan memvalidasi batas tahun (1900 s/d tahun berjalan)
    public static int ReadPublicationYear(string prompt)
    {
        int currentYear = DateTime.Now.Year;
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int year) && LibraryItem.IsValidYear(year))
            {
                return year;
            }

            Console.WriteLine($"[Error] Tahun terbit harus berupa angka antara 1900 sampai {currentYear}.");
        }
    }
}
