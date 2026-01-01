using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace FaceID;

/// <summary>
/// Yüz kayıt işlemlerini yöneten servis sınıfı.
/// State machine mantığı ile banka tipi kayıt senaryosunu gerçekleştirir.
/// SOLID prensiplerine uygun: Tek sorumluluk (yüz kayıt yönetimi).
/// </summary>
public class FaceRegistrationService : IDisposable
{
    private RegistrationState _currentState = RegistrationState.Idle;
    private int _currentUserId = 1;
    private int _photosTakenInCurrentState = 0;
    private const int PHOTOS_PER_STATE = 5;
    private readonly string _trainedFacesFolder;
    private bool _disposed = false;

    /// <summary>
    /// Mevcut kayıt durumunu döndürür.
    /// </summary>
    public RegistrationState CurrentState => _currentState;

    /// <summary>
    /// Mevcut durumda çekilen fotoğraf sayısını döndürür.
    /// </summary>
    public int PhotosTakenInCurrentState => _photosTakenInCurrentState;

    /// <summary>
    /// Kayıt başlatıldığında tetiklenen event.
    /// </summary>
    public event EventHandler<RegistrationState>? StateChanged;

    /// <summary>
    /// Fotoğraf çekildiğinde tetiklenen event.
    /// </summary>
    public event EventHandler<(int userId, RegistrationState state, int photoNumber, string filePath)>? PhotoTaken;

    /// <summary>
    /// Kayıt tamamlandığında tetiklenen event.
    /// </summary>
    public event EventHandler<int>? RegistrationCompleted;

    /// <summary>
    /// FaceRegistrationService constructor'ı.
    /// </summary>
    /// <param name="trainedFacesFolder">Kayıtlı yüzlerin saklanacağı klasör yolu (varsayılan: TrainedFaces)</param>
    public FaceRegistrationService(string trainedFacesFolder = "TrainedFaces")
    {
        _trainedFacesFolder = trainedFacesFolder;
        EnsureTrainedFacesFolderExists();
    }

    /// <summary>
    /// TrainedFaces klasörünün varlığını garanti eder.
    /// </summary>
    private void EnsureTrainedFacesFolderExists()
    {
        try
        {
            if (!Directory.Exists(_trainedFacesFolder))
            {
                Directory.CreateDirectory(_trainedFacesFolder);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"TrainedFaces klasoru olusturulamadi: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Kayıt sürecini başlatır.
    /// </summary>
    /// <param name="userId">Kaydedilecek kullanıcının ID'si (varsayılan: otomatik artırılır)</param>
    public void StartRegistration(int? userId = null)
    {
        if (_currentState != RegistrationState.Idle && _currentState != RegistrationState.Completed)
        {
            // Zaten bir kayıt süreci devam ediyorsa başlatma
            return;
        }

        // Yeni kullanıcı ID'si belirle
        if (userId.HasValue)
        {
            _currentUserId = userId.Value;
        }
        else
        {
            // Otomatik ID artırma (mevcut kullanıcıların sayısına göre)
            _currentUserId = GetNextUserId();
        }

        // İlk duruma geç
        _currentState = RegistrationState.LookingFront;
        _photosTakenInCurrentState = 0;

        OnStateChanged(_currentState);
    }

    /// <summary>
    /// Bir sonraki kullanıcı ID'sini belirler.
    /// </summary>
    private int GetNextUserId()
    {
        try
        {
            if (!Directory.Exists(_trainedFacesFolder))
            {
                return 1;
            }

            var files = Directory.GetFiles(_trainedFacesFolder, "User_*.bmp");
            if (files.Length == 0)
            {
                return 1;
            }

            int maxId = 0;
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                // Format: User_{ID}_{Sıra}
                var parts = fileName.Split('_');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int id))
                {
                    if (id > maxId)
                    {
                        maxId = id;
                    }
                }
            }

            return maxId + 1;
        }
        catch
        {
            return 1;
        }
    }

