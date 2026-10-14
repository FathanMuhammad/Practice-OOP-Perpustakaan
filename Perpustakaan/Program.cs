using LibraryManagement;
using System;

namespace LibraryManagement
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("=== TEST MANDIRI PART A (LIBRARY ITEM) ===\n");

            //// 1. Simpan di Array sesuai syarat soal (Tanpa Collection)
            //LibraryItem[] items = new LibraryItem[3];
            //items[0] = new Book("B001", "Clean Code in C#", 2020);
            //items[1] = new Magazine("M001", "National Geographic", 2024);
            //items[2] = new Dvd("D001", "The Imitation Game", 2014);

            //// 2. Uji pesan validasi tahun terbit di luar 1900 - tahun ini
            //Console.WriteLine("--- Test Validasi Tahun ---");
            //items[0].SetPublicationYear(1850); // Harus gagal

            //// 3. Uji peminjaman dan cetak daftar
            //Console.WriteLine("\n--- Daftar Bahan Pustaka ---");
            //items[0].MarkAsBorrowed("Budi");
            //items[2].MarkAsBorrowed("Siti");

            //for (int i = 0; i < items.Length; i++)
            //{
            //    Console.WriteLine(items[i].GetItemInfo());
            //}

            //// 4. Uji perpanjangan Buku (Maks 2x) dan DVD (Ditolak karena DVD)
            //Console.WriteLine("\n--- Test Perpanjangan ---");
            //Console.WriteLine($"Perpanjang Buku 1: {items[0].RenewLoan()} ({items[0].GetRenewalCount()}/{items[0].GetMaxRenewals()})");
            //Console.WriteLine($"Perpanjang Buku 2: {items[0].RenewLoan()} ({items[0].GetRenewalCount()}/{items[0].GetMaxRenewals()})");
            //Console.WriteLine($"Perpanjang Buku 3 (Melebihi batas): {items[0].RenewLoan()}");
            //Console.WriteLine($"Perpanjang DVD (Tidak bisa): {items[2].RenewLoan()}");

            //Console.ReadLine();
        }
    }
}