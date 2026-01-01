using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace FaceID;

/// <summary>
/// Yüz algılama işlemlerini yöneten servis sınıfı.
/// Haar Cascade kullanarak yüz tespiti yapar.
/// SOLID prensiplerine uygun: Tek sorumluluk (yüz algılama).
/// </summary>
public class FaceDetectionService : IDisposable
{
    private CascadeClassifier? _faceCascade;
    private bool _disposed = false;
    private readonly string _cascadeFilePath;

    /// <summary>
    /// FaceDetectionService constructor'ı.
    /// </summary>
    /// <param name="cascadeFilePath">Haar Cascade XML dosyasının yolu (varsayılan: haarcascade_frontalface_default.xml)</param>
    public FaceDetectionService(string cascadeFilePath = "haarcascade_frontalface_default.xml")
    {
        _cascadeFilePath = cascadeFilePath;
        LoadCascade();
    }

    /// <summary>
    /// Haar Cascade dosyasını yükler.
    /// </summary>
    private void LoadCascade()
    {
        try
        {
            // Cascade dosyasının varlığını kontrol et
            if (!File.Exists(_cascadeFilePath))
            {
                throw new FileNotFoundException($"Haar Cascade dosyası bulunamadı: {_cascadeFilePath}\n" +
                    "Lutfen dosyanin bin/Debug klasorunde oldugundan emin olun!");
            }

            // CascadeClassifier'ı oluştur
            _faceCascade = new CascadeClassifier(_cascadeFilePath);

            if (_faceCascade == null)
            {
                throw new InvalidOperationException("Haar Cascade dosyasi yuklenemedi veya gecersiz!");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Yuz algilama servisi baslatilamadi: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Görüntüdeki yüzleri tespit eder.
    /// </summary>
    /// <param name="image">Tespit yapılacak görüntü (RGB veya GrayScale)</param>
    /// <returns>Tespit edilen yüzlerin dikdörtgen koordinatları listesi</returns>
    public List<Rectangle> DetectFaces(Mat image)
    {
        if (_faceCascade == null)
        {
            return new List<Rectangle>();
        }

        try
        {
            // Görüntüyü gri tona çevir (yüz algılama için gerekli)
            Mat grayImage = new Mat();
            if (image.NumberOfChannels == 3)
            {
                CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);
            }
            else
            {
                image.CopyTo(grayImage);
            }

            // Yüzleri tespit et
            using (grayImage)
            {
                Rectangle[] faces = _faceCascade.DetectMultiScale(
                    grayImage,
                    scaleFactor: 1.1,
                    minNeighbors: 5,
                    minSize: new System.Drawing.Size(30, 30)
                );

                return new List<Rectangle>(faces);
            }
        }
        catch (Exception)
        {
            // Hata durumunda boş liste döndür
            return new List<Rectangle>();
        }
    }

    /// <summary>
    /// Bitmap üzerinde yüz algılama yapar.
    /// </summary>
    /// <param name="bitmap">Tespit yapılacak Bitmap görüntü</param>
    /// <returns>Tespit edilen yüzlerin dikdörtgen koordinatları listesi</returns>
    public List<Rectangle> DetectFaces(Bitmap bitmap)
    {
        if (bitmap == null)
        {
            return new List<Rectangle>();
        }

        try
        {
            using (Mat image = bitmap.ToMat())
            {
                return DetectFaces(image);
            }
        }
        catch (Exception)
        {
            return new List<Rectangle>();
        }
    }

    /// <summary>
    /// Görüntü üzerine yüz dikdörtgenlerini çizer.
    /// </summary>
    /// <param name="image">Çizim yapılacak görüntü</param>
    /// <param name="faces">Çizilecek yüz dikdörtgenleri</param>
    /// <param name="color">Dikdörtgen rengi (varsayılan: Yeşil)</param>
    /// <param name="thickness">Çizgi kalınlığı (varsayılan: 2)</param>
    public void DrawFaces(Mat image, List<Rectangle> faces, MCvScalar color, int thickness = 2)
    {
        if (image == null || faces == null || faces.Count == 0)
        {
            return;
        }

        try
        {
            foreach (var face in faces)
            {
                CvInvoke.Rectangle(image, face, color, thickness);
            }
        }
        catch (Exception)
        {
            // Çizim hatası - sessizce atlanır
        }
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
                // Yönetilen kaynakları temizle
                if (_faceCascade != null)
                {
                    _faceCascade.Dispose();
                    _faceCascade = null;
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer (ek güvenlik için).
    /// </summary>
    ~FaceDetectionService()
    {
        Dispose(false);
    }
}

