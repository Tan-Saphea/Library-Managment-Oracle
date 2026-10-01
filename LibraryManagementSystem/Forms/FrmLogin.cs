using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Repositories;
using Oracle.ManagedDataAccess.Client;

namespace LibraryManagementSystem.Forms
{
    public class FrmLogin : Form
    {
        private Label lblHeader = null!;
        private Label lblUsername = null!;
        private TextBox txtUsername = null!;
        private Label lblPassword = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Button btnExit = null!;

        private readonly LoginRepository _loginRepo = new();

        public FrmLogin()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Login - Library Management System";
            this.Size = new Size(420, 290);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            lblHeader = new Label
            {
                Text = "LIBRARY MANAGEMENT SYSTEM",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 20),
                Size = new Size(365, 30)
            };

            lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(45, 65),
                Size = new Size(100, 20)
            };

            txtUsername = new TextBox
            {
                Location = new Point(45, 90),
                Size = new Size(315, 27),
                Text = "admin"
            };

            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(45, 125),
                Size = new Size(100, 20)
            };

            txtPassword = new TextBox
            {
                Location = new Point(45, 150),
                Size = new Size(315, 27),
                UseSystemPasswordChar = true,
                Text = "123"
            };

            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(105, 195),
                Size = new Size(100, 32),
                Cursor = Cursors.Hand
            };
            btnLogin.Click += BtnLogin_Click;

            btnExit = new Button
            {
                Text = "Exit",
                Location = new Point(215, 195),
                Size = new Size(100, 32),
                Cursor = Cursors.Hand
            };
            btnExit.Click += (s, e) => Application.Exit();

            this.Controls.Add(lblHeader);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnExit);

            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter a username.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                bool success = _loginRepo.ValidateLogin(username, password);
                if (success)
                {
                    this.Hide();
                    var mainForm = new FrmMain();
                    mainForm.FormClosed += (s, args) => this.Close();
                    mainForm.Show();
                }
                else
                {
                    MessageBox.Show(
                        "Invalid username or password.",
                        "Login",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Oracle Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
