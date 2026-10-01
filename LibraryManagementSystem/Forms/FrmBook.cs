using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;
using Oracle.ManagedDataAccess.Client;

namespace LibraryManagementSystem.Forms
{
    public class FrmBook : Form
    {
        private Label lblHeader = null!;

        private Label lblBookID = null!;
        private TextBox txtBookID = null!;

        private Label lblBookTitle = null!;
        private TextBox txtBookTitle = null!;

        private Label lblBookType = null!;
        private ComboBox cboBookType = null!;

        private Label lblPublishDate = null!;
        private DateTimePicker dtpPublishDate = null!;

        private Label lblNumOfPages = null!;
        private NumericUpDown nudNumOfPages = null!;

        private Label lblNumCopies = null!;
        private NumericUpDown nudNumCopies = null!;

        private Label lblEdition = null!;
        private TextBox txtEdition = null!;

        private Label lblPublisher = null!;
        private TextBox txtPublisher = null!;

        private Label lblBookSource = null!;
        private TextBox txtBookSource = null!;

        private Label lblRemark = null!;
        private TextBox txtRemark = null!;

        private Button btnNew = null!;
        private Button btnSave = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        private Label lblSearch = null!;
        private TextBox txtSearch = null!;

        private DataGridView dgvBooks = null!;

        private readonly BookRepository _bookRepo = new();

        public FrmBook()
        {
            InitializeComponent();
            LoadBookTypes();
            LoadBooks();
        }

        private void InitializeComponent()
        {
            this.Text = "Book Management";
            this.Size = new Size(1000, 720);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            lblHeader = new Label
            {
                Text = "BOOK MANAGEMENT",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(400, 30)
            };

            // Left Column
            lblBookID = new Label { Text = "Book ID:", Location = new Point(20, 60), Size = new Size(100, 25) };
            txtBookID = new TextBox { Location = new Point(125, 57), Size = new Size(180, 27), ReadOnly = true };

            lblBookTitle = new Label { Text = "Book Title:", Location = new Point(20, 95), Size = new Size(100, 25) };
            txtBookTitle = new TextBox { Location = new Point(125, 92), Size = new Size(260, 27) };

            lblBookType = new Label { Text = "Book Type:", Location = new Point(20, 130), Size = new Size(100, 25) };
            cboBookType = new ComboBox { Location = new Point(125, 127), Size = new Size(260, 27), DropDownStyle = ComboBoxStyle.DropDownList };

            lblPublishDate = new Label { Text = "Publish Date:", Location = new Point(20, 165), Size = new Size(100, 25) };
            dtpPublishDate = new DateTimePicker
            {
                Location = new Point(125, 162),
                Size = new Size(180, 27),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                ShowCheckBox = true
            };

            lblNumOfPages = new Label { Text = "No. Pages:", Location = new Point(20, 200), Size = new Size(100, 25) };
            nudNumOfPages = new NumericUpDown { Location = new Point(125, 197), Size = new Size(120, 27), Minimum = 0, Maximum = 100000, Value = 0 };

            // Middle Column
            lblNumCopies = new Label { Text = "No. Copies:", Location = new Point(415, 60), Size = new Size(90, 25) };
            nudNumCopies = new NumericUpDown { Location = new Point(510, 57), Size = new Size(120, 27), Minimum = 0, Maximum = 10000, Value = 0 };

            lblEdition = new Label { Text = "Edition:", Location = new Point(415, 95), Size = new Size(90, 25) };
            txtEdition = new TextBox { Location = new Point(510, 92), Size = new Size(220, 27) };

            lblPublisher = new Label { Text = "Publisher:", Location = new Point(415, 130), Size = new Size(90, 25) };
            txtPublisher = new TextBox { Location = new Point(510, 127), Size = new Size(220, 27) };

            lblBookSource = new Label { Text = "Book Source:", Location = new Point(415, 165), Size = new Size(90, 25) };
            txtBookSource = new TextBox { Location = new Point(510, 162), Size = new Size(220, 27) };

            // Right Column (Remark)
            lblRemark = new Label { Text = "Remark:", Location = new Point(755, 60), Size = new Size(60, 25) };
            txtRemark = new TextBox { Location = new Point(755, 87), Size = new Size(200, 100), Multiline = true };

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
            dgvBooks = new DataGridView
            {
                Location = new Point(20, 335),
                Size = new Size(935, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            dgvBooks.CellClick += DgvBooks_CellClick;

            this.Controls.Add(lblHeader);
            this.Controls.Add(lblBookID);
            this.Controls.Add(txtBookID);
            this.Controls.Add(lblBookTitle);
            this.Controls.Add(txtBookTitle);
            this.Controls.Add(lblBookType);
            this.Controls.Add(cboBookType);
            this.Controls.Add(lblPublishDate);
            this.Controls.Add(dtpPublishDate);
            this.Controls.Add(lblNumOfPages);
            this.Controls.Add(nudNumOfPages);
            this.Controls.Add(lblNumCopies);
            this.Controls.Add(nudNumCopies);
            this.Controls.Add(lblEdition);
            this.Controls.Add(txtEdition);
            this.Controls.Add(lblPublisher);
            this.Controls.Add(txtPublisher);
            this.Controls.Add(lblBookSource);
            this.Controls.Add(txtBookSource);
            this.Controls.Add(lblRemark);
            this.Controls.Add(txtRemark);
            this.Controls.Add(btnNew);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnUpdate);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnClear);
            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);
            this.Controls.Add(dgvBooks);
        }

