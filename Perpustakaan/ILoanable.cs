namespace LibraryManagement;

/*
 * INTERFACE ILoanable:
 * Kontrak abstraksi untuk setiap entitas yang memiliki siklus peminjaman
 * dalam sistem perpustakaan (Prinsip OOP: Abstraction & Realization).
 */
public interface ILoanable
{
    // Status & Perhitungan Masa Pinjam
    bool IsAvailable { get; }
    int LoanDuration { get; }
    int RenewalDays { get; }
    int TotalAllowedDays { get; }

    // Operasi Siklus Peminjaman
    void MarkAsBorrowed(string memberName, int loanDays);
    void MarkAsReturned();
    bool RenewLoan(int extraDays);

    // Aturan Polimorfik Tiap Bahan Pustaka
    int GetDailyFine();
    int GetMaxRenewals();
}
