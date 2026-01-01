using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;

namespace FaceID;

/// <summary>
/// Yuz tanima islemlerini yoneten servis sinifi.
/// LBPH (Local Binary Patterns Histograms) algoritmasi kullanir.
/// SOLID prensiplerine uygun: Tek sorumluluk (yuz tanima).
/// </summary>
public class FaceRecognitionService : IDisposable
{
    private LBPHFaceRecognizer? _faceRecognizer;
    private bool _isTrained = false;
    private bool _disposed = false;
    private const double THRESHOLD = 100.0;
    private readonly string _trainedFacesFolder;

    /// <summary>
    /// Modelin egitilip egitilmedigini gosterir.
    /// </summary>
    public bool IsTrained => _isTrained;

    /// <summary>
    /// FaceRecognitionService constructor'i.
    /// </summary>
    /// <param name="trainedFacesFolder">Egitim verilerinin bulundugu klasor yolu (varsayilan: TrainedFaces)</param>
    public FaceRecognitionService(string trainedFacesFolder = "TrainedFaces")
    {
        _trainedFacesFolder = trainedFacesFolder;
        
        // LBPHFaceRecognizer'i baslat
        try
        {
            _faceRecognizer = new LBPHFaceRecognizer(radius: 1, neighbors: 8, gridX: 8, gridY: 8);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LBPHFaceRecognizer baslatilamadi: {ex.Message}");
            _faceRecognizer = null;
        }
    }

    /// <summary>
    /// Modeli egitir. TrainedFaces klasorundeki tum yuz goruntulerini okur ve egitir.
    /// Klasor yoksa veya bos ise egitim atlanir, uygulama cokmez.
    /// </summary>
    public void TrainModel()
    {
        if (_faceRecognizer == null)
        {
            System.Diagnostics.Debug.WriteLine("LBPHFaceRecognizer mevcut degil, egitim yapilamadi.");
            return;
        }

        try
        {
            // Klasor kontrolu
            if (!Directory.Exists(_trainedFacesFolder))
            {
                System.Diagnostics.Debug.WriteLine("TrainedFaces klasoru bulunamadi. Egitilecek veri yok.");
                _isTrained = false;
                return;
            }

            // Klasordeki dosyalari al
            var files = Directory.GetFiles(_trainedFacesFolder, "*.bmp")
                .Concat(Directory.GetFiles(_trainedFacesFolder, "*.jpg"))
                .Concat(Directory.GetFiles(_trainedFacesFolder, "*.png"))
                .ToArray();

            if (files.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("TrainedFaces klasoru bos. Egitilecek veri yok.");
                _isTrained = false;
                return;
            }

            // Veri listelerini hazirla
            List<Mat> images = new List<Mat>();
            List<int> labels = new List<int>();

            // Her dosyayi oku ve label cikar
            foreach (var filePath in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    
                    // Dosya adindan ID bilgisini cikar: User_{ID}_{Sira}.bmp -> Label = ID
                    // Ornek: User_1_5.bmp -> Label = 1
                    var parts = fileName.Split('_');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int userId))
                    {
                        // Goruntuyu oku
                        using (Mat originalMat = CvInvoke.Imread(filePath, ImreadModes.Grayscale))
                        {
                            if (!originalMat.IsEmpty)
                            {
                                // Grayscale'e cevir (zaten grayscale olabilir ama emin olmak icin)
                                Mat grayMat = new Mat();
                                if (originalMat.NumberOfChannels == 3)
                                {
                                    CvInvoke.CvtColor(originalMat, grayMat, ColorConversion.Bgr2Gray);
                                }
                                else
                                {
                                    originalMat.CopyTo(grayMat);
                                }

                                // 100x100'e boyutlandir (egitim verileri standart boyutta olmali)
                                Mat resizedMat = new Mat();
                                CvInvoke.Resize(grayMat, resizedMat, new System.Drawing.Size(100, 100), interpolation: Inter.Linear);
                                
                                // Listeye ekle (kopya olustur)
                                Mat imageCopy = new Mat();
                                resizedMat.CopyTo(imageCopy);
                                images.Add(imageCopy);
                                labels.Add(userId);

                                // Gecici mat'leri temizle
                                grayMat.Dispose();
                                resizedMat.Dispose();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Dosya okuma hatasi - sessizce atla
                    System.Diagnostics.Debug.WriteLine($"Dosya okunurken hata: {filePath} - {ex.Message}");
                }
            }

            // Egitim verisi yoksa cik
            if (images.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Egitilecek gecerli veri bulunamadi.");
                _isTrained = false;
                return;
            }

            // Modeli egit
            _faceRecognizer.Train(images.ToArray(), labels.ToArray());
            _isTrained = true;

            System.Diagnostics.Debug.WriteLine($"Model basariyla egitildi. {images.Count} goruntu, {labels.Distinct().Count()} kullanici.");

            // Egitim icin kullanilan Mat'leri temizle
            foreach (var img in images)
            {
                img.Dispose();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Model egitilirken hata olustu: {ex.Message}");
            _isTrained = false;
        }
    }

    /// <summary>
    /// Yuz goruntusunu tanir ve sonucu dondurur.
    /// </summary>
    /// <param name="faceImage">Tanimlanacak yuz goruntusu (Bitmap)</param>
    /// <returns>Tanima sonucu (PredictedId, Distance, IsRecognized)</returns>
    public (int PredictedId, double Distance, bool IsRecognized) RecognizeFace(Bitmap faceImage)
    {
        if (!_isTrained || _faceRecognizer == null || faceImage == null)
        {
            return (-1, double.MaxValue, false);
        }

        try
        {
            // Bitmap'i Mat'e cevir
            using (Mat originalMat = faceImage.ToMat())
            {
                // Grayscale'e cevir
                Mat grayMat = new Mat();
                if (originalMat.NumberOfChannels == 3)
                {
                    CvInvoke.CvtColor(originalMat, grayMat, ColorConversion.Bgr2Gray);
                }
                else
                {
                    originalMat.CopyTo(grayMat);
                }

                // 100x100'e boyutlandir (egitim verileriyle ayni boyutta olmali)
                using (grayMat)
                {
                    Mat resizedMat = new Mat();
                    CvInvoke.Resize(grayMat, resizedMat, new System.Drawing.Size(100, 100), interpolation: Inter.Linear);

                    using (resizedMat)
                    {
                        // Tahmin yap
                        FaceRecognizer.PredictionResult result = _faceRecognizer.Predict(resizedMat);

                        int predictedId = result.Label;
                        double distance = result.Distance;

                        // Threshold karar mantigi
                        // Distance 0'a ne kadar yakinsa eslesme o kadar iyidir
                        bool isRecognized = distance <= THRESHOLD;

                        return (predictedId, distance, isRecognized);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Yuz tanima hatasi: {ex.Message}");
            return (-1, double.MaxValue, false);
        }
    }

    /// <summary>
    /// Kaynaklari serbest birakir (IDisposable implementasyonu).
    /// Memory leak'i onlemek icin kritik.
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
                // Yonetilen kaynaklari temizle
                if (_faceRecognizer != null)
                {
                    _faceRecognizer.Dispose();
                    _faceRecognizer = null;
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer (ek guvenlik icin).
    /// </summary>
    ~FaceRecognitionService()
    {
        Dispose(false);
    }
}

