using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace FaceID
{
    /// <summary>
    /// Kamera işlemlerini yöneten servis sınıfı.
    /// SOLID prensiplerine uygun olarak UI'dan bağımsız çalışır.
    /// </summary>
    public class CameraService : IDisposable
    {
        private VideoCapture? _videoCapture;
        private bool _isCapturing = false;
        private bool _disposed = false;

        /// <summary>
        /// Kameradan görüntü alınıp alınmadığını gösterir.
        /// </summary>
        public bool IsCapturing => _isCapturing;

        /// <summary>
        /// Yeni frame yakalandığında tetiklenen event.
        /// </summary>
        public event EventHandler<Bitmap>? FrameReady;

        /// <summary>
        /// Hata oluştuğunda tetiklenen event.
        /// </summary>
        public event EventHandler<string>? ErrorOccurred;

        /// <summary>
        /// Kamerayı başlatır.
        /// </summary>
        /// <returns>Başarılı olursa true, aksi halde false döner.</returns>
        public bool StartCamera()
        {
            try
            {
                // Eğer kamera zaten açıksa önce kapat
                if (_isCapturing)
                {
                    StopCamera();
                }

                // VideoCapture nesnesini oluştur (0 = varsayılan kamera)
                _videoCapture = new VideoCapture(0, VideoCapture.API.DShow);

                // Kamera bağlantısını kontrol et
                if (_videoCapture == null || !_videoCapture.IsOpened)
                {
                    OnErrorOccurred("Kamera bulunamadı veya açılamadı!");
                    return false;
                }

                // Kamera ayarlarını yapılandır
                _videoCapture.Set(CapProp.FrameWidth, 640);
                _videoCapture.Set(CapProp.FrameHeight, 480);
                _videoCapture.Set(CapProp.Fps, 30);

                _isCapturing = true;
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Kamera başlatılırken hata oluştu: {ex.Message}");
                StopCamera(); // Güvenli kapanış
                return false;
            }
        }

        /// <summary>
        /// Kamerayı durdurur ve kaynakları serbest bırakır.
        /// </summary>
        public void StopCamera()
        {
            try
            {
                _isCapturing = false;

                // VideoCapture nesnesini güvenli şekilde dispose et
                if (_videoCapture != null)
                {
                    _videoCapture.Stop();
                    _videoCapture.Dispose();
                    _videoCapture = null;
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Kamera durdurulurken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Kameradan bir frame yakalar ve Bitmap olarak döner.
        /// Application.Idle event'i içinde çağrılmalıdır.
        /// </summary>
        /// <returns>Yakalanan frame, hata olursa null</returns>
        public Bitmap? CaptureFrame()
        {
            // Kamera açık değilse null döndür
            if (!_isCapturing || _videoCapture == null || !_videoCapture.IsOpened)
            {
                return null;
            }

            try
            {
                // Yeni bir Mat nesnesi oluştur
                using (Mat frame = new Mat())
                {
                    // Frame'i oku
                    bool success = _videoCapture.Read(frame);

                    // Okuma başarısızsa null döndür
                    if (!success || frame.IsEmpty)
                    {
                        return null;
                    }

                    // Mat'i Bitmap'e dönüştür
                    Bitmap bitmap = frame.ToBitmap();

                    // FrameReady event'ini tetikle (UI güncellemesi için)
                    FrameReady?.Invoke(this, bitmap);

                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Frame yakalanırken hata oluştu: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Hata event'ini güvenli şekilde tetikler.
        /// </summary>
        private void OnErrorOccurred(string errorMessage)
        {
            ErrorOccurred?.Invoke(this, errorMessage);
        }

        /// <summary>
        /// Kaynakları serbest bırakır (IDisposable implementasyonu).
        /// Memory leak'i önlemek için kritik.
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
                    // Yönetilen kaynakları temizle
                    StopCamera();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer (ek güvenlik için).
        /// </summary>
        ~CameraService()
        {
            Dispose(false);
        }
    }
}
