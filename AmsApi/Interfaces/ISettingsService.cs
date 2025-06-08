namespace AmsApi.Interfaces
{
    public interface ISettingsService
    {
        Task<string?> GetValueAsync(string key);
        Task<bool> UpdateValueAsync(string key, string value);
    }

}

