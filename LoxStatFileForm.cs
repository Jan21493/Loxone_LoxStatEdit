using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.RegularExpressions;
using System.Data;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Reflection;
using LoxStatEdit.Properties;

namespace LoxStatEdit
{
    public partial class LoxStatFileForm: Form
    {
        const int _valueColumnOffset = 2;
        readonly string[] _args;
        LoxStatFile _loxStatFile;
        IList<LoxStatProblem> _problems;
        DataTable _dataTable;
        MiniserverForm _parent;
        IList<MiniserverForm.FileItem> _fileItems;

        public LoxStatFileForm(MiniserverForm parent, IList<MiniserverForm.FileItem> fileItems, params string[] args)
        {
            _parent = parent;
            _args = args;
            _fileItems = fileItems;

            InitializeComponent();

            // Load screen sizes and settings from user config if found, otherwise use defaults
            if (Properties.Settings.Default.IsMaximizedFileWindow)
                WindowState = FormWindowState.Maximized;
            else if (Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(Properties.Settings.Default.FileWindowPosition))) {
                StartPosition = FormStartPosition.Manual;
                DesktopBounds = Properties.Settings.Default.FileWindowPosition;
                WindowState = FormWindowState.Normal;
            }
            if (Properties.Settings.Default.FileWindowSplitterDistance > 0)
                this.splitContainer.SplitterDistance = Properties.Settings.Default.FileWindowSplitterDistance;

            // Subscribe to the MouseClick event of the chart
            _chart.MouseClick += _chartMouseClick;

            // Subscribe to the MouseMove event
            _dataGridView.MouseMove += DataGridView_MouseMove;

            // Subscribe to cell validation and end of edit for DGV
            _dataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.DataGridView_CellValidating);
            _dataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_CellEndEdit);
        }

        private void LoadFile(int offset = 0)
        {
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();

            _fileNameTextBox.Text = Path.GetFullPath(_fileNameTextBox.Text);
            // load statistics file for previous month
            if (offset != 0) {
                string fileName = _fileNameTextBox.Text;
                DateTime yearMonth;
                bool success = DateTime.TryParseExact(
                    Path.GetExtension(fileName).Substring(1),
                    "yyyyMM",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.NoCurrentDateDefault,
                    out yearMonth);
                if (success)
                    _fileNameTextBox.Text = Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fileName) + "." + yearMonth.AddMonths(offset).ToString("yyyyMM");
            }
            
            _loxStatFile = LoxStatFile.Load(_fileNameTextBox.Text);
            if (_loxStatFile.LoadException != null) {
                _fileInfoTextBox.Text = "ERROR: " + _loxStatFile.LoadException.Message;
            } else {
                _fileInfoTextBox.Text = string.Format("{0}: {1} data point{2} with {3} value{4}",
               _loxStatFile.Text,
               _loxStatFile.DataPoints.Count,
               _loxStatFile.DataPoints.Count == 1 ? "" : "s",
               _loxStatFile.ValueCount,
               _loxStatFile.ValueCount == 1 ? "" : "s");
            }
           

            // remove optional columns from DGV, but keep index, timestamp, value
            var columns = _dataGridView.Columns;
            while (columns.Count > (_valueColumnOffset + 1))
                columns.RemoveAt(columns.Count - 1);

            // create new data table with all columns
            _dataTable = new DataTable();
            _dataTable.Columns.Add(new DataColumn(indexColumn.Name, typeof(int)));
            _dataTable.Columns.Add(new DataColumn(timestampColumn.Name, typeof(DateTime)));
            _dataTable.Columns.Add(new DataColumn(valueColumn.Name, typeof(double)));
            for (int i = 0; i <= _valueColumnOffset; i++) {
                columns[i].DataPropertyName = columns[i].Name;
            }
            // new statistic files are using UTC time stamps
            if (_loxStatFile.msStatsType > 0)
                this.timestampColumn.HeaderText = "Timestamp (UTC)";
            else
                this.timestampColumn.HeaderText = "Timestamp (local)";
            // add additional columns to DGV and data table
            for (int i = _valueColumnOffset; i <= _loxStatFile.ValueCount; i++) {
                // clone column 'Value' (no 2) and add to dgv
                var newColumn = (DataGridViewColumn)columns[_valueColumnOffset].Clone();
                var newName = string.Format("{0}{1}", newColumn.Name, i);
                newColumn.Name = newName;
                newColumn.HeaderText = string.Format("{0} {1}", newColumn.HeaderText, i);
                newColumn.DataPropertyName = newName;
                columns.Add(newColumn);
                // add column to data table and bind to dgv
                _dataTable.Columns.Add(new DataColumn(newName, typeof(double)));
            }
            _dataGridView.MultiSelect = true;

            // fill data table with DPs from file
            if (_loxStatFile.DataPoints != null) {
                for (int row = 0; row < _loxStatFile.DataPoints.Count; row++) {
                    var dataPoint = _loxStatFile.DataPoints[row];
                    DataRow dataRow = _dataTable.NewRow();

                    // add data to row for each column
                    dataRow[indexColumn.Name] = dataPoint.Index;
                    dataRow[timestampColumn.Name] = dataPoint.Timestamp;
                    for (int i = 0; i < _loxStatFile.ValueCount; i++)
                        dataRow[columns[i + _valueColumnOffset].Name] = dataPoint.Values[i];
                    _dataTable.Rows.Add(dataRow);
                }
                _saveButton.Enabled = true;
                _loadButton.Enabled = true;

            } else {
                _problemButton.Enabled = _problems.Any();
                _saveButton.Enabled = false;
                _loadButton.Enabled = false;
            }
            // Bind data table to DGV as data source
            _dataGridView.DataSource = _dataTable;

            RefreshProblems();

            // suspend drawing until chart is filled
            _chart.Series.SuspendUpdates();
