using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementNew;

public static class LinqQuery
{
    // Fitur 1: Bahan pustaka tersedia diurutkan berdasarkan judul
    public static void TampilkanBahanPustakaTersediaUrutJudul(Library library)
    {
        Console.WriteLine("\n--- BAHAN PUSTAKA TERSEDIA (URUT JUDUL) ---");

        // LINQ Method Syntax:
        var query = library.GetAllItems()
                            .Where(item => item.Status == "Tersedia")
                            .OrderBy(item => item.Title);

        int nomor = 1;
        bool adaItem = false;

        foreach (var item in query)
        {
            adaItem = true;
            Console.Write($"{nomor++}. ");
            item.Info();
        }

        if (!adaItem)
        {
            Console.WriteLine("Tidak ada bahan pustaka yang berstatus Tersedia.");
        }
    }

    // Fitur 2: Jumlah bahan pustaka per jenis
    public static void TampilkanJumlahBahanPustakaPerJenis(Library library)
    {
        Console.WriteLine("\n--- JUMLAH BAHAN PUSTAKA PER JENIS ---");

        // LINQ Method Syntax:
        var query = library.GetAllItems()
                            .GroupBy(item => item.GetType().Name)
                            .Select(g => new { Jenis = g.Key, Jumlah = g.Count() });

        foreach (var item in query)
        {
            Console.WriteLine($"- {item.Jenis}: {item.Jumlah} item");
        }
    }

    // Fitur 3: Total denda per anggota
    public static void TampilkanTotalDendaPerAnggota(Library library)
    {
        Console.WriteLine("\n--- TOTAL DENDA PER ANGGOTA ---");

        // LINQ Method Syntax:
        var query = library.GetAllMembers()
                            .OrderByDescending(member => member.TotalFines);

        int nomor = 1;
        foreach (var member in query)
        {
            string statusMember = member.Premium ? "Premium" : "Reguler";
            Console.WriteLine($"{nomor++}. [{member.MemberNumber}] {member.Name} ({statusMember}) - Total Denda: Rp {member.TotalFines:N0}");
        }
    }
}
