// ============================================
// DashboardForm.cs
// ============================================
// Dashboard form - Kullanici bilgilerini gosterir

namespace FaceID;

/// <summary>
/// Dashboard form - Kullanici bilgilerini gosterir.
/// </summary>
public partial class DashboardForm : Form
{
    private Label _labelWelcome = null!;
    private Label _labelBalance = null!;
    private Button _buttonLogout = null!;

    /// <summary>
    /// DashboardForm constructor'i.
    /// </summary>
    /// <param name="userName">Kullanici adi</param>
    /// <param name="balance">Bakiye</param>
    public DashboardForm(string userName, decimal balance)
    {
        InitializeComponent();
        UpdateUserInfo(userName, balance);
    }

    /// <summary>
    /// Kullanici bilgilerini gunceller.
    /// </summary>
    /// <param name="userName">Kullanici adi</param>
    /// <param name="balance">Bakiye</param>
    public void UpdateUserInfo(string userName, decimal balance)
    {
        _labelWelcome.Text = $"Hosgeldin {userName}";
        _labelBalance.Text = $"Bakiye: {balance:C}";
    }

    /// <summary>
    /// Form tasarimini baslatir.
    /// </summary>
    private void InitializeComponent()
    {
        this._labelWelcome = new Label();
        this._labelBalance = new Label();
        this._buttonLogout = new Button();
        this.SuspendLayout();

        // Welcome Label
        this._labelWelcome.AutoSize = true;
        this._labelWelcome.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
        this._labelWelcome.Location = new System.Drawing.Point(50, 100);
        this._labelWelcome.Name = "_labelWelcome";
        this._labelWelcome.Size = new System.Drawing.Size(0, 32);
        this._labelWelcome.TabIndex = 0;

        // Balance Label
        this._labelBalance.AutoSize = true;
        this._labelBalance.Font = new System.Drawing.Font("Segoe UI", 14F);
        this._labelBalance.Location = new System.Drawing.Point(50, 150);
        this._labelBalance.Name = "_labelBalance";
        this._labelBalance.Size = new System.Drawing.Size(0, 25);
        this._labelBalance.TabIndex = 1;

        // Logout Button
        this._buttonLogout.Location = new System.Drawing.Point(50, 220);
        this._buttonLogout.Name = "_buttonLogout";
        this._buttonLogout.Size = new System.Drawing.Size(200, 40);
        this._buttonLogout.TabIndex = 2;
        this._buttonLogout.Text = "Cikis Yap";
        this._buttonLogout.UseVisualStyleBackColor = true;
        this._buttonLogout.Click += ButtonLogout_Click;

        // Form
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(600, 400);
        this.Controls.Add(this._buttonLogout);
        this.Controls.Add(this._labelBalance);
        this.Controls.Add(this._labelWelcome);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Name = "DashboardForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Dashboard";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    /// <summary>
    /// Cikis butonu click event handler'i.
    /// </summary>
    private void ButtonLogout_Click(object? sender, EventArgs e)
    {
        this.Close();
    }
}

