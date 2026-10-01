using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;
using Oracle.ManagedDataAccess.Client;

namespace LibraryManagementSystem.Forms
{
    public class FrmAuthor : Form
    {
        private Label lblHeader = null!;

        private Label lblAuthorID = null!;
        private TextBox txtAuthorID = null!;

        private Label lblAuthorName = null!;
        private TextBox txtAuthorName = null!;

        private Label lblGender = null!;
        private ComboBox cboGender = null!;

        private Label lblDOB = null!;
        private DateTimePicker dtpDOB = null!;

        private Label lblPOB = null!;
        private TextBox txtPOB = null!;

        private Label lblAddress = null!;
        private TextBox txtAddress = null!;

        private Label lblPhone = null!;
        private TextBox txtPhone = null!;

        private Label lblEmail = null!;
        private TextBox txtEmail = null!;

        private Label lblPhoto = null!;
        private PictureBox picPhoto = null!;
        private Button btnBrowsePhoto = null!;

        private Button btnNew = null!;
        private Button btnSave = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        private Label lblSearch = null!;
        private TextBox txtSearch = null!;

        private DataGridView dgvAuthors = null!;

        private byte[]? _currentPhotoBytes;
        private readonly AuthorRepository _authorRepo = new();

        public FrmAuthor()
        {
            InitializeComponent();
            LoadAuthors();
        }

        private void InitializeComponent()
        {
            this.Text = "Author Management";
            this.Size = new Size(950, 700);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            lblHeader = new Label
            {
                Text = "AUTHOR MANAGEMENT",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(400, 30)
            };

            lblAuthorID = new Label { Text = "Author ID:", Location = new Point(20, 60), Size = new Size(90, 25) };
            txtAuthorID = new TextBox { Location = new Point(115, 57), Size = new Size(180, 27), ReadOnly = true };

            lblAuthorName = new Label { Text = "Name:", Location = new Point(20, 95), Size = new Size(90, 25) };
            txtAuthorName = new TextBox { Location = new Point(115, 92), Size = new Size(240, 27) };

            lblGender = new Label { Text = "Gender:", Location = new Point(20, 130), Size = new Size(90, 25) };
            cboGender = new ComboBox { Location = new Point(115, 127), Size = new Size(180, 27), DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
            cboGender.SelectedIndex = 0;

            lblDOB = new Label { Text = "DOB:", Location = new Point(20, 165), Size = new Size(90, 25) };
            dtpDOB = new DateTimePicker
            {
                Location = new Point(115, 162),
                Size = new Size(180, 27),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                ShowCheckBox = true
            };

            lblPOB = new Label { Text = "POB:", Location = new Point(20, 200), Size = new Size(90, 25) };
            txtPOB = new TextBox { Location = new Point(115, 197), Size = new Size(240, 27) };

            lblAddress = new Label { Text = "Address:", Location = new Point(390, 60), Size = new Size(70, 25) };
            txtAddress = new TextBox { Location = new Point(470, 57), Size = new Size(240, 60), Multiline = true };

            lblPhone = new Label { Text = "Phone:", Location = new Point(390, 130), Size = new Size(70, 25) };
            txtPhone = new TextBox { Location = new Point(470, 127), Size = new Size(240, 27) };

            lblEmail = new Label { Text = "Email:", Location = new Point(390, 165), Size = new Size(70, 25) };
            txtEmail = new TextBox { Location = new Point(470, 162), Size = new Size(240, 27) };

            lblPhoto = new Label { Text = "Photo:", Location = new Point(745, 60), Size = new Size(60, 25) };
            picPhoto = new PictureBox
            {
                Location = new Point(805, 57),
                Size = new Size(110, 130),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            btnBrowsePhoto = new Button
            {
                Text = "Browse...",
                Location = new Point(805, 195),
                Size = new Size(110, 30),
                Cursor = Cursors.Hand
            };
            btnBrowsePhoto.Click += BtnBrowsePhoto_Click;

            btnNew = new Button { Text = "New", Location = new Point(20, 245), Size = new Size(80, 32), Cursor = Cursors.Hand };
            btnNew.Click += (s, e) => ResetFields();

            btnSave = new Button { Text = "Save", Location = new Point(110, 245), Size = new Size(80, 32), Cursor = Cursors.Hand };
            btnSave.Click += BtnSave_Click;

            btnUpdate = new Button { Text = "Update", Location = new Point(200, 245), Size = new Size(80, 32), Cursor = Cursors.Hand };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button { Text = "Delete", Location = new Point(290, 245), Size = new Size(80, 32), Cursor = Cursors.Hand };
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "Clear", Location = new Point(380, 245), Size = new Size(80, 32), Cursor = Cursors.Hand };
            btnClear.Click += (s, e) => ResetFields();

            lblSearch = new Label { Text = "Search:", Location = new Point(20, 295), Size = new Size(60, 25) };
            txtSearch = new TextBox { Location = new Point(85, 292), Size = new Size(350, 27) };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            dgvAuthors = new DataGridView
            {
                Location = new Point(20, 335),
                Size = new Size(895, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            dgvAuthors.CellClick += DgvAuthors_CellClick;

            this.Controls.Add(lblHeader);
            this.Controls.Add(lblAuthorID);
            this.Controls.Add(txtAuthorID);
            this.Controls.Add(lblAuthorName);
            this.Controls.Add(txtAuthorName);
            this.Controls.Add(lblGender);
            this.Controls.Add(cboGender);
            this.Controls.Add(lblDOB);
            this.Controls.Add(dtpDOB);
            this.Controls.Add(lblPOB);
            this.Controls.Add(txtPOB);
            this.Controls.Add(lblAddress);
            this.Controls.Add(txtAddress);
            this.Controls.Add(lblPhone);
            this.Controls.Add(txtPhone);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblPhoto);
            this.Controls.Add(picPhoto);
            this.Controls.Add(btnBrowsePhoto);
            this.Controls.Add(btnNew);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnUpdate);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnClear);
            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);
            this.Controls.Add(dgvAuthors);
        }

