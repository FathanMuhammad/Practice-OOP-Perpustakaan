namespace LibraryManagementNew;

/// <summary>
/// Generic Repository untuk penyimpanan bahan pustaka.
/// Memiliki type constraint ke class induk LibraryItem (where T : LibraryItem).
/// Menggunakan List<T> untuk penyimpanan dan Dictionary<string, T> untuk pencarian berbasis kode.
/// </summary>
public class Repository<T> where T : LibraryItem
{
    private readonly List<T> items = new();
    private readonly Dictionary<string, T> itemsByCode = new(StringComparer.OrdinalIgnoreCase);

    // Menambah bahan pustaka ke repository
    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (itemsByCode.ContainsKey(item.Code))
        {
            throw new InvalidOperationException($"Bahan pustaka dengan kode '{item.Code}' sudah ada di repository.");
        }

        items.Add(item);
        itemsByCode[item.Code] = item;
    }

    // Pencarian cepat lewat kode memakai Dictionary
    public T? GetByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        itemsByCode.TryGetValue(code, out var item);
        return item;
    }

    // Mengambil seluruh koleksi bahan pustaka memakai List
    public IReadOnlyList<T> GetAll() => items;

    // Total bahan pustaka yang tersimpan
    public int Count => items.Count;
}
