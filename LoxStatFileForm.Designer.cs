namespace LoxStatEdit
{
    partial class LoxStatFileForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label fileInfoLabel;
            System.Windows.Forms.Button browseButton;
            System.Windows.Forms.Label fileNameLabel;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoxStatFileForm));
            this._saveButton = new System.Windows.Forms.Button();
            this._loadButton = new System.Windows.Forms.Button();
            this._problemButton = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this._fileInfoTextBox = new System.Windows.Forms.TextBox();
            this._dataGridView = new System.Windows.Forms.DataGridView();
            this.indexColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.timestampColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileNameTextBox = new System.Windows.Forms.TextBox();
            this._chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._prevButton = new System.Windows.Forms.Button();
            this._nextButton = new System.Windows.Forms.Button();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            fileInfoLabel = new System.Windows.Forms.Label();
            browseButton = new System.Windows.Forms.Button();
            fileNameLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._chart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileInfoLabel
            // 
            fileInfoLabel.Location = new System.Drawing.Point(12, 43);
            fileInfoLabel.Name = "fileInfoLabel";
            fileInfoLabel.Size = new System.Drawing.Size(60, 13);
            fileInfoLabel.TabIndex = 4;
            fileInfoLabel.Text = "File Info:";
            // 
            // browseButton
            // 
            browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            browseButton.Location = new System.Drawing.Point(716, 11);
            browseButton.Name = "browseButton";
            browseButton.Size = new System.Drawing.Size(75, 23);
            browseButton.TabIndex = 2;
            browseButton.Text = "&Browse...";
            this.toolTip.SetToolTip(browseButton, "Browse local statistic files (ALT + B)");
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // fileNameLabel
            // 
            fileNameLabel.Location = new System.Drawing.Point(12, 17);
            fileNameLabel.Name = "fileNameLabel";
            fileNameLabel.Size = new System.Drawing.Size(60, 13);
            fileNameLabel.TabIndex = 0;
            fileNameLabel.Text = "File Name:";
            // 
            // _saveButton
            // 
            this._saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._saveButton.Location = new System.Drawing.Point(796, 38);
            this._saveButton.Name = "_saveButton";
            this._saveButton.Size = new System.Drawing.Size(75, 23);
            this._saveButton.TabIndex = 7;
            this._saveButton.Text = "&Save";
            this.toolTip.SetToolTip(this._saveButton, "Save current statistic file (ALT + S)");
            this._saveButton.UseVisualStyleBackColor = true;
            this._saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // _loadButton
            // 
            this._loadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._loadButton.Location = new System.Drawing.Point(796, 11);
            this._loadButton.Name = "_loadButton";
            this._loadButton.Size = new System.Drawing.Size(75, 23);
            this._loadButton.TabIndex = 3;
            this._loadButton.Text = "&Load";
            this.toolTip.SetToolTip(this._loadButton, "Load selected statistic file (ALT + L)");
            this._loadButton.UseVisualStyleBackColor = true;
            this._loadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // _problemButton
            // 
            this._problemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._problemButton.Enabled = false;
            this._problemButton.ForeColor = System.Drawing.Color.Red;
            this._problemButton.Location = new System.Drawing.Point(716, 38);
            this._problemButton.Name = "_problemButton";
            this._problemButton.Size = new System.Drawing.Size(75, 23);
            this._problemButton.TabIndex = 6;
            this._problemButton.Text = "&Problems";
            this.toolTip.SetToolTip(this._problemButton, "Show recognized problems of current statistic file (ALT + P)");
            this._problemButton.UseVisualStyleBackColor = true;
            this._problemButton.Click += new System.EventHandler(this.ProblemButton_Click);
            // 
            // _fileInfoTextBox
            // 
            this._fileInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._fileInfoTextBox.Location = new System.Drawing.Point(78, 40);
            this._fileInfoTextBox.Name = "_fileInfoTextBox";
            this._fileInfoTextBox.ReadOnly = true;
            this._fileInfoTextBox.Size = new System.Drawing.Size(552, 20);
            this._fileInfoTextBox.TabIndex = 5;
            this._fileInfoTextBox.Text = "Enter a file name and press the load button or browse a file";
            // 
            // _dataGridView
            // 
            this._dataGridView.AllowUserToAddRows = false;
            this._dataGridView.AllowUserToDeleteRows = false;
            this._dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.indexColumn,
            this.timestampColumn,
            this.valueColumn});
            this._dataGridView.Location = new System.Drawing.Point(0, 0);
            this._dataGridView.Name = "_dataGridView";
            this._dataGridView.RowHeadersWidth = 30;
            this._dataGridView.Size = new System.Drawing.Size(286, 483);
            this._dataGridView.TabIndex = 8;
            this._dataGridView.VirtualMode = true;
            this._dataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_CellContentClick);
            this._dataGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridView_MouseClick);
            this._dataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridView_KeyDown);
            // 
            // indexColumn
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle1.Format = "N0";
            dataGridViewCellStyle1.NullValue = null;
            this.indexColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.indexColumn.HeaderText = "Index";
            this.indexColumn.MinimumWidth = 8;
            this.indexColumn.Name = "indexColumn";
            this.indexColumn.ReadOnly = true;
            this.indexColumn.Width = 60;
            // 
            // timestampColumn
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.Format = "G";
            dataGridViewCellStyle2.NullValue = null;
            this.timestampColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.timestampColumn.HeaderText = "Timestamp";
            this.timestampColumn.MinimumWidth = 8;
            this.timestampColumn.Name = "timestampColumn";
            this.timestampColumn.Width = 130;
            // 
            // valueColumn
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N3";
            dataGridViewCellStyle3.NullValue = null;
            this.valueColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.valueColumn.HeaderText = "Value";
            this.valueColumn.MinimumWidth = 8;
            this.valueColumn.Name = "valueColumn";
            this.valueColumn.Width = 130;
            // 
            // _fileNameTextBox
            // 
            this._fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._fileNameTextBox.Location = new System.Drawing.Point(78, 14);
            this._fileNameTextBox.Name = "_fileNameTextBox";
            this._fileNameTextBox.Size = new System.Drawing.Size(552, 20);
            this._fileNameTextBox.TabIndex = 1;
            // 
            // _chart
            // 
            this._chart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._chart.BorderlineColor = System.Drawing.SystemColors.WindowFrame;
            this._chart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            this._chart.BorderSkin.BorderColor = System.Drawing.Color.IndianRed;
            this._chart.BorderSkin.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartArea1.AxisX.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisX.MinorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisX.MinorTickMark.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY.MinorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY.MinorTickMark.LineColor = System.Drawing.Color.LightGray;
            chartArea1.Name = "ChartArea1";
            this._chart.ChartAreas.Add(chartArea1);
            this._chart.Cursor = System.Windows.Forms.Cursors.Cross;
            this._chart.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            this._chart.Legends.Add(legend1);
            this._chart.Location = new System.Drawing.Point(0, 0);
            this._chart.Margin = new System.Windows.Forms.Padding(0);
            this._chart.Name = "_chart";
            this._chart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this._chart.PaletteCustomColors = new System.Drawing.Color[] {
        System.Drawing.Color.Lime,
        System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))))};
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.EmptyPointStyle.LabelForeColor = System.Drawing.Color.Bisque;
            series1.IsValueShownAsLabel = true;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this._chart.Series.Add(series1);
            this._chart.Size = new System.Drawing.Size(562, 483);
            this._chart.TabIndex = 9;
            this._chart.Text = "chart1";
            // 
            // _prevButton
            // 
            this._prevButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._prevButton.Location = new System.Drawing.Point(636, 11);
            this._prevButton.Name = "_prevButton";
            this._prevButton.Size = new System.Drawing.Size(75, 23);
            this._prevButton.TabIndex = 11;
            this._prevButton.Text = "&Previous";
            this.toolTip.SetToolTip(this._prevButton, "Load previous statistics file from list in main window (ALT + P)");
            this._prevButton.UseVisualStyleBackColor = true;
            this._prevButton.Click += new System.EventHandler(this.PreviousButton_Click);
            // 
            // _nextButton
            // 
            this._nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._nextButton.Location = new System.Drawing.Point(636, 38);
            this._nextButton.Name = "_nextButton";
            this._nextButton.Size = new System.Drawing.Size(75, 23);
            this._nextButton.TabIndex = 12;
            this._nextButton.Text = "&Next";
            this.toolTip.SetToolTip(this._nextButton, "Load next statistics file from list in main window (ALT + N)");
            this._nextButton.UseVisualStyleBackColor = true;
            this._nextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.splitContainer.Location = new System.Drawing.Point(12, 66);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this._dataGridView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this._chart);
            this.splitContainer.Size = new System.Drawing.Size(860, 483);
            this.splitContainer.SplitterDistance = 286;
            this.splitContainer.SplitterWidth = 6;
            this.splitContainer.TabIndex = 10;
            // 
            // LoxStatFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this._nextButton);
            this.Controls.Add(this._prevButton);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this._problemButton);
            this.Controls.Add(this._saveButton);
            this.Controls.Add(this._fileInfoTextBox);
            this.Controls.Add(fileInfoLabel);
            this.Controls.Add(fileNameLabel);
            this.Controls.Add(this._loadButton);
            this.Controls.Add(this._fileNameTextBox);
            this.Controls.Add(browseButton);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(575, 276);
            this.Name = "LoxStatFileForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Loxone Stats Editor - File Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FileForm_Closing);
            this.Load += new System.EventHandler(this.FileForm_Load);
            this.ResizeEnd += new System.EventHandler(this.FileForm_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._chart)).EndInit();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TextBox _fileInfoTextBox;
        private System.Windows.Forms.DataGridView _dataGridView;
        private System.Windows.Forms.Button _problemButton;
        private System.Windows.Forms.TextBox _fileNameTextBox;
        private System.Windows.Forms.DataVisualization.Charting.Chart _chart;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.DataGridViewTextBoxColumn indexColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn timestampColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueColumn;
        private System.Windows.Forms.Button _saveButton;
        private System.Windows.Forms.Button _loadButton;
        private System.Windows.Forms.Button _prevButton;
        private System.Windows.Forms.Button _nextButton;
    }
}