        private void LoadBookTypes()
        {
            try
            {
                var types = _bookRepo.GetBookTypes();
                cboBookType.DataSource = types;
                cboBookType.DisplayMember = "BookTypeName";
                cboBookType.ValueMember = "BookTypeID";
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

        private void LoadBooks()
        {
            try
            {
                var list = _bookRepo.GetAll();
                dgvBooks.DataSource = list;
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
            if (dgvBooks.Columns["BookTypeID"] != null) dgvBooks.Columns["BookTypeID"].Visible = false;
            if (dgvBooks.Columns["BookID"] != null) dgvBooks.Columns["BookID"].HeaderText = "ID";
            if (dgvBooks.Columns["BookTitle"] != null) dgvBooks.Columns["BookTitle"].HeaderText = "Book Title";
            if (dgvBooks.Columns["BookTypeName"] != null) dgvBooks.Columns["BookTypeName"].HeaderText = "Book Type";
            if (dgvBooks.Columns["PublishDate"] != null) dgvBooks.Columns["PublishDate"].HeaderText = "Date";
            if (dgvBooks.Columns["NumOfPages"] != null) dgvBooks.Columns["NumOfPages"].HeaderText = "Pages";
            if (dgvBooks.Columns["NumCopies"] != null) dgvBooks.Columns["NumCopies"].HeaderText = "Copies";
            if (dgvBooks.Columns["Edition"] != null) dgvBooks.Columns["Edition"].HeaderText = "Edition";
            if (dgvBooks.Columns["Publisher"] != null) dgvBooks.Columns["Publisher"].HeaderText = "Publisher";
            if (dgvBooks.Columns["BookSource"] != null) dgvBooks.Columns["BookSource"].HeaderText = "Source";
            if (dgvBooks.Columns["Remark"] != null) dgvBooks.Columns["Remark"].HeaderText = "Remark";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBookTitle.Text))
            {
                MessageBox.Show("Book Title is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBookTitle.Focus();
                return;
            }

            if (cboBookType.SelectedValue == null)
            {
                MessageBox.Show("Book Type is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboBookType.Focus();
                return;
            }

            if (nudNumOfPages.Value < 0)
            {
                MessageBox.Show("Number of pages cannot be negative.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudNumOfPages.Focus();
                return;
            }

            if (nudNumCopies.Value < 0)
            {
                MessageBox.Show("Number of copies cannot be negative.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudNumCopies.Focus();
                return;
            }

            try
            {
                var book = new Book
                {
                    BookTitle = txtBookTitle.Text.Trim(),
                    BookTypeID = Convert.ToInt32(cboBookType.SelectedValue),
                    PublishDate = dtpPublishDate.Checked ? dtpPublishDate.Value.Date : null,
                    NumOfPages = (int)nudNumOfPages.Value,
                    NumCopies = (int)nudNumCopies.Value,
                    Edition = string.IsNullOrWhiteSpace(txtEdition.Text) ? null : txtEdition.Text.Trim(),
                    Publisher = string.IsNullOrWhiteSpace(txtPublisher.Text) ? null : txtPublisher.Text.Trim(),
                    BookSource = string.IsNullOrWhiteSpace(txtBookSource.Text) ? null : txtBookSource.Text.Trim(),
                    Remark = string.IsNullOrWhiteSpace(txtRemark.Text) ? null : txtRemark.Text.Trim()
                };

                _bookRepo.Insert(book);
                MessageBox.Show("Book saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBooks();
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
            if (string.IsNullOrWhiteSpace(txtBookID.Text))
            {
                MessageBox.Show("Please select a book from the list to update.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBookTitle.Text))
            {
                MessageBox.Show("Book Title is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBookTitle.Focus();
                return;
            }

            if (cboBookType.SelectedValue == null)
            {
                MessageBox.Show("Book Type is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboBookType.Focus();
                return;
            }

            try
            {
                var book = new Book
                {
                    BookID = int.Parse(txtBookID.Text),
                    BookTitle = txtBookTitle.Text.Trim(),
                    BookTypeID = Convert.ToInt32(cboBookType.SelectedValue),
                    PublishDate = dtpPublishDate.Checked ? dtpPublishDate.Value.Date : null,
                    NumOfPages = (int)nudNumOfPages.Value,
                    NumCopies = (int)nudNumCopies.Value,
                    Edition = string.IsNullOrWhiteSpace(txtEdition.Text) ? null : txtEdition.Text.Trim(),
                    Publisher = string.IsNullOrWhiteSpace(txtPublisher.Text) ? null : txtPublisher.Text.Trim(),
                    BookSource = string.IsNullOrWhiteSpace(txtBookSource.Text) ? null : txtBookSource.Text.Trim(),
                    Remark = string.IsNullOrWhiteSpace(txtRemark.Text) ? null : txtRemark.Text.Trim()
                };

                _bookRepo.Update(book);
                MessageBox.Show("Book updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBooks();
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
            if (string.IsNullOrWhiteSpace(txtBookID.Text))
            {
                MessageBox.Show("Please select a book from the list to delete.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialogResult = MessageBox.Show(
                "Are you sure you want to delete this book?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    int id = int.Parse(txtBookID.Text);
                    _bookRepo.Delete(id);
                    MessageBox.Show("Book deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadBooks();
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
                LoadBooks();
                return;
            }

            try
            {
                var list = _bookRepo.Search(keyword);
                dgvBooks.DataSource = list;
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

        private void DgvBooks_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvBooks.Rows[e.RowIndex].DataBoundItem is Book book)
            {
                txtBookID.Text = book.BookID.ToString();
                txtBookTitle.Text = book.BookTitle;
                cboBookType.SelectedValue = book.BookTypeID;

                if (book.PublishDate.HasValue)
                {
                    dtpPublishDate.Checked = true;
                    dtpPublishDate.Value = book.PublishDate.Value;
                }
                else
                {
                    dtpPublishDate.Checked = false;
                }

                nudNumOfPages.Value = Math.Max(0, book.NumOfPages);
                nudNumCopies.Value = Math.Max(0, book.NumCopies);
                txtEdition.Text = book.Edition ?? string.Empty;
                txtPublisher.Text = book.Publisher ?? string.Empty;
                txtBookSource.Text = book.BookSource ?? string.Empty;
                txtRemark.Text = book.Remark ?? string.Empty;
            }
        }

        private void ResetFields()
        {
            txtBookID.Clear();
            txtBookTitle.Clear();
            if (cboBookType.Items.Count > 0) cboBookType.SelectedIndex = 0;
            dtpPublishDate.Checked = false;
            nudNumOfPages.Value = 0;
            nudNumCopies.Value = 0;
            txtEdition.Clear();
            txtPublisher.Clear();
            txtBookSource.Clear();
            txtRemark.Clear();
        }
    }
}
