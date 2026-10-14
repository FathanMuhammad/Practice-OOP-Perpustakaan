using LibraryManagement;
using System;

namespace LibraryManagement;

internal class LibrarySystem
{
    // Penyimpanan menggunakan Array murni (maksimal 100 item dan 50 member)
    private LibraryItem[] items;
    private int itemCount;

    private Member[] members;
    private int memberCount;

    // Constructor
    public LibrarySystem()
    {
        items = new LibraryItem[100];
        itemCount = 0;

        members = new Member[50];
        memberCount = 0;
    }

    // 1. Menambah Bahan Pustaka (Book, Magazine, DVD)
    public void AddItem(LibraryItem item)
    {
        if (itemCount < items.Length)
        {
            items[itemCount] = item;
            itemCount++;
            Console.WriteLine("Sukses: Bahan pustaka berhasil ditambahkan.");
        }
        else
        {
            Console.WriteLine("Error: Kapasitas penyimpanan bahan pustaka penuh.");
        }
    }

    // 2. Menambah Anggota
    public void AddMember(Member member)
    {
        if (memberCount < members.Length)
        {
            members[memberCount] = member;
            memberCount++;
            Console.WriteLine("Sukses: Anggota baru berhasil didaftarkan.");
        }
        else
        {
            Console.WriteLine("Error: Kapasitas pendaftaran anggota penuh.");
        }
    }

    // Helper: Mencari Item berdasarkan Kode
    public LibraryItem FindItem(string code)
    {
        for (int i = 0; i < itemCount; i++)
        {
            if (items[i].GetCode() == code)
            {
                return items[i];
            }
        }
        return null;
    }

    // Helper: Mencari Member berdasarkan ID
    public Member FindMember(string memberId)
    {
        for (int i = 0; i < memberCount; i++)
        {
            if (members[i].MemberId == memberId)
            {
                return members[i];
            }
        }
        return null;
    }

    // 3. Fitur Pinjam
    public void BorrowItem(string memberId, string itemCode)
    {
        Member member = FindMember(memberId);
        LibraryItem item = FindItem(itemCode);

        if (member == null)
        {
            Console.WriteLine("Gagal: Anggota dengan ID tersebut tidak ditemukan.");
            return;
        }

        if (item == null)
        {
            Console.WriteLine("Gagal: Bahan pustaka dengan kode tersebut tidak ditemukan.");
            return;
        }

        // Validasi ketersediaan bahan pustaka
        if (!item.GetIsAvailable())
        {
            Console.WriteLine("Ditolak: Bahan pustaka sedang dipinjam oleh " + item.GetBorrowedBy() + ".");
            return;
        }

        // Validasi batas pinjam member
        if (!member.CanBorrow())
        {
            Console.WriteLine("Ditolak: Batas pinjaman anggota ini sudah tercapai (" + member.GetActiveLoanCount() + "/" + member.GetMaxLoanLimit() + ").");
            return;
        }

        // Proses Peminjaman Berhasil
        item.MarkAsBorrowed(member.Name);
        member.AddLoan(item.GetCode(), item.Title);
        Console.WriteLine("Berhasil: " + member.Name + " sukses meminjam [" + item.GetCode() + "] " + item.Title);
    }

    // 4. Fitur Perpanjang
    public void RenewItem(string itemCode)
    {
        LibraryItem item = FindItem(itemCode);

        if (item == null)
        {
            Console.WriteLine("Gagal: Bahan pustaka tidak ditemukan.");
            return;
        }

        if (item.GetIsAvailable())
        {
            Console.WriteLine("Gagal: Bahan pustaka ini sedang tidak dipinjam.");
            return;
        }

        // Cek aturan jenis item (DVD tidak bisa diperpanjang, Buku maks 2x, Majalah maks 1x)
        if (item.GetItemType() == "DVD")
        {
            Console.WriteLine("Ditolak: DVD tidak dapat diperpanjang.");
            return;
        }

        bool success = item.RenewLoan();
        if (success)
        {
            Console.WriteLine("Berhasil: Perpanjangan ke-" + item.RenewalCount + " untuk [" + item.GetCode() + "] " + item.Title);
        }
        else
        {
            Console.WriteLine("Ditolak: Batas maksimal perpanjangan untuk jenis " + item.GetItemType() + " telah tercapai.");
        }
    }

