// ============================================
// DatabaseService.cs
// ============================================
// PostgreSQL baglanti yonetimi servisi
// Connection pooling ve using bloklari ile guvenli baglanti yonetimi

using Npgsql;

namespace FaceID;

/// <summary>
/// PostgreSQL veritabani baglanti yonetimi servisi.
/// Tum baglanti islemleri using bloklari ile yapilir.
/// </summary>
public class DatabaseService
{
    // NOT: Password kismini kendi PostgreSQL sifrenizle degistirin
    private const string CONNECTION_STRING = "Host=localhost;Username=postgres;Password=postgres;Database=FaceID_DB";

    /// <summary>
    /// Veritabani baglantisi olusturur.
    /// </summary>
    /// <returns>NpgsqlConnection nesnesi</returns>
    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(CONNECTION_STRING);
    }

    /// <summary>
    /// Veritabani baglantisini test eder.
    /// </summary>
    /// <returns>Baglanti basarili ise true</returns>
    public bool TestConnection()
    {
        try
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Veritabani baglanti hatasi: {ex.Message}");
            return false;
        }
    }
}

