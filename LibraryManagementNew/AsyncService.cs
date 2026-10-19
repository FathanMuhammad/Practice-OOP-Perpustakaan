using System.Diagnostics;
namespace LibraryManagementNew;

/// 1. Task.WhenAll & Task.Delay (Pengiriman pesan/pengingat bersamaan).
/// 2. CancellationTokenSource & CancellationToken (Pembatalan manual & timeout otomatis).
public static class AsyncService
{
    // FITUR 1: Pengingat jatuh tempo dikirim bersamaan dengan Task.WhenAll
    public static async Task KirimPengingatJatuhTempoAsync(Library library)
    {
        Console.WriteLine("\n--- PENGIRIMAN PENGINGAT JATUH TEMPO (ASYNC / Task.WhenAll) ---");

        DateTime besok = DateTime.Now.Date.AddDays(1);

        // Cari bahan pustaka yang sedang dipinjam dan jatuh tempo besok
        var daftarPinjaman = library.GetAllItems()
                                    .Where(item => item.Status == "Dipinjam"
                                                && item.Borrower != null
                                                && item.DueDate.HasValue
                                                && item.DueDate.Value.Date == besok)
                                    .ToList();

        if (daftarPinjaman.Count == 0)
        {
            Console.WriteLine($"Tidak ada pinjaman bahan pustaka yang jatuh tempo besok ({besok:dd/MM/yyyy}).");
            return;
        }

        Console.WriteLine($"Ditemukan {daftarPinjaman.Count} pinjaman yang jatuh tempo besok ({besok:dd/MM/yyyy}).");
        Console.WriteLine("Memulai pengiriman pengingat secara bersamaan (konkuren)...\n");

        var stopwatch = Stopwatch.StartNew();

        // Buat kumpulan Task pengiriman pengingat
        var tasks = new List<Task>();
        foreach (var item in daftarPinjaman)
        {
            tasks.Add(KirimPesanPengingatAsync(item.Borrower!, item));
        }

        // Menjalankan semua task pengiriman secara bersamaan
        await Task.WhenAll(tasks);

        stopwatch.Stop();
        Console.WriteLine($"\n[Selesai] Semua ({daftarPinjaman.Count}) pengingat berhasil dikirim dalam {stopwatch.ElapsedMilliseconds} ms.");
    }

    // Method simulasi pengiriman pesan (asinkron menggunakan Task.Delay)
    private static async Task KirimPesanPengingatAsync(Member member, LibraryItem item)
    {
        Console.WriteLine($"[Kirim...] Menghubungi {member.Name} untuk item [{item.Code}] {item.Title}...");

        // Simulasi jeda pengiriman jaringan / SMS (1000 - 1800 ms)
        var random = new Random();
        int delay = random.Next(1000, 1800);
        await Task.Delay(delay);

        Console.WriteLine($"[Terkirim] Pengingat berhasil dikirim ke {member.Name} (Selesai dalam {delay} ms).");
    }

    // FITUR 2: Pengecekan seluruh bahan pustaka (Stock Opname)
    // Bisa dibatalkan oleh pengguna (tombol 'c') atau berhenti otomatis (timeout)
    public static async Task StockOpnameAsync(Library library)
    {
        Console.WriteLine("\n--- PENGECEKAN BAHAN PUSTAKA (STOCK OPNAME) ---");
        Console.WriteLine("Pengecekan fisik berjalan untuk seluruh bahan pustaka.");
        Console.WriteLine("- Batas waktu otomatis (Timeout): 6 detik.");
        Console.WriteLine("- Pembatalan manual: Tekan tombol 'c' kapan saja untuk membatalkan.\n");

        var allItems = library.GetAllItems();
        if (allItems.Count == 0)
        {
            Console.WriteLine("Belum ada data bahan pustaka di perpustakaan.");
            return;
        }

        // CancellationTokenSource dengan timeout otomatis 6 detik
        using var cts = new CancellationTokenSource();
        int batasWaktuDetik = 6;
        cts.CancelAfter(TimeSpan.FromSeconds(batasWaktuDetik));

        bool dibatalkanManual = false;
        CancellationToken token = cts.Token;

        // Background task untuk mendengarkan tombol 'c' dari pengguna
        var cancelListener = Task.Run(() =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        if (key.KeyChar == 'c' || key.KeyChar == 'C')
                        {
                            dibatalkanManual = true;
                            cts.Cancel();
                            break;
                        }
                    }
                    Thread.Sleep(50);
                }
            }
            catch
            {
            }
        });

        int diperiksa = 0;

        try
        {
            foreach (var item in allItems)
            {
                // Cek apakah ada permintaan pembatalan
                token.ThrowIfCancellationRequested();

                // Simulasi durasi pengecekan per item (300 ms)
                await Task.Delay(300, token);

                diperiksa++;
                Console.WriteLine($"[{diperiksa,2}/{allItems.Count}] Memeriksa [{item.Code}] {item.Title,-30} | Status: {item.Status}");
            }

            Console.WriteLine($"\n[Selesai] Stock opname selesai! Total {diperiksa} bahan pustaka berhasil diperiksa.");
        }
        catch (OperationCanceledException)
        {
            if (dibatalkanManual)
            {
                Console.WriteLine($"\n[Dibatalkan] Proses stock opname dibatalkan oleh pengguna pada item ke-{diperiksa}.");
            }
            else
            {
                Console.WriteLine($"\n[Batas Waktu Habis] Stock opname berhenti otomatis karena melewati batas waktu ({batasWaktuDetik} detik). Terperiksa: {diperiksa}/{allItems.Count} item.");
            }
        }
        finally
        {
            // Pastikan listener berhenti
            cts.Cancel();
            try { await cancelListener; } catch { }
        }
    }
}
