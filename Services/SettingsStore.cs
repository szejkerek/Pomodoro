using System.IO;
using System.Text.Json;
using Pomodoro.Models;

namespace Pomodoro.Services
{
    public sealed class SettingsStore : ISettingsStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private readonly string settingsFilePath;

        public SettingsStore()
        {
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string applicationDirectory = Path.Combine(appDataDirectory, "Pomodoro");
            Directory.CreateDirectory(applicationDirectory);
            settingsFilePath = Path.Combine(applicationDirectory, "settings.json");
        }

        public AppSettings Load()
        {
            if (File.Exists(settingsFilePath) == false)
            {
                return new AppSettings();
            }

            try
            {
                string json = File.ReadAllText(settingsFilePath);
                AppSettings? loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded is null)
                {
                    return new AppSettings();
                }

                // Tokens are stored encrypted (DPAPI); decrypt for runtime use. Legacy plaintext passes through.
                loaded.TodoistToken = TokenProtector.Unprotect(loaded.TodoistToken);
                loaded.ClickUpToken = TokenProtector.Unprotect(loaded.ClickUpToken);
                return loaded;
            }
            catch (JsonException)
            {
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            // Encrypt tokens in the file only; the live settings object keeps them in plaintext for the gateways.
            string todoistToken = settings.TodoistToken;
            string clickUpToken = settings.ClickUpToken;
            settings.TodoistToken = TokenProtector.Protect(todoistToken);
            settings.ClickUpToken = TokenProtector.Protect(clickUpToken);
            try
            {
                string json = JsonSerializer.Serialize(settings, SerializerOptions);
                File.WriteAllText(settingsFilePath, json);
            }
            finally
            {
                settings.TodoistToken = todoistToken;
                settings.ClickUpToken = clickUpToken;
            }
        }
    }
}
