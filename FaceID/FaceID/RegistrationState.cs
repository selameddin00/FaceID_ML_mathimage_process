// ============================================
// RegistrationState.cs
// ============================================
// Kayıt sürecindeki durumları tanımlayan enum

namespace FaceID;

/// <summary>
/// Yüz kayıt sürecindeki durumları belirten enum.
/// Banka tipi kayıt senaryosu için kullanılır.
/// </summary>
public enum RegistrationState
{
    /// <summary>
    /// Başlangıç durumu - kayıt başlamamış.
    /// </summary>
    Idle,

    /// <summary>
    /// Kullanıcıdan düz bakması isteniyor (5 fotoğraf çekilecek).
    /// </summary>
    LookingFront,

    /// <summary>
    /// Kullanıcıdan sağa bakması isteniyor (5 fotoğraf çekilecek).
    /// </summary>
    LookingRight,

    /// <summary>
    /// Kullanıcıdan sola bakması isteniyor (5 fotoğraf çekilecek).
    /// </summary>
    LookingLeft,

    /// <summary>
    /// Kayıt başarıyla tamamlandı.
    /// </summary>
    Completed
}

