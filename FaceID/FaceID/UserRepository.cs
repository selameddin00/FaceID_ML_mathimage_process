// ============================================
// UserRepository.cs
// ============================================
// Kullanici veritabani islemleri (Repository Pattern)
// TUM SQL sorgulari parametreli olarak yapilir (SQL injection korumasi)

using Npgsql;

namespace FaceID;

/// <summary>
/// Kullanici veritabani islemlerini yoneten repository sinifi.
/// Repository Pattern kullanilarak UI katmanindan ayrilmistir.
/// </summary>
public class UserRepository
{
    private readonly DatabaseService _databaseService;

    /// <summary>
    /// UserRepository constructor'i.
    /// </summary>
    /// <param name="databaseService">DatabaseService ornegi</param>
    public UserRepository(DatabaseService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
    }

    /// <summary>
    /// Users tablosunun var olup olmadigini kontrol eder, yoksa olusturur.
    /// </summary>
    public void EnsureUsersTableExists()
    {
        try
        {
            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();

                // Tablo var mi kontrol et
                using (var checkCmd = new NpgsqlCommand(
                    "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'users')",
                    connection))
                {
                    bool tableExists = (bool)(checkCmd.ExecuteScalar() ?? false);

                    if (!tableExists)
                    {
                        // Tablo yoksa olustur
                        using (var createCmd = new NpgsqlCommand(
                            @"CREATE TABLE Users (
                                Id SERIAL PRIMARY KEY,
                                Name TEXT NOT NULL,
                                Balance DECIMAL(18, 2) DEFAULT 0,
                                Role TEXT NOT NULL
                            )",
                            connection))
                        {
                            createCmd.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine("Users tablosu olusturuldu.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Users tablosu kontrol/olusturma hatasi: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// ID'ye gore kullanici bilgisini getirir.
    /// </summary>
    /// <param name="id">Kullanici ID</param>
    /// <returns>Kullanici bilgisi veya null</returns>
    public User? GetUserById(int id)
    {
        try
        {
            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();

                // Parametreli sorgu (SQL injection korumasi)
                using (var cmd = new NpgsqlCommand(
                    "SELECT Id, Name, Balance, Role FROM Users WHERE Id = @Id",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Balance = reader.GetDecimal(2),
                                Role = reader.GetString(3)
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Kullanici getirme hatasi (ID: {id}): {ex.Message}");
            return null;
        }

        return null;
    }

    /// <summary>
    /// Baslangic kullanici verisini ekler (seed data).
    /// ID=1, Name='Kral', Role='Admin'
    /// ON CONFLICT ile tekrar ekleme yapilmaz.
    /// </summary>
    public void SeedInitialUser()
    {
        try
        {
            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();

                // Parametreli sorgu ile INSERT ... ON CONFLICT DO NOTHING
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO Users (Id, Name, Balance, Role) 
                      VALUES (@Id, @Name, @Balance, @Role)
                      ON CONFLICT (Id) DO NOTHING",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@Id", 1);
                    cmd.Parameters.AddWithValue("@Name", "Kral");
                    cmd.Parameters.AddWithValue("@Balance", 0m);
                    cmd.Parameters.AddWithValue("@Role", "Admin");

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Baslangic kullanici verisi eklendi (Kral, Admin).");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Baslangic kullanici zaten mevcut.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Seed data ekleme hatasi: {ex.Message}");
            throw;
        }
    }
}

