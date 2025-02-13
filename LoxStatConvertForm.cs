using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.RegularExpressions;
using static LoxStatEdit.MiniserverForm;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using System.Collections;
using System.Security.Principal;
using static LoxStatEdit.LoxStatConvertForm;
using System.Xml.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;
using System.ComponentModel.Design.Serialization;
using System.Net;
using System.Collections.ObjectModel;
using static System.Net.WebRequestMethods;
using System.Diagnostics.Eventing.Reader;

namespace LoxStatEdit {
    public partial class LoxStatConvertForm : Form {
        //const int _valueColumnOffset = 2;
        //bool _inProgress;
        MiniserverForm.FileItem _fileItem;
        MiniserverForm.FileItem[] _selectedFileItems;
        IList<MiniserverForm.FileItem> _fileItems;

        IList<LoxFunctionBlock> loxFunctionBlocks;
        LoxFunctionBlock loxFunctionBlock;

        LoxStatFile _loxStatFile;

        private int convertAction = 0;

        public class LoxFunctionBlock {

            /// <summary>The UUID has a format like 1cc8df9f-0229-8b1d-ffffefc088fafadd and is used as an identifier for each Loxone function block.
            /// </summary>
            public string UUID {
                get; set;
            }

            /// <summary>The description is the name that is defined in Loxone config for the function block. It can be changed over time. This property retrieves the latest one.   
            /// </summary>
            public string LatestDescription {
                get; set;
            }

            /// <summary>This property retrieves and sets the latest year and month where a statistics file with this UUID is available.
            /// </summary>
            public DateTime LatestYearMonth {
                get; set;
            }

            /// <summary>This property retrieves and sets the associated file item array of the statistics file. For each UUID up to 10 files are technically available: without _, _1, _2, ... , _9
            /// </summary>
            public MiniserverForm.FileItem[] FileItem {
                get; set;
            }


            /// <summary>Statistics files with new style use an underscore with number as an identifier and are stored in FileItem[1-9]
            /// </summary>
            public bool IsNewFormat {
                get {
                    if (FileItem != null)
                        for (var i = 1; i < 10; i++)
                        if (FileItem[i] != null)
                            return true;
                    return false;   
                }
            }
        }

        public DateTime LatestYearMonth {
            get;
            private set;
        }

        public LoxStatConvertForm(IList<MiniserverForm.FileItem> fileItems, params MiniserverForm.FileItem[] selectedFileItems) {
            // ToDo: handle more than one file - selection
            _selectedFileItems = selectedFileItems;
            _fileItems = fileItems;
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            // 
        }

        private void DataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e) {
            var columnIndex = e.ColumnIndex;
            var dataPoint = _loxStatFile.DataPoints[e.RowIndex];
        }

