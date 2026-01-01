// ============================================
// Program.cs
// ============================================
// Uygulamanın giriş noktası.
// .NET 8 Windows Forms Banka Yüz Tanıma Sistemi

namespace FaceID;

static class Program
{
    /// <summary>
    /// Uygulamanın ana giriş noktası.
    /// Veritabani baslatma ve seed islemi yapilir.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Uygulama yapılandırmasını başlat (DPI ayarları, varsayılan font vb.)
        ApplicationConfiguration.Initialize();
        
        // Veritabani baslatma ve seed islemi
        try
        {
            var databaseService = new DatabaseService();
            var userRepository = new UserRepository(databaseService);
            
            // Users tablosunu olustur
            userRepository.EnsureUsersTableExists();
            
            // Seed data ekle
            userRepository.SeedInitialUser();
            
            System.Diagnostics.Debug.WriteLine("Veritabani baslatma basarili.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Veritabani baslatma hatasi: {ex.Message}");
            // Hata durumunda uygulama yine de calisir (kullaniciya gosterilmez)
        }
        
        // Ana formu başlat ve uygulamayı çalıştır
        Application.Run(new Form1());
    }    
}