    // 5. Fitur Kembalikan & Perhitungan Denda
    public void ReturnItem(string memberId, string itemCode, int lateDays)
    {
        Member member = FindMember(memberId);
        LibraryItem item = FindItem(itemCode);

        if (member == null || item == null)
        {
            Console.WriteLine("Gagal: Anggota atau bahan pustaka tidak ditemukan.");
            return;
        }

        // Cek apakah item ini benar sedang dipinjam oleh member bersangkutan
        if (!member.HasBorrowedItem(itemCode))
        {
            Console.WriteLine("Gagal: Anggota ini tidak tercatat meminjam bahan pustaka dengan kode tersebut.");
            return;
        }

        // Hitung denda berdasarkan jenis item (Buku: 1000, Majalah: 500, DVD: 3000)
        int dailyFine = item.GetDailyFine();
        double finalFine = member.CalculateFinalFine(lateDays, dailyFine, out double discount);

        // Ubah status item kembali tersedia dan hapus dari daftar pinjaman member
        item.MarkAsReturned();
        member.RemoveLoan(itemCode);

        // Cetak Rincian Pengembalian
        Console.WriteLine("\n--- RINCIAN PENGEMBALIAN ---");
        Console.WriteLine("Judul Item    : " + item.Title);
        Console.WriteLine("Peminjam      : " + member.Name + " (" + member.GetMemberType() + ")");
        Console.WriteLine("Hari Terlambat: " + lateDays + " hari");
        Console.WriteLine("Denda Dasar   : Rp " + (lateDays * dailyFine));
        Console.WriteLine("Potongan Premi: Rp " + discount);
        Console.WriteLine("Total Bayar   : Rp " + finalFine);
        Console.WriteLine("----------------------------");
    }

    // 6. Cetak Daftar Seluruh Bahan Pustaka
    public void PrintAllItems()
    {
        Console.WriteLine("\n=== DAFTAR SELURUH BAHAN PUSTAKA ===");
        if (itemCount == 0)
        {
            Console.WriteLine("Belum ada data bahan pustaka.");
            return;
        }

        for (int i = 0; i < itemCount; i++)
        {
            Console.WriteLine((i + 1) + ". " + items[i].GetItemInfo());
        }
    }

    // 7. Cetak Daftar Seluruh Anggota
    public void PrintAllMembers()
    {
        Console.WriteLine("\n=== DAFTAR SELURUH ANGGOTA ===");
        if (memberCount == 0)
        {
            Console.WriteLine("Belum ada data anggota.");
            return;
        }

        for (int i = 0; i < memberCount; i++)
        {
            Console.WriteLine((i + 1) + ". " + members[i].GetMemberInfo());
        }
    }

    // 8. Laporan Hari Ini
    public void PrintDailyReport()
    {
        int countBuku = 0, countMajalah = 0, countDVD = 0;
        int totalDipinjam = 0;

        for (int i = 0; i < itemCount; i++)
        {
            string type = items[i].GetItemType();
            if (type == "Book") countBuku++;
            else if (type == "Magazine") countMajalah++;
            else if (type == "DVD") countDVD++;

            if (!items[i].GetIsAvailable())
            {
                totalDipinjam++;
            }
        }

        Console.WriteLine("\n=== LAPORAN HARIAN PERPUSTAKAAN ===");
        Console.WriteLine("Jumlah Buku   : " + countBuku);
        Console.WriteLine("Jumlah Majalah: " + countMajalah);
        Console.WriteLine("Jumlah DVD    : " + countDVD);
        Console.WriteLine("Total Dipinjam: " + totalDipinjam);
        Console.WriteLine("Total Denda Hari Ini: Rp " + Member.GetTotalFineToday());
        Console.WriteLine("===================================");
    }
}