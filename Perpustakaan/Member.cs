using System;

namespace LibraryManagement;

internal class Member
{
    // Static Field untuk Akumulasi Denda Harian
    private static double totalFineToday = 0;

    // C# Properties (Encapsulation gaya SeriesB)
    public string MemberId { get; private set; }
    public string Name { get; set; }
    public bool IsPremium { get; private set; }

    // Array Murni & Counter
    private string[] activeLoanCodes;
    private int activeLoanCount;

    private string[] loanHistory;
    private int historyCount;

    // Constructor
    public Member(string memberId, string name, bool isPremium)
    {
        MemberId = memberId;
        Name = name;
        IsPremium = isPremium;

        activeLoanCodes = new string[GetMaxLoanLimit()];
        loanHistory = new string[100];
        activeLoanCount = 0;
        historyCount = 0;
    }

    // Static Methods
    public static double GetTotalFineToday() => totalFineToday;

    public static void AddFineToday(double amount)
    {
        if (amount > 0) totalFineToday += amount;
    }

    // Aturan & Batasan Keanggotaan
    public string GetMemberType() => IsPremium ? "Premium" : "Reguler";

    public int GetMaxLoanLimit() => IsPremium ? 4 : 2;

    public bool CanBorrow() => activeLoanCount < GetMaxLoanLimit();

    public int GetActiveLoanCount() => activeLoanCount;

    // Cek Apakah Item Sedang Dipinjam
    public bool HasBorrowedItem(string code)
    {
        for (int i = 0; i < activeLoanCount; i++)
        {
            if (activeLoanCodes[i] == code) return true;
        }
        return false;
    }

    // Tambah Pinjaman Baru
    public bool AddLoan(string code, string title)
    {
        if (!CanBorrow() || HasBorrowedItem(code)) return false;

        activeLoanCodes[activeLoanCount] = code;
        activeLoanCount++;

        if (historyCount < loanHistory.Length)
        {
            loanHistory[historyCount] = $"[{code}] {title}";
            historyCount++;
        }

        return true;
    }

    // Kembalikan / Hapus Pinjaman (Array Shifting)
    public bool RemoveLoan(string code)
    {
        int foundIndex = -1;
        for (int i = 0; i < activeLoanCount; i++)
        {
            if (activeLoanCodes[i] == code)
            {
                foundIndex = i;
                break;
            }
        }

        if (foundIndex == -1) return false;

        for (int i = foundIndex; i < activeLoanCount - 1; i++)
        {
            activeLoanCodes[i] = activeLoanCodes[i + 1];
        }

        activeLoanCodes[activeLoanCount - 1] = "";
        activeLoanCount--;
        return true;
    }

    // Perhitungan Denda & Diskon 50% untuk Premium
    public double CalculateFinalFine(int lateDays, int dailyFine, out double discount)
    {
        discount = 0;
        if (lateDays <= 0 || dailyFine <= 0) return 0;

        double baseFine = lateDays * dailyFine;
        if (IsPremium) discount = baseFine * 0.5;

        double finalFine = baseFine - discount;
        AddFineToday(finalFine);

        return finalFine;
    }

    // Informasi & Riwayat
    public string GetMemberInfo() =>
        $"Nomor: {MemberId} | Nama: {Name} | Jenis: {GetMemberType()} | Pinjaman Aktif: {activeLoanCount}/{GetMaxLoanLimit()}";

    public void ShowLoanHistory()
    {
        Console.WriteLine($"Riwayat Peminjaman : {Name} ({MemberId})");
        if (historyCount == 0)
        {
            Console.WriteLine("- Belum ada riwayat peminjaman.");
            return;
        }

        for (int i = 0; i < historyCount; i++)
        {
            Console.WriteLine($"{i + 1}. {loanHistory[i]}");
        }
    }
}