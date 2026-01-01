// ============================================
// Form1.cs
// ============================================
// Ana form sınıfı - UI işlemleri ve CameraService entegrasyonu
// SOLID prensiplerine uygun: UI iş mantığından ayrıştırılmış

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace FaceID;

public partial class Form1 : Form
{
    private CameraService? _cameraService;
    private FaceDetectionService? _faceDetectionService;
    private FaceRegistrationService? _faceRegistrationService;
    private FaceRecognitionService? _faceRecognitionService;
    private UserRepository? _userRepository;
    private bool _isCameraRunning = false;
    private int _frameCounter = 0;
    private const int RECOGNITION_FRAME_INTERVAL = 10; // Her 10 frame'de bir tanima yap
    private bool _isLoginInProgress = false; // Login debounce kontrolu
    private const double LOGIN_SECURITY_THRESHOLD = 80.0; // Distance < 80 guvenlik kriteri

    /// <summary>
    /// Form constructor'ı.
    /// </summary>
    public Form1()
    {
        InitializeComponent();
        
        // CameraService'i oluştur
        _cameraService = new CameraService();
        
        // CameraService event'lerini bağla
        _cameraService.FrameReady += CameraService_FrameReady;
        _cameraService.ErrorOccurred += CameraService_ErrorOccurred;
        
        // FaceDetectionService'i oluştur
        try
        {
            _faceDetectionService = new FaceDetectionService("haarcascade_frontalface_default.xml");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Yuz algilama servisi baslatilamadi: {ex.Message}\n\n" +
                "Lutfen 'haarcascade_frontalface_default.xml' dosyasinin bin/Debug klasorunde oldugundan emin olun!",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        // FaceRegistrationService'i oluştur
        try
        {
            _faceRegistrationService = new FaceRegistrationService("TrainedFaces");
            _faceRegistrationService.StateChanged += FaceRegistrationService_StateChanged;
            _faceRegistrationService.RegistrationCompleted += FaceRegistrationService_RegistrationCompleted;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kayit servisi baslatilamadi: {ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        // FaceRecognitionService'i oluştur
        try
        {
            _faceRecognitionService = new FaceRecognitionService("TrainedFaces");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Yuz tanima servisi baslatilamadi: {ex.Message}");
        }
        
        // UserRepository'yi oluştur (DatabaseService ile)
        try
        {
            var databaseService = new DatabaseService();
            _userRepository = new UserRepository(databaseService);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UserRepository olusturulamadi: {ex.Message}");
        }
        
        // Application.Idle event'ini bağla - UI thread'i kilitlemeyecek şekilde frame yakalama
        Application.Idle += Application_Idle;
        
        // Form Load event'ini bağla - modeli egit
        this.Load += Form1_Load;
    }

    /// <summary>
    /// Form Load event handler'i.
    /// TrainedFaces klasoru doluysa sessizce modeli egitir.
    /// </summary>
    private void Form1_Load(object? sender, EventArgs e)
    {
        // TrainedFaces klasorunu kontrol et
        if (Directory.Exists("TrainedFaces"))
        {
            var files = Directory.GetFiles("TrainedFaces", "*.bmp")
                .Concat(Directory.GetFiles("TrainedFaces", "*.jpg"))
                .Concat(Directory.GetFiles("TrainedFaces", "*.png"))
                .ToArray();

            if (files.Length > 0 && _faceRecognitionService != null)
            {
                // Sessizce modeli egit (kullaniciya popup gosterme)
                _faceRecognitionService.TrainModel();
            }
        }
    }

    /// <summary>
    /// Application.Idle event handler'ı.
    /// UI thread boşta kaldığında frame yakalar.
    /// Timer kullanmak yerine bu yaklaşım tercih edilir (daha verimli).
    /// </summary>
    private void Application_Idle(object? sender, EventArgs e)
    {
        // Kamera çalışıyorsa frame yakala
        if (_isCameraRunning && _cameraService != null)
        {
            _cameraService.CaptureFrame();
        }
    }

    /// <summary>
    /// Kamerayı başlat/durdur butonu click event handler'ı.
    /// </summary>
    private void buttonStartStop_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!_isCameraRunning)
            {
                // Kamerayı başlat
                if (_cameraService != null && _cameraService.StartCamera())
                {
                    _isCameraRunning = true;
                    buttonStartStop.Text = "Kamerayı Durdur";
                }
            }
            else
            {
                // Kamerayı durdur
                if (_cameraService != null)
                {
                    _cameraService.StopCamera();
                    _isCameraRunning = false;
                    buttonStartStop.Text = "Kamerayı Başlat";
                    
                    // PictureBox'ı temizle
                    if (pictureBoxCamera.Image != null)
                    {
                        pictureBoxCamera.Image.Dispose();
                        pictureBoxCamera.Image = null;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// CameraService'ten frame geldiğinde çağrılır.
    /// UI thread'inde güvenli şekilde PictureBox'ı günceller.
    /// </summary>
    private void CameraService_FrameReady(object? sender, Bitmap frame)
    {
        // UI güncellemesi için Invoke kontrolü yap
        if (InvokeRequired)
        {
            BeginInvoke(new Action<Bitmap>(UpdatePictureBox), frame);
        }
        else
        {
            UpdatePictureBox(frame);
        }
    }

    /// <summary>
    /// PictureBox'ı yeni frame ile günceller.
    /// Yüz algılama yapılır ve dikdörtgen çizilir.
    /// Kayıt modunda değilse yüz tanıma yapılır (performans için belirli aralıklarla).
    /// Önceki görüntüyü dispose ederek memory leak'i önler.
    /// </summary>
    private void UpdatePictureBox(Bitmap newFrame)
    {
        Bitmap? displayFrame = null;
        
        try
        {
            // Frame sayacını artır (performans optimizasyonu için)
            _frameCounter++;
            
            // Yüz algılama servisi aktifse yüzleri tespit et ve çiz
            if (_faceDetectionService != null && _faceRegistrationService != null)
            {
                // Sadece kayıt süreci devam ediyorsa yüz algılama yap
                if (_faceRegistrationService.CurrentState != RegistrationState.Idle &&
                    _faceRegistrationService.CurrentState != RegistrationState.Completed)
                {
                    // Yüzleri tespit et
                    var faces = _faceDetectionService.DetectFaces(newFrame);
                    
                    if (faces.Count > 0)
                    {
                        // Frame'i Mat'e dönüştür (kopya oluştur)
                        using (Mat frameMat = newFrame.ToMat())
                        {
                            // Yüz dikdörtgenlerini çiz (yeşil renk)
                            _faceDetectionService.DrawFaces(frameMat, faces, new MCvScalar(0, 255, 0), 2);
                            
                            // Mat'i Bitmap'e dönüştür (yeni Bitmap oluştur)
                            displayFrame = frameMat.ToBitmap();
                            
                            // İlk yüzü al ve kayıt servisine gönder
                            var firstFace = faces[0];
                            // Yüz bölgesini kırp (GetSubRect kullanarak)
                            using (Mat faceMat = new Mat(frameMat, firstFace))
                            {
                                Bitmap faceBitmap = faceMat.ToBitmap();
                                _faceRegistrationService.ProcessDetectedFace(faceBitmap);
                            }
                        }
                    }
                }
                // Kayıt modunda değilse yüz tanıma yap (performans için belirli aralıklarla)
                else if (_faceRecognitionService != null && _faceRecognitionService.IsTrained)
                {
                    // Her N frame'de bir tanıma yap (performans optimizasyonu)
                    if (_frameCounter % RECOGNITION_FRAME_INTERVAL == 0)
                    {
                        // Yüzleri tespit et
                        var faces = _faceDetectionService.DetectFaces(newFrame);
                        
                        if (faces.Count > 0)
                        {
                            // Frame'i Mat'e dönüştür (kopya oluştur)
                            using (Mat frameMat = newFrame.ToMat())
                            {
                                // Yüz dikdörtgenlerini çiz (mavi renk - tanıma modu)
                                _faceDetectionService.DrawFaces(frameMat, faces, new MCvScalar(255, 0, 0), 2);
                                
                                // İlk yüzü al ve tanıma yap
                                var firstFace = faces[0];
                                using (Mat faceMat = new Mat(frameMat, firstFace))
                                {
                                    Bitmap faceBitmap = faceMat.ToBitmap();
                                    
                                    // Yüz tanıma yap (senkron - zaten her 10 frame'de bir yapılıyor)
                                    var recognitionResult = _faceRecognitionService.RecognizeFace(faceBitmap);
                                    
                                    // Tanıma sonucunu görüntü üzerine çiz
                                    DrawRecognitionResult(frameMat, firstFace, recognitionResult);
                                    
                                    // Login kontrolu (debounce ve guvenlik kriteri)
                                    if (!_isLoginInProgress && recognitionResult.IsRecognized && recognitionResult.Distance < LOGIN_SECURITY_THRESHOLD)
                                    {
                                        HandleLogin(recognitionResult.PredictedId);
                                    }
                                    
                                    faceBitmap.Dispose();
                                }
                                
                                // Mat'i Bitmap'e dönüştür (yeni Bitmap oluştur)
                                displayFrame = frameMat.ToBitmap();
                            }
                        }
                    }
                }
            }
            
            // Eğer yüz algılama yapılmadıysa orijinal frame'i kullan
            if (displayFrame == null)
            {
                displayFrame = newFrame;
            }
            
            // Önceki görüntüyü temizle (memory leak önleme)
            if (pictureBoxCamera.Image != null)
            {
                pictureBoxCamera.Image.Dispose();
            }
            
            // Yeni görüntüyü ayarla
            pictureBoxCamera.Image = displayFrame;
        }
        catch (Exception ex)
        {
            // UI güncelleme hatası - sessizce logla (kullanıcıyı rahatsız etme)
            System.Diagnostics.Debug.WriteLine($"PictureBox güncellenirken hata: {ex.Message}");
            
            // Hata durumunda displayFrame'i temizle
            if (displayFrame != null && displayFrame != newFrame)
            {
                try
                {
                    displayFrame.Dispose();
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Tanıma sonucunu görüntü üzerine çizer.
    /// Veritabanından kullanıcı bilgisi çekilir ve gösterilir.
    /// </summary>
    private void DrawRecognitionResult(Mat frameMat, Rectangle faceRect, (int PredictedId, double Distance, bool IsRecognized) result)
    {
        try
        {
            string text;
            MCvScalar color;

            if (result.IsRecognized && result.Distance < LOGIN_SECURITY_THRESHOLD)
            {
                // Tanındı - Veritabanından kullanıcı bilgisi çek
                User? user = _userRepository?.GetUserById(result.PredictedId);
                
                if (user != null)
                {
                    // Kullanıcı bilgisi bulundu - İsim ve Rol göster
                    text = $"{user.Name} ({user.Role})";
                    color = new MCvScalar(0, 255, 0); // Yeşil
                }
                else
                {
                    // Kullanıcı bulunamadı - ID göster
                    text = $"ID: {result.PredictedId}";
                    color = new MCvScalar(0, 255, 0); // Yeşil
                }
            }
            else
            {
                // Tanınmadı veya güvenlik kriteri sağlanmadı
                text = "Bilinmiyor";
                color = new MCvScalar(0, 0, 255); // Kırmızı
            }

            // Metni yüzün altına yaz
            System.Drawing.Point textPosition = new System.Drawing.Point(
                faceRect.X,
                faceRect.Y + faceRect.Height + 20
            );

            CvInvoke.PutText(
                frameMat,
                text,
                textPosition,
                FontFace.HersheySimplex,
                0.6,
                color,
                2
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Tanima sonucu cizilirken hata: {ex.Message}");
        }
    }

    /// <summary>
    /// Login akışını yönetir (debounce ve thread-safe form geçişi).
    /// </summary>
    /// <param name="userId">Tanınan kullanıcı ID</param>
    private void HandleLogin(int userId)
    {
        // Login debounce kontrolu
        if (_isLoginInProgress)
        {
            return;
        }

        _isLoginInProgress = true;

        // Veritabanından kullanıcı bilgisi çek (asenkron olarak)
        Task.Run(() =>
        {
            try
            {
                User? user = _userRepository?.GetUserById(userId);
                
                if (user != null)
                {
                    // 2 saniye bekle
                    Thread.Sleep(2000);
                    
                    // Thread-safe form geçişi
                    if (InvokeRequired)
                    {
                        BeginInvoke(new Action<User>(OpenDashboard), user);
                    }
                    else
                    {
                        OpenDashboard(user);
                    }
                }
                else
                {
                    // Kullanıcı bulunamadı - login flag'ini sıfırla
                    _isLoginInProgress = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login akisi hatasi: {ex.Message}");
                _isLoginInProgress = false;
            }
        });
    }

    /// <summary>
    /// Dashboard formunu açar (thread-safe).
    /// </summary>
    /// <param name="user">Kullanıcı bilgisi</param>
    private void OpenDashboard(User user)
    {
        try
        {
            // Form1'i gizle
            this.Hide();
            
            // Dashboard formunu oluştur ve göster
            var dashboardForm = new DashboardForm(user.Name, user.Balance);
            dashboardForm.FormClosed += (s, e) =>
            {
                // Dashboard kapatıldığında Form1'i tekrar göster ve login flag'ini sıfırla
                _isLoginInProgress = false;
                this.Show();
            };
            
            dashboardForm.Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard acma hatasi: {ex.Message}");
            _isLoginInProgress = false;
            this.Show(); // Hata durumunda Form1'i tekrar göster
        }
    }

    /// <summary>
    /// CameraService'ten hata geldiğinde çağrılır.
    /// Kullanıcıya MessageBox ile bilgi verir.
    /// </summary>
    private void CameraService_ErrorOccurred(object? sender, string errorMessage)
    {
        // UI thread'inde MessageBox göster
        if (InvokeRequired)
        {
            BeginInvoke(new Action<string>(ShowErrorMessage), errorMessage);
        }
        else
        {
            ShowErrorMessage(errorMessage);
        }
        
        // Hata durumunda kamerayı güvenli şekilde durdur
        if (_isCameraRunning && _cameraService != null)
        {
            _cameraService.StopCamera();
            _isCameraRunning = false;
            buttonStartStop.Text = "Kamerayı Başlat";
        }
    }

    /// <summary>
    /// Hata mesajını MessageBox ile gösterir.
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        MessageBox.Show(message, "Kamera Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    /// <summary>
    /// Kayıt başlat butonu click event handler'ı.
    /// </summary>
    private void buttonStartRegistration_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_faceRegistrationService == null)
            {
                MessageBox.Show("Kayit servisi mevcut degil!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kamera çalışmıyorsa önce kamerayı başlat
            if (!_isCameraRunning)
            {
                MessageBox.Show("Lutfen once kamerayi baslatin!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Kayıt sürecini başlat
            _faceRegistrationService.StartRegistration();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata olustu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// FaceRegistrationService state değiştiğinde çağrılır.
    /// Label'ı günceller.
    /// </summary>
    private void FaceRegistrationService_StateChanged(object? sender, RegistrationState newState)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateInstructionsLabel()));
        }
        else
        {
            UpdateInstructionsLabel();
        }
    }

    /// <summary>
    /// Instructions label'ını günceller.
    /// </summary>
    private void UpdateInstructionsLabel()
    {
        if (_faceRegistrationService != null)
        {
            labelInstructions.Text = _faceRegistrationService.GetStateMessage();
        }
    }

    /// <summary>
    /// FaceRegistrationService kayıt tamamlandığında çağrılır.
    /// </summary>
    private void FaceRegistrationService_RegistrationCompleted(object? sender, int userId)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<int>(ShowRegistrationCompletedMessage), userId);
        }
        else
        {
            ShowRegistrationCompletedMessage(userId);
        }
    }

    /// <summary>
    /// Kayıt tamamlandı mesajını gösterir.
    /// </summary>
    private void ShowRegistrationCompletedMessage(int userId)
    {
        MessageBox.Show($"Kayit basariyla tamamlandi!\nKullanici ID: {userId}\n\n" +
            "15 adet yuz goruntusu 'TrainedFaces' klasorune kaydedildi.",
            "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
        // 3 saniye sonra kayıt sürecini sıfırla
        Task.Delay(3000).ContinueWith(_ =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    if (_faceRegistrationService != null)
                    {
                        _faceRegistrationService.Reset();
                    }
                }));
            }
            else
            {
                if (_faceRegistrationService != null)
                {
                    _faceRegistrationService.Reset();
                }
            }
        });
    }

    /// <summary>
    /// Form kapatılırken kaynakları temizler.
    /// Memory leak'i önlemek için kritik.
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Application.Idle event'ini kaldır
        Application.Idle -= Application_Idle;
        
        // Kamerayı durdur
        if (_isCameraRunning && _cameraService != null)
        {
            _cameraService.StopCamera();
        }
        
        // CameraService event'lerini kaldır
        if (_cameraService != null)
        {
            _cameraService.FrameReady -= CameraService_FrameReady;
            _cameraService.ErrorOccurred -= CameraService_ErrorOccurred;
            _cameraService.Dispose();
            _cameraService = null;
        }
        
        // FaceRegistrationService event'lerini kaldır ve dispose et
        if (_faceRegistrationService != null)
        {
            _faceRegistrationService.StateChanged -= FaceRegistrationService_StateChanged;
            _faceRegistrationService.RegistrationCompleted -= FaceRegistrationService_RegistrationCompleted;
            _faceRegistrationService.Dispose();
            _faceRegistrationService = null;
        }
        
        // FaceDetectionService'i dispose et
        if (_faceDetectionService != null)
        {
            _faceDetectionService.Dispose();
            _faceDetectionService = null;
        }
        
        // FaceRecognitionService'i dispose et
        if (_faceRecognitionService != null)
        {
            _faceRecognitionService.Dispose();
            _faceRecognitionService = null;
        }
        
        // PictureBox görüntüsünü temizle
        if (pictureBoxCamera.Image != null)
        {
            pictureBoxCamera.Image.Dispose();
            pictureBoxCamera.Image = null;
        }
        
        base.OnFormClosing(e);
    }
}
