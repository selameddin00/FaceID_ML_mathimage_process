// ============================================
// User.cs
// ============================================
// Kullanici veri modeli

namespace FaceID;

/// <summary>
/// Kullanici veri modeli.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Role { get; set; } = string.Empty;
}

