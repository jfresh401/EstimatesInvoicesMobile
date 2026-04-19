using FreshEstimate.Mobile.Models;
using SQLite;

namespace FreshEstimate.Mobile.Services;

public sealed class AppRepository
{
    private readonly SemaphoreSlim _syncGate = new(1, 1);
    private SQLiteAsyncConnection? _db;

    public event EventHandler? DataChanged;

    private async Task InitAsync()
    {
        if (_db is not null)
            return;

        await _syncGate.WaitAsync();
        try
        {
            if (_db is not null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "fresh_estimate.db3");
            _db = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            await _db.CreateTableAsync<Customer>();
            await _db.CreateTableAsync<BusinessDocument>();
            await _db.CreateTableAsync<BrandingSettings>();
            await _db.CreateTableAsync<LineItemTemplate>();

            if (await _db.Table<BrandingSettings>().CountAsync() == 0)
                await _db.InsertAsync(new BrandingSettings());

            if (await _db.Table<LineItemTemplate>().CountAsync() == 0)
                await SeedTemplatesAsync();
        }
        finally
        {
            _syncGate.Release();
        }
    }

    private async Task SeedTemplatesAsync()
    {
        var defaults = new[]
        {
            new LineItemTemplate { Name = "Labor", Description = "Labor", DefaultQuantity = 1, DefaultUnitPrice = 0, DefaultTaxRatePercent = 0 },
            new LineItemTemplate { Name = "Labor & Materials", Description = "Labor & Materials", DefaultQuantity = 1, DefaultUnitPrice = 0, DefaultTaxRatePercent = 0 },
            new LineItemTemplate { Name = "CC Surcharge", Description = "Credit card surcharge", DefaultQuantity = 1, DefaultUnitPrice = 0, DefaultTaxRatePercent = 0 },
            new LineItemTemplate { Name = "RI Sales Tax", Description = "Rhode Island Sales Tax", DefaultQuantity = 1, DefaultUnitPrice = 0, DefaultTaxRatePercent = 7m },
            new LineItemTemplate { Name = "MA Sales Tax", Description = "Massachusetts Sales Tax", DefaultQuantity = 1, DefaultUnitPrice = 0, DefaultTaxRatePercent = 6.25m },
        };

        foreach (var item in defaults)
            await _db!.InsertAsync(item);
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        await InitAsync();
        return await _db!.Table<Customer>().OrderBy(x => x.DisplayName).ToListAsync();
    }

    public async Task SaveCustomerAsync(Customer customer)
    {
        await InitAsync();
        await _db!.InsertOrReplaceAsync(customer);
        OnDataChanged();
    }

    public async Task DeleteCustomerAsync(Customer customer)
    {
        await InitAsync();
        await _db!.DeleteAsync(customer);
        OnDataChanged();
    }

    public async Task<List<BusinessDocument>> GetDocumentsAsync()
    {
        await InitAsync();
        var customers = await GetCustomersAsync();
        var docs = await _db!.Table<BusinessDocument>().OrderByDescending(x => x.IssueDate).ToListAsync();
        foreach (var doc in docs)
            doc.HydrateTransientProperties(customers);
        return docs;
    }

    public async Task SaveDocumentAsync(BusinessDocument document)
    {
        await InitAsync();
        document.SyncPersistenceFields();
        await _db!.InsertOrReplaceAsync(document);
        OnDataChanged();
    }

    public async Task DeleteDocumentAsync(BusinessDocument document)
    {
        await InitAsync();
        await _db!.DeleteAsync(document);
        OnDataChanged();
    }

    public async Task<List<LineItemTemplate>> GetTemplatesAsync()
    {
        await InitAsync();
        return await _db!.Table<LineItemTemplate>().OrderBy(x => x.Name).ToListAsync();
    }

    public async Task SaveTemplateAsync(LineItemTemplate template)
    {
        await InitAsync();
        await _db!.InsertOrReplaceAsync(template);
        OnDataChanged();
    }

    public async Task DeleteTemplateAsync(LineItemTemplate template)
    {
        await InitAsync();
        await _db!.DeleteAsync(template);
        OnDataChanged();
    }

    public async Task<BrandingSettings> GetBrandingAsync()
    {
        await InitAsync();
        return await _db!.Table<BrandingSettings>().FirstAsync();
    }

    public async Task SaveBrandingAsync(BrandingSettings branding)
    {
        await InitAsync();
        branding.Id = 1;
        await _db!.InsertOrReplaceAsync(branding);
        OnDataChanged();
    }

    private void OnDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);
}
