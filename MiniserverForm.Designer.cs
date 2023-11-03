﻿using System.Drawing;
using System.Windows.Forms;

namespace LoxStatEdit
{
    partial class MiniserverForm
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
            if(disposing && (components != null))
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
            System.Windows.Forms.Label urlLabel;
            System.Windows.Forms.Button refreshFolderButton;
            System.Windows.Forms.Button refreshMsButton;
            System.Windows.Forms.Label folderLabel;
            System.Windows.Forms.Button downloadButton;
            System.Windows.Forms.Label aboutLabel;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MiniserverForm));
            this._urlTextBox = new System.Windows.Forms.TextBox();
            this._folderTextBox = new System.Windows.Forms.TextBox();
            this._dataGridView = new System.Windows.Forms.DataGridView();
            this._nameCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._yearMonthCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._statusCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._downloadCol = new System.Windows.Forms.DataGridViewButtonColumn();
            this._editCol = new System.Windows.Forms.DataGridViewButtonColumn();
            this._uploadCol = new System.Windows.Forms.DataGridViewButtonColumn();
            this.githubLabel = new System.Windows.Forms.LinkLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            urlLabel = new System.Windows.Forms.Label();
            refreshFolderButton = new System.Windows.Forms.Button();
            refreshMsButton = new System.Windows.Forms.Button();
            folderLabel = new System.Windows.Forms.Label();
            downloadButton = new System.Windows.Forms.Button();
            aboutLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // urlLabel
            // 
            urlLabel.Location = new System.Drawing.Point(12, 17);
            urlLabel.Name = "urlLabel";
            urlLabel.Size = new System.Drawing.Size(82, 18);
            urlLabel.TabIndex = 0;
            urlLabel.Text = "Miniserver:";
            // 
            // refreshFolderButton
            // 
            refreshFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            refreshFolderButton.Location = new System.Drawing.Point(632, 38);
            refreshFolderButton.Name = "refreshFolderButton";
            refreshFolderButton.Size = new System.Drawing.Size(75, 23);
            refreshFolderButton.TabIndex = 5;
            refreshFolderButton.Text = "Refresh &FS";
            this.toolTip.SetToolTip(refreshFolderButton, "ALT + F");
            refreshFolderButton.UseVisualStyleBackColor = true;
            refreshFolderButton.Click += new System.EventHandler(this.RefreshFolderButton_Click);
            // 
            // refreshMsButton
            // 
            refreshMsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            refreshMsButton.Location = new System.Drawing.Point(632, 12);
            refreshMsButton.Name = "refreshMsButton";
            refreshMsButton.Size = new System.Drawing.Size(75, 23);
            refreshMsButton.TabIndex = 2;
            refreshMsButton.Text = "Refresh &MS";
            this.toolTip.SetToolTip(refreshMsButton, "ALT + M");
            refreshMsButton.UseVisualStyleBackColor = true;
            refreshMsButton.Click += new System.EventHandler(this.RefreshMsButton_Click);
            // 
            // folderLabel
            // 
            folderLabel.Location = new System.Drawing.Point(12, 43);
            folderLabel.Name = "folderLabel";
            folderLabel.Size = new System.Drawing.Size(82, 18);
            folderLabel.TabIndex = 3;
            folderLabel.Text = "Working Folder:";
            // 
            // downloadButton
            // 
            downloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            downloadButton.Location = new System.Drawing.Point(596, 527);
            downloadButton.Name = "downloadButton";
            downloadButton.Size = new System.Drawing.Size(111, 23);
            downloadButton.TabIndex = 7;
            downloadButton.Text = "&Download selected";
            this.toolTip.SetToolTip(downloadButton, "ALT + D");
            downloadButton.UseVisualStyleBackColor = true;
            downloadButton.Click += new System.EventHandler(this.DownloadButton_Click);
            // 
            // aboutLabel
            // 
            aboutLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            aboutLabel.Location = new System.Drawing.Point(12, 532);
            aboutLabel.Name = "aboutLabel";
            aboutLabel.Size = new System.Drawing.Size(171, 18);
            aboutLabel.TabIndex = 8;
            aboutLabel.Text = "LoxStatEdit v1.0.3.0 (2023.11.02)";
            // 
            // _urlTextBox
            // 
            this._urlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._urlTextBox.Location = new System.Drawing.Point(100, 14);
            this._urlTextBox.Name = "_urlTextBox";
            this._urlTextBox.Size = new System.Drawing.Size(526, 20);
            this._urlTextBox.TabIndex = 1;
            this._urlTextBox.Text = "ftp://adminname:adminpassword@miniserver-ip-or-hostname:21";
            this._urlTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this._urlTextBox_KeyDown);
            // 
            // _folderTextBox
            // 
            this._folderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._folderTextBox.Location = new System.Drawing.Point(100, 40);
            this._folderTextBox.Name = "_folderTextBox";
            this._folderTextBox.Size = new System.Drawing.Size(526, 20);
            this._folderTextBox.TabIndex = 4;
            this._folderTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this._folderTextBox_KeyDown);
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
            this._nameCol,
            this._yearMonthCol,
            this._statusCol,
            this._downloadCol,
            this._editCol,
            this._uploadCol});
            this._dataGridView.Location = new System.Drawing.Point(12, 67);
            this._dataGridView.Name = "_dataGridView";
            this._dataGridView.RowHeadersWidth = 30;
            this._dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._dataGridView.Size = new System.Drawing.Size(695, 453);
            this._dataGridView.TabIndex = 6;
            this._dataGridView.VirtualMode = true;
            this._dataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_CellContentClick);
            this._dataGridView.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.DataGridView_CellValueNeeded);
            // 
            // _nameCol
            // 
            this._nameCol.DataPropertyName = "Name";
            this._nameCol.HeaderText = "Name";
            this._nameCol.MinimumWidth = 8;
            this._nameCol.Name = "_nameCol";
            this._nameCol.ReadOnly = true;
            this._nameCol.Width = 250;
            // 
            // _yearMonthCol
            // 
            this._yearMonthCol.DataPropertyName = "YearMonth";
            dataGridViewCellStyle1.Format = "yyyy-MM";
            this._yearMonthCol.DefaultCellStyle = dataGridViewCellStyle1;
            this._yearMonthCol.HeaderText = "Year/Month";
            this._yearMonthCol.MinimumWidth = 8;
            this._yearMonthCol.Name = "_yearMonthCol";
            this._yearMonthCol.ReadOnly = true;
            this._yearMonthCol.Width = 71;
            // 
            // _statusCol
            // 
            this._statusCol.DataPropertyName = "Status";
            this._statusCol.HeaderText = "Status";
            this._statusCol.MinimumWidth = 8;
            this._statusCol.Name = "_statusCol";
            this._statusCol.ReadOnly = true;
            this._statusCol.Width = 150;
            // 
            // _downloadCol
            // 
            this._downloadCol.HeaderText = "Download";
            this._downloadCol.MinimumWidth = 8;
            this._downloadCol.Name = "_downloadCol";
            this._downloadCol.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this._downloadCol.Text = "Download";
            this._downloadCol.Width = 60;
            // 
            // _editCol
            // 
            this._editCol.HeaderText = "Edit";
            this._editCol.MinimumWidth = 8;
            this._editCol.Name = "_editCol";
            this._editCol.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this._editCol.Text = "Edit";
            this._editCol.Width = 50;
            // 
            // _uploadCol
            // 
            this._uploadCol.HeaderText = "Upload";
            this._uploadCol.MinimumWidth = 8;
            this._uploadCol.Name = "_uploadCol";
            this._uploadCol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._uploadCol.Text = "Upload";
            this._uploadCol.Width = 60;
            // 
            // githubLabel
            // 
            this.githubLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.githubLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 10);
            this.githubLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.githubLabel.LinkColor = System.Drawing.Color.Blue;
            this.githubLabel.Location = new System.Drawing.Point(184, 533);
            this.githubLabel.Name = "githubLabel";
            this.githubLabel.Size = new System.Drawing.Size(42, 18);
            this.githubLabel.TabIndex = 9;
            this.githubLabel.TabStop = true;
            this.githubLabel.Text = "GitHub";
            this.githubLabel.UseCompatibleTextRendering = true;
            this.githubLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.githubLinkLabel_LinkClicked);
            // 
            // MiniserverForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 561);
            this.Controls.Add(aboutLabel);
            this.Controls.Add(this.githubLabel);
            this.Controls.Add(downloadButton);
            this.Controls.Add(this._dataGridView);
            this.Controls.Add(this._folderTextBox);
            this.Controls.Add(folderLabel);
            this.Controls.Add(refreshMsButton);
            this.Controls.Add(urlLabel);
            this.Controls.Add(this._urlTextBox);
            this.Controls.Add(refreshFolderButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MiniserverForm";
            this.Text = "Loxone Stats Editor - Miniserver Browser";
            this.Load += new System.EventHandler(this.MiniserverForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _urlTextBox;
        private System.Windows.Forms.TextBox _folderTextBox;
        private System.Windows.Forms.DataGridView _dataGridView;
        private LinkLabel githubLabel;
        private ToolTip toolTip;
        private DataGridViewTextBoxColumn _nameCol;
        private DataGridViewTextBoxColumn _yearMonthCol;
        private DataGridViewTextBoxColumn _statusCol;
        private DataGridViewButtonColumn _downloadCol;
        private DataGridViewButtonColumn _editCol;
        private DataGridViewButtonColumn _uploadCol;
    }
}