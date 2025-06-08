public class SettingsService : ISettingsService
{
    private readonly AmsDbContext _context;

    public SettingsService(AmsDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task<bool> UpdateValueAsync(string key, string value)
    {
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null)
        {
            _context.Settings.Add(new Setting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
        }
        await _context.SaveChangesAsync();
        return true;
    }
}


