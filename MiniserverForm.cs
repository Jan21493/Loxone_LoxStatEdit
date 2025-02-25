﻿using LoxStatEdit.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LoxStatEdit.MiniserverForm;

namespace LoxStatEdit
{
    public partial class MiniserverForm : Form {

        #region class FileItem
        public class FileItem : IComparable<FileItem> {
            internal MsFileInfo MsFileInfo;
            internal FileInfo FileInfo;
            private string _name;
            private string _description;
            private ushort _valueCount;
            private bool _isValidStatsContent;

            public string FileName {
                get {
                    return (MsFileInfo != null) ? MsFileInfo.FileName : FileInfo.Name;
                }
            }
            public bool IsValidMsStatsFile {
                get {
                    var fileName = (MsFileInfo != null) ? MsFileInfo.FileName : FileInfo.Name;

                    // verify if file has valid Loxone stats format with UUID and .yyyyMM extension
                    var pattern = @"([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{16})(_[1-9])?\.([12][0-9]{3})([01][0-9])";
                    // 1st group (): UUID 
                    // 2nd group (): type (_#) or empty
                    // 3rd group (): yyyy
                    // 4th group (): MM 
                    var result = Regex.Match(fileName, pattern);

                    // no further evaluation of Regex so far

                    var validFormat = false;
                    if (result.Success)
                        validFormat = true;

                    return (validFormat);
                }
            }

            public ushort msStatsType {
                get {
                    var fileName = (MsFileInfo != null) ? MsFileInfo.FileName : FileInfo.Name;

                    // get the type from the file name after UUID and _ 
                    // if there is no _ in the filename after an UUID (35 characters), old type (=0 is assumed)
                    // e.g. 1cc8df9f-0229-8b1d-ffffefc088fafadd_1.202404
                    ushort typeNo = 0;
                    if (fileName.Length > 38 && fileName[35] == '_')
                        typeNo = (ushort)(fileName[36] - '0');
                    if (typeNo > 0 && typeNo < 10)
                        return typeNo;
                    else
                        return 0;
                }
            }

            public string UUID {
                get {
                    var fileName = (MsFileInfo != null) ? MsFileInfo.FileName : FileInfo.Name;

                    // verify if file has valid Loxone stats format with UUID and .yyyyMM extension
                    var pattern = @"([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{16})(_[1-9])?\.";
                    // 1st group (): UUID 
                    // 2nd group (): _# for files with newer format
                    var result = Regex.Match(fileName, pattern);
                    var groups = result.Groups;
                    // no further evaluation of Regex so far
                    if (result.Success && groups.Count > 1) {
                        return groups[1].Value;
                    } else {
                        return "Filename is not a valid UUID. Maybe corrupted?";
                    }
                }
            }
            public string Name {
                get {
                    if (_name == null) {
                        try {
                            if (FileInfo != null) {
                                LoxStatFile loxStatFile = LoxStatFile.Load(FileInfo.FullName, true);
                                if (loxStatFile.IsValidStatsContent) {

                                    _name = loxStatFile.Text;

                                    // Append the file extension as a suffix could be _1.202407 or 202407
                                    // This is a workaround until the sorting on two levels is implemented
                                    // e.g. sort by description and then by filename, else the files are not in historical order
                                    _name += " [";

                                    // check if filename contains _
                                    if (FileInfo.Name.Contains('_')) {
                                        _name += FileInfo.Name.Substring(FileInfo.Name.IndexOf('_') + 1);
                                    } else {
                                        _name += FileInfo.Name.Substring(FileInfo.Name.Length - 6);
                                    }

                                    _name += "]";
                                    _isValidStatsContent = true;
                                } else {
                                    _name = "No valid statistics file";
                                    _isValidStatsContent = false;
                                }
                            }
                        }
                        catch (Exception) {
                            _name = null;
                        }
                        if (string.IsNullOrEmpty(_name))
                            //_name = FileName;
                            _name = "Download to view description";
                    }
                    return _name;
                }
            }
            public string Description {
                get {
                    if (_description == null) {
                        try {
                            if (FileInfo != null) {
                                LoxStatFile loxStatFile = LoxStatFile.Load(FileInfo.FullName, true);
                                if (loxStatFile.IsValidStatsContent) {

                                    _description = loxStatFile.Text;
                                    _isValidStatsContent = true;
                                } else {
                                    _description = "No valid statistics file";
                                    _isValidStatsContent = false;
                                }
                            }
                        }
                        catch (Exception) {
                            _description = null;
                        }
                        if (string.IsNullOrEmpty(_description))
                            // No file on local FS, so there is no description
                            _description = "Download to view description";
                    }
                    return _description;
                }
            }

            public ushort ValueCount {
                get {
                    if (_valueCount == 0) {
                        try {
                            if (FileInfo != null) {
                                LoxStatFile loxStatFile = LoxStatFile.Load(FileInfo.FullName, true);
                                if (loxStatFile.IsValidStatsContent) {

                                    _valueCount = loxStatFile.ValueCount;
                                    _isValidStatsContent = true;
                                } else {
                                    _valueCount = 0;
                                    _isValidStatsContent = false;
                                }
                            }
                        }
                        catch (Exception) {
                            _valueCount = 0;
                        }
                        if (_valueCount == 0)
                            // No file on local FS, so guess the number of columns
                            _valueCount = 1;
                    }
                    return _valueCount;
                }
            }


            public DateTime YearMonth {
                get {
                    return LoxStatFile.GetYearMonth(FileName);
                }
            }

