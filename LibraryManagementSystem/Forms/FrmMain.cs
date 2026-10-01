using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagementSystem.Forms
{
    public class FrmMain : Form
    {
        private Panel pnlSidebar = null!;
        private Label lblAppTitle = null!;
        private Button btnDashboard = null!;
        private Button btnStudents = null!;
        private Button btnAuthors = null!;
        private Button btnBooks = null!;
        private Button btnExit = null!;
        private Panel pnlContent = null!;

        private Form? _activeForm;

        public FrmMain()
        {
            InitializeComponent();
            ShowDashboard();
        }

        private void InitializeComponent()
        {
            this.Text = "Library Management System";
            this.Size = new Size(1180, 780);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // Left Sidebar
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 210,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = SystemColors.Control
            };

            lblAppTitle = new Label
            {
                Text = "Library Management",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 20),
                Size = new Size(198, 30)
            };

            btnDashboard = new Button
            {
                Text = "Dashboard",
                Location = new Point(15, 75),
                Size = new Size(178, 38),
                Cursor = Cursors.Hand
            };
            btnDashboard.Click += (s, e) => ShowDashboard();

            btnStudents = new Button
            {
                Text = "Students",
                Location = new Point(15, 125),
                Size = new Size(178, 38),
                Cursor = Cursors.Hand
            };
            btnStudents.Click += (s, e) => OpenChildForm(new FrmStudent());

            btnAuthors = new Button
            {
                Text = "Authors",
                Location = new Point(15, 175),
                Size = new Size(178, 38),
                Cursor = Cursors.Hand
            };
            btnAuthors.Click += (s, e) => OpenChildForm(new FrmAuthor());

            btnBooks = new Button
            {
                Text = "Books",
                Location = new Point(15, 225),
                Size = new Size(178, 38),
                Cursor = Cursors.Hand
            };
            btnBooks.Click += (s, e) => OpenChildForm(new FrmBook());

            btnExit = new Button
            {
                Text = "Exit",
                Location = new Point(15, 300),
                Size = new Size(178, 38),
                Cursor = Cursors.Hand
            };
            btnExit.Click += (s, e) =>
            {
                if (MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            };

            pnlSidebar.Controls.Add(lblAppTitle);
            pnlSidebar.Controls.Add(btnDashboard);
            pnlSidebar.Controls.Add(btnStudents);
            pnlSidebar.Controls.Add(btnAuthors);
            pnlSidebar.Controls.Add(btnBooks);
            pnlSidebar.Controls.Add(btnExit);

            // Right Content Area
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SystemColors.Window
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
        }

        private void OpenChildForm(Form childForm)
        {
            if (_activeForm != null)
            {
                _activeForm.Close();
                _activeForm.Dispose();
            }

            _activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(childForm);
            pnlContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void ShowDashboard()
        {
            if (_activeForm != null)
            {
                _activeForm.Close();
                _activeForm.Dispose();
                _activeForm = null;
            }

            pnlContent.Controls.Clear();

            var pnlDashboard = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30)
            };

            var lblDashTitle = new Label
            {
                Text = "Welcome to Library Management System",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(30, 30),
                Size = new Size(600, 35)
            };

            var lblDashDesc = new Label
            {
                Text = "Select a module from the left menu to start managing library records.\n\n" +
                       "- Students: Register and manage student member profiles and photos.\n" +
                       "- Authors: Catalog authors, biographical details, and portraits.\n" +
                       "- Books: Maintain book titles, inventory copies, and genres.\n\n" +
                       "Connected Database: Oracle Database Free (localhost:1521/FREEPDB1)\n" +
                       "Data Access: Oracle Stored Procedures exclusively.",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Location = new Point(30, 80),
                Size = new Size(700, 160)
            };

            pnlDashboard.Controls.Add(lblDashTitle);
            pnlDashboard.Controls.Add(lblDashDesc);

            pnlContent.Controls.Add(pnlDashboard);
        }
    }
}
