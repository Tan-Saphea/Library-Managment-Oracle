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
    public class FrmStudent : Form
    {
        private Label lblHeader = null!;

        private Label lblStuID = null!;
        private TextBox txtStuID = null!;

        private Label lblStuName = null!;
        private TextBox txtStuName = null!;

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

        private DataGridView dgvStudents = null!;

        private byte[]? _currentPhotoBytes;
        private readonly StudentRepository _studentRepo = new();

        public FrmStudent()
        {
            InitializeComponent();
            LoadStudents();
        }

        private void InitializeComponent()
        {
            this.Text = "Student Management";
            this.Size = new Size(950, 700);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // Title
            lblHeader = new Label
            {
                Text = "STUDENT MANAGEMENT",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(400, 30)
            };

            // Input Controls
            lblStuID = new Label { Text = "Student ID:", Location = new Point(20, 60), Size = new Size(90, 25) };
            txtStuID = new TextBox { Location = new Point(115, 57), Size = new Size(180, 27), ReadOnly = true };

            lblStuName = new Label { Text = "Name:", Location = new Point(20, 95), Size = new Size(90, 25) };
            txtStuName = new TextBox { Location = new Point(115, 92), Size = new Size(240, 27) };

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

            // Photo Controls
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

            // Buttons
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

            // Search
            lblSearch = new Label { Text = "Search:", Location = new Point(20, 295), Size = new Size(60, 25) };
            txtSearch = new TextBox { Location = new Point(85, 292), Size = new Size(350, 27) };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // DataGridView
            dgvStudents = new DataGridView
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
            dgvStudents.CellClick += DgvStudents_CellClick;

            this.Controls.Add(lblHeader);
            this.Controls.Add(lblStuID);
            this.Controls.Add(txtStuID);
            this.Controls.Add(lblStuName);
            this.Controls.Add(txtStuName);
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
            this.Controls.Add(dgvStudents);
        }

        private void LoadStudents()
        {
            try
            {
                var list = _studentRepo.GetAll();
                dgvStudents.DataSource = list;
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
            if (dgvStudents.Columns["Photo"] != null)
            {
                dgvStudents.Columns["Photo"].Visible = false;
            }
            if (dgvStudents.Columns["StuID"] != null) dgvStudents.Columns["StuID"].HeaderText = "ID";
            if (dgvStudents.Columns["StuName"] != null) dgvStudents.Columns["StuName"].HeaderText = "Student Name";
            if (dgvStudents.Columns["Gender"] != null) dgvStudents.Columns["Gender"].HeaderText = "Gender";
            if (dgvStudents.Columns["DOB"] != null) dgvStudents.Columns["DOB"].HeaderText = "DOB";
            if (dgvStudents.Columns["POB"] != null) dgvStudents.Columns["POB"].HeaderText = "POB";
            if (dgvStudents.Columns["Address"] != null) dgvStudents.Columns["Address"].HeaderText = "Address";
            if (dgvStudents.Columns["Phone"] != null) dgvStudents.Columns["Phone"].HeaderText = "Phone";
            if (dgvStudents.Columns["Email"] != null) dgvStudents.Columns["Email"].HeaderText = "Email";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStuName.Text))
            {
                MessageBox.Show("Student Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStuName.Focus();
                return;
            }

            try
            {
                var student = new Student
                {
                    StuName = txtStuName.Text.Trim(),
                    Gender = cboGender.SelectedItem?.ToString(),
                    DOB = dtpDOB.Checked ? dtpDOB.Value.Date : null,
                    POB = string.IsNullOrWhiteSpace(txtPOB.Text) ? null : txtPOB.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Photo = _currentPhotoBytes
                };

                _studentRepo.Insert(student);
                MessageBox.Show("Student saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadStudents();
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
            if (string.IsNullOrWhiteSpace(txtStuID.Text))
            {
                MessageBox.Show("Please select a student from the list to update.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtStuName.Text))
            {
                MessageBox.Show("Student Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStuName.Focus();
                return;
            }

            try
            {
                var student = new Student
                {
                    StuID = int.Parse(txtStuID.Text),
                    StuName = txtStuName.Text.Trim(),
                    Gender = cboGender.SelectedItem?.ToString(),
                    DOB = dtpDOB.Checked ? dtpDOB.Value.Date : null,
                    POB = string.IsNullOrWhiteSpace(txtPOB.Text) ? null : txtPOB.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Photo = _currentPhotoBytes
                };

                _studentRepo.Update(student);
                MessageBox.Show("Student updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadStudents();
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
            if (string.IsNullOrWhiteSpace(txtStuID.Text))
            {
                MessageBox.Show("Please select a student from the list to delete.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialogResult = MessageBox.Show(
                "Are you sure you want to delete this student?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    int id = int.Parse(txtStuID.Text);
                    _studentRepo.Delete(id);
                    MessageBox.Show("Student deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStudents();
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
                LoadStudents();
                return;
            }

            try
            {
                var list = _studentRepo.Search(keyword);
                dgvStudents.DataSource = list;
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

        private void DgvStudents_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvStudents.Rows[e.RowIndex].DataBoundItem is Student student)
            {
                txtStuID.Text = student.StuID.ToString();
                txtStuName.Text = student.StuName;
                cboGender.SelectedItem = student.Gender ?? "Male";

                if (student.DOB.HasValue)
                {
                    dtpDOB.Checked = true;
                    dtpDOB.Value = student.DOB.Value;
                }
                else
                {
                    dtpDOB.Checked = false;
                }

                txtPOB.Text = student.POB ?? string.Empty;
                txtAddress.Text = student.Address ?? string.Empty;
                txtPhone.Text = student.Phone ?? string.Empty;
                txtEmail.Text = student.Email ?? string.Empty;

                _currentPhotoBytes = student.Photo;
                picPhoto.Image = BytesToImage(_currentPhotoBytes);
            }
        }

        private void BtnBrowsePhoto_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png",
                Title = "Select Student Photo"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var img = Image.FromFile(ofd.FileName);
                    _currentPhotoBytes = ImageToBytes(img);
                    picPhoto.Image = BytesToImage(_currentPhotoBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Image Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static byte[]? ImageToBytes(Image? image)
        {
            if (image == null) return null;
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        public static Image? BytesToImage(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            using var ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        private void ResetFields()
        {
            txtStuID.Clear();
            txtStuName.Clear();
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