;
            RefreshChart();

            // resume drawing
            _chart.Series.ResumeUpdates();
            _chart.Series.Invalidate();
            //_chart.Series.SuspendUpdates();

            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void RefreshProblems()
        {
            if (_loxStatFile.LoadException != null)
                _problems = LoxStatProblem.GetProblems(_loxStatFile);
            else 
                _problems = new List<LoxStatProblem>();
            _problemButton.Enabled = _problems.Any();
        }

        private void RefreshChart()
        {
            // Now we can set up the Chart:
            List<Color> colors = new List<Color> { Color.Green, Color.Red, Color.Black, Color.Blue, Color.Violet, Color.Turquoise, Color.YellowGreen, Color.AliceBlue, Color.Beige, Color.Chocolate, Color.CornflowerBlue, Color.Firebrick };

            _chart.Series.Clear();
            
            // one chart series for each data row (value)
            for (int i = _valueColumnOffset; i < _dataGridView.Columns.Count; i++)
            {
                Series S = _chart.Series.Add(_dataGridView.Columns[i].HeaderText);
                //S.ChartType = SeriesChartType.Spline;
                S.ChartType = SeriesChartType.FastLine;
                S.Color = colors[i];
            }
            
            // Bind data to chart series (source and columns)
            for (int chartSerie = 0; chartSerie < _chart.Series.Count; chartSerie++) {

                _chart.Series[chartSerie].Points.DataBind(_dataTable.Rows, timestampColumn.Name, _dataTable.Columns[_valueColumnOffset + chartSerie].ColumnName,"");
                _chart.Series[chartSerie].XValueMember = timestampColumn.Name;
                _chart.Series[chartSerie].YValueMembers = _dataTable.Columns[_valueColumnOffset + chartSerie].ColumnName;
                
            }
            // bind data table to chart as data source
            _chart.DataSource = _dataTable;
            _chart.DataBind();

            string myShortDateFormatString = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

            // additional settings for chart, x-axis labels, interval, min and max
            for (int chartSerie = 0; chartSerie < _chart.Series.Count; chartSerie++) {
                // format x-axis 
                _chart.Series[0].XValueType = ChartValueType.DateTime;
                DateTime minDate = new DateTime(_loxStatFile.YearMonth.Year, _loxStatFile.YearMonth.Month, 01).AddHours(-1);
                DateTime maxDate = new DateTime(_loxStatFile.YearMonth.Year, _loxStatFile.YearMonth.Month, 01).AddMonths(1);
                _chart.Series[chartSerie].XValueType = ChartValueType.DateTime;
                _chart.ChartAreas[0].AxisX.LabelStyle.Format = myShortDateFormatString;
                _chart.ChartAreas[0].AxisX.Interval = 5;
                _chart.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
                _chart.ChartAreas[0].AxisX.IntervalOffset = 1;
                _chart.ChartAreas[0].AxisX.Minimum = minDate.ToOADate();
                _chart.ChartAreas[0].AxisX.Maximum = maxDate.ToOADate();

                _chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                _chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;

                // Set automatic scrolling 
                _chart.ChartAreas[0].CursorX.AutoScroll = true;
                _chart.ChartAreas[0].CursorY.AutoScroll = true;

                // Allow user selection for Zoom
                _chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                _chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            }
        }

        private void ShowProblems()
        {
            MessageBox.Show(this, string.Format("{1} Problems (showing max 50):{0}{2}", Environment.NewLine, 
                _problems.Count, string.Join(Environment.NewLine, _problems.Take(50))),
                Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            // open file dialog to browse for statistics file and load it
            openFileDialog.FileName = _fileNameTextBox.Text;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(_fileNameTextBox.Text);
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                _fileNameTextBox.Text = openFileDialog.FileName;

            LoadFile();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            LoadFile();
        }

        private void PreviousButton_Click(object sender, EventArgs e) {

            // move selection in main window up
            DataGridView dgv = _parent.DGV;
            int selectedRowIndex = 0;

            // only one row is selected when EDIT button was pressed
            if (dgv.SelectedRows.Count == 1)
                selectedRowIndex = dgv.SelectedRows[0].Index;
            // move selection upwards
            if (selectedRowIndex > 0) {
                dgv.Rows[selectedRowIndex].Selected = false;
                dgv.Rows[selectedRowIndex - 1].Selected = true;
                dgv.CurrentCell = dgv.Rows[selectedRowIndex - 1].Cells[0];
                _fileNameTextBox.Text = _fileItems[selectedRowIndex - 1].FileInfo.FullName;
            }
            LoadFile();
        }

        private void NextButton_Click(object sender, EventArgs e) {

            // move selection in main window up
            DataGridView dgv = _parent.DGV;
            int selectedRowIndex = dgv.Rows.Count;

            // only one row is selected when EDIT button was pressed
            if (dgv.SelectedRows.Count == 1)
                selectedRowIndex = dgv.SelectedRows[0].Index;
            // move selection downwards
            if (selectedRowIndex < dgv.Rows.Count - 1) {
                dgv.Rows[selectedRowIndex].Selected = false;
                dgv.Rows[selectedRowIndex + 1].Selected = true;
                dgv.CurrentCell = dgv.Rows[selectedRowIndex + 1].Cells[0];
                _fileNameTextBox.Text = _fileItems[selectedRowIndex + 1].FileInfo.FullName;
            }
            LoadFile();
        }

        private void DataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {

            int? rowIndex = e?.RowIndex;
            int? colIndex = e?.ColumnIndex;
            string input = e.FormattedValue.ToString();
            if (colIndex == 1) {
                DateTime valueDateTime;
                string[] formats = { "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm" };
                bool isDateTime = DateTime.TryParseExact(input, formats, CultureInfo.CurrentCulture, DateTimeStyles.None, out valueDateTime);
                if (String.IsNullOrEmpty(input) || !isDateTime) {
                    MessageBox.Show($"The value '{e.FormattedValue.ToString()}' is not a valid date / time.\nValid formats are {string.Join("\n", formats)}", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
            } else if (colIndex > 1) {
                double valueDouble;
                bool isDouble = double.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, out valueDouble);
                if (String.IsNullOrEmpty(input) || !isDouble) {
                    MessageBox.Show($"The value '{e.FormattedValue.ToString()}' is not a valid number.", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
            }
        }

        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
            int? rowIndex = e?.RowIndex;
            int? colIndex = e?.ColumnIndex;
            if (rowIndex.HasValue && colIndex.HasValue) {
                var dgv = (DataGridView)sender;
                var cell = dgv?.Rows?[rowIndex.Value]?.Cells?[colIndex.Value]?.Value;
                if (rowIndex != null && rowIndex > -1 && rowIndex < _loxStatFile.DataPoints.Count) {
                    // update DP
                    var dataPoint = _loxStatFile.DataPoints[rowIndex ?? 0];
                    if (!string.IsNullOrEmpty(cell?.ToString())) {
                        if (colIndex == 1)
                            dataPoint.Timestamp = Convert.ToDateTime(cell);
                        else if (colIndex > 1)
                            dataPoint.Values[(colIndex ?? 2) - _valueColumnOffset] = Convert.ToDouble(cell);
                        // update chart
                        _chart.DataBind();
                    };
                }
            };
        }

        private void DataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            var columnIndex = e.ColumnIndex;
            var dataPoint = _loxStatFile.DataPoints[e.RowIndex];

            if (columnIndex == 1) {
                dataPoint.Timestamp = Convert.ToDateTime(e.Value);
                _dataTable.Rows[e.RowIndex][timestampColumn.Name] = Convert.ToDateTime(e.Value);

            } else if (columnIndex > 1) {
                dataPoint.Values[columnIndex - _valueColumnOffset] = Convert.ToDouble(e.Value.ToString());
                _dataTable.Rows[e.RowIndex][columnIndex] = Convert.ToDouble(e.Value.ToString());
            }
                
            RefreshProblems();
            RefreshChart();
        }

        private void DataGridView_MouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu m = new ContextMenu();
                m.MenuItems.Add(new MenuItem("Calculate selected ...", modalCalcSelected_Click));
                m.MenuItems.Add(new MenuItem("Interpolate selected from neighboring cells ...", modalInterpolateSelected_Click));
                m.MenuItems.Add(new MenuItem("Calculate downwards to end of table ...", modalCalcFrom_Click));
                m.MenuItems.Add(new MenuItem("Insert entry (row) above", modalInsertEntry_Click));
                m.MenuItems.Add(new MenuItem("Insert entry (row) below", modalInsertEntryBelow_Click));
                m.MenuItems.Add(new MenuItem("Delete selected entries (rows)", modalDeleteSelected_Click));
                m.MenuItems.Add(new MenuItem("Fill and fix all entries (hourly interval)", modalFillEntries_Click));

                bool allowCalcSelected = true;
                bool allowInterpolateSelected = true;
                bool allowCalcFrom = true;
                bool allowInsertEntry = true;
                bool allowDeleteSelected = true;

                foreach (DataGridViewCell cell in _dataGridView.SelectedCells)
                {
                    // check if "calculate selected" is allowed
                    // at least one row or calculable cell has to be selected
                    if (_dataGridView.SelectedRows.Count == 0 && cell.ColumnIndex < 2)
                    {
                        // At least one cell from the second row onwards has to be selected
                        allowCalcSelected = false;
                        allowInterpolateSelected = false;
                    }
                    // first and last row have no neighbors on both sides
                    if ((cell.RowIndex < 1) || (cell.RowIndex >= _dataGridView.Rows.Count - 1)) {
                        // At least one cell from the second row onwards has to be selected
                        allowInterpolateSelected = false;
                    }
                    //Console.WriteLine($"row: {cell.RowIndex}, column: {cell.ColumnIndex} is selected.");
                }
                // Check if at least one cell is selected
                if (_dataGridView.SelectedCells.Count <= 0) {
                    allowInterpolateSelected = false;
                }

                //Console.WriteLine($"Selected rows: {_dataGridView.SelectedRows.Count}");
                //Console.WriteLine("---");

                // check if "calculate downwards" is allowed
                if (
                    (_dataGridView.SelectedRows.Count != 1 && _dataGridView.SelectedCells[0].ColumnIndex == 0)
                    ||
                    (_dataGridView.SelectedRows.Count == 0 && _dataGridView.SelectedCells.Count != 1)
                    ||
                    (_dataGridView.SelectedRows.Count == 0 && _dataGridView.SelectedCells[0].ColumnIndex < 2)
                )
                {
                    allowCalcFrom = false;
                }

                // check if "insert entry" is allowed
                if (_dataGridView.SelectedRows.Count != 1)
                {
                    allowInsertEntry = false;
                }

                // check if "delete selected entries" is allowed
                if (_dataGridView.SelectedRows.Count < 1)
                {
                    allowDeleteSelected = false;
                }

                // Calculate selected
                m.MenuItems[0].Enabled = allowCalcSelected;
                // Calculate selected
                m.MenuItems[1].Enabled = allowInterpolateSelected;
                // Calculate downwards from this row
                m.MenuItems[2].Enabled = allowCalcFrom;
                // Insert entry
                m.MenuItems[3].Enabled = allowInsertEntry;
                // Insert entry
                m.MenuItems[4].Enabled = allowInsertEntry;
                // Delete selected entries
                m.MenuItems[5].Enabled = allowDeleteSelected;

                // Show the context menu on mouse position
                m.Show(_dataGridView, new Point(_dataGridView.PointToClient(Control.MousePosition).X, _dataGridView.PointToClient(Control.MousePosition).Y));

            }
        }

        private void modalCalcSelected_Click(object sender, EventArgs e) {
            string chrDec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string chrGrp = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            string input = "v - 100";
            string pattern = "v";
            string replacement = "1";
            double myValue;
            string formula = "";
            bool isFormula = true;

            // Show the input dialog and check the result
            DialogResult dialogResult = ShowInputDialog(ref input, this);
            if (dialogResult == DialogResult.Cancel) {
                // User clicked "Cancel" or pressed "ESC", abort the operation
                Console.WriteLine("User cancelled the operation.");
                return;
            }


            // Replace comma, if it is the decimal separator
            if (chrDec == ",") {
                input = Regex.Replace(input, "/.", "");
                input = Regex.Replace(input, chrDec, chrGrp);
            }

            // Check if the input is a formula
            formula = Regex.Replace(input, pattern, replacement);
            try {
                myValue = Convert.ToDouble(new System.Data.DataTable().Compute(formula, null));
            }
            catch (System.Data.DataException de) {
                MessageBox.Show(de.Message);
                isFormula = false;
            }

            if (isFormula) {
                // Check if at least one cell is selected
                if (_dataGridView.SelectedCells.Count > 0) {
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
                        foreach (DataGridViewCell cell in _dataGridView.SelectedCells) {
                            int rowIndex = cell.RowIndex;
                            int colIndex = cell.ColumnIndex;

                            // if a row is selected, skip first two columns
                            if (colIndex >= 2) {
                                replacement = Convert.ToDouble(_dataGridView.Rows[rowIndex].Cells[colIndex].Value).ToString("#.###", CultureInfo.CreateSpecificCulture("en-EN"));
                                formula = Regex.Replace(input, pattern, replacement);
                                try {
                                    myValue = Convert.ToDouble(new System.Data.DataTable().Compute(formula, null));
                                }
                                catch (System.Data.DataException) {
                                    myValue = Convert.ToDouble(replacement);
                                }

                                _dataGridView.Rows[rowIndex].Cells[colIndex].Value = myValue.ToString();
                                _dataTable.Rows[rowIndex][colIndex] = myValue;
                            }
                        }
                    }
                    finally {
                        // Close the loading form after the loop
                        busyForm.Close();
                    }
                } else {
                    MessageBox.Show("Something did not match. Please try again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

        }

        private void modalInterpolateSelected_Click(object sender, EventArgs e) {

            // insert one row above the selected row (only one row is allowed to be selected)
            Cursor = Cursors.WaitCursor;
            
            // Problem: SelectedRows is not sorted, so SelectedRows[0] can be the highest row that is selected!
            // int rowIndex = _dataGridView.SelectedRows[0].Index;
            int firstSelectedRowIndex = _dataGridView.Rows.Count;
            int lastSelectedRowIndex = 0;
            int selCount = _dataGridView.SelectedRows.Count;

            // loop through selected rows to find first and last selected cell
            foreach (DataGridViewCell cell in _dataGridView.SelectedCells) {
                int rowIndex = cell.RowIndex;
                int colIndex = cell.ColumnIndex;

                // delete rows from DP list and data table
                // get lowest selected row
                if (rowIndex < firstSelectedRowIndex)
                    firstSelectedRowIndex = rowIndex;
                if (rowIndex > lastSelectedRowIndex)
                    lastSelectedRowIndex = rowIndex;
            }

            double xOldDiff, xNewDiff;
            double yOldDiff, newValue;

            // new time stamp is before any existing data - use longest time difference to calculate linear graph
            xOldDiff = (_dataTable.Rows[lastSelectedRowIndex + 1].Field<DateTime>("timestampColumn") - _dataTable.Rows[firstSelectedRowIndex - 1].Field<DateTime>("timestampColumn")).TotalSeconds;

            foreach (DataGridViewCell cell in _dataGridView.SelectedCells) {
                int rowIndex = cell.RowIndex;
                int colIndex = cell.ColumnIndex;

                xNewDiff = (_dataTable.Rows[rowIndex].Field<DateTime>("timestampColumn") - _dataTable.Rows[firstSelectedRowIndex - 1].Field<DateTime>("timestampColumn")).TotalSeconds;

                // if a row is selected, skip first two columns - only interpolate any values
                if (colIndex > 1) {

                    yOldDiff = (double)_dataTable.Rows[lastSelectedRowIndex + 1][colIndex] - (double)_dataTable.Rows[firstSelectedRowIndex - 1][colIndex];

                    if (xOldDiff > 0)
                        newValue = Math.Round((double)_dataTable.Rows[firstSelectedRowIndex - 1][colIndex] + (yOldDiff / xOldDiff) * xNewDiff, 3);
                    else
                        newValue = Math.Round((double)_dataTable.Rows[firstSelectedRowIndex - 1][colIndex], 3);
                    _dataTable.Rows[rowIndex][colIndex] = newValue;
                    _dataGridView.Rows[rowIndex].Cells[colIndex].Value = newValue;

                    var dataPoint = _loxStatFile.DataPoints[rowIndex];
                    dataPoint.Values[colIndex - _valueColumnOffset] = newValue;
                    
                    
                }
            }
            // update chart
            _chart.DataBind();
            Cursor = Cursors.Default;
        }

        private void modalCalcFrom_Click(object sender, EventArgs e)
        {
            string chrDec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string chrGrp = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            string input = "v - 100";
            string pattern = "v";
            string replacement = "1";
            double myValue;
            string formula = "";
            bool isFormula = true;

            // Show the input dialog and check the result
            DialogResult dialogResult = ShowInputDialog(ref input, this);
            if (dialogResult == DialogResult.Cancel)
            {
                // User clicked "Cancel" or pressed "ESC", abort the operation
                Console.WriteLine("User cancelled the operation.");
                return;
            }

            // Replace comma, if it is the decimal separator
            if (chrDec == ",")
            {
                input = Regex.Replace(input, "/.", "");
                input = Regex.Replace(input, chrDec, chrGrp);
            }

            formula = Regex.Replace(input, pattern, replacement);
            try
            {
                myValue = Convert.ToDouble(new System.Data.DataTable().Compute(formula, null));
            }
            catch (System.Data.DataException de)
            {
                MessageBox.Show(de.Message);
                isFormula = false;
            }

            if (isFormula)
            {
                // Check if at least one row is selected
                if (_dataGridView.SelectedRows.Count == 1 || _dataGridView.SelectedCells.Count == 1)
                {
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

                    try
                    {
                        int y_max;

                        // Check if all columns need to be recalculated or only one
                        if (_dataGridView.SelectedRows.Count == 1)
                        {
                            y_max = _dataGridView.ColumnCount;
                        }
                        else
                        {
                            y_max = _dataGridView.SelectedCells[0].ColumnIndex + 1;
                        }

                        for (int x = _dataGridView.SelectedCells[0].RowIndex; x < _dataGridView.Rows.Count; x++)
                        {

                            for (int y = _dataGridView.SelectedCells[0].ColumnIndex; y < y_max; y++)
                            {
                                //Console.WriteLine($"x: {x}, y: {y}");

                                // if a row is selected, skip first two columns
                                if (y >= 2)
                                {
                                    replacement = Convert.ToDouble(_dataGridView.Rows[x].Cells[y].Value).ToString("#.###", CultureInfo.CreateSpecificCulture("en-EN"));
                                    formula = Regex.Replace(input, pattern, replacement);
                                    try
                                    {
                                        myValue = Convert.ToDouble(new System.Data.DataTable().Compute(formula, null));
                                    }
                                    catch (System.Data.DataException)
                                    {
                                        myValue = Convert.ToDouble(replacement);
                                    }

                                    _dataGridView.Rows[x].Cells[y].Value = myValue.ToString();
                                    _dataTable.Rows[x][y] = myValue;
                                }
                            }
                        }
                    }
                    finally
                    {
                        // Close the loading form after the loop
                        busyForm.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Something did not match. Please try again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void modalInsertEntry_Click(object sender, EventArgs e)
        {
            // insert one row above the selected row (only one row is allowed to be selected)
            Cursor = Cursors.WaitCursor;

            //DataGridViewRow newRow = (DataGridViewRow)_dataGridView.Rows[0].Clone();
            DataGridViewRow atInsert = _dataGridView.SelectedRows[0];
            LoxStatDataPoint beforeDP = null; // Initialize beforeDP to null
            LoxStatDataPoint newDP;
            LoxStatDataPoint atDP = _loxStatFile.DataPoints[atInsert.Index];

            bool insertAllowed = false;

            if (atInsert.Index == 0)
            {
                insertAllowed = true; // Insertion at the beginning is always possible
            }
            else
            {
                // Ensure beforeDP is assigned before this point
                beforeDP = _loxStatFile.DataPoints[atDP.Index - 1];
                if (beforeDP != null && beforeDP.Timestamp < atDP.Timestamp.AddMinutes(-119)) // Check for null to satisfy the compiler
                {
                    insertAllowed = true; // Insertion is possible based on timestamp comparison
                }
                else
                {
                    MessageBox.Show("There is no place to insert an entry!\n\nPlease check again, if there is a missing entry.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            // Only adjust indices and insert new data point if insertion is confirmed
            if (insertAllowed)
            {
                // increase index for selected row up to the end of the table
                for (int x = atDP.Index; x < _loxStatFile.DataPoints.Count; x++)
                {
                    _loxStatFile.DataPoints[x].Index += 1;
                    _dataTable.Rows[x][indexColumn.Name] = (int)_dataTable.Rows[x][0] + 1;
                }
                DataRow newDataRow = _dataTable.NewRow();

                // insert new DP at the beginning of the list
                if (atInsert.Index == 0)
                {
                    newDP = atDP.Clone();
                    newDP.Index = 0;
                    DateTime timestamp = new DateTime(atDP.Timestamp.Year, atDP.Timestamp.Month, 1, 0, 0, 0);
                    newDP.Timestamp = timestamp;
                    _loxStatFile.DataPoints.Insert(0, newDP);
                    //_dataGridView.Rows.Insert(0, newRow);

                    // add data to row for each column
                    newDataRow[indexColumn.Name] = 0;
                    newDataRow[timestampColumn.Name] = timestamp;
                    for (int i = 0; i < _loxStatFile.ValueCount; i++)
                        newDataRow[i + _valueColumnOffset] = newDP.Values[i];
                    _dataTable.Rows.InsertAt(newDataRow, 0);
                }
                else
                {
                    // Insertion logic for non-first row already validated above
                    if (beforeDP != null) // Additional null check for safety
                    {
                        newDP = beforeDP.Clone();
                        newDP.Index = atInsert.Index;
                        DateTime timestamp = beforeDP.Timestamp.AddHours(1);
                        newDP.Timestamp = timestamp;
                        _loxStatFile.DataPoints.Insert(atInsert.Index, newDP);
                        //_dataGridView.Rows.Insert(atInsert.Index - 1, newRow);
                        newDataRow[indexColumn.Name] = atInsert.Index;
                        newDataRow[timestampColumn.Name] = timestamp;
                        for (int i = 0; i < _loxStatFile.ValueCount; i++)
                            newDataRow[i + _valueColumnOffset] = beforeDP.Values[i];
                        _dataTable.Rows.InsertAt(newDataRow, atInsert.Index);
                    }
                }
                _dataGridView.Refresh();
                RefreshProblems();
                RefreshChart();
            }

            Cursor = Cursors.Default;
        }

        private void modalInsertEntryBelow_Click(object sender, EventArgs e) {

            // insert one row below the selected row  (only one row is allowed to be selected)
            Cursor = Cursors.WaitCursor;

            //DataGridViewRow newRow = (DataGridViewRow)_dataGridView.Rows[0].Clone();
            DataGridViewRow atInsert = _dataGridView.SelectedRows[0];
            LoxStatDataPoint afterDP = null; // Initialize afterDP to null
            LoxStatDataPoint newDP;
            LoxStatDataPoint atDP = _loxStatFile.DataPoints[atInsert.Index];

            bool insertAllowed = false;

            if (atInsert.Index == _loxStatFile.DataPoints.Count - 1) {
                insertAllowed = true; // Insertion at the end is always possible
            } else {
                // Ensure afterDP is assigned after this point
                afterDP = _loxStatFile.DataPoints[atDP.Index + 1];
                if (afterDP != null && afterDP.Timestamp > atDP.Timestamp.AddMinutes(+119)) // Check for null to satisfy the compiler
                {
                    insertAllowed = true; // Insertion is possible based on timestamp comparison
                } else {
                    MessageBox.Show("There is no place to insert an entry!\n\nPlease check again, if there is a missing entry.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            // Only adjust indices and insert new data point if insertion is confirmed
            if (insertAllowed) {
                for (int x = atDP.Index + 1; x < _loxStatFile.DataPoints.Count; x++) {
                    _loxStatFile.DataPoints[x].Index += 1;
                    _dataTable.Rows[x][indexColumn.Name] = (int)_dataTable.Rows[x][0] + 1;
                }
                DataRow newDataRow = _dataTable.NewRow();

                // insert new DP at the end of the list
                if (atInsert.Index == _loxStatFile.DataPoints.Count - 1) {
                    newDP = atDP.Clone();
                    newDP.Index = _loxStatFile.DataPoints.Count;
                    DateTime timestamp = new DateTime(atDP.Timestamp.Year, atDP.Timestamp.Month, 1, 0, 0, 0).AddMonths(1);
                    newDP.Timestamp = timestamp;
                    _loxStatFile.DataPoints.Add(newDP);
                    //_dataGridView.Rows.Insert(0, newRow);

                    // add data to row for each column
                    newDataRow[indexColumn.Name] = _dataTable.Rows.Count;
                    newDataRow[timestampColumn.Name] = timestamp;
                    for (int i = 0; i < _loxStatFile.ValueCount; i++)
                        newDataRow[i + _valueColumnOffset] = newDP.Values[i];
                    _dataTable.Rows.Add(newDataRow);
                } else {
                    // insert new DP after selected DP (atDP)
                    newDP = atDP.Clone();
                    newDP.Index = atInsert.Index + 1;
                    DateTime timestamp = atDP.Timestamp.AddHours(1);
                    newDP.Timestamp = timestamp;
                    _loxStatFile.DataPoints.Insert(atInsert.Index + 1, newDP);
                    newDataRow[indexColumn.Name] = atInsert.Index + 1;
                    newDataRow[timestampColumn.Name] = timestamp;
                    for (int i = 0; i < _loxStatFile.ValueCount; i++)
                        newDataRow[i + _valueColumnOffset] = atDP.Values[i];
                    _dataTable.Rows.InsertAt(newDataRow, atInsert.Index + 1);
                }
                _dataGridView.Refresh();
                RefreshProblems();
                RefreshChart();
            }

            Cursor = Cursors.Default;
        }

        private void modalDeleteSelected_Click(object sender, EventArgs e)
        {
            // delete all selected rows
            Cursor = Cursors.WaitCursor;

            // Problem: SelectedRows is not sorted, so SelectedRows[0] can be the highest row that is selected!
            // int rowIndex = _dataGridView.SelectedRows[0].Index;
            int firstSelectedRowIndex = _dataTable.Rows.Count;
            int selCount = _dataGridView.SelectedRows.Count;

            // loop through selected rows backwards to delete the correct rows
            for (int rowIndex = _dataTable.Rows.Count - 1; rowIndex > 0; rowIndex--) {
                if (_dataGridView.Rows[rowIndex].Selected) {
                    // delete rows from DP list and data table
                    _loxStatFile.DataPoints.RemoveAt(rowIndex);
                    _dataTable.Rows.RemoveAt(rowIndex);
                    // get lowest selected row
                    if (rowIndex < firstSelectedRowIndex)
                        firstSelectedRowIndex = rowIndex;
                }
            }
            // re-write index starting by firstSelectedRowIndex (the seleted rows were deleted!)
            int newIndex = 0;
            for (int rowIndex = firstSelectedRowIndex; rowIndex < _dataTable.Rows.Count; rowIndex++) {
                if (rowIndex > 0)
                    newIndex = _loxStatFile.DataPoints[rowIndex - 1].Index + 1;
                _loxStatFile.DataPoints[rowIndex].Index = newIndex;
                _dataTable.Rows[rowIndex][0] = newIndex;
            }
            _dataGridView.Refresh();
            RefreshProblems();
            RefreshChart();
            Cursor = Cursors.Default;
        }

        private void modalFillEntries_Click(object sender, EventArgs e)
        {
            // fill and fix entries -
            // - adds a DPs (one per hour) if there is a gap between two DP that is greater than an hour 
            //   - value(s) are calculated by use this and the next DP to interpolate the missing DPs through a linear graph for each data column
            Cursor = Cursors.WaitCursor;

            // Sort list by timestamp, remove duplicates
            _loxStatFile.DataPoints = _loxStatFile.DataPoints
                .OrderBy(dp => dp.Timestamp)  // Sort by timestamp
                .GroupBy(dp => dp.Timestamp)  // Group by timestamp
                .Select(g => g.First())  // Remove duplicated timestamps
                .Where(dp =>
                    dp.Index >= 0 &&  // Ensure Index is set
                    dp.Timestamp != null  // Ensure Timestamp is set
                )
                .Select((dp, index) => { dp.Index = index; return dp; })  // Re-index
                .ToList();

            // Update DataGridView row count, else it might by that there are empty rows shown
            //_dataGridView.RowCount = _loxStatFile.DataPoints.Count;

            // in case DPs were rearranged, rewrite the whole list of DPs to data table
            for (int i = 0; i < _loxStatFile.DataPoints.Count; i++) {
                _dataTable.Rows[i][0] = i;
                _dataTable.Rows[i][1] = _loxStatFile.DataPoints[i].Timestamp;
                for (int j = 0; j < _loxStatFile.ValueCount; j++)
                    _dataTable.Rows[i][_valueColumnOffset + j] = _loxStatFile.DataPoints[i].Values[j];
            }
            // reduce data table if DPs had same timestamps
            for (int i = _loxStatFile.DataPoints.Count; i < _dataTable.Rows.Count; i++) {
                _dataTable.Rows.RemoveAt(_loxStatFile.DataPoints.Count);
            }

            for (int i = 0; i < _loxStatFile.DataPoints.Count - 1; i++)
            {
                LoxStatDataPoint currentDP = _loxStatFile.DataPoints[i];
                LoxStatDataPoint nextDP = _loxStatFile.DataPoints[i + 1];
                TimeSpan ts = nextDP.Timestamp - currentDP.Timestamp;
                double hoursDiff = ts.TotalHours;

                // Check if there's more than 1 hour difference indicating a gap
                if (hoursDiff > 1)
                {
                    // one DP per hour will be added - may be enhanced to be selectable from (5, 10, 15, 30 min up to 1h)
                    double insertCount = hoursDiff - 1;
                    // use this and the next DP to interpolate the missing DPs through a linear graph for each data column
                    double[] diff = new double[_loxStatFile.ValueCount];
                    for (int j = 0; j < _loxStatFile.ValueCount; j++)
                        diff[j] = (nextDP.Values[j] - currentDP.Values[j]) / (insertCount + 1);

                    // Adjust indices for subsequent data points
                    for (int x = nextDP.Index; x < _loxStatFile.DataPoints.Count; x++)
                    {
                        _loxStatFile.DataPoints[x].Index += (int)insertCount;
                        _dataTable.Rows[x][0] = (int)_dataTable.Rows[x][0] + (int)insertCount;
                    }

                    // Insert missing data points
                    for (int x = 1; x <= insertCount; x++)
                    {
                        LoxStatDataPoint newDP = currentDP.Clone();
                        newDP.Index += x;
                        DateTime timestamp = newDP.Timestamp.AddHours(x);
                        newDP.Timestamp = timestamp;
                        for (int j = 0; j < _loxStatFile.ValueCount; j++)
                            newDP.Values[j] += diff[j] * x;
                        _loxStatFile.DataPoints.Insert(newDP.Index, newDP);
                        //_dataGridView.Rows.Insert(newDP.Index, 1);

                        DataRow newDataRow = _dataTable.NewRow();
                        newDataRow[indexColumn.Name] = currentDP.Index + x;
                        newDataRow[timestampColumn.Name] = timestamp;
                        for (int j = 0; j < _loxStatFile.ValueCount; j++)
                            newDataRow[j + _valueColumnOffset] = currentDP.Values[j] + diff[j] * x;
                        _dataTable.Rows.InsertAt(newDataRow, newDP.Index);
                    }

                    // Adjust loop counter to skip newly inserted points
                    i += (int)insertCount;
                }
            }

            _dataGridView.Refresh();
            RefreshProblems();
            RefreshChart();
            Cursor = Cursors.Default;
        }

        private static DialogResult ShowInputDialog(ref string input, Form parentForm)
        {
            System.Drawing.Size size = new System.Drawing.Size(250, 130);
            Form inputBox = new Form {
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog,
                ClientSize = size,
                Text = "Name",
                StartPosition = FormStartPosition.CenterParent // Set the start position
            };

            System.Windows.Forms.TextBox textBox = new TextBox {
                Size = new System.Drawing.Size(size.Width - 20, 23),
                Location = new System.Drawing.Point(10, 10),
                Text = input
            };
            inputBox.Controls.Add(textBox);

            Label lblInfo = new Label {
                Size = new Size(size.Width - 30, 60),
                Location = new Point(15, 37),
                Text = "Insert a formula for calculating the values. Allowed operators: + - / * ( )\n" +
                "Use v for the original value.\n" +
                "Like: v - 100"
            };
            inputBox.Controls.Add(lblInfo);

            Button okButton = new Button {
                DialogResult = System.Windows.Forms.DialogResult.OK,
                Name = "okButton",
                Size = new System.Drawing.Size(75, 23),
                Text = "&OK",
                Location = new System.Drawing.Point(size.Width - 80 - 80, 99)
            };
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button {
                DialogResult = System.Windows.Forms.DialogResult.Cancel,
                Name = "cancelButton",
                Size = new System.Drawing.Size(75, 23),
                Text = "&Cancel",
                Location = new System.Drawing.Point(size.Width - 80, 99)
            };
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }

        private void FileForm_Load(object sender, EventArgs e)
        {
            // if not remote desktop session then enable double-buffering optimization
            if (!System.Windows.Forms.SystemInformation.TerminalServerSession)
            {
                typeof(DataGridView).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty, null, _dataGridView, new object[] { true });
            }

            // load file and fill form
            if ((_args != null) && (_args.Length > 0)) {
                _fileNameTextBox.Text = _args[0];
                LoadFile();
            }
        }

        private void FileForm_Closing(object sender, FormClosingEventArgs e) {
            this.SaveWindowPosition(sender, e);
        }

        private void FileForm_ResizeEnd(object sender, EventArgs e) {
            this.SaveWindowPosition(sender, e);
        }

        private void SaveWindowPosition(object sender, EventArgs e) {
            Properties.Settings.Default.IsMaximizedFileWindow = WindowState == FormWindowState.Maximized;
            Properties.Settings.Default.FileWindowPosition = DesktopBounds;
            Properties.Settings.Default.FileWindowSplitterDistance = this.splitContainer.SplitterDistance;
            Properties.Settings.Default.Save();
        }


        private void SaveButton_Click(object sender, EventArgs e) {
            RefreshProblems();
            if(_problems.Any())
                ShowProblems();
            if(MessageBox.Show(
                this,
                "### DISCLAIMER ###\n" + 
                "- I will not (and can not) take any responsibility for any\n" + 
                "  issues of your Loxone installation whatsoever!\n" +
                "- Loxone will most likely not support any issues, if you use the\n" +
                "  files generated by this tool either!\n" +
                "- I am just a desparate Loxone user in need of a solution for a\n" +
                "  problem and lucky enough to be a professional coder!\n" +
                "- Loxone did not provide me a full file format documentation\n" +
                "  and I used Hexinator (thank you!) to decode the file format\n" + 
                "  to some degree!\n\n" +
                "### NOTICE ###\n" +
                "To apply the modified statistics further steps are needed!\n" +
                "1. Upload the modified statistics\n" +
                "2. Restart the Miniserver\n" + "" +
                "3. Clear the app cache\n" + 
                "If you don't know how to clear the app cache, then remove\n" +
                "and re-add the Miniserver in the app. Should this not help\n" + "" +
                "then uninstall and install the app again.\n\n" +
                "Would you still like to save the file?",
                "DISCLAIMER! USE AT YOUR OWN RISK!",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
                ) != DialogResult.Yes)
                return;
            _loxStatFile.FileName = Path.GetFullPath(_fileNameTextBox.Text);
            _loxStatFile.Save();
            //MessageBox.Show(this, "Save successful!", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ProblemButton_Click(object sender, EventArgs e) {
            ShowProblems();
        }

        private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            return;
        }

        private void DataGridView_KeyDown(object sender, KeyEventArgs e) {

            if (e.Control && e.KeyCode == Keys.V)
            {
                string CopiedContent = Clipboard.GetText();
                string[] Lines = CopiedContent.Split('\n');
                int StartingRow = _dataGridView.CurrentCell.RowIndex;
                int StartingColumn = _dataGridView.CurrentCell.ColumnIndex;
                foreach (var line in Lines)
                {
                    if (StartingRow <= (_dataGridView.Rows.Count - 1))
                    {
                        string[] cells = line.Split('\t');
                        int ColumnIndex = StartingColumn;
                        for (int i = 0; i < cells.Length && ColumnIndex <= (_dataGridView.Columns.Count - 1); i++, ColumnIndex++)
                        {
                            if (!String.IsNullOrEmpty(cells[i]))
                            {
                                if (ColumnIndex == 0) // Specific check for column with index 0
                                {
                                    // Do nothing, as the index is not editable
                                }
                                else if (ColumnIndex == 1) // Specific check for column with index 1
                                {
                                    var input = cells[i].Trim();

                                    // Attempt to parse the cell value as a DateTime
                                    DateTime valueDateTime;
                                    string[] formats = { "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm" };
                                    // bool isDateTime = DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out valueDateTime);
                                    bool isDateTime = DateTime.TryParseExact(input, formats, CultureInfo.CurrentCulture, DateTimeStyles.None, out valueDateTime);

                                    if (isDateTime)
                                    {
                                        // If the value is a valid DateTime, assign it
                                        var dataPoint = _loxStatFile.DataPoints[StartingRow];
                                        //dataPoint.Values[ColumnIndex - _valueColumnOffset] = valueDateTime;
                                        //Console.WriteLine($"StartingRow: {StartingRow} - StartingColumn: {StartingColumn} - i: {i} - ColumnIndex: {ColumnIndex}");

                                        dataPoint.Timestamp = valueDateTime;
                                        //_dataTable.Rows[StartingRow][timestampColumn.Name] = valueDateTime;
                                        //_dataGridView.UpdateCellValue(ColumnIndex, StartingRow);
                                    }
                                    else
                                    {
                                        // Handle parsing failure, e.g., by showing an error message or skipping the assignment
                                        MessageBox.Show($"The value '{input}' is not a valid date time. Please use 'dd.MM.yyyy HH:mm:ss'.", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                                else // Existing check for other columns
                                {
                                    string chrDec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                                    string chrGrp = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;

                                    var input = cells[i].Trim();

                                    // Replace comma, if it is the decimal separator
                                    // Is this really needed? Have to test with different locales
                                    if (chrDec == ",")
                                    {
                                        input = Regex.Replace(input, "/.", "");
                                        input = Regex.Replace(input, chrDec, chrGrp);
                                    }

                                    // Attempt to parse the cell value as a double
                                    double valueDouble;
                                    bool isDouble = double.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, out valueDouble);

                                    if (isDouble)
                                    {
                                        // If the value is a valid double, assign it
                                        var dataPoint = _loxStatFile.DataPoints[StartingRow];
                                        dataPoint.Values[ColumnIndex - _valueColumnOffset] = valueDouble;
                                        //_dataTable.Rows[StartingRow][ColumnIndex] = valueDouble;
                                        //_dataGridView.UpdateCellValue(ColumnIndex, StartingRow);
                                    }
                                    else
                                    {
                                        // If the value is not a valid double, show an error message
                                        MessageBox.Show($"The value '{input}' is not a valid number.", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                            }
                        }
                        StartingRow++;
                    }
                }
                RefreshProblems();
                RefreshChart();
                _dataGridView.Refresh();
            }
        }

        private void _chartMouseClick(object sender, MouseEventArgs e) {
            // Define a tolerance in pixels
            _chart.DataBind();
            const double tolerance = 20;

            // Perform a hit test to find the clicked chart element
            HitTestResult result = _chart.HitTest(e.X, e.Y);

            // Initialize variables to track the closest data point and its distance
            double minDistance = double.MaxValue;
            int closestPointIndex = -1;

            // Check all series in the chart
            foreach (Series series in _chart.Series) {
                for (int i = 0; i < series.Points.Count; i++) {
                    DataPoint point = series.Points[i];
                    // Convert data point position to pixel position in the chart
                    double pointX = _chart.ChartAreas[0].AxisX.ValueToPixelPosition(point.XValue);
                    double pointY = _chart.ChartAreas[0].AxisY.ValueToPixelPosition(point.YValues[0]);

                    // Calculate distance from the click to this data point
                    double distance = Math.Sqrt(Math.Pow(e.X - pointX, 2) + Math.Pow(e.Y - pointY, 2));

                    // Check if this is the closest data point so far
                    if (distance < minDistance) {
                        minDistance = distance;
                        closestPointIndex = i;
                    }
                }
            }

            // If the closest data point is within the tolerance, treat it as a click on that point
            if (minDistance <= tolerance && closestPointIndex != -1) {
                // Assuming the closest data point index corresponds to the row index in the DataGridView
                // Make sure to validate the index before using it
                if (closestPointIndex >= 0 && closestPointIndex < _dataGridView.Rows.Count) {
                    // Clear the current selection
                    _dataGridView.ClearSelection();

                    // Select the corresponding row in the DataGridView
                    _dataGridView.Rows[closestPointIndex].Selected = true;

                    // Optionally, scroll to the selected row if it's not visible
                    // Optionally, scroll to the selected row if it's not visible
                    _dataGridView.FirstDisplayedScrollingRowIndex = Math.Min(closestPointIndex + 5, _dataGridView.Rows.Count - 1);
                    _dataGridView.FirstDisplayedScrollingRowIndex = Math.Max(closestPointIndex - 5, 0);
                }
            }
        }
        private void _chartMouseClick_Fast(object sender, MouseEventArgs e)
        {
            // Perform a hit test to find the clicked chart element
            HitTestResult result = _chart.HitTest(e.X, e.Y);

            // Initialize variables to track the closest data point and its distance
            double minDistance = double.MaxValue;
            int closestPointIndex = 0;

            // Check all series in the chart
            for (int i = 0; i < _chart.Series[0].Points.Count; i++) {
                DataPoint point = _chart.Series[0].Points[i];
                // Convert data point X position to pixel position in the chart
                double pointX = _chart.ChartAreas[0].AxisX.ValueToPixelPosition(point.XValue);

                // Calculate distance from the click to this data point
                double distance = Math.Abs(e.X - pointX);

                // Check if this is the closest data point so far
                if (distance < minDistance) {
                    minDistance = distance;
                    closestPointIndex = i;
                }
            }

            // Assuming the closest data point index corresponds to the row index in the DataGridView
            // Make sure to validate the index before using it
            if (closestPointIndex >= 0 && closestPointIndex < _dataGridView.Rows.Count)
            {
                // Clear the current selection
                _dataGridView.ClearSelection();

                // Select the corresponding row in the DataGridView
                _dataGridView.Rows[closestPointIndex].Selected = true;

                // Optionally, scroll to the selected row if it's not visible
                _dataGridView.FirstDisplayedScrollingRowIndex = Math.Min(closestPointIndex + 5, _dataGridView.Rows.Count - 1);
                _dataGridView.FirstDisplayedScrollingRowIndex = Math.Max(closestPointIndex - 5, 0);
            }
        }

        private void DataGridView_MouseMove(object sender, MouseEventArgs e)
        {
            // Perform a hit test to find the column index under the mouse
            var hitTestInfo = _dataGridView.HitTest(e.X, e.Y);
            if (hitTestInfo.Type == DataGridViewHitTestType.Cell)
            {
                // Check if the column under the mouse is read-only
                bool isReadOnly = _dataGridView.Columns[hitTestInfo.ColumnIndex].ReadOnly;
                if (isReadOnly)
                {
                    // Change the cursor to indicate the column is read-only
                    _dataGridView.Cursor = Cursors.No;
                }
                else
                {
                    // Reset the cursor to the default
                    _dataGridView.Cursor = Cursors.Default;
                }
            }
            else
            {
                // Reset the cursor to the default if not over a cell
                _dataGridView.Cursor = Cursors.Default;
            }
        }

    }
}