            public DateTime DateModified {
                get {
                    return (FileInfo != null) ? FileInfo.LastWriteTime : MsFileInfo.Date;
                }
            }

            public long Size {
                get {
                    return (FileInfo != null) ? FileInfo.Length : MsFileInfo.Size;
                }
            }

            public bool IsValidStatsContent {
                get {
                    return _isValidStatsContent;
                }
            }

            public const int IDStatusOnlyOnMS = 1;

            public const int IDStatusOnlyOnFS = 2;

            public const int IDStatusNewerOnMS = 3;

            public const int IDStatusNewerOnFS = 4;

            public const int IDStatusLargerOnMS = 5;

            public const int IDStatusLargerOnFS = 6;

            public const int IDStatusSame = 7;
            public int Status {
                get {
                    if (FileInfo == null) {
                        return IDStatusOnlyOnMS;
                    }
                    if (MsFileInfo == null) {
                        return IDStatusOnlyOnFS;
                    }
                    if (FileInfo.LastWriteTime < MsFileInfo.Date) {
                        return IDStatusNewerOnMS;
                    }
                    if (FileInfo.LastWriteTime > MsFileInfo.Date) {
                        return IDStatusNewerOnFS;
                    }
                    if (FileInfo.Length < MsFileInfo.Size) {
                        return IDStatusLargerOnMS;
                    }
                    if (FileInfo.Length > MsFileInfo.Size) {
                        return IDStatusLargerOnFS;
                    }
                    return IDStatusSame;
                }
            }

            public string StatusString {
                get {
                    string statusString;

                    switch (Status) {
                        case IDStatusOnlyOnMS:
                            statusString = "Only on MS";
                            break;
                        case IDStatusOnlyOnFS:
                            statusString = "Only on FS";
                            break;
                        case IDStatusNewerOnMS:
                            statusString = "Newer on MS";
                            break;
                        case IDStatusNewerOnFS:
                            statusString = "Older on MS";
                            break;
                        case IDStatusLargerOnMS:
                            statusString = "Larger on MS";
                            break;
                        case IDStatusLargerOnFS:
                            statusString = "Smaller on MS";
                            break;
                        default:
                            // IDStatusSame
                            statusString = "Same date/size";
                            break;
                    }

                    return statusString;
                }
            }

            public override string ToString() {
                return string.Format("{0} ({1})", Name, StatusString);
            }

            public int CompareTo(FileItem other) {
                var c = StringComparer.OrdinalIgnoreCase.Compare(Name, other.Name);
                return (c != 0) ? c : YearMonth.CompareTo(other.YearMonth);
            }
        }

        #endregion

        #region Fields
        readonly string[] _args;
        IList<MsFileInfo> _msFolder = new MsFileInfo[0];
        IList<FileInfo> _localFolder = new FileInfo[0];
        IList<FileItem> _fileItems;

        #endregion