        private void LoadAuthors()
        {
            try
            {
                var list = _authorRepo.GetAll();
                dgvAuthors.DataSource = list;
                FormatGrid();
            }
            catch (OracleException ex)
            {
                MessageBox.Show(ex.Message, "Oracle Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGrid()
        {
            if (dgvAuthors.Columns["Photo"] != null)
            {
                dgvAuthors.Columns["Photo"].Visible = false;
            }
            if (dgvAuthors.Columns["AuthorID"] != null) dgvAuthors.Columns["AuthorID"].HeaderText = "ID";
            if (dgvAuthors.Columns["AuthorName"] != null) dgvAuthors.Columns["AuthorName"].HeaderText = "Author Name";
            if (dgvAuthors.Columns["Gender"] != null) dgvAuthors.Columns["Gender"].HeaderText = "Gender";
            if (dgvAuthors.Columns["DOB"] != null) dgvAuthors.Columns["DOB"].HeaderText = "DOB";
            if (dgvAuthors.Columns["POB"] != null) dgvAuthors.Columns["POB"].HeaderText = "POB";
            if (dgvAuthors.Columns["Address"] != null) dgvAuthors.Columns["Address"].HeaderText = "Address";
            if (dgvAuthors.Columns["Phone"] != null) dgvAuthors.Columns["Phone"].HeaderText = "Phone";
            if (dgvAuthors.Columns["Email"] != null) dgvAuthors.Columns["Email"].HeaderText = "Email";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAuthorName.Text))
            {
                MessageBox.Show("Author Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAuthorName.Focus();
                return;
            }

            try
            {
                var author = new Author
                {
                    AuthorName = txtAuthorName.Text.Trim(),
                    Gender = cboGender.SelectedItem?.ToString(),
                    DOB = dtpDOB.Checked ? dtpDOB.Value.Date : null,
                    POB = string.IsNullOrWhiteSpace(txtPOB.Text) ? null : txtPOB.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Photo = _currentPhotoBytes
                };

                _authorRepo.Insert(author);
                MessageBox.Show("Author saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAuthors();
                ResetFields();
            }
            catch (OracleException ex)
            {
                MessageBox.Show(ex.Message, "Oracle Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAuthorID.Text))
            {
                MessageBox.Show("Please select an author from the list to update.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAuthorName.Text))
            {
                MessageBox.Show("Author Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAuthorName.Focus();
                return;
            }

            try
            {
                var author = new Author
                {
                    AuthorID = int.Parse(txtAuthorID.Text),
                    AuthorName = txtAuthorName.Text.Trim(),
                    Gender = cboGender.SelectedItem?.ToString(),
                    DOB = dtpDOB.Checked ? dtpDOB.Value.Date : null,
                    POB = string.IsNullOrWhiteSpace(txtPOB.Text) ? null : txtPOB.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Photo = _currentPhotoBytes
                };

                _authorRepo.Update(author);
                MessageBox.Show("Author updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAuthors();
                ResetFields();
            }
            catch (OracleException ex)
            {
                MessageBox.Show(ex.Message, "Oracle Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAuthorID.Text))
            {
                MessageBox.Show("Please select an author from the list to delete.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialogResult = MessageBox.Show(
                "Are you sure you want to delete this author?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    int id = int.Parse(txtAuthorID.Text);
                    _authorRepo.Delete(id);
                    MessageBox.Show("Author deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAuthors();
                    ResetFields();
                }
                catch (OracleException ex)
                {
                    MessageBox.Show(ex.Message, "Oracle Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                LoadAuthors();
                return;
            }

            try
            {
                var list = _authorRepo.Search(keyword);
                dgvAuthors.DataSource = list;
                FormatGrid();
            }
            catch (OracleException ex)
            {
                MessageBox.Show(ex.Message, "Oracle Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvAuthors_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvAuthors.Rows[e.RowIndex].DataBoundItem is Author author)
            {
                txtAuthorID.Text = author.AuthorID.ToString();
                txtAuthorName.Text = author.AuthorName;
                cboGender.SelectedItem = author.Gender ?? "Male";

                if (author.DOB.HasValue)
                {
                    dtpDOB.Checked = true;
                    dtpDOB.Value = author.DOB.Value;
                }
                else
                {
                    dtpDOB.Checked = false;
                }

                txtPOB.Text = author.POB ?? string.Empty;
                txtAddress.Text = author.Address ?? string.Empty;
                txtPhone.Text = author.Phone ?? string.Empty;
                txtEmail.Text = author.Email ?? string.Empty;

                _currentPhotoBytes = author.Photo;
                picPhoto.Image = FrmStudent.BytesToImage(_currentPhotoBytes);
            }
        }

        private void BtnBrowsePhoto_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png",
                Title = "Select Author Photo"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var img = Image.FromFile(ofd.FileName);
                    _currentPhotoBytes = FrmStudent.ImageToBytes(img);
                    picPhoto.Image = FrmStudent.BytesToImage(_currentPhotoBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Image Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ResetFields()
        {
            txtAuthorID.Clear();
            txtAuthorName.Clear();
            cboGender.SelectedIndex = 0;
            dtpDOB.Checked = false;
            txtPOB.Clear();
            txtAddress.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            _currentPhotoBytes = null;
            picPhoto.Image = null;
        }
    }
}
