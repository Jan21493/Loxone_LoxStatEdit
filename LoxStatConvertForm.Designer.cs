namespace LoxStatEdit
{
    partial class LoxStatConvertForm
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
            System.Windows.Forms.Button CancelButton;
            System.Windows.Forms.Label fileNameLabel;
            System.Windows.Forms.Button OkButton;
            System.Windows.Forms.Label changeIntervalLabel;
            System.Windows.Forms.Label changeIntervalPowerlabel;
            System.Windows.Forms.Label selectUUIDlabel;
            System.Windows.Forms.Label writeValuelabel;
            System.Windows.Forms.Label changeIntervalTolabel;
            System.Windows.Forms.Label newDescriptionLabel;
            System.Windows.Forms.Label infoLabel;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoxStatConvertForm));
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.fileNameTextBox = new System.Windows.Forms.TextBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.convertOldRadioButton = new System.Windows.Forms.RadioButton();
            this.modifyIntervalRadioButton = new System.Windows.Forms.RadioButton();
            this.modifyInfoNameRadioButton = new System.Windows.Forms.RadioButton();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.meterIntervalComboBox = new System.Windows.Forms.ComboBox();
            this.powerIntervalComboBox = new System.Windows.Forms.ComboBox();
            this.convertOldGroupBox = new System.Windows.Forms.GroupBox();
            this.value2RadioButton = new System.Windows.Forms.RadioButton();
            this.value1RadioButton = new System.Windows.Forms.RadioButton();
            this.overwriteCheckBox = new System.Windows.Forms.CheckBox();
            this.uuidComboBox = new System.Windows.Forms.ComboBox();
            this.modifyIntervalGroupBox = new System.Windows.Forms.GroupBox();
            this.newIntervalComboBox = new System.Windows.Forms.ComboBox();
            this.disclaimerTextBox = new System.Windows.Forms.TextBox();
            this.newDescriptionTextBox = new System.Windows.Forms.TextBox();
            this.modifyNameGroupBox = new System.Windows.Forms.GroupBox();
            this.fileInfoTextBox = new System.Windows.Forms.TextBox();
            fileInfoLabel = new System.Windows.Forms.Label();
            CancelButton = new System.Windows.Forms.Button();
            fileNameLabel = new System.Windows.Forms.Label();
            OkButton = new System.Windows.Forms.Button();
            changeIntervalLabel = new System.Windows.Forms.Label();
            changeIntervalPowerlabel = new System.Windows.Forms.Label();
            selectUUIDlabel = new System.Windows.Forms.Label();
            writeValuelabel = new System.Windows.Forms.Label();
            changeIntervalTolabel = new System.Windows.Forms.Label();
            newDescriptionLabel = new System.Windows.Forms.Label();
            infoLabel = new System.Windows.Forms.Label();
            this.convertOldGroupBox.SuspendLayout();
            this.modifyIntervalGroupBox.SuspendLayout();
            this.modifyNameGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileInfoLabel
            // 
            fileInfoLabel.Location = new System.Drawing.Point(9, 45);
            fileInfoLabel.Name = "fileInfoLabel";
            fileInfoLabel.Size = new System.Drawing.Size(68, 20);
            fileInfoLabel.TabIndex = 2;
            fileInfoLabel.Text = "Description:";
            fileInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CancelButton
            // 
            CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            CancelButton.Location = new System.Drawing.Point(566, 537);
            CancelButton.Name = "CancelButton";
            CancelButton.Size = new System.Drawing.Size(75, 23);
            CancelButton.TabIndex = 26;
            CancelButton.Text = "C&ancel";
            this.toolTip.SetToolTip(CancelButton, "Cancel any conversion (ALT + C)");
            CancelButton.UseVisualStyleBackColor = true;
            CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // fileNameLabel
            // 
            fileNameLabel.Location = new System.Drawing.Point(9, 16);
            fileNameLabel.Name = "fileNameLabel";
            fileNameLabel.Size = new System.Drawing.Size(60, 20);
            fileNameLabel.TabIndex = 0;
            fileNameLabel.Text = "File Name:";
            fileNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // OkButton
            // 
            OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            OkButton.Location = new System.Drawing.Point(475, 537);
            OkButton.Name = "OkButton";
            OkButton.Size = new System.Drawing.Size(75, 23);
            OkButton.TabIndex = 25;
            OkButton.Text = "&OK";
            this.toolTip.SetToolTip(OkButton, "Convert statistics file(s) with selected settings (ALT + O)");
            OkButton.UseVisualStyleBackColor = true;
            OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // changeIntervalLabel
            // 
            changeIntervalLabel.Location = new System.Drawing.Point(11, 73);
            changeIntervalLabel.Name = "changeIntervalLabel";
            changeIntervalLabel.Size = new System.Drawing.Size(227, 21);
            changeIntervalLabel.TabIndex = 15;
            changeIntervalLabel.Text = "Change recording interval for meter reading to:";
            changeIntervalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // changeIntervalPowerlabel
            // 
            changeIntervalPowerlabel.Location = new System.Drawing.Point(11, 19);
            changeIntervalPowerlabel.Name = "changeIntervalPowerlabel";
            changeIntervalPowerlabel.Size = new System.Drawing.Size(227, 21);
            changeIntervalPowerlabel.TabIndex = 10;
            changeIntervalPowerlabel.Text = "Change recording interval for power/flow to:";
            changeIntervalPowerlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // selectUUIDlabel
            // 
            selectUUIDlabel.Location = new System.Drawing.Point(11, 100);
            selectUUIDlabel.Name = "selectUUIDlabel";
            selectUUIDlabel.Size = new System.Drawing.Size(483, 21);
            selectUUIDlabel.TabIndex = 17;
            selectUUIDlabel.Text = "Select the new meter function block by UUID / file info (must exist  on local FS)" +
    ":";
            selectUUIDlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // writeValuelabel
            // 
            writeValuelabel.Location = new System.Drawing.Point(11, 46);
            writeValuelabel.Name = "writeValuelabel";
            writeValuelabel.Size = new System.Drawing.Size(220, 21);
            writeValuelabel.TabIndex = 12;
            writeValuelabel.Text = "Write values for meter reading to:";
            writeValuelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // changeIntervalTolabel
            // 
            changeIntervalTolabel.Location = new System.Drawing.Point(11, 19);
            changeIntervalTolabel.Name = "changeIntervalTolabel";
            changeIntervalTolabel.Size = new System.Drawing.Size(220, 21);
            changeIntervalTolabel.TabIndex = 22;
            changeIntervalTolabel.Text = "Change recording interval to:";
            changeIntervalTolabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // newDescriptionLabel
            // 
            newDescriptionLabel.Location = new System.Drawing.Point(11, 17);
            newDescriptionLabel.Name = "newDescriptionLabel";
            newDescriptionLabel.Size = new System.Drawing.Size(88, 20);
            newDescriptionLabel.TabIndex = 6;
            newDescriptionLabel.Text = "New description:";
            newDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // infoLabel
            // 
            infoLabel.Location = new System.Drawing.Point(363, 45);
            infoLabel.Name = "infoLabel";
            infoLabel.Size = new System.Drawing.Size(32, 20);
            infoLabel.TabIndex = 8;
            infoLabel.Text = "Info";
            infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.descriptionTextBox.Location = new System.Drawing.Point(75, 45);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.ReadOnly = true;
            this.descriptionTextBox.Size = new System.Drawing.Size(280, 20);
            this.descriptionTextBox.TabIndex = 3;
            this.descriptionTextBox.Text = "Description (name of function block)";
            // 
            // fileNameTextBox
            // 
            this.fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileNameTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.fileNameTextBox.Location = new System.Drawing.Point(75, 16);
            this.fileNameTextBox.Name = "fileNameTextBox";
            this.fileNameTextBox.ReadOnly = true;
            this.fileNameTextBox.Size = new System.Drawing.Size(602, 20);
            this.fileNameTextBox.TabIndex = 1;
            this.fileNameTextBox.Text = "File name";
            // 
            // toolTip
            // 
            this.toolTip.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip_Popup);
            // 
            // convertOldRadioButton
            // 
            this.convertOldRadioButton.AutoSize = true;
            this.convertOldRadioButton.Location = new System.Drawing.Point(16, 144);
            this.convertOldRadioButton.Name = "convertOldRadioButton";
            this.convertOldRadioButton.Size = new System.Drawing.Size(333, 17);
            this.convertOldRadioButton.TabIndex = 8;
            this.convertOldRadioButton.Text = "Convert statistics data from old to new style (see tooltip for details)";
            this.toolTip.SetToolTip(this.convertOldRadioButton, resources.GetString("convertOldRadioButton.ToolTip"));
            this.convertOldRadioButton.UseVisualStyleBackColor = true;
            this.convertOldRadioButton.CheckedChanged += new System.EventHandler(this.ConvertOldRadioButton_CheckedChanged);
            // 
            // modifyIntervalRadioButton
            // 
            this.modifyIntervalRadioButton.AutoSize = true;
            this.modifyIntervalRadioButton.Location = new System.Drawing.Point(16, 345);
            this.modifyIntervalRadioButton.Name = "modifyIntervalRadioButton";
            this.modifyIntervalRadioButton.Size = new System.Drawing.Size(198, 17);
            this.modifyIntervalRadioButton.TabIndex = 20;
            this.modifyIntervalRadioButton.Text = "Modify interval (see tooltip for details)";
            this.toolTip.SetToolTip(this.modifyIntervalRadioButton, resources.GetString("modifyIntervalRadioButton.ToolTip"));
            this.modifyIntervalRadioButton.UseVisualStyleBackColor = true;
            this.modifyIntervalRadioButton.CheckedChanged += new System.EventHandler(this.ModifyIntervalRadioButton_CheckedChanged);
            // 
            // modifyInfoNameRadioButton
            // 
            this.modifyInfoNameRadioButton.AutoSize = true;
            this.modifyInfoNameRadioButton.Checked = true;
            this.modifyInfoNameRadioButton.Location = new System.Drawing.Point(16, 75);
            this.modifyInfoNameRadioButton.Name = "modifyInfoNameRadioButton";
            this.modifyInfoNameRadioButton.Size = new System.Drawing.Size(317, 17);
            this.modifyInfoNameRadioButton.TabIndex = 4;
            this.modifyInfoNameRadioButton.TabStop = true;
            this.modifyInfoNameRadioButton.Text = "Modify file info name to the latest name (see tooltop for details)";
            this.toolTip.SetToolTip(this.modifyInfoNameRadioButton, resources.GetString("modifyInfoNameRadioButton.ToolTip"));
            this.modifyInfoNameRadioButton.UseVisualStyleBackColor = true;
            this.modifyInfoNameRadioButton.CheckedChanged += new System.EventHandler(this.ModifyInfoNameRadioButton_CheckedChanged);
            // 
            // meterIntervalComboBox
            // 
            this.meterIntervalComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.meterIntervalComboBox.FormattingEnabled = true;
            this.meterIntervalComboBox.Items.AddRange(new object[] {
            "Keep existing interval and all data.",
            "Interval 5 minutes.",
            "Interval 10 minutes.",
            "Interval 15 minutes.",
            "Interval 30 minutes.",
            "Interval 60 minutes."});
            this.meterIntervalComboBox.Location = new System.Drawing.Point(237, 73);
            this.meterIntervalComboBox.Name = "meterIntervalComboBox";
            this.meterIntervalComboBox.Size = new System.Drawing.Size(419, 21);
            this.meterIntervalComboBox.TabIndex = 16;
            this.meterIntervalComboBox.SelectedIndexChanged += new System.EventHandler(this.meterIntervalComboBox_SelectedIndexChanged);
            // 
            // powerIntervalComboBox
            // 
            this.powerIntervalComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.powerIntervalComboBox.FormattingEnabled = true;
            this.powerIntervalComboBox.Items.AddRange(new object[] {
            "Keep existing intervall and all data.",
            "Interval 5 minutes.",
            "Interval 10 minutes.",
            "Interval 15 minutes.",
            "Interval 30 minutes.",
            "Interval 60 minutes.",
            "Do NOT write any data for power/flow to the new meter."});
            this.powerIntervalComboBox.Location = new System.Drawing.Point(237, 19);
            this.powerIntervalComboBox.Name = "powerIntervalComboBox";
            this.powerIntervalComboBox.Size = new System.Drawing.Size(419, 21);
            this.powerIntervalComboBox.TabIndex = 11;
            // 
            // convertOldGroupBox
            // 
            this.convertOldGroupBox.Controls.Add(this.powerIntervalComboBox);
            this.convertOldGroupBox.Controls.Add(this.value2RadioButton);
            this.convertOldGroupBox.Controls.Add(this.value1RadioButton);
            this.convertOldGroupBox.Controls.Add(writeValuelabel);
            this.convertOldGroupBox.Controls.Add(this.overwriteCheckBox);
            this.convertOldGroupBox.Controls.Add(this.uuidComboBox);
            this.convertOldGroupBox.Controls.Add(selectUUIDlabel);
            this.convertOldGroupBox.Controls.Add(changeIntervalPowerlabel);
            this.convertOldGroupBox.Controls.Add(this.meterIntervalComboBox);
            this.convertOldGroupBox.Controls.Add(changeIntervalLabel);
            this.convertOldGroupBox.Location = new System.Drawing.Point(10, 158);
            this.convertOldGroupBox.Name = "convertOldGroupBox";
            this.convertOldGroupBox.Size = new System.Drawing.Size(667, 181);
            this.convertOldGroupBox.TabIndex = 9;
            this.convertOldGroupBox.TabStop = false;
            // 
            // value2RadioButton
            // 
            this.value2RadioButton.AutoSize = true;
            this.value2RadioButton.Location = new System.Drawing.Point(433, 47);
            this.value2RadioButton.Name = "value2RadioButton";
            this.value2RadioButton.Size = new System.Drawing.Size(90, 17);
            this.value2RadioButton.TabIndex = 14;
            this.value2RadioButton.Text = "feed (value 2)";
            this.value2RadioButton.UseVisualStyleBackColor = true;
            // 
            // value1RadioButton
            // 
            this.value1RadioButton.AutoSize = true;
            this.value1RadioButton.Checked = true;
            this.value1RadioButton.Location = new System.Drawing.Point(250, 47);
            this.value1RadioButton.Name = "value1RadioButton";
            this.value1RadioButton.Size = new System.Drawing.Size(120, 17);
            this.value1RadioButton.TabIndex = 13;
            this.value1RadioButton.TabStop = true;
            this.value1RadioButton.Text = "consumption (value)";
            this.value1RadioButton.UseVisualStyleBackColor = true;
            this.value1RadioButton.CheckedChanged += new System.EventHandler(this.selectValueRadioButton_CheckedChanged);
            // 
            // overwriteCheckBox
            // 
            this.overwriteCheckBox.AutoSize = true;
            this.overwriteCheckBox.Location = new System.Drawing.Point(25, 154);
            this.overwriteCheckBox.Name = "overwriteCheckBox";
            this.overwriteCheckBox.Size = new System.Drawing.Size(283, 17);
            this.overwriteCheckBox.TabIndex = 19;
            this.overwriteCheckBox.Text = "Overwrite existing data. USE WITH EXTREME CARE!";
            this.overwriteCheckBox.UseVisualStyleBackColor = true;
            // 
            // uuidComboBox
            // 
            this.uuidComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.uuidComboBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uuidComboBox.FormattingEnabled = true;
            this.uuidComboBox.Items.AddRange(new object[] {
            "List of UUIDs"});
            this.uuidComboBox.Location = new System.Drawing.Point(14, 125);
            this.uuidComboBox.Name = "uuidComboBox";
            this.uuidComboBox.Size = new System.Drawing.Size(642, 19);
            this.uuidComboBox.TabIndex = 18;
            // 
            // modifyIntervalGroupBox
            // 
            this.modifyIntervalGroupBox.Controls.Add(this.newIntervalComboBox);
            this.modifyIntervalGroupBox.Controls.Add(changeIntervalTolabel);
            this.modifyIntervalGroupBox.Location = new System.Drawing.Point(10, 359);
            this.modifyIntervalGroupBox.Name = "modifyIntervalGroupBox";
            this.modifyIntervalGroupBox.Size = new System.Drawing.Size(667, 51);
            this.modifyIntervalGroupBox.TabIndex = 21;
            this.modifyIntervalGroupBox.TabStop = false;
            // 
            // newIntervalComboBox
            // 
            this.newIntervalComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.newIntervalComboBox.FormattingEnabled = true;
            this.newIntervalComboBox.Items.AddRange(new object[] {
            "Interval 5 minutes",
            "Interval 10 minutes",
            "Interval 15 minutes",
            "Interval 30 minutes",
            "Interval 60 minutes"});
            this.newIntervalComboBox.Location = new System.Drawing.Point(237, 19);
            this.newIntervalComboBox.Name = "newIntervalComboBox";
            this.newIntervalComboBox.Size = new System.Drawing.Size(419, 21);
            this.newIntervalComboBox.TabIndex = 23;
            // 
            // disclaimerTextBox
            // 
            this.disclaimerTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.disclaimerTextBox.Location = new System.Drawing.Point(10, 431);
            this.disclaimerTextBox.Multiline = true;
            this.disclaimerTextBox.Name = "disclaimerTextBox";
            this.disclaimerTextBox.ReadOnly = true;
            this.disclaimerTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.disclaimerTextBox.Size = new System.Drawing.Size(667, 91);
            this.disclaimerTextBox.TabIndex = 24;
            this.disclaimerTextBox.Text = resources.GetString("disclaimerTextBox.Text");
            // 
            // newDescriptionTextBox
            // 
            this.newDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.newDescriptionTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.newDescriptionTextBox.Location = new System.Drawing.Point(105, 17);
            this.newDescriptionTextBox.MaxLength = 200;
            this.newDescriptionTextBox.Name = "newDescriptionTextBox";
            this.newDescriptionTextBox.Size = new System.Drawing.Size(280, 20);
            this.newDescriptionTextBox.TabIndex = 7;
            this.newDescriptionTextBox.Text = "New description";
            // 
            // modifyNameGroupBox
            // 
            this.modifyNameGroupBox.Controls.Add(this.newDescriptionTextBox);
            this.modifyNameGroupBox.Controls.Add(newDescriptionLabel);
            this.modifyNameGroupBox.Location = new System.Drawing.Point(10, 89);
            this.modifyNameGroupBox.Name = "modifyNameGroupBox";
            this.modifyNameGroupBox.Size = new System.Drawing.Size(667, 48);
            this.modifyNameGroupBox.TabIndex = 5;
            this.modifyNameGroupBox.TabStop = false;
            // 
            // fileInfoTextBox
            // 
            this.fileInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileInfoTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.fileInfoTextBox.Location = new System.Drawing.Point(397, 45);
            this.fileInfoTextBox.Name = "fileInfoTextBox";
            this.fileInfoTextBox.ReadOnly = true;
            this.fileInfoTextBox.Size = new System.Drawing.Size(280, 20);
            this.fileInfoTextBox.TabIndex = 27;
            this.fileInfoTextBox.Text = "info to file";
            // 
            // LoxStatConvertForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = CancelButton;
            this.ClientSize = new System.Drawing.Size(687, 570);
            this.Controls.Add(this.fileInfoTextBox);
            this.Controls.Add(infoLabel);
            this.Controls.Add(this.modifyIntervalRadioButton);
            this.Controls.Add(this.convertOldRadioButton);
            this.Controls.Add(this.modifyInfoNameRadioButton);
            this.Controls.Add(this.modifyNameGroupBox);
            this.Controls.Add(this.disclaimerTextBox);
            this.Controls.Add(this.modifyIntervalGroupBox);
            this.Controls.Add(OkButton);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(fileInfoLabel);
            this.Controls.Add(fileNameLabel);
            this.Controls.Add(CancelButton);
            this.Controls.Add(this.fileNameTextBox);
            this.Controls.Add(this.convertOldGroupBox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(575, 276);
            this.Name = "LoxStatConvertForm";
            this.Text = "Loxone Stats Editor - Convert Statistics";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.Form_Resize);
            this.convertOldGroupBox.ResumeLayout(false);
            this.convertOldGroupBox.PerformLayout();
            this.modifyIntervalGroupBox.ResumeLayout(false);
            this.modifyNameGroupBox.ResumeLayout(false);
            this.modifyNameGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.TextBox fileNameTextBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.ComboBox meterIntervalComboBox;
        private System.Windows.Forms.RadioButton convertOldRadioButton;
        private System.Windows.Forms.ComboBox powerIntervalComboBox;
        private System.Windows.Forms.GroupBox convertOldGroupBox;
        private System.Windows.Forms.ComboBox uuidComboBox;
        private System.Windows.Forms.CheckBox overwriteCheckBox;
        private System.Windows.Forms.RadioButton value1RadioButton;
        private System.Windows.Forms.RadioButton value2RadioButton;
        private System.Windows.Forms.GroupBox modifyIntervalGroupBox;
        private System.Windows.Forms.ComboBox newIntervalComboBox;
        private System.Windows.Forms.RadioButton modifyIntervalRadioButton;
        private System.Windows.Forms.TextBox disclaimerTextBox;
        private System.Windows.Forms.RadioButton modifyInfoNameRadioButton;
        private System.Windows.Forms.TextBox newDescriptionTextBox;
        private System.Windows.Forms.GroupBox modifyNameGroupBox;
        private System.Windows.Forms.TextBox fileInfoTextBox;
    }
}

