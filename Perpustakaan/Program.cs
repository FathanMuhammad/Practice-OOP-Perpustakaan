namespace LibraryManagement;

/*
 * PROGRAM ENTRY POINT:
 * Titik masuk utama aplikasi (Main method).
 * Mengikuti prinsip Separation of Concerns & Clean Code:
 * Program.cs hanya bertindak sebagai launcher yang menjalankan LibraryApp.
 */
internal class Program
{
    static void Main(string[] args)
    {
        LibraryApp app = new LibraryApp();
        app.Run();
    }
}