        #region Constructor
        public MiniserverForm(params string[] args) {
            _args = args;
            InitializeComponent();

            // Set the start position of the form to the center of the screen
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        #endregion

        #region Methods

        public DataGridView DGV {
            get {
                return _dataGridView;
            }
        }

        private void RefreshGridView() {
            // Show the loading form
            var busyForm = new BusyForm {
                // Manually set the start position
                StartPosition = FormStartPosition.Manual
            };
            var parent = this; // Reference to the parent form
            busyForm.Left = parent.Left + (parent.Width - busyForm.Width) / 2;
            busyForm.Top = parent.Top + (parent.Height - busyForm.Height) / 2;
            busyForm.Show(this); // Or busyForm.ShowDialog(this) for a modal form
            Application.DoEvents(); // Process events to ensure the loading form is displayed

            try {
                // Save the current sort conditions
                var sortColumn = _dataGridView.SortedColumn;
                var sortOrder = _dataGridView.SortOrder;

                // Save the current scrolling position
                int scrollPosition = _dataGridView.FirstDisplayedScrollingRowIndex;
                //Console.WriteLine($"Scroll position: {scrollPosition}");

                // Save the current selection
                var selectedCells = _dataGridView.SelectedCells.Cast<DataGridViewCell>()
                    .Select(cell => new { cell.RowIndex, cell.ColumnIndex })
                    .ToList();

                // Save the current active cell (for restoring the selection arrow)
                int? activeCellRowIndex = _dataGridView.CurrentCell?.RowIndex;
                int? activeCellColumnIndex = _dataGridView.CurrentCell?.ColumnIndex;

                // Retrieve the filter text
                string filterText = _filterComboBox.Text.ToLower();

                _fileItems = GetFilteredFileItems(filterText);

                // Disable automatic column generation
                _dataGridView.AutoGenerateColumns = false;

                // Clear existing columns
                _dataGridView.Columns.Clear();

                // Add Cell Formatting
                _dataGridView.CellFormatting += DataGridView_CellFormatting;

                // Add Tool Tips
                _dataGridView.CellToolTipTextNeeded += DataGridView_CellToolTipTextNeeded;

                // Add columns manually in the order you want
                _dataGridView.Columns.Add(new DataGridViewTextBoxColumn {
                    DataPropertyName = "FileName",
                    HeaderText = "FileName",
                    Width = 250,
                    ToolTipText = "Name of the file on the Loxone MS and local file system (FS).\n" +
                        "NOTE: This is typically an UUID plus extension with .yyyyMM",
                });
                _dataGridView.Columns.Add(new DataGridViewTextBoxColumn {
                    DataPropertyName = "Name",
                    HeaderText = "Description",
                    MinimumWidth = 250,
                    ToolTipText = "Defined in Loxone Config and retrieved from statistics data.",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                });
                _dataGridView.Columns.Add(new DataGridViewTextBoxColumn {
                    DataPropertyName = "YearMonth",
                    HeaderText = "Year/Month",
                    Width = 90,
                    ToolTipText = "Year and month from file extension on MS. \n" +
                        "NOTE: This date is used on MS to fetch statistics data from a specific year and month.",
                    DefaultCellStyle = { Format = "yyyy-MM" }
                });
                _dataGridView.Columns.Add(new DataGridViewTextBoxColumn {
                    DataPropertyName = "DateModified",
                    HeaderText = "Date modified",
                    Width = 120,
                    ToolTipText = "Date and time when the file was modified.\n\nThe date and time is from the local file system (FS), if the file is found there.\n" +
                        "If the file is not found on the local FS,\nit's the date and time that is received from the MS via FTP.\n" +
                        "If there is any difference the 'status' column will give you more information.\n\n" +
                        "NOTE: files that were downloaded via FTP always have a date with this or last year.\n" +
                        "The reason is that the Loxone MS does not transmit any year in directory listings via FTP.",
                    DefaultCellStyle = { Format = "dd.MM.yyyy - HH:mm:ss" }
                });
                _dataGridView.Columns.Add(new DataGridViewTextBoxColumn {
                    DataPropertyName = "Size",
                    HeaderText = "Size",
                    Width = 90,
                    ToolTipText = "Size of the file on the local file system (FS) or MS.\n\n" +
                        "The size is from the local FS, if the file is found there.\n" +
                        "If the file is not found on the local FS,\\nit's the size that is received from the MS via FTP.\n" +
                        "If there is any difference for files with same modification date/time,\n" +
                        "the 'status' column will give you more information.",
                    DefaultCellStyle = new DataGridViewCellStyle() {
                        Alignment = DataGridViewContentAlignment.MiddleRight,
                        ForeColor = System.Drawing.Color.Black
                    },
                    //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                });
                _dataGridView.Columns.Add(new DataGridViewTextBoxColumn {
                    DataPropertyName = "StatusString",
                    HeaderText = "Status",
                    Width = 100,
                    ToolTipText = "Result of a comparison between the file on Loxone MS and local file system (FS).",
                    DefaultCellStyle = new DataGridViewCellStyle() {
                        BackColor = System.Drawing.Color.White,
                        ForeColor = System.Drawing.Color.Black
                    },
                });
                _dataGridView.Columns.Add(new DataGridViewButtonColumn {
                    DataPropertyName = "Edit",
                    HeaderText = "Edit",
                    Width = 50,
                    ToolTipText = "Edit statistical data (entries) in the file on the local file system (FS)."
                });

                // Bind the SortableBindingList to the DataGridView
                _dataGridView.DataSource = _fileItems;

                // Restore the sort conditions
                if (sortColumn != null && sortOrder != SortOrder.None) {
                    // Determine the ListSortDirection from the SortOrder
                    ListSortDirection direction = sortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;

                    // Find the DataGridViewColumn to sort by
                    DataGridViewColumn columnToSort = _dataGridView.Columns
                        .OfType<DataGridViewColumn>()
                        .FirstOrDefault(c => c.DataPropertyName == sortColumn.DataPropertyName);

                    // TODO: Add second sort level, to always sort by FileName as second level with same direction as first level

                    if (columnToSort != null) {
                        // Reapply the sort
                        _dataGridView.Sort(columnToSort, direction);
                    }
                }

                // Restore the scrolling position
                if (_dataGridView.RowCount > 0 && scrollPosition != -1 && scrollPosition < _dataGridView.RowCount) {
                    _dataGridView.FirstDisplayedScrollingRowIndex = scrollPosition;
                }

                // Clear the default selection
                _dataGridView.ClearSelection();

                // Restore the CurrentCell to restore the selection arrow
                if (activeCellRowIndex.HasValue && activeCellColumnIndex.HasValue
                    && activeCellRowIndex.Value < _dataGridView.RowCount
                    && activeCellColumnIndex.Value < _dataGridView.ColumnCount) {
                    _dataGridView.CurrentCell = _dataGridView.Rows[activeCellRowIndex.Value].Cells[activeCellColumnIndex.Value];
                }

                // Restore the selection
                if (selectedCells.Any()) {
                    foreach (var cell in selectedCells) {
                        if (cell.RowIndex < _dataGridView.RowCount && cell.ColumnIndex < _dataGridView.ColumnCount) {
                            _dataGridView.Rows[cell.RowIndex].Cells[cell.ColumnIndex].Selected = true;
                        }
                    }
                }
            }
            finally {
                // Close the loading form after the loop
                busyForm.Close();
            }
        }

        private void BrowseFolderButton_Click(object sender, EventArgs e) {
            folderBrowserDialog.SelectedPath = _folderTextBox.Text;
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                _folderTextBox.Text = folderBrowserDialog.SelectedPath;
            RefreshLocal();
            RefreshGridView();
            //Console.WriteLine("Refreshed local folder");
        }

        private void openFolderButton_Click(object sender, EventArgs e) {
            Process.Start("explorer.exe", _folderTextBox.Text);
        }

        public IList<FileItem> GetFilteredFileItems(string filterText = "") {

            IList<FileItem> fileItems;

            // provide a empty list to prevent crash
            if (_msFolder == null) {
                _msFolder = new List<MsFileInfo>();
            }

            ILookup<string, MsFileInfo> msMap = _msFolder.ToLookup(e => e.FileName, StringComparer.OrdinalIgnoreCase);
            ILookup<string, FileInfo> localMap = _localFolder.ToLookup(e => e.Name, StringComparer.OrdinalIgnoreCase);

            // Create a new SortableBindingList
            fileItems = new SortableBindingList<FileItem>();

            // Get the data
            List<FileItem> data = msMap.Select(e => e.Key).Union(localMap.Select(e => e.Key)).
                Select(f => new FileItem {
                    MsFileInfo = msMap[f].FirstOrDefault(),
                    FileInfo = localMap[f].FirstOrDefault(),
                }).
                // Order by filename if available else by name
                Where(f => f.FileName.ToLower().Contains(filterText) || f.Name.ToLower().Contains(filterText)).
                OrderBy(f => f.MsFileInfo?.FileName ?? f.FileInfo?.Name).
                ToList();

            // Add the data to the SortableBindingList
            foreach (var item in data) {
                fileItems.Add(item);
            }
            return fileItems;
        }

        public IList<FileItem> GetFilteredLocalFileItems(string filterText = "") {

            IList<FileItem> fileItems;

            ILookup<string, FileInfo> localMap = _localFolder.ToLookup(e => e.Name, StringComparer.OrdinalIgnoreCase);

            // Create a new SortableBindingList
            fileItems = new SortableBindingList<FileItem>();

            // Get the data
            foreach (FileInfo file in _localFolder) {
                FileItem fileItem = new FileItem {
                    MsFileInfo = null,
                    FileInfo = file
                };
                if (fileItem.FileName.ToLower().Contains(filterText) || fileItem.Name.ToLower().Contains(filterText))
                    fileItems.Add(fileItem);
            }
            return fileItems.OrderBy(f => f.FileInfo?.Name).ToList();
        }

        private void RefreshMs() {
            var uriBuilder = GetMsUriBuilder();
            if (uriBuilder != null) {
                _msFolder = MsFileInfo.EnumerateStatFiles(uriBuilder.Uri);
            }
        }

        private void RefreshLocal() {
            Directory.CreateDirectory(_folderTextBox.Text);
            _localFolder = Directory.EnumerateFiles(_folderTextBox.Text).
                Select(fileName => new FileInfo(fileName)).ToList();
        }

        private UriBuilder GetMsUriBuilder() {
            try {
                // escape # in uri by replacing with hex
                var uriBuilder = new UriBuilder(new Uri(_urlTextBox.Text.Replace("#", "%23")).
                    GetComponents(UriComponents.Scheme | UriComponents.UserInfo |
                    UriComponents.Host | UriComponents.Port, UriFormat.UriEscaped)) {
                    Path = "stats"
                };
                return uriBuilder;
            }
            catch (UriFormatException ex) {
                MessageBox.Show(ex.Message, "Error - UriBuilder", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }
        }


        private Uri GetFileNameUri(string fileName) {
            var uriBuilder = GetMsUriBuilder();
            uriBuilder.Path += "/" + fileName;
            return uriBuilder.Uri;
        }

        private bool Download(FileItem fileItem) {
            try {
                var ftpWebRequest = (FtpWebRequest)FtpWebRequest.
                Create(GetFileNameUri(fileItem.FileName));
                ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                var targetFileName = Path.Combine(_folderTextBox.Text, fileItem.FileName);
                using (var response = ftpWebRequest.GetResponse())
                using (var ftpStream = response.GetResponseStream())
                using (var fileStream = File.OpenWrite(targetFileName))
                    ftpStream.CopyTo(fileStream);
                File.SetLastWriteTime(targetFileName, fileItem.MsFileInfo.Date);

                // Log FTP output
                // File.AppendAllText("./custom.log", $"Downloaded {fileItem.FileName} - Setting last write time to {fileItem.MsFileInfo.Date}\n\n");

                return true;
            }
            catch (WebException ex) {
                FtpWebResponse response = ex.Response as FtpWebResponse;
                if (response != null) {
                    MessageBox.Show(ex.Message, "Error  - FTP connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
            catch (Exception ex) {
                MessageBox.Show($"# Message\n{ex.Message}\n\n# Data\n{ex.Data}\n\n# StackTrace\n{ex.StackTrace}",
                    "Error - IList", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        private bool Upload(FileItem fileItem) {
            try {
                var ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(GetFileNameUri(fileItem.FileName));
                ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                using (var fileStream = File.OpenRead(
                    Path.Combine(_folderTextBox.Text, fileItem.FileInfo.FullName)))
                using (var ftpStream = ftpWebRequest.GetRequestStream())
                    fileStream.CopyTo(ftpStream);
                using (var response = ftpWebRequest.GetResponse()) {
                }

                return true;
            }
            catch (WebException ex) {
                var response = ex.Response as FtpWebResponse;
                if (response != null) {
                    MessageBox.Show(ex.Message, "Error  - FTP connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
            catch (Exception ex) {
                MessageBox.Show($"# Message\n{ex.Message}\n\n# Data\n{ex.Data}\n\n# StackTrace\n{ex.StackTrace}",
                    "Error - IList", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        private bool Convert(FileItem fileItem) {
            // ToDo: Code missing
            // System.Threading.Thread.Sleep(50);
            return true;
        }

        private bool Delete(FileItem fileItem) {
            try {
                // Delete file on Loxone MS
                if (fileItem.MsFileInfo != null) {
                    FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(GetFileNameUri(fileItem.FileName));
                    ftpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                    FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                    //Console.WriteLine("Delete status: {0}", ftpWebResponse.StatusDescription);
                    ftpWebResponse.Close();
                }
                // Delete file on local file system
                if (fileItem.FileInfo != null) {
                    var fileNameWithPath = Path.Combine(_folderTextBox.Text, fileItem.FileName);
                    if (File.Exists(fileNameWithPath))
                        File.Delete(fileNameWithPath);
                }

                return true;
            }
            catch (WebException ex) {
                var response = ex.Response as FtpWebResponse;
                if (response != null) {
                    MessageBox.Show(ex.Message, "Error  - FTP connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
            catch (Exception ex) {
                MessageBox.Show($"# Message\n{ex.Message}\n\n# Data\n{ex.Data}\n\n# StackTrace\n{ex.StackTrace}",
                    "Error - local file system", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        private static void ExportText(FileStream fs, string value) {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }
        private bool Export(FileItem fileItem, string exportPath) {

            LoxStatFile loxStatFile = null;

            // show error message, if file is not downloaded yet
            if ((!fileItem.IsValidMsStatsFile) || (!fileItem.IsValidStatsContent)) {
                MessageBox.Show($"The file \"{fileItem.FileName}\" is not a valid Loxone statistics file and " +
                   "can't be exported.\n\n" +
                   "Either the file name or the content are invalid.", "Error - File not valid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
           
            try {

                // Export file from local file system
                string fileNameWithPath = Path.Combine(_folderTextBox.Text, fileItem.FileName);
                if (File.Exists(fileNameWithPath)) {
                    loxStatFile = LoxStatFile.Load(fileItem.FileInfo.FullName);
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"# Message\n{ex.Message}\n\n# Data\n{ex.Data}\n\n# StackTrace\n{ex.StackTrace}",
                    "Error - local file system", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            if (loxStatFile == null)
                return false;

            exportPath += "\\" + fileItem.FileName + ".txt";

            // Delete the file if it exists.
            if (File.Exists(exportPath)) {
                File.Delete(exportPath + ".bak");
                File.Move(exportPath, exportPath + ".bak");
                
            }
            int dataPointsCount = 0;
            if (loxStatFile.DataPoints != null)
                dataPointsCount = loxStatFile.DataPoints.Count;
            //Create the file.
            using (FileStream fs = File.Create(exportPath)) {
                ExportText(fs, ";Info;Loxone Statistics file - exported from LoxStatEdit\r\n");
                ExportText(fs, ";FileName;" + fileItem.FileInfo.FullName + "\r\n");
                ExportText(fs, ";GUID;" + loxStatFile.Guid.ToString("D") + "\r\n");
                ExportText(fs, ";ValueCount;" + loxStatFile.ValueCount + "\r\n");
                ExportText(fs, ";Unknown0x02;" + loxStatFile.Unknown0x02.ToString("X4") + "\r\n");
                ExportText(fs, ";Unknown0x04;" + loxStatFile.Unknown0x04.ToString("X8") + "\r\n");
                ExportText(fs, ";TextLength;" + loxStatFile.TextLength + "\r\n");
                ExportText(fs, ";TextBytes;" + loxStatFile.Text + "\r\n");
                if (loxStatFile.msStatsType > 0)
                    ExportText(fs, ";TimeFormat;UTC\r\n");
                else
                    ExportText(fs, ";TimeFormat;Local\r\n");
                ExportText(fs, "DataPoints;" + dataPointsCount + "\r\n");

                for (var index = 0; index < dataPointsCount; index++) {
                    LoxStatDataPoint dP = loxStatFile.DataPoints[index];

                    ExportText(fs, index +";" + dP.Timestamp);
                    for (var j = 0; j < loxStatFile.ValueCount; j++)
                        ExportText(fs, ";" + dP.Values[j]);
                    ExportText(fs, "\r\n");
                }
            }
            return true;
        } // end of export function

        #endregion

        #region Events
        private void RefreshMsButton_Click(object sender, EventArgs e) {
            RefreshMs();
            RefreshGridView();
        }

        private void RefreshFolderButton_Click(object sender, EventArgs e) {
            RefreshLocal();
            RefreshGridView();
        }

        private void FilterButton_Click(object sender, EventArgs e) {
            if (_filterComboBox.FindStringExact(_filterComboBox.Text.ToLower()) == -1)
                _filterComboBox.Items.Add(_filterComboBox.Text.ToLower());
            RefreshGridView();
        }

        private void ClearFilterButton_Click(object sender, EventArgs e) {

            _filterComboBox.Text = "";
            RefreshGridView();
        }


        private void _urlTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(_folderTextBox.Text)) {
                RefreshMsButton_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void _folderTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(_folderTextBox.Text)) {
                RefreshFolderButton_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void _filterComboBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(_folderTextBox.Text)) {
                if (_filterComboBox.FindStringExact(_filterComboBox.Text.ToLower()) == -1)
                    _filterComboBox.Items.Add(_filterComboBox.Text.ToLower());
                RefreshGridView();
            }
        }

        private void MiniserverForm_Load(object sender, EventArgs e) {

            // if not remote desktop session then enable double-buffering optimization
            if (!System.Windows.Forms.SystemInformation.TerminalServerSession) {
                typeof(DataGridView).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty, null, _dataGridView, new object[] { true });
            }

            // Load screen sizes and settings from user config if found, otherwise use defaults

            //Existing user config does not exist, so load settings from previous assembly
            if (Settings.Default.IsSettingsUpgradeRequired) //this defaults to true when a new version of this software has been released
            {
                Settings.Default.Upgrade(); //upgrade the settings to the newer version
                Settings.Default.Reload();
                Settings.Default.IsSettingsUpgradeRequired = false;
                Settings.Default.Save();
            }

            if (Properties.Settings.Default.IsMaximizedMainWindow)
                WindowState = FormWindowState.Maximized;
            else if (Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(Properties.Settings.Default.MainWindowPosition))) {
                StartPosition = FormStartPosition.Manual;
                DesktopBounds = Properties.Settings.Default.MainWindowPosition;
                WindowState = FormWindowState.Normal;
            }

            _folderTextBox.Text = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments), "LoxStatEdit");
            if (_args.Length >= 1) {
                _urlTextBox.Text = _args[0];
                RefreshMs();
            }
            if (_args.Length >= 2)
                _folderTextBox.Text = Path.GetFullPath(_args[1]);
            RefreshLocal();
            RefreshGridView();
        }

        private void MiniserverForm_Closing(object sender, FormClosingEventArgs e) {
            this.SaveWindowPosition(sender, e);
        }

        private void MiniserverForm_ResizeEnd(object sender, EventArgs e) {
            this.SaveWindowPosition(sender, e);
        }

        private void SaveWindowPosition(object sender, EventArgs e) {
            Properties.Settings.Default.IsMaximizedMainWindow = WindowState == FormWindowState.Maximized;
            Properties.Settings.Default.MainWindowPosition = DesktopBounds;
            Properties.Settings.Default.Save();
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if ((e.RowIndex < 0) || (e.RowIndex >= _fileItems.Count)) {
                e.Value = null;
                return;
            }
            if (e.ColumnIndex == 0) // The FileName column is at index 0
            {
                var fileItem = _fileItems[e.RowIndex];
                if (!fileItem.IsValidMsStatsFile) {
                    e.CellStyle.BackColor = System.Drawing.Color.Red;
                }
            }
            if (e.ColumnIndex == 1) // The Description / Name column is at index 1
            {
                var fileItem = _fileItems[e.RowIndex];
                if (fileItem.Name == "Download to view description") {
                    e.CellStyle.Font = new System.Drawing.Font(e.CellStyle.Font, System.Drawing.FontStyle.Italic);
                } else {
                    e.CellStyle.Font = new System.Drawing.Font(e.CellStyle.Font, System.Drawing.FontStyle.Regular);
                }
            }
            if (e.ColumnIndex == 5) // The Status column is at index 5
            {
                var fileItem = _fileItems[e.RowIndex];
                switch (fileItem.Status) {
                    case FileItem.IDStatusOnlyOnMS:
                        e.CellStyle.BackColor = System.Drawing.Color.LightGray;
                        break;
                    case FileItem.IDStatusOnlyOnFS:
                        e.CellStyle.BackColor = System.Drawing.Color.LightGoldenrodYellow;
                        break;
                    case FileItem.IDStatusNewerOnMS:
                        e.CellStyle.BackColor = System.Drawing.Color.LightGreen;
                        break;
                    case FileItem.IDStatusNewerOnFS:
                        e.CellStyle.BackColor = System.Drawing.Color.LightSkyBlue;
                        break;
                    case FileItem.IDStatusLargerOnMS:
                        e.CellStyle.BackColor = System.Drawing.Color.LightGreen;
                        break;
                    case FileItem.IDStatusLargerOnFS:
                        e.CellStyle.BackColor = System.Drawing.Color.LightSkyBlue;
                        break;
                    default:
                        // IDStatusSame:
                        e.CellStyle.BackColor = System.Drawing.Color.White;
                        break;
                }
            }

        }

        private void DataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e) {
            try {
                if ((e.RowIndex < 0) || (e.RowIndex >= _fileItems.Count)) {
                    e.Value = null;
                    return;
                }
                var fileItem = _fileItems[e.RowIndex];
                switch (e.ColumnIndex) {
                    case 0:
                        e.Value = fileItem.FileName;
                        break;
                    case 1:
                        e.Value = fileItem.Name;
                        break;
                    case 2:
                        e.Value = fileItem.YearMonth;
                        break;
                    case 3:
                        e.Value = fileItem.DateModified;
                        break;
                    case 4:
                        e.Value = fileItem.Size;
                        break;
                    case 5:
                        e.Value = fileItem.StatusString;
                        break;
                    case 6:
                        e.Value = "Edit";
                        break;
                    default:
                        e.Value = null;
                        break;
                }
            }
            catch {
#if DEBUG
                Debugger.Break();
#endif
                e.Value = null;
            }
        }

        private void DataGridView_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e) {
            try {
                if ((e.RowIndex < 0) || (e.RowIndex >= _fileItems.Count)) {
                    //e.ToolTipText = null;
                    return;
                }
                
                var fileItem = _fileItems[e.RowIndex];
                switch (e.ColumnIndex) {
                    case 0: // fileItem.FileName; 
                        if (!fileItem.IsValidMsStatsFile) {
                            e.ToolTipText = "The UUID and/or file extension looks to be malformed.\n\n" +
                                "Please check, if it is used by the MS. If not, you may consider to remove the file.";
                        }
                        break;
                    case 1: // fileItem.Name; 
                        break;
                    case 2: // fileItem.YearMonth; 
                        break;
                    case 3: // fileItem.DateModified; 
                        break;
                    case 4: // fileItem.Size; 
                        break;
                    case 5: // fileItem.StatusString; 
                        break;
                    default:
                        break;
                }
            }
            catch {
#if DEBUG
                Debugger.Break();
#endif
                e.ToolTipText = "";
            }
        }

        private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            DialogResult result;

            // clean up progress bar
            progressBar.Value = 0;
            progressBar.SetState(1);
            progressLabel.Text = "Idle";

            if (e.RowIndex < 0)
                return; //When clicking the header row
            var fileItem = _fileItems[e.RowIndex];

            switch (e.ColumnIndex) {
                case 6: //Edit
                    // show error message, if file is not downloaded yet
                    if (fileItem.FileInfo == null) {
                        MessageBox.Show($"The file \"{fileItem.FileName}\" cannot be edited, since it's not available on the filesystem.\n\n" +
                            "Please download it first.", "Error - File not downloaded", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }
                    //
                    if (!fileItem.IsValidStatsContent) {
                        MessageBox.Show($"The file \"{fileItem.FileName}\" is not a valid Loxone statistics file and " +
                           "can't be edited.", "Error - File has no valid content", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }
                    //Console.WriteLine(fileItem.FileInfo.FullName);
                    using (LoxStatFileForm form = new LoxStatFileForm(this, _fileItems, fileItem.FileInfo.FullName)) {
                        // Calculate the new location
                        System.Drawing.Rectangle fileWindowRect = Properties.Settings.Default.FileWindowPosition;
                        if (fileWindowRect.Top == 0) {
                            form.StartPosition = FormStartPosition.Manual; // Allows manual positioning
                            form.Location = new System.Drawing.Point(this.Location.X + 40, this.Location.Y + 70);
                        }

                        // Show the form as a dialog
                        form.ShowDialog(this);
                    }
                    RefreshLocal();
                    RefreshGridView();
                    break;
            }
        }

        private async void DownloadButton_Click(object sender, EventArgs e) {
            progressBar.Maximum = _dataGridView.SelectedRows.Count;
            progressBar.Value = 0;
            progressBar.SetState(1);
            progressLabel.Text = "Starting download...";

            foreach (DataGridViewRow row in _dataGridView.SelectedRows) {
                int rowIndex = row.Index; // Capture the index for the closure
                bool result = await Task.Run(() => Download(_fileItems[rowIndex]));
                if (!result) {
                    progressBar.Value = progressBar.Maximum;
                    progressBar.SetState(2);
                    progressLabel.Text = "Download failed!";
                    return;
                }

                progressBar.Value++;
                progressLabel.Text = $"Downloaded {progressBar.Value} of {progressBar.Maximum}";
            }

            progressLabel.Text = "Download complete!";

            RefreshLocal();
            RefreshGridView();
        }

        private async void UploadButton_Click(object sender, EventArgs e) {

            progressBar.Maximum = _dataGridView.SelectedRows.Count;
            progressBar.Value = 0;
            progressBar.SetState(1);
            progressLabel.Text = "Starting upload...";

            foreach (DataGridViewRow row in _dataGridView.SelectedRows) {
                int rowIndex = row.Index; // Capture the index for the closure
                bool result = true;
                // show error message, if there is no file to upload
                if (_fileItems[rowIndex].FileInfo == null) {
                    MessageBox.Show($"The file \"{_fileItems[rowIndex].FileName}\" cannot be uploaded, since it's not available on the filesystem.\n\nPlease download it first.", "Error - File not downloaded", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    result = false;
                } else if ((!_fileItems[rowIndex].IsValidMsStatsFile) || (!_fileItems[rowIndex].IsValidStatsContent)) {
                    MessageBox.Show($"The file \"{_fileItems[rowIndex].FileName}\" is not a valid Loxone statistics file and " +
                       "can't be uploaded.\n\n" +
                       "Either the file name or the content are invalid.", "Error - File not valid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    result = false;
                }
                if (result)
                    result = await Task.Run(() => Upload(_fileItems[rowIndex]));
                if (!result) {
                    progressBar.Value = progressBar.Maximum;
                    progressBar.SetState(2);
                    progressLabel.Text = "Upload failed!";
                    RefreshMs();
                    RefreshLocal();
                    RefreshGridView();
                    return;
                }

                progressBar.Value++;
                progressLabel.Text = $"Uploaded {progressBar.Value} of {progressBar.Maximum} files";
            }
            progressLabel.Text = "Upload complete!";
            RefreshMs();
            RefreshLocal();
            RefreshGridView();
        }

        // Launch project link
        private void githubLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/Jan21493/Loxone_LoxStatEdit/releases");
        }

        private void donateLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://www.loxforum.com/Spende");
        }

        #endregion

        private void aboutLabel_Click(object sender, EventArgs e) {

        }

        private void ConvertButton_Click(object sender, EventArgs e) {

            progressBar.Maximum = _dataGridView.SelectedRows.Count;
            progressBar.Value = 0;
            progressBar.SetState(1);
            progressLabel.Text = "Starting conversion...";

            // build a list of file names and file items for selected rows
            IList<FileItem> selectedFileItems = new List<FileItem>();
            IList<string> selectedFileNames = new List<string>();

            string uuid = ""; 
            foreach (DataGridViewRow row in _dataGridView.SelectedRows) {
                int rowIndex = row.Index; // Capture the index for the closure
                selectedFileItems.Add(_fileItems[rowIndex]);
                selectedFileNames.Add(_fileItems[rowIndex].FileName);
                if (uuid == "") {
                    uuid = _fileItems[rowIndex].UUID;
                }
                else if (uuid != _fileItems[rowIndex].UUID) {
                    MessageBox.Show($"You can only convert files with the same UUID,\n" +
                        "but multiple UUIS's were selected!",
                        "Error - Multiple UUID's selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (_fileItems[rowIndex].FileInfo == null) {
                    MessageBox.Show($"All files must be available on the local filesystem to be converted,\n" +
                        $"but file \"{_fileItems[rowIndex].FileName}\" is missing. Please download it (and all other files) first.",
                        "Error - Conversion cancelled", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            if ((selectedFileItems == null) || (selectedFileItems.Count == 0)) {
                MessageBox.Show($"Conversion cancelled, because no files were selected.",
                    "Error - No files selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Show dialog to convert all selected files
            using (var form = new LoxStatConvertForm(GetFilteredLocalFileItems(), selectedFileItems.ToArray())) {
                // Calculate the new location
                int offsetX = 100; // Horizontal offset from the parent form
                int offsetY = 50; // Vertical offset from the parent form
                form.StartPosition = FormStartPosition.Manual; // Allows manual positioning
                form.Location = new System.Drawing.Point(this.Location.X + offsetX, this.Location.Y + offsetY);

                // Show the form as a dialog
                form.ShowDialog(this);
            }
            progressBar.Value = progressBar.Maximum;
            progressLabel.Text = "Conversion completed!";
            RefreshLocal();
            RefreshGridView();

        } // end of ConvertButton_Click

        private void DeleteButton_Click(object sender, EventArgs e) {

            // build a list of file names and file items for selected rows
            IList<FileItem> selectedFileItems = new List<FileItem>();
            IList<string> selectedFileNames = new List<string>();

            foreach (DataGridViewRow row in _dataGridView.SelectedRows) {
                int rowIndex = row.Index; // Capture the index for the closure
                selectedFileItems.Add(_fileItems[rowIndex]);
                selectedFileNames.Add(_fileItems[rowIndex].FileName);
            }

            progressBar.Maximum = selectedFileItems.Count;
            progressBar.Value = 0;
            progressBar.SetState(1);
            progressLabel.Text = "Starting deletion...";

            // Delete all selected files after dialog to confirm
            if ((selectedFileItems.Count > 0)) {
                DialogResult result = MessageBox.Show($"Do you really want to delete the following file(s):\n {string.Join("\n ", selectedFileNames)}\n\nfrom the Loxone MS AND local file system?\n\n" +
                                          "IMPORTANT: There is NO undo function for this action!"
                    , "Question - Delete Stats File(s) from MS and FS?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes) {
                    int failedCounter = 0;
                    foreach (FileItem fileItem in selectedFileItems) {
                        progressBar.Value += 1;
                        progressLabel.Text = $"Deleting {fileItem.FileName} ...";
                        if (!Delete(fileItem))
                            failedCounter += 1;
                    }
                    if (failedCounter > 0) {
                        progressBar.SetState(2);
                        progressLabel.Text = $"Deletion completed. {failedCounter} files has/have failed!";
                    } else {
                        progressLabel.Text = "Deletion completed!";
                    }

                    RefreshMs();
                    RefreshLocal();
                    RefreshGridView();
                }
            }
        } // end of DeleteButton_Click

        private void ExportButton_Click(object sender, EventArgs e) {

            // build a list of file names and file items for selected rows
            IList<FileItem> selectedFileItems = new List<FileItem>();
            IList<string> selectedFileNames = new List<string>();

            foreach (DataGridViewRow row in _dataGridView.SelectedRows) {
                int rowIndex = row.Index; // Capture the index for the closure
                selectedFileItems.Add(_fileItems[rowIndex]);
                selectedFileNames.Add(_fileItems[rowIndex].FileName);

                // file must be available on local FS
                if (_fileItems[rowIndex].FileInfo == null) {
                    MessageBox.Show($"All files must be available on the local filesystem to be exported,\n" +
                        $"but (at least) file \"{_fileItems[rowIndex].FileName}\" is missing. Please download it (and all other files) first.",
                        "Error - Export cancelled", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            if ((selectedFileItems == null) || (selectedFileItems.Count == 0)) {
                MessageBox.Show($"Export cancelled, because no files were selected.",
                    "Error - No files selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string exportPath;
            folderBrowserDialog.SelectedPath = _folderTextBox.Text;
            DialogResult result = folderBrowserDialog.ShowDialog(this);
            if (result == DialogResult.OK) {
                exportPath = folderBrowserDialog.SelectedPath;

                int failedCounter = 0;
                progressBar.Maximum = selectedFileItems.Count;
                progressBar.Value = 0;
                progressBar.SetState(1);
                progressLabel.Text = "Starting export...";
                foreach (FileItem fileItem in selectedFileItems) {
                    progressBar.Value += 1;
                    progressLabel.Text = $"Exporting {fileItem.FileName} ...";
                    if (!Export(fileItem, exportPath))
                        failedCounter += 1;
                }
                if (failedCounter > 0) {
                    progressBar.SetState(2);
                    progressLabel.Text = $"Exporting complete. {failedCounter} files has/have failed!";
                } else {
                    progressLabel.Text = "Exporting complete!";
                }
            }
        } // end of ExportButton_Click
    }
}