        private void MainForm_Load(object sender, EventArgs e) {

            bool multipleFilesSelected = false;

            if (_selectedFileItems.Length > 1) {
                // multiple files selected - they have the same UUID (verified in MiniserverForm.cs)
                multipleFilesSelected = true;
                _fileItem = _selectedFileItems[0];
                fileNameTextBox.Text = Path.GetDirectoryName(_fileItem.FileInfo.FullName) + "\\" + _fileItem.UUID + ".<multiple>";
                descriptionTextBox.Text = _fileItem.Description;
                fileInfoTextBox.Text = "Multiple files selected!";
            } else {
                // a single file was selected
                _fileItem = _selectedFileItems[0];
                fileNameTextBox.Text = _fileItem.FileInfo.FullName;
                _loxStatFile = LoxStatFile.Load(_fileItem.FileInfo.FullName);
                descriptionTextBox.Text = _fileItem.Description;
                fileInfoTextBox.Text = string.Format("{0}, {1} data point{2}, each with {3} value{4}",
                                _loxStatFile.YearMonth.ToString("yyyy-MM"),
                                _loxStatFile.DataPoints.Count,
                                _loxStatFile.DataPoints.Count == 1 ? "" : "s",
                                _loxStatFile.ValueCount,
                                _loxStatFile.ValueCount == 1 ? "" : "s");
            }

            // create a list of Loxone function blocks with UUID, description from latest file
            loxFunctionBlocks = new List<LoxFunctionBlock>();
            LoxFunctionBlock defaultLoxFunctionBlock = new LoxFunctionBlock {
                UUID = "",
                LatestDescription = "",
                FileItem = new MiniserverForm.FileItem[10],
                LatestYearMonth = new DateTime()
            };

            // build a list of Loxone function blocks from all files in the list - only look on filename to be fast
            foreach (FileItem fileItem in _fileItems) {
                // only add valid UUIDs to list
                if (fileItem.IsValidMsStatsFile) {
                    // get function block from list (or null)
                    loxFunctionBlock = loxFunctionBlocks.FirstOrDefault(item => item.UUID == fileItem.UUID);
                    // UUID was not in list, so add it
                    if (loxFunctionBlock == null) {
                        loxFunctionBlock = new LoxFunctionBlock {
                            UUID = fileItem.UUID,
                            LatestDescription = "Not available",
                            FileItem = new MiniserverForm.FileItem[10],
                            LatestYearMonth = fileItem.YearMonth
                        };
                        loxFunctionBlock.FileItem[fileItem.msStatsType] = fileItem;
                        loxFunctionBlocks.Add(loxFunctionBlock);
                    } else {  // UUID was found, but statistics file might be from a newer month (yyyyMM)
                        if (fileItem.YearMonth > loxFunctionBlock.LatestYearMonth) {
                            loxFunctionBlock.LatestYearMonth = fileItem.YearMonth;
                            loxFunctionBlock.FileItem = new MiniserverForm.FileItem[10];
                            loxFunctionBlock.FileItem[fileItem.msStatsType] = fileItem;
                        } else if (fileItem.YearMonth == loxFunctionBlock.LatestYearMonth) {
                            // or from the same (latest) month
                            loxFunctionBlock.FileItem[fileItem.msStatsType] = fileItem;
                        }

                    }
                }
            } // end foreach

            uuidComboBox.Items.Clear();
           // disable "convert old format to new" if the file already has the new format 
            if (_fileItem.msStatsType > 0) {
                convertOldRadioButton.Enabled = false;
            } else {
                // create a drop-down list with UUIDs for function blocks with new meter style, but currently only for uni- and bidirectional meters
                // those meters have _1 and _2 file data, but no _3 (used for storage meters)
                foreach (LoxFunctionBlock loxFunctionBlock in loxFunctionBlocks) {
                    if ((loxFunctionBlock.FileItem[1] != null) &&
                        (loxFunctionBlock.FileItem[2] != null) &&
                        (loxFunctionBlock.FileItem[3] == null))
                        uuidComboBox.Items.Add(loxFunctionBlock.UUID + " - " + loxFunctionBlock.FileItem[2].Description);
                }
            }

            // get file info text from latest file (highest yyyyMM) for this UUID
            string newDescription = loxFunctionBlocks.FirstOrDefault(item => item.UUID == _fileItem.UUID)?.FileItem[_fileItem.msStatsType].Description;
            if (newDescription.Length > 200)
                newDescription = newDescription.Substring(0, 200);
            newDescriptionTextBox.Text = newDescription;

            timeZoneComboBox.DisplayMember = "Text";
            timeZoneComboBox.ValueMember = "Value";

            var timezones = new[] {
                new { Value="NoConversion", Text="Do not convert any time stamps" },
                new { Value="Morocco Standard Time", Text="(GMT) Casablanca" },
                new { Value="GMT Standard Time", Text="(GMT) Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London" },
                new { Value="Greenwich Standard Time", Text="(GMT) Monrovia, Reykjavik" },
                new { Value="W. Europe Standard Time", Text="(GMT+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna" },
                new { Value="Central Europe Standard Time", Text="(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague" },
                new { Value="Romance Standard Time", Text="(GMT+01:00) Brussels, Copenhagen, Madrid, Paris" },
                new { Value="Central European Standard Time", Text="(GMT+01:00) Sarajevo, Skopje, Warsaw, Zagreb" },
                new { Value="W. Central Africa Standard Time", Text="(GMT+01:00) West Central Africa" },
                new { Value="Jordan Standard Time", Text="(GMT+02:00) Amman" },
                new { Value="GTB Standard Time", Text="(GMT+02:00) Athens, Bucharest, Istanbul" },
                new { Value="Middle East Standard Time", Text="(GMT+02:00) Beirut" },
                new { Value="Egypt Standard Time", Text="(GMT+02:00) Cairo" },
                new { Value="South Africa Standard Time", Text="(GMT+02:00) Harare, Pretoria" },
                new { Value="FLE Standard Time", Text="(GMT+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius" },
                new { Value="Israel Standard Time", Text="(GMT+02:00) Jerusalem" },
                new { Value="E. Europe Standard Time", Text="(GMT+02:00) Minsk" },
                new { Value="Namibia Standard Time", Text="(GMT+02:00) Windhoek" },
                new { Value="Arabic Standard Time", Text="(GMT+03:00) Baghdad" },
                new { Value="Arab Standard Time", Text="(GMT+03:00) Kuwait, Riyadh" },
                new { Value="Russian Standard Time", Text="(GMT+03:00) Moscow, St. Petersburg, Volgograd" },
                new { Value="E. Africa Standard Time", Text="(GMT+03:00) Nairobi" },
                new { Value="Georgian Standard Time", Text="(GMT+03:00) Tbilisi" },
                new { Value="Iran Standard Time", Text="(GMT+03:30) Tehran" },
                new { Value="Arabian Standard Time", Text="(GMT+04:00) Abu Dhabi, Muscat" },
                new { Value="Azerbaijan Standard Time", Text="(GMT+04:00) Baku" },
                new { Value="Mauritius Standard Time", Text="(GMT+04:00) Port Louis" },
                new { Value="Caucasus Standard Time", Text="(GMT+04:00) Yerevan" },
                new { Value="Afghanistan Standard Time", Text="(GMT+04:30) Kabul" },
                new { Value="Ekaterinburg Standard Time", Text="(GMT+05:00) Ekaterinburg" },
                new { Value="Pakistan Standard Time", Text="(GMT+05:00) Islamabad, Karachi" },
                new { Value="West Asia Standard Time", Text="(GMT+05:00) Tashkent" },
                new { Value="India Standard Time", Text="(GMT+05:30) Chennai, Kolkata, Mumbai, New Delhi" },
                new { Value="Sri Lanka Standard Time", Text="(GMT+05:30) Sri Jayawardenepura" },
                new { Value="Nepal Standard Time", Text="(GMT+05:45) Kathmandu" },
                new { Value="N. Central Asia Standard Time", Text="(GMT+06:00) Almaty, Novosibirsk" },
                new { Value="Central Asia Standard Time", Text="(GMT+06:00) Astana, Dhaka" },
                new { Value="Myanmar Standard Time", Text="(GMT+06:30) Yangon (Rangoon)" },
                new { Value="SE Asia Standard Time", Text="(GMT+07:00) Bangkok, Hanoi, Jakarta" },
                new { Value="North Asia Standard Time", Text="(GMT+07:00) Krasnoyarsk" },
                new { Value="China Standard Time", Text="(GMT+08:00) Beijing, Chongqing, Hong Kong, Urumqi" },
                new { Value="North Asia East Standard Time", Text="(GMT+08:00) Irkutsk, Ulaan Bataar" },
                new { Value="Singapore Standard Time", Text="(GMT+08:00) Kuala Lumpur, Singapore" },
                new { Value="W. Australia Standard Time", Text="(GMT+08:00) Perth" },
                new { Value="Taipei Standard Time", Text="(GMT+08:00) Taipei" },
                new { Value="Tokyo Standard Time", Text="(GMT+09:00) Osaka, Sapporo, Tokyo" },
                new { Value="Korea Standard Time", Text="(GMT+09:00) Seoul" },
                new { Value="Yakutsk Standard Time", Text="(GMT+09:00) Yakutsk" },
                new { Value="Cen. Australia Standard Time", Text="(GMT+09:30) Adelaide" },
                new { Value="AUS Central Standard Time", Text="(GMT+09:30) Darwin" },
                new { Value="E. Australia Standard Time", Text="(GMT+10:00) Brisbane" },
                new { Value="AUS Eastern Standard Time", Text="(GMT+10:00) Canberra, Melbourne, Sydney" },
                new { Value="West Pacific Standard Time", Text="(GMT+10:00) Guam, Port Moresby" },
                new { Value="Tasmania Standard Time", Text="(GMT+10:00) Hobart" },
                new { Value="Vladivostok Standard Time", Text="(GMT+10:00) Vladivostok" },
                new { Value="Central Pacific Standard Time", Text="(GMT+11:00) Magadan, Solomon Is., New Caledonia" },
                new { Value="New Zealand Standard Time", Text="(GMT+12:00) Auckland, Wellington" },
                new { Value="Fiji Standard Time", Text="(GMT+12:00) Fiji, Kamchatka, Marshall Is." },
                new { Value="Tonga Standard Time", Text="(GMT+13:00) Nuku'alofa" },
                new { Value="Azores Standard Time", Text="(GMT-01:00) Azores" },
                new { Value="Cape Verde Standard Time", Text="(GMT-01:00) Cape Verde Is." },
                new { Value="Mid-Atlantic Standard Time", Text="(GMT-02:00) Mid-Atlantic" },
                new { Value="E. South America Standard Time", Text="(GMT-03:00) Brasilia" },
                new { Value="Argentina Standard Time", Text="(GMT-03:00) Buenos Aires" },
                new { Value="SA Eastern Standard Time", Text="(GMT-03:00) Georgetown" },
                new { Value="Greenland Standard Time", Text="(GMT-03:00) Greenland" },
                new { Value="Montevideo Standard Time", Text="(GMT-03:00) Montevideo" },
                new { Value="Newfoundland Standard Time", Text="(GMT-03:30) Newfoundland" },
                new { Value="Atlantic Standard Time", Text="(GMT-04:00) Atlantic Time (Canada)" },
                new { Value="SA Western Standard Time", Text="(GMT-04:00) La Paz" },
                new { Value="Central Brazilian Standard Time", Text="(GMT-04:00) Manaus" },
                new { Value="Pacific SA Standard Time", Text="(GMT-04:00) Santiago" },
                new { Value="Venezuela Standard Time", Text="(GMT-04:30) Caracas" },
                new { Value="SA Pacific Standard Time", Text="(GMT-05:00) Bogota, Lima, Quito, Rio Branco" },
                new { Value="Eastern Standard Time", Text="(GMT-05:00) Eastern Time (US & Canada)" },
                new { Value="US Eastern Standard Time", Text="(GMT-05:00) Indiana (East)" },
                new { Value="Central America Standard Time", Text="(GMT-06:00) Central America" },
                new { Value="Central Standard Time", Text="(GMT-06:00) Central Time (US & Canada)" },
                new { Value="Central Standard Time (Mexico)", Text="(GMT-06:00) Guadalajara, Mexico City, Monterrey" },
                new { Value="Canada Central Standard Time", Text="(GMT-06:00) Saskatchewan" },
                new { Value="US Mountain Standard Time", Text="(GMT-07:00) Arizona" },
                new { Value="Mountain Standard Time (Mexico)", Text="(GMT-07:00) Chihuahua, La Paz, Mazatlan" },
                new { Value="Mountain Standard Time", Text="(GMT-07:00) Mountain Time (US & Canada)" },
                new { Value="Pacific Standard Time", Text="(GMT-08:00) Pacific Time (US & Canada)" },
                new { Value="Pacific Standard Time (Mexico)", Text="(GMT-08:00) Tijuana, Baja California" },
                new { Value="Alaskan Standard Time", Text="(GMT-09:00) Alaska" },
                new { Value="Hawaiian Standard Time", Text="(GMT-10:00) Hawaii" },
                new { Value="Samoa Standard Time", Text="(GMT-11:00) Midway Island, Samoa" },
                new { Value="Dateline Standard Time", Text="(GMT-12:00) International Date Line West" }
                };

            timeZoneComboBox.DataSource = timezones;
            // get local time zone ID
            timeZoneComboBox.SelectedValue = TimeZoneInfo.Local.Id;

            // check DST if local time zone supports it
            // NOTE: local time zone depends on Windows setting "automatically adjust time zone" and "automatically adjust to DST"
            useDSTcheckBox.Checked = TimeZoneInfo.Local.SupportsDaylightSavingTime;
            // enable DST if local time zone supports it in general
            useDSTcheckBox.Enabled = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).SupportsDaylightSavingTime;

