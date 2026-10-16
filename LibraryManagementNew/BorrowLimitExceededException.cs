namespace LibraryManagementNew;

/// <summary>
/// Exception yang dilempar ketika seorang anggota mencoba meminjam melebihi batas kuota peminjaman.
/// Membawa informasi nomor anggota (MemberNumber) dan batas pinjamnya (BorrowLimit).
/// </summary>
public class BorrowLimitExceededException : Exception
{
    public string MemberNumber { get; }
    public int BorrowLimit { get; }

    public BorrowLimitExceededException(string memberNumber, int borrowLimit)
        : base($"Anggota [{memberNumber}] telah melebihi batas maksimal peminjaman ({borrowLimit} item).")
    {
        MemberNumber = memberNumber;
        BorrowLimit = borrowLimit;
    }

    public BorrowLimitExceededException(string memberNumber, int borrowLimit, string message)
        : base(message)
    {
        MemberNumber = memberNumber;
        BorrowLimit = borrowLimit;
    }
}
