// ============================================
// Form1.Designer.cs
// ============================================
// Form tasarım dosyası - UI kontrollerinin tanımlandığı yer

namespace FaceID;

partial class Form1
{
    /// <summary>
    /// Tasarım bileşenleri için gerekli değişken.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Kullanılan tüm kaynakları temizler.
    /// </summary>
    /// <param name="disposing">Yönetilen kaynaklar temizlenecekse true, aksi halde false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Tasarımcı desteği için gerekli metot - kod düzenleyici ile içeriği değiştirmeyin.
    /// </summary>
    private void InitializeComponent()
    {
        // UI kontrollerini oluştur
        this.pictureBoxCamera = new System.Windows.Forms.PictureBox();
        this.buttonStartStop = new System.Windows.Forms.Button();
        this.labelInstructions = new System.Windows.Forms.Label();
        this.buttonStartRegistration = new System.Windows.Forms.Button();
        
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCamera)).BeginInit();
        this.SuspendLayout();
        
        // PictureBox ayarları - Kamera görüntüsü burada gösterilecek
        this.pictureBoxCamera.Location = new System.Drawing.Point(12, 12);
        this.pictureBoxCamera.Name = "pictureBoxCamera";
        this.pictureBoxCamera.Size = new System.Drawing.Size(776, 400);
        this.pictureBoxCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        this.pictureBoxCamera.TabIndex = 0;
        this.pictureBoxCamera.TabStop = false;
        
        // Button ayarları - Kamerayı başlat/durdur butonu
        this.buttonStartStop.Location = new System.Drawing.Point(12, 428);
        this.buttonStartStop.Name = "buttonStartStop";
        this.buttonStartStop.Size = new System.Drawing.Size(200, 40);
        this.buttonStartStop.TabIndex = 1;
        this.buttonStartStop.Text = "Kamerayı Başlat";
        this.buttonStartStop.UseVisualStyleBackColor = true;
        this.buttonStartStop.Click += new System.EventHandler(this.buttonStartStop_Click);
        
        // Label ayarları - Talimatlar için
        this.labelInstructions.Location = new System.Drawing.Point(230, 428);
        this.labelInstructions.Name = "labelInstructions";
        this.labelInstructions.Size = new System.Drawing.Size(400, 40);
        this.labelInstructions.TabIndex = 2;
        this.labelInstructions.Text = "Kayıt başlatmak için 'Kayıt Başlat' butonuna basın.";
        this.labelInstructions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.labelInstructions.AutoSize = false;
        
        // Button ayarları - Kayıt başlat butonu
        this.buttonStartRegistration.Location = new System.Drawing.Point(650, 428);
        this.buttonStartRegistration.Name = "buttonStartRegistration";
        this.buttonStartRegistration.Size = new System.Drawing.Size(138, 40);
        this.buttonStartRegistration.TabIndex = 3;
        this.buttonStartRegistration.Text = "Kayıt Başlat";
        this.buttonStartRegistration.UseVisualStyleBackColor = true;
        this.buttonStartRegistration.Click += new System.EventHandler(this.buttonStartRegistration_Click);
        
        // Form ayarları
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 480);
        this.Controls.Add(this.buttonStartRegistration);
        this.Controls.Add(this.labelInstructions);
        this.Controls.Add(this.buttonStartStop);
        this.Controls.Add(this.pictureBoxCamera);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Banka Yüz Tanıma Sistemi - Kamera Kontrol";
        
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCamera)).EndInit();
        this.ResumeLayout(false);
    }

    #endregion

    // UI kontrolleri
    private System.Windows.Forms.PictureBox pictureBoxCamera;
    private System.Windows.Forms.Button buttonStartStop;
    private System.Windows.Forms.Label labelInstructions;
    private System.Windows.Forms.Button buttonStartRegistration;
}