                // initialize radio buttons
            modifyInfoNameRadioButton.Checked = true;
            convertOldRadioButton.Checked = false;
            modifyIntervalRadioButton.Checked = false;

            // initialize form items
            modifyNameGroupBox.Enabled = true;
            newDescriptionTextBox.Enabled = true;
            convertOldGroupBox.Enabled = false;
            powerIntervalComboBox.Enabled = false;
            meterIntervalComboBox.Enabled = false;
            value1RadioButton.Enabled = false;
            value2RadioButton.Enabled = false;
            uuidComboBox.Enabled = false;
            overwriteCheckBox.Enabled = false;
            timeZoneComboBox.Enabled = false;
            useDSTcheckBox.Enabled = false;
            modifyIntervalGroupBox.Enabled = false;
            newIntervalComboBox.Enabled = false;
        }

        private void Form_Resize(object sender, EventArgs e) {

        }

        private int ModifyInfoNameForFile(MiniserverForm.FileItem fileItem) {

            if (fileItem.FileInfo == null) {
                return 2;
            }

            _loxStatFile = LoxStatFile.Load(fileItem.FileInfo.FullName);
            _loxStatFile.Text = newDescriptionTextBox.Text;
            _loxStatFile.Save();

            return 0;
        }

        private int WriteNewFile(int fileNo, MiniserverForm.FileItem fileItem, bool allowOverwrite, LoxFunctionBlock loxFunctionBlock, int selectedIndex, int newValueIndex) {

            // Note: tested with UTC+1 = CET. May not work for time zones behind UTC, e.g. U.S. East coast / Washington EST = UTC-5

            // load existing statistics file
            _loxStatFile = LoxStatFile.Load(fileItem.FileInfo.FullName);
            //Console.WriteLine("----------------------------------------------------------------------------------------------------------------");
            //Console.WriteLine("Reading old format from existing file: {0} ", fileItem.FileInfo.FullName);

            // load existing statistics file for next month - due to time conversion from local time to UTC
            string nextMonthFileName = Path.GetDirectoryName(fileItem.FileInfo.FullName) + "\\" + Path.GetFileNameWithoutExtension(fileItem.FileInfo.FullName) + "." + _loxStatFile.YearMonth.AddMonths(1).ToString("yyyyMM");
            LoxStatFile _nextMonthLoxStatFile = LoxStatFile.Load(nextMonthFileName);
            //Console.WriteLine("Reading existing file for next month: {0} ", nextMonthFileName);

            // compose new file name from old name (path, extension) plus UUID_x
            string newFileName = Path.GetDirectoryName(fileItem.FileInfo.FullName) + "\\" + loxFunctionBlock.UUID + "_" + fileNo.ToString() + Path.GetExtension(fileItem.FileInfo.FullName);
            //Console.WriteLine("Writing new format to file: {0} ", newFileName);
            
            // try to load file with new format for same yyyyMM - if it does not exist, then create a new file
            bool newFileExists = System.IO.File.Exists(newFileName);

            // get Unknown0x02 and Unknown0x04 from latest file (only header is loaded)
            LoxStatFile latestLoxStatFile = LoxStatFile.Load(loxFunctionBlock.FileItem[fileNo].FileInfo.FullName, true);
            
            // prepare the new file where data is written to
            LoxStatFile _newLoxStatFile;
            if (allowOverwrite && newFileExists) {
                try {
                    _newLoxStatFile = LoxStatFile.Load(newFileName);
                }
                catch (Exception ex) {
                    MessageBox.Show($"# Message\n{ex.Message}\n\n# Data\n{ex.Data}\n\n# StackTrace\n{ex.StackTrace}",
                        "Error - IList", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return 1;
                }
                // _newLoxStatFile is filled properly. Quit if ValueCount differs
                if ((_newLoxStatFile != null) && (_newLoxStatFile.ValueCount != loxFunctionBlock.FileItem[fileNo].ValueCount))
                    return 2;
            } else if (newFileExists) {
                // error - new file exists, but overwrite is not allowed
                return 1; 
            } else {
                // get values from old file and some information from latest file with new format
                _newLoxStatFile = new LoxStatFile {
                    FileName = newFileName,
                    Guid = new Guid(),
                    ValueCount = latestLoxStatFile.ValueCount,
                    Text = latestLoxStatFile.Text,
                    Unknown0x02 = latestLoxStatFile.Unknown0x02,
                    Unknown0x04 = latestLoxStatFile.Unknown0x04,
                    TextTerminator = latestLoxStatFile.TextTerminator
                };
            }
            
            // quit, if column does not exist, newValueIndex is either 0 or 1.
            if (newValueIndex + 1 > _newLoxStatFile.ValueCount)
                return 3;

            // calculate number of days in month 
            int days = DateTime.DaysInMonth(_loxStatFile.YearMonth.Year, _loxStatFile.YearMonth.Month);
            int newDPs, interval;
            switch (selectedIndex) {
                case 0:
                    // Keep existing interval
                    if (_loxStatFile.DataPoints != null)
                        newDPs = _loxStatFile.DataPoints.Count;
                    else
                        newDPs = 0;
                    interval = 0;
                    break;
                case 1:
                    // Interval 5 minutes
                    newDPs = days * 24 * 60 / 5 + 1;
                    interval = 5;
                    break;
                case 2:
                    // Interval 10 minutes
                    newDPs = days * 24 * 60 / 10 + 1;
                    interval = 10;
                    break;
                case 3:
                    // Interval 15 minutes
                    newDPs = days * 24 * 60 / 15 + 1;
                    interval = 15;
                    break;
                case 4:
                    // Interval 30 minutes
                    newDPs = days * 24 * 60 / 30 + 1;
                    interval = 30;
                    break;
                default:
                    // Interval 60 minutes
                    newDPs = days * 24 * 60 / 60 + 1;
                    interval = 60;
                    break;
            }
            // Console.WriteLine("Interval: " + interval);

            // NOTE: Loxone time stamps count seconds since 01.01.2009 in seconds,
            // For statistics files with old style (without _#) the time stamps are stored in local time. There are two problems for any calculations:
            //  - there is a one hour gap in March when the clock is adjusted for daylight saving time
            //  - in October when daylight saving time ends, the time stamps are jumping back one hour, so for one hour the time stamps are the same
            // For statistics files with new style (with _#) the time stamps are stored in UTC (most likely), so there are no problems with calculations.
            // All statistics files start with 1st day in each month, 0:00 either local time or UTC (depending on the style).
            // 
            bool hasDST;
            int baseTzOffsetSeconds;
            int dstTzOffsetSeconds;
            TimeZoneInfo selectedZone;
            if (timeZoneComboBox.SelectedValue.ToString() != "NoConversion") {
                hasDST = TimeZoneInfo.FindSystemTimeZoneById(timeZoneComboBox.SelectedValue.ToString()).SupportsDaylightSavingTime;
                selectedZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneComboBox.SelectedValue.ToString());
                baseTzOffsetSeconds = (int)selectedZone.BaseUtcOffset.TotalSeconds;
                if (hasDST)
                    dstTzOffsetSeconds = (int)selectedZone.GetAdjustmentRules()[0].DaylightDelta.TotalSeconds;
                else
                    dstTzOffsetSeconds = 0;
            } else {
                hasDST = false;
                selectedZone = TimeZoneInfo.Utc;
                baseTzOffsetSeconds = 0;
                dstTzOffsetSeconds = 0;
            }

            // normalize time stamps: set year and months from file, set first day in months, first hour to count for time zone offset     
            DateTime newTimeStamp = new DateTime(_loxStatFile.YearMonth.Year, _loxStatFile.YearMonth.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
            DateTime newTimeStampUTC = TimeZoneInfo.ConvertTimeToUtc(newTimeStamp, selectedZone);
            double diffThisMonth;
            bool useDST = useDSTcheckBox.Checked;
            if (useDST)
                diffThisMonth = (newTimeStamp - newTimeStampUTC).TotalSeconds;
            else
                diffThisMonth = selectedZone.BaseUtcOffset.TotalSeconds;

            DateTime newTimeStampNextMonth = newTimeStamp.AddMonths(1);
            //DateTime newTimeStampNextMonthUTC = newTimeStampUTC.AddMinutes(diffThisMonth).AddMonths(1).AddMinutes(-diffThisMonth);
            DateTime newTimeStampNextMonthUTC = TimeZoneInfo.ConvertTimeToUtc(newTimeStampNextMonth, selectedZone);
            double diffNextMonth;
            if (useDST)
                diffNextMonth = (newTimeStampNextMonth - newTimeStampNextMonthUTC).TotalSeconds;
            else
                diffNextMonth = selectedZone.BaseUtcOffset.TotalSeconds;

            /*
            Console.WriteLine("This month - Time in {0} zone: {1}", selectedZone.IsDaylightSavingTime(newTimeStamp) ?
                       selectedZone.DaylightName : selectedZone.StandardName, newTimeStamp);
            Console.WriteLine("             UTC Time: {0}", TimeZoneInfo.ConvertTimeToUtc(newTimeStamp, selectedZone));
            Console.WriteLine("Next month - Time in {0} zone: {1}", selectedZone.IsDaylightSavingTime(newTimeStampNextMonth) ?
                       selectedZone.DaylightName : selectedZone.StandardName, newTimeStampNextMonth);
            Console.WriteLine("             UTC Time: {0}", TimeZoneInfo.ConvertTimeToUtc(newTimeStampNextMonth, selectedZone));

            Console.WriteLine("Diff to UTC (in min) this month: " + diffThisMonth / 60 );
            Console.WriteLine("Diff to UTC (in min) next month: " + diffNextMonth / 60);
            */

            uint timeStampOffsetPrev = 0;
            bool endOfDST = false;
            DateTime lastTimeStamp = newTimeStamp;
            if (_loxStatFile.DataPoints != null)
                for (var index = 0; index < _loxStatFile.DataPoints.Count; index++) {
                    LoxStatDataPoint dP = _loxStatFile.DataPoints[index];

                    // if time goes 'backwards' then assume end of daylight savings time for rest of month
                    if (dP.TimestampOffset < timeStampOffsetPrev)
                        endOfDST = true;

                    // if daylight saving time is active (02:00 local time is no longer DST)
                    if (useDST && !endOfDST && 
                         (selectedZone.IsDaylightSavingTime(dP.Timestamp) ||
                          selectedZone.IsDaylightSavingTime(dP.Timestamp.AddHours(-1))))
                        dP.TimestampOffset = (uint)(dP.TimestampOffset - baseTzOffsetSeconds - dstTzOffsetSeconds);
                    else
                        dP.TimestampOffset = (uint)(dP.TimestampOffset - baseTzOffsetSeconds);

                    lastTimeStamp = dP.Timestamp;
                    timeStampOffsetPrev = dP.TimestampOffset;
                }

            // add data point for next month due to time adjustment from local time to UTC
            if ((_nextMonthLoxStatFile.DataPoints != null) && (baseTzOffsetSeconds != 0) &&
                (lastTimeStamp < newTimeStampNextMonth))
                for (var index = 0; index < _nextMonthLoxStatFile.DataPoints.Count; index++) {
                    LoxStatDataPoint dP = _nextMonthLoxStatFile.DataPoints[index];

                    // if time goes 'backwards' then assume end of daylight savings time for rest of month
                    if (dP.TimestampOffset < timeStampOffsetPrev)
                        endOfDST = true;

                    // if daylight saving time is active (02:00 local time is no longer DST)
                    if (useDST && !endOfDST &&
                         (selectedZone.IsDaylightSavingTime(dP.Timestamp) ||
                          selectedZone.IsDaylightSavingTime(dP.Timestamp.AddHours(-1))))
                        dP.TimestampOffset = (uint)(dP.TimestampOffset - baseTzOffsetSeconds - dstTzOffsetSeconds);
                    else
                        dP.TimestampOffset = (uint)(dP.TimestampOffset - baseTzOffsetSeconds);

                    _loxStatFile.DataPoints.Add(dP);
                    timeStampOffsetPrev = dP.TimestampOffset;

                    // end if first date from next month is reached
                    if (dP.Timestamp >= newTimeStampNextMonth)
                        break;
                }

            // sort existing data points by time / date, remove duplicates (same time / date) and convert to array
            LoxStatDataPoint[] oldDataPoints = _loxStatFile.DataPoints.Distinct(new LoxStatDataPointEqualityComparer()).OrderBy(dP => dP.TimestampOffset).ToArray();
            int oldDpIndex = 0;
            bool setToZero = false;

            if (oldDataPoints.Length == 1) {
                // special case - if there are no useful existing data points, we set every dp to zero
                setToZero = true;
            }

            ushort oldValueIndex = _loxStatFile.ValueCount;

            // power / flow data are written to _1 file
            if (fileNo == 1) {
                // if there is no 'value 2' available with power/flow data, we don't write any power/flow data
                if (_loxStatFile.ValueCount < 2)
                    return 4;
                else
                    oldValueIndex = 1;
            } else if (fileNo == 2) {
                // meter reading data is in column 0 (value)
                oldValueIndex = 0;
            }

            // calcuate uid part1/2 from UUID
            ushort objectUidPart1;
            ushort objectUidPart2;
            Guid loxGuid;
            Guid.TryParseExact(loxFunctionBlock.UUID.Replace("-", ""), "N", out loxGuid);
            if (loxGuid == Guid.Empty) {
                objectUidPart1 = 0;
                objectUidPart2 = 0;
                // AddProblem(collection, "Uid part mismatch");
            } else {
                var guidBytes = loxGuid.ToByteArray();
                objectUidPart1 = BitConverter.ToUInt16(guidBytes, 6);
                objectUidPart2 = BitConverter.ToUInt16(guidBytes, 4);
            }

            // create a list of dataPoints that are filled from existing ones (if possible)
            List<LoxStatDataPoint> newDataPoints = new List<LoxStatDataPoint>();
            for (var index = 0; index < newDPs; index++) {
                LoxStatDataPoint newDataPoint = new LoxStatDataPoint {
                    LoxStatFile = _newLoxStatFile,
                    Index = index,
                };
                newDataPoints.Add(newDataPoint);
                newDataPoint.ObjectUidPart1 = objectUidPart1;
                newDataPoint.ObjectUidPart2 = objectUidPart2;
                newDataPoint.Values = new double[_newLoxStatFile.ValueCount];
                newDataPoint.Padding = new byte[_newLoxStatFile.GetPaddingLength(8 + _newLoxStatFile.ValueCount * 8)];

                if (interval == 0) {
                    // transfer values 1:1 to new format with timestamps from existing data 
                    newDataPoint.TimestampOffset = oldDataPoints[index].TimestampOffset;
                    newDataPoint.Values[newValueIndex] = oldDataPoints[index].Values[oldValueIndex];
                } else {
                    // set time stamp to interval and convert old value with time correction
                    newDataPoint.Timestamp = newTimeStamp;

                    uint xOldDiff, xNewDiff;
                    double yOldDiff;

                    if (setToZero) {
                        // special case - set new values to zero
                        for (int i = 0; i < _newLoxStatFile.ValueCount; i++) {
                            newDataPoint.Values[i] = 0;
                        }
                    } else {
                        // find first element in old data points that is newer
                        while ((oldDpIndex < oldDataPoints.Length) &&
                                (oldDataPoints[oldDpIndex].TimestampOffset < newDataPoint.TimestampOffset))
                            oldDpIndex++;
                        if (oldDpIndex == oldDataPoints.Length) {
                            // new time stamp is past any existing data - use longest time difference to calculate linear graph
                            xOldDiff = oldDataPoints[oldDataPoints.Length - 1].TimestampOffset - oldDataPoints[0].TimestampOffset;
                            xNewDiff = newDataPoint.TimestampOffset - oldDataPoints[oldDataPoints.Length - 1].TimestampOffset;

                            if (fileNo == 1) {
                                yOldDiff = oldDataPoints[oldDataPoints.Length - 1].Values[oldValueIndex] - oldDataPoints[0].Values[oldValueIndex];
                                newDataPoint.Values[0] = oldDataPoints[oldDataPoints.Length - 1].Values[oldValueIndex] + (yOldDiff / xOldDiff) * xNewDiff;
                            }
                            if (fileNo == 2) {
                                yOldDiff = oldDataPoints[oldDataPoints.Length - 1].Values[oldValueIndex] - oldDataPoints[0].Values[oldValueIndex];
                                newDataPoint.Values[newValueIndex] = oldDataPoints[oldDataPoints.Length - 1].Values[oldValueIndex] + (yOldDiff / xOldDiff) * xNewDiff;
                            }
                        } else if (oldDataPoints[oldDpIndex].TimestampOffset == newDataPoint.TimestampOffset) {
                            // special case new and old time stamp are the same - this one is easy to handle
                            if (fileNo == 1) {
                                newDataPoint.Values[0] = oldDataPoints[oldDpIndex].Values[oldValueIndex];
                            }
                            if (fileNo == 2) {
                                newDataPoint.Values[newValueIndex] = oldDataPoints[oldDpIndex].Values[oldValueIndex];
                            }
                        } else if (oldDpIndex == 0) {
                            // new time stamp is before any existing data - use longest time difference to calculate linear graph
                            xOldDiff = oldDataPoints[oldDataPoints.Length - 1].TimestampOffset - oldDataPoints[0].TimestampOffset;
                            xNewDiff = oldDataPoints[0].TimestampOffset - newDataPoint.TimestampOffset;

                            if (fileNo == 1) {
                                yOldDiff = oldDataPoints[oldDataPoints.Length - 1].Values[oldValueIndex] - oldDataPoints[0].Values[oldValueIndex];
                                newDataPoint.Values[0] = oldDataPoints[0].Values[oldValueIndex] - (yOldDiff / xOldDiff) * xNewDiff;
                            }
                            if (fileNo == 2) {
                                yOldDiff = oldDataPoints[oldDataPoints.Length - 1].Values[oldValueIndex] - oldDataPoints[0].Values[oldValueIndex];
                                newDataPoint.Values[newValueIndex] = oldDataPoints[0].Values[oldValueIndex] - (yOldDiff / xOldDiff) * xNewDiff;
                            }
                        } else {
                            // new time stamp is between existing data - use nearest two points to calculate linear graph
                            xOldDiff = oldDataPoints[oldDpIndex].TimestampOffset - oldDataPoints[oldDpIndex - 1].TimestampOffset;
                            xNewDiff = newDataPoint.TimestampOffset - oldDataPoints[oldDpIndex - 1].TimestampOffset;
                            
                            if (fileNo == 1) {
                                yOldDiff = oldDataPoints[oldDpIndex].Values[oldValueIndex] - oldDataPoints[oldDpIndex - 1].Values[oldValueIndex];
                                newDataPoint.Values[0] = oldDataPoints[oldDpIndex - 1].Values[oldValueIndex] + (yOldDiff / xOldDiff) * xNewDiff;
                            }
                            if (fileNo == 2) {
                                yOldDiff = oldDataPoints[oldDpIndex].Values[oldValueIndex] - oldDataPoints[oldDpIndex - 1].Values[oldValueIndex];
                                newDataPoint.Values[newValueIndex] = oldDataPoints[oldDpIndex - 1].Values[oldValueIndex] + (yOldDiff / xOldDiff) * xNewDiff;
                            }
                        }
                        /*
                        Console.Write("Index: " + index + ", time: " + newTimeStamp.ToLocalTime().ToString("G"));
                        for (int i = 0; i < _newLoxStatFile.ValueCount; i++)
                            Console.Write(", DP[" + i + "]=" + newDataPoint.Values[i]);
                        Console.WriteLine("");*/

                        // add time interval for next loop
                        newTimeStamp = newTimeStamp.AddMinutes(interval);
                    }
                } // else - (no 1:1 transfer)
            } // for loop

            // set data points in file to new ones
            _newLoxStatFile.DataPoints = newDataPoints;

            ////// write to new file
            _newLoxStatFile.Save();
            //Console.WriteLine("File written!");
            return 0;
        }

        private int ConvertOldToNewFile(MiniserverForm.FileItem fileItem) {

            int errorCode = 0;
            if (fileItem.FileInfo == null) {
                return 2;
            }

            bool allowOverwrite = overwriteCheckBox.Checked;

            if (powerIntervalComboBox.SelectedIndex < 0)
                return 2;

            if (meterIntervalComboBox.SelectedIndex < 0)
                return 2;

            if (timeZoneComboBox.SelectedValue == null)
                return 2;

            string newUUID = uuidComboBox.SelectedItem.ToString().Substring(0, 35);
            LoxFunctionBlock loxFunctionBlock = loxFunctionBlocks.FirstOrDefault(item => item.UUID == newUUID);

            // write values either as as consumption (value0) or feed (value1)
            int valueIndex = value2RadioButton.Checked ? 1 : 0;

            // write power / flow data to _1 file, first data column
            // skip if last option was choosen
            if (powerIntervalComboBox.SelectedIndex < 6)
                errorCode = WriteNewFile(1, fileItem, allowOverwrite, loxFunctionBlock, powerIntervalComboBox.SelectedIndex, 0);
            //Console.WriteLine("Error code for _1 : {0} ", errorCode);

            // write meter reading data to _2 file
            // if the new meter only has one column (consumption), column is automatically adjusted from feed to consumption
            if (valueIndex + 1 > loxFunctionBlock.FileItem[2].ValueCount)
                valueIndex = 0;
            // skip if last option was choosen
            if (powerIntervalComboBox.SelectedIndex < 6)
                errorCode += WriteNewFile(2, fileItem, allowOverwrite, loxFunctionBlock, meterIntervalComboBox.SelectedIndex, valueIndex);
            //Console.WriteLine("Error code for _2 : {0} ", errorCode);

            return errorCode;
        }

        private int ModifyIntervalForFile(MiniserverForm.FileItem fileItem) {

            if (fileItem.FileInfo == null) {
                return 2;
            }

            _loxStatFile = LoxStatFile.Load(fileItem.FileInfo.FullName);

            // calculate number of days in month 
            int days = DateTime.DaysInMonth(_loxStatFile.YearMonth.Year, _loxStatFile.YearMonth.Month);
            int newDPs, interval;
            switch (newIntervalComboBox.SelectedIndex) {
                case 0:
                    // Interval 5 minutes
                    newDPs = days * 24 * 60 / 5 + 1;
                    interval = 5;
                    break;
                case 1:
                    // Interval 10 minutes
                    newDPs = days * 24 * 60 / 10 + 1;
                    interval = 10;
                    break;
                case 2:
                    // Interval 15 minutes
                    newDPs = days * 24 * 60 / 15 + 1;
                    interval = 15;
                    break;
                case 3:
                    // Interval 30 minutes
                    newDPs = days * 24 * 60 / 30 + 1;
                    interval = 30;
                    break;
                default:
                    // Interval 60 minutes
                    newDPs = days * 24 * 60 / 60 + 1;
                    interval = 60;
                    break;
            }
            // Console.WriteLine("Interval: " + interval);

            // NOTE: Loxone time stamps count seconds since 01.01.2009 in seconds, but reference time zone is MET (UTC+1) 
            // normalize time stamps: set year and months from file, set first day in months, first hour to count for zime zone offset     
            DateTime timeStamp = new DateTime(_loxStatFile.YearMonth.Year, _loxStatFile.YearMonth.Month, 1); //, 0, 0, 0, DateTimeKind.Utc);

            // sort existing data points by time / date, remove duplicates (same time / date) and convert to array
            LoxStatDataPoint[] oldDataPoints = _loxStatFile.DataPoints.Distinct(new LoxStatDataPointEqualityComparer()).OrderBy(dP => dP.Timestamp).ToArray();
            int oldDpIndex = 0;
            bool setToZero = false;

            if (oldDataPoints[oldDataPoints.Length - 1].TimestampOffset - oldDataPoints[0].TimestampOffset == 0) {
                // special case - there are no useful existing data points, so we set every dp to zero
                setToZero = true;
            }

            ushort objectUidPart1 = _loxStatFile.DataPoints.FirstOrDefault().ObjectUidPart1;
            ushort objectUidPart2 = _loxStatFile.DataPoints.FirstOrDefault().ObjectUidPart2;

            // create a list of dataPoints that are filled from existing ones
            List<LoxStatDataPoint> newDataPoints = new List<LoxStatDataPoint>();
            for (var index = 0; index < newDPs; index++) {
                LoxStatDataPoint newDataPoint = new LoxStatDataPoint {
                    LoxStatFile = _loxStatFile,
                    Index = index,
                };
                newDataPoints.Add(newDataPoint);
                newDataPoint.ObjectUidPart1 = objectUidPart1;
                newDataPoint.ObjectUidPart2 = objectUidPart2;
                newDataPoint.Timestamp = timeStamp;
                newDataPoint.Values = new double[_loxStatFile.ValueCount];

                uint xOldDiff, xNewDiff;
                double yOldDiff;

                if (setToZero) {
                    // special case - set new values to zero
                    for (int i = 0; i < _loxStatFile.ValueCount; i++) {
                        newDataPoint.Values[i] = 0;
                    }
                } else {
                    // find first element in old data points that is newer
                    while ((oldDpIndex < oldDataPoints.Length) &&
                            (oldDataPoints[oldDpIndex].TimestampOffset < newDataPoint.TimestampOffset))
                        oldDpIndex++;
                    if (oldDpIndex == 0) {
                        // new time stamp is before any existing data - use longest time difference to calculate linear graph
                        xOldDiff = oldDataPoints[oldDataPoints.Length - 1].TimestampOffset - oldDataPoints[0].TimestampOffset;
                        xNewDiff = oldDataPoints[0].TimestampOffset - newDataPoint.TimestampOffset;

                        for (int i = 0; i < _loxStatFile.ValueCount; i++) {
                            yOldDiff = oldDataPoints[oldDataPoints.Length - 1].Values[i] - oldDataPoints[0].Values[i];
                            newDataPoint.Values[i] = oldDataPoints[0].Values[i] - (yOldDiff / xOldDiff) * xNewDiff;
                        }
                    } else if (oldDpIndex == oldDataPoints.Length) {
                        // new time stamp is past any existing data - use longest time difference to calculate linear graph
                        xOldDiff = oldDataPoints[oldDataPoints.Length - 1].TimestampOffset - oldDataPoints[0].TimestampOffset;
                        xNewDiff = newDataPoint.TimestampOffset - oldDataPoints[0].TimestampOffset;
                        for (int i = 0; i < _loxStatFile.ValueCount; i++) {
                            yOldDiff = oldDataPoints[oldDataPoints.Length - 1].Values[i] - oldDataPoints[0].Values[i];
                            newDataPoint.Values[i] = oldDataPoints[oldDataPoints.Length - 1].Values[i] + (yOldDiff / xOldDiff) * xNewDiff;
                        }
                    } else if (oldDataPoints[oldDpIndex].TimestampOffset == newDataPoint.TimestampOffset) {
                        // special case new and old time stamp are the same - this one is easy to handle
                        for (int i = 0; i < _loxStatFile.ValueCount; i++) {
                            newDataPoint.Values[i] = oldDataPoints[oldDpIndex].Values[i];
                        }
                    } else {
                        // new time stamp is between existing data - use nearest two points to calculate linear graph
                        xOldDiff = oldDataPoints[oldDpIndex].TimestampOffset - oldDataPoints[oldDpIndex - 1].TimestampOffset;
                        xNewDiff = newDataPoint.TimestampOffset - oldDataPoints[oldDpIndex - 1].TimestampOffset;
                        for (int i = 0; i < _loxStatFile.ValueCount; i++) {
                            yOldDiff = oldDataPoints[oldDpIndex].Values[i] - oldDataPoints[oldDpIndex - 1].Values[i];
                            newDataPoint.Values[i] = oldDataPoints[oldDpIndex - 1].Values[i] + (yOldDiff / xOldDiff) * xNewDiff;
                        }
                    }
                    /* Console.Write("Index: " + index + ", time: " + timeStamp.ToLocalTime().ToString("G"));
                    for (int i = 0; i < _loxStatFile.ValueCount; i++)
                        Console.Write(", DP[" + i + "]=" + newDataPoint.Values[i]);
                    Console.WriteLine("");*/

                    newDataPoint.Padding = new byte[_loxStatFile.GetPaddingLength(8 + _loxStatFile.ValueCount * 8)];
                    // add time interval for next loop
                    timeStamp = timeStamp.AddMinutes(interval);
                }
            }

            // set data points in file to new ones
            _loxStatFile.DataPoints = newDataPoints;

            _loxStatFile.Save();
            return 0;
        }

        private void OkButton_Click(object sender, EventArgs e) {

            int errorCode = 0;

            foreach (FileItem fileItem in _selectedFileItems) {

                if (modifyInfoNameRadioButton.Checked)
                    errorCode += ModifyInfoNameForFile(fileItem);

                if (convertOldRadioButton.Checked)
                    errorCode += ConvertOldToNewFile(fileItem);

                if (modifyIntervalRadioButton.Checked)
                    errorCode += ModifyIntervalForFile(fileItem);
            }

            if (errorCode == 0)
                this.Close();
        }

        private void label4_Click(object sender, EventArgs e) {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {

        }

        private void fileInfoTextBox_TextChanged(object sender, EventArgs e) {

        }

        private void toolTip_Popup(object sender, PopupEventArgs e) {

        }

        private void selectValueRadioButton_CheckedChanged(object sender, EventArgs e) {

        }

        private void newFileInfoTextBox_TextChanged(object sender, EventArgs e) {

        }

        private void ModifyInfoNameRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (modifyInfoNameRadioButton.Checked) {
                modifyNameGroupBox.Enabled = true;
                newDescriptionTextBox.Enabled = true;
                convertOldGroupBox.Enabled = false;
                powerIntervalComboBox.Enabled = false;
                meterIntervalComboBox.Enabled = false;
                value1RadioButton.Enabled = false;
                value2RadioButton.Enabled = false;
                uuidComboBox.Enabled = false;
                overwriteCheckBox.Enabled = false;
                timeZoneComboBox.Enabled = false;
                useDSTcheckBox.Enabled = false;
                modifyIntervalGroupBox.Enabled = false;
                newIntervalComboBox.Enabled = false;
            }
        }

        private void ConvertOldRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (convertOldRadioButton.Checked) {
                //modifyInfoNameRadioButton
                modifyNameGroupBox.Enabled = false;
                newDescriptionTextBox.Enabled = false;
                convertOldGroupBox.Enabled = true;
                powerIntervalComboBox.Enabled = true;
                meterIntervalComboBox.Enabled = true;
                value1RadioButton.Enabled = true;
                value2RadioButton.Enabled = true;
                uuidComboBox.Enabled = true;
                overwriteCheckBox.Enabled = true;
                timeZoneComboBox.Enabled = true;
                useDSTcheckBox.Enabled = true;
                modifyIntervalGroupBox.Enabled = false;
                newIntervalComboBox.Enabled = false;
            }
        }

        private void ModifyIntervalRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (modifyIntervalRadioButton.Checked) {
                modifyNameGroupBox.Enabled = false;
                newDescriptionTextBox.Enabled = false;
                convertOldGroupBox.Enabled = false;
                powerIntervalComboBox.Enabled = false;
                meterIntervalComboBox.Enabled = false;
                value1RadioButton.Enabled = false;
                value2RadioButton.Enabled = false;
                uuidComboBox.Enabled = false;
                overwriteCheckBox.Enabled = false;
                timeZoneComboBox.Enabled = false;
                useDSTcheckBox.Enabled = false;
                modifyIntervalGroupBox.Enabled = true;
                newIntervalComboBox.Enabled = true;
            }
        }

        private void meterIntervalComboBox_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void timeZoneComboBox_SelectionChangeCommitted(object sender, EventArgs e) {
            // enable and check DST option if time zone supports DST, otherwise disable and uncheck DST option
            if (timeZoneComboBox.SelectedValue != null)
                if ((timeZoneComboBox.SelectedValue.ToString() != "NoConversion") &&
                    (TimeZoneInfo.FindSystemTimeZoneById(timeZoneComboBox.SelectedValue.ToString()).SupportsDaylightSavingTime)) {
                    // check DST if local time zone supports it
                    // NOTE: local time zone depends on Windows setting "automatically adjust time zone" and "automatically adjust to DST"
                    if (timeZoneComboBox.SelectedValue.ToString() == TimeZoneInfo.Local.Id)
                        useDSTcheckBox.Checked = TimeZoneInfo.Local.SupportsDaylightSavingTime;
                    else
                        useDSTcheckBox.Checked = true;
                    // enable DST if local time zone supports it in general
                    useDSTcheckBox.Enabled = true;
                } else {
                    useDSTcheckBox.Enabled = false;
                    useDSTcheckBox.Checked = false;
                }
        }
    }
}
