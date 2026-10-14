using System;

namespace LibraryManagement;

/*
 * ITEM FEATURE CLASS:
 * Bertanggung jawab eksklusif terhadap alur fitur bahan pustaka (LibraryItem),
 * seperti penambahan buku, majalah, atau DVD baru menggunakan prinsip Polymorphism.
 */
internal static class ItemFeature
{
    public static void ProcessAddNewItem(LibrarySystem system)
    {
        Console.WriteLine("\n--- TAMBAH BAHAN PUSTAKA BARU ---");
        Console.WriteLine("Pilih Jenis Bahan Pustaka:");
        Console.WriteLine("1. Buku (Denda Rp 1.000/hari, Maks. Perpanjang 2x)");
        Console.WriteLine("2. Majalah (Denda Rp 500/hari, Maks. Perpanjang 1x)");
        Console.WriteLine("3. DVD (Denda Rp 3.000/hari, Tidak Bisa Diperpanjang)");

        int jenisPilihan = Validation.ReadIntInRange("Pilihan (1-3): ", 1, 3);

        string kodeBaru = Validation.ReadNonEmptyString("Masukkan Kode Bahan Pustaka (unik): ");

        // Cek duplikasi kode item sebelum membuat objek
        if (system.FindItem(kodeBaru) != null)
        {
            Console.WriteLine($"\n[Gagal] Kode bahan pustaka '{kodeBaru}' sudah terdaftar dalam sistem.");
            return;
        }

        string judulBaru = Validation.ReadNonEmptyString("Masukkan Judul: ");
        int tahunBaru = Validation.ReadPublicationYear($"Masukkan Tahun Terbit (1900-{DateTime.Now.Year}): ");

        // POLYMORPHISM: Membuat instance child class sesuai pilihan, ditampung sebagai LibraryItem
        LibraryItem? newItem = jenisPilihan switch
        {
            1 => new Book(kodeBaru, judulBaru, tahunBaru),
            2 => new Magazine(kodeBaru, judulBaru, tahunBaru),
            3 => new Dvd(kodeBaru, judulBaru, tahunBaru),
            _ => null
        };

        if (newItem != null)
        {
            system.AddItem(newItem);
        }
    }
}
