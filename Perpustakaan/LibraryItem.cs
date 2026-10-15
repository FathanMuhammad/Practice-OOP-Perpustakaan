namespace LibraryManagement
{
    public abstract class LibraryItem : ILoanable
    {
        // Static Attribute sesuai Class Diagram
        private static int MIN_YEAR = 1900;

        // C# Properties (Encapsulation)
        public string Code { get; }  // {readOnly} - kode tidak bisa diubah, hanya bisa di-set lewat constructor
        public string Title
        {
            get;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    field = value;
                }
            }
        }
        public int PublicationYear { get; private set; }
        public bool IsAvailable { get; private set; } = true;
        public string BorrowedBy { get; private set; } = "";
        public int RenewalCount { get; protected set; } = 0;

        // Properties untuk tracking masa pinjam
        public int LoanDuration { get; private set; } = 0;     // Masa pinjam awal (hari)
        public int RenewalDays { get; private set; } = 0;      // Total hari perpanjangan
        public int TotalAllowedDays => LoanDuration + RenewalDays;  // Total masa pinjam yang diizinkan

        // Constructor (Protected karena abstract class hanya dipanggil oleh child class)
        protected LibraryItem(string code, string title, int publicationYear)
        {
            Code = code;
            Title = title;

            // Validasi tahun terbit saat objek dibuat
            if (!SetPublicationYear(publicationYear))
            {
                // Default fallback jika tahun tidak valid saat inisialisasi
                PublicationYear = DateTime.Now.Year;
            }
        }

        // Static Method untuk mengecek apakah tahun valid (1900 s/d tahun ini)
        public static bool IsValidYear(int yr)
        {
            int currentYear = DateTime.Now.Year;
            return yr >= MIN_YEAR && yr <= currentYear;
        }

        // Setter dengan pesan validasi sesuai kebutuhan sistem (return bool untuk feedback)
        public bool SetPublicationYear(int yr)
        {
            int currentYear = DateTime.Now.Year;
            if (!IsValidYear(yr))
            {
                Console.WriteLine($"[Validasi Gagal] Tahun terbit ({yr}) tidak valid! Harus berada di antara {MIN_YEAR} sampai {currentYear}.");
                return false;
            }

            PublicationYear = yr;
            return true;
        }

        // Method untuk menandai bahan pustaka sedang dipinjam
        public void MarkAsBorrowed(string memberName, int loanDays)
        {
            IsAvailable = false;
            BorrowedBy = memberName;
            RenewalCount = 0;
            LoanDuration = loanDays;
            RenewalDays = 0;
        }

        // Method untuk menandai bahan pustaka telah dikembalikan
        public void MarkAsReturned()
        {
            IsAvailable = true;
            BorrowedBy = "";
            RenewalCount = 0;
            LoanDuration = 0;
            RenewalDays = 0;
        }

        // Method untuk proses perpanjangan peminjaman
        public bool RenewLoan(int extraDays)
        {
            // Tidak bisa diperpanjang jika belum dipinjam atau sudah mencapai batas maksimal
            // (Untuk DVD, GetMaxRenewals() bernilai 0, sehingga otomatis return false)
            if (IsAvailable || RenewalCount >= GetMaxRenewals())
            {
                return false;
            }

            RenewalCount++;
            RenewalDays += extraDays;
            return true;
        }

        // Method untuk mencetak informasi bahan pustaka (Fitur Utama: Daftar bahan pustaka)
        public string GetItemInfo()
        {
            string statusInfo = IsAvailable
                ? "Tersedia"
                : $"Dipinjam oleh {BorrowedBy} (masa pinjam: {TotalAllowedDays} hari)";
            return $"[{Code}] {Title} | Jenis: {GetItemType()} | Tahun: {PublicationYear} | Status: {statusInfo}";
        }

        // Abstract Methods yang wajib di-override oleh Book, Magazine, dan DVD (Abstraction & Polymorphism)
        public abstract string GetItemType();
        public abstract int GetDailyFine();
        public abstract int GetMaxRenewals();
    }
}