    /// <summary>
    /// Yüz algılandığında çağrılır. Fotoğraf çeker ve durumu günceller.
    /// </summary>
    /// <param name="faceBitmap">Tespit edilen yüzün Bitmap görüntüsü</param>
    /// <returns>Fotoğraf başarıyla çekildiyse true, aksi halde false</returns>
    public bool ProcessDetectedFace(Bitmap faceBitmap)
    {
        if (_currentState == RegistrationState.Idle || _currentState == RegistrationState.Completed)
        {
            return false;
        }

        if (faceBitmap == null)
        {
            return false;
        }

        try
        {
            // Yüzü işle ve kaydet
            string filePath = ProcessAndSaveFace(faceBitmap, _currentUserId, _currentState, _photosTakenInCurrentState + 1);
            
            _photosTakenInCurrentState++;

            // Event'i tetikle
            OnPhotoTaken(_currentUserId, _currentState, _photosTakenInCurrentState, filePath);

            // Bu durumda yeterli fotoğraf çekildi mi kontrol et
            if (_photosTakenInCurrentState >= PHOTOS_PER_STATE)
            {
                MoveToNextState();
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Bir sonraki duruma geçer.
    /// </summary>
    private void MoveToNextState()
    {
        _photosTakenInCurrentState = 0;

        switch (_currentState)
        {
            case RegistrationState.LookingFront:
                _currentState = RegistrationState.LookingRight;
                break;

            case RegistrationState.LookingRight:
                _currentState = RegistrationState.LookingLeft;
                break;

            case RegistrationState.LookingLeft:
                _currentState = RegistrationState.Completed;
                OnRegistrationCompleted(_currentUserId);
                break;

            default:
                _currentState = RegistrationState.Idle;
                break;
        }

        OnStateChanged(_currentState);
    }

    /// <summary>
    /// Yüz görüntüsünü işler (gri tona çevir, 100x100'e boyutlandır) ve kaydeder.
    /// </summary>
    /// <param name="faceBitmap">İşlenecek yüz görüntüsü</param>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="state">Kayıt durumu</param>
    /// <param name="sequence">Fotoğraf sıra numarası (state içinde)</param>
    /// <returns>Kaydedilen dosyanın tam yolu</returns>
    private string ProcessAndSaveFace(Bitmap faceBitmap, int userId, RegistrationState state, int sequence)
    {
        try
        {
            // Genel sıra numarasını hesapla (LookingFront: 1-5, LookingRight: 6-10, LookingLeft: 11-15)
            int globalSequence = state switch
            {
                RegistrationState.LookingFront => sequence,
                RegistrationState.LookingRight => PHOTOS_PER_STATE + sequence,
                RegistrationState.LookingLeft => (PHOTOS_PER_STATE * 2) + sequence,
                _ => sequence
            };

            // Dosya adını oluştur: User_{ID}_{Sıra}.bmp
            string fileName = $"User_{userId}_{globalSequence}.bmp";
            string filePath = Path.Combine(_trainedFacesFolder, fileName);

            // Yüzü işle: Gri tona çevir ve 100x100'e boyutlandır
            using (Mat originalMat = faceBitmap.ToMat())
            {
                // Gri tona çevir
                Mat grayMat = new Mat();
                if (originalMat.NumberOfChannels == 3)
                {
                    CvInvoke.CvtColor(originalMat, grayMat, ColorConversion.Bgr2Gray);
                }
                else
                {
                    originalMat.CopyTo(grayMat);
                }

                // 100x100'e boyutlandır
                using (grayMat)
                {
                    Mat resizedMat = new Mat();
                    CvInvoke.Resize(grayMat, resizedMat, new System.Drawing.Size(100, 100), interpolation: Inter.Linear);

                    using (resizedMat)
                    {
                        // Bitmap'e dönüştür ve kaydet
                        Bitmap finalBitmap = resizedMat.ToBitmap();
                        finalBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);
                        finalBitmap.Dispose();
                    }
                }
            }

            return filePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Yuz goruntusu islenirken hata olustu: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Kayıt sürecini sıfırlar (Idle durumuna döner).
    /// </summary>
    public void Reset()
    {
        _currentState = RegistrationState.Idle;
        _photosTakenInCurrentState = 0;
        OnStateChanged(_currentState);
    }

    /// <summary>
    /// State değiştiğinde event'i tetikler.
    /// </summary>
    private void OnStateChanged(RegistrationState newState)
    {
        StateChanged?.Invoke(this, newState);
    }

    /// <summary>
    /// Fotoğraf çekildiğinde event'i tetikler.
    /// </summary>
    private void OnPhotoTaken(int userId, RegistrationState state, int photoNumber, string filePath)
    {
        PhotoTaken?.Invoke(this, (userId, state, photoNumber, filePath));
    }

    /// <summary>
    /// Kayıt tamamlandığında event'i tetikler.
    /// </summary>
    private void OnRegistrationCompleted(int userId)
    {
        RegistrationCompleted?.Invoke(this, userId);
    }

    /// <summary>
    /// Durum mesajını döndürür (UI'da gösterilmek üzere).
    /// </summary>
    public string GetStateMessage()
    {
        return _currentState switch
        {
            RegistrationState.Idle => "Kayıt başlatmak için 'Kayıt Başlat' butonuna basın.",
            RegistrationState.LookingFront => $"Lutfen kameraya DUZ bakin ({_photosTakenInCurrentState}/{PHOTOS_PER_STATE})",
            RegistrationState.LookingRight => $"Lutfen kafanizi hafifce SAGA cevirin ({_photosTakenInCurrentState}/{PHOTOS_PER_STATE})",
            RegistrationState.LookingLeft => $"Lutfen kafanizi hafifce SOLA cevirin ({_photosTakenInCurrentState}/{PHOTOS_PER_STATE})",
            RegistrationState.Completed => $"Kayıt Basariyla Tamamlandi! Kullanici ID: {_currentUserId}",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Kaynakları serbest bırakır (IDisposable implementasyonu).
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose metodu.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Yönetilen kaynakları temizle (şu an gerekli değil ama gelecekte kullanılabilir)
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer (ek güvenlik için).
    /// </summary>
    ~FaceRegistrationService()
    {
        Dispose(false);
    }
}

