namespace LibraryManagement
{
    public abstract class LibraryItem
    {
        // Static Attribute sesuai Class Diagram
        private static int MIN_YEAR = 1900;

        // Private & Protected Attributes (Encapsulation)
        private readonly string code; // {readOnly} - kode tidak bisa diubah
        private string title;
        private int publicationYear;
        private bool isAvailable = true;
        private string borrowedBy = "";
        protected int renewalCount = 0;

        // Constructor (Protected karena abstract class hanya dipanggil oleh child class)
        protected LibraryItem(string code, string title, int publicationYear)
        {
            this.code = code;
            this.title = title;

            // Validasi tahun terbit saat objek dibuat
            if (!SetPublicationYear(publicationYear))
            {
                // Default fallback jika tahun tidak valid saat inisialisasi
                this.publicationYear = DateTime.Now.Year;
            }
        }

        // Static Method untuk mengecek apakah tahun valid (1900 s/d tahun ini)
        public static bool IsValidYear(int yr)
        {
            int currentYear = DateTime.Now.Year;
            return yr >= MIN_YEAR && yr <= currentYear;
        }

        // Setter dengan pesan validasi sesuai kebutuhan sistem
        public bool SetPublicationYear(int yr)
        {
            int currentYear = DateTime.Now.Year;
            if (!IsValidYear(yr))
            {
                Console.WriteLine($"[Validasi Gagal] Tahun terbit ({yr}) tidak valid! Harus berada di antara {MIN_YEAR} sampai {currentYear}.");
                return false;
            }

            this.publicationYear = yr;
            return true;
        }

        // Getters untuk akses data dari luar class (Encapsulation)
        public string GetCode() => code;
        public string GetTitle() => title;
        public int GetPublicationYear() => publicationYear;
        public bool GetIsAvailable() => isAvailable;
        public string GetBorrowedBy() => borrowedBy;
        public int GetRenewalCount() => renewalCount;

        // C# Properties
        public string Code => code;
        public string Title => title;
        public int PublicationYear => publicationYear;
        public bool IsAvailable => isAvailable;
        public string BorrowedBy => borrowedBy;
        public int RenewalCount => renewalCount;

        public void SetTitle(string newTitle)
        {
            if (!string.IsNullOrWhiteSpace(newTitle))
            {
                this.title = newTitle;
            }
        }

        // Method untuk menandai bahan pustaka sedang dipinjam
        public void MarkAsBorrowed(string memberName)
        {
            this.isAvailable = false;
            this.borrowedBy = memberName;
            this.renewalCount = 0;
        }

        // Method untuk menandai bahan pustaka telah dikembalikan
        public void MarkAsReturned()
        {
            this.isAvailable = true;
            this.borrowedBy = "";
            this.renewalCount = 0;
        }

        // Method untuk proses perpanjangan peminjaman
        public bool RenewLoan()
        {
            // Tidak bisa diperpanjang jika belum dipinjam atau sudah mencapai batas maksimal
            // (Untuk DVD, GetMaxRenewals() bernilai 0, sehingga otomatis return false)
            if (isAvailable || renewalCount >= GetMaxRenewals())
            {
                return false;
            }

            renewalCount++;
            return true;
        }

        // Method untuk mencetak informasi bahan pustaka (Fitur Utama: Daftar bahan pustaka)
        public string GetItemInfo()
        {
            string statusInfo = isAvailable ? "Tersedia" : $"Dipinjam oleh {borrowedBy}";
            return $"[{code}] {title} | Jenis: {GetItemType()} | Tahun: {publicationYear} | Status: {statusInfo}";
        }

        // Abstract Methods yang wajib di-override oleh Book, Magazine, dan DVD (Abstraction & Polymorphism)
        public abstract string GetItemType();
        public abstract int GetDailyFine();
        public abstract int GetMaxRenewals();
    }
}