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

            /// <summary>This property retrieves the latest year and month where a statistics file with this UUID is available.
            /// </summary>
            public DateTime LatestYearMonth {
                get; set;
            }

            /// <summary>This property retrieves the number of values (columns) that are stored for each data point (timestamp) the statistics file.
            /// </summary>
            public ushort[] ValueCount {
                get; set; 
            }

            /// <summary>Statistics files with new style use an underscore with number as an identifier.
            /// </summary>
            public bool IsNewFormat {
                get {
                    // va
                    for (var i = 1; i < 10; i++)
                        if (ValueCount[i] > 0)
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
            _fileItem = _selectedFileItems[0];
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

            Console.WriteLine("Start ...");
            // ToDo: handle multiple files - so far only _fileItem is assumed (one fileItem)
            if ((_fileItem != null) && (_fileItem.FileName.Length > 0)) {

                // load file to get some infos
                if (_fileItem.FileInfo != null) {
                    
                    fileNameTextBox.Text = _fileItem.FileInfo.FullName;
                    _loxStatFile = LoxStatFile.Load(_fileItem.FileInfo.FullName);
                    descriptionTextBox.Text = _fileItem.Description;
                    fileInfoTextBox.Text = string.Format("{0}, {1} data point{2}, each with {3} value{4}",
                                    _loxStatFile.YearMonth.ToString("yyyy-MM"),
                                    _loxStatFile.DataPoints.Count,
                                    _loxStatFile.DataPoints.Count == 1 ? "" : "s",
                                    _loxStatFile.ValueCount,
                                    _loxStatFile.ValueCount == 1 ? "" : "s");
                } else {
                    fileNameTextBox.Text = _fileItem.FileName;
                    descriptionTextBox.Text = $"The file \"{_fileItem.FileName}\" cannot be converted, since it's not available on the filesystem.";
                    fileInfoTextBox.Text = "Please download it first.";
                }
            }

            Console.WriteLine("Build Loxone function blocks ... ");
            // create a list of Loxone function blocks with UUID, description from latest file
            loxFunctionBlocks = new List<LoxFunctionBlock>();
            LoxFunctionBlock defaultLoxFunctionBlock = new LoxFunctionBlock {
                UUID = "",
                LatestDescription = "",
                LatestYearMonth = new DateTime(),
                ValueCount = new ushort[10]
            };

            // fill list of Loxone function blocks from all files in the list
            foreach (FileItem fileItem in _fileItems) {
                // only add valid UUIDs to list
                if (fileItem.IsValidMsStatsFile) {
                    // get function block from list (or null)
                    loxFunctionBlock = loxFunctionBlocks.FirstOrDefault(item => item.UUID == fileItem.UUID);
                    // UUID was not in list, so add it
                    if (loxFunctionBlock == null) {
                        loxFunctionBlock = new LoxFunctionBlock {
                            UUID = fileItem.UUID,
                            LatestDescription = fileItem.Description,
                            LatestYearMonth = fileItem.YearMonth,
                            ValueCount = new ushort[10]
                        };
                        loxFunctionBlock.ValueCount[fileItem.msStatsType] = fileItem.ValueCount;
                        loxFunctionBlocks.Add(loxFunctionBlock);
                    } else {  // UUID was found, but description might be newer
                        if (fileItem.YearMonth > loxFunctionBlock.LatestYearMonth) {
                            loxFunctionBlock.LatestYearMonth = fileItem.YearMonth;
                            loxFunctionBlock.LatestDescription = fileItem.Description;
                        }
                        // there might be more files with same UUID for new style meters (with '_#'), take number of values from latest file
                        if (fileItem.YearMonth >= loxFunctionBlock.LatestYearMonth) {
                            loxFunctionBlock.ValueCount[fileItem.msStatsType] = fileItem.ValueCount;
                        }
                    }
                }
            } // end foreach

            Console.WriteLine("Building list ... ");
            // create a drop-down list with UUIDs for function blocks with new format (with _ in filename)
            uuidComboBox.Items.Clear();
            foreach (LoxFunctionBlock loxFunctionBlock in loxFunctionBlocks) {
                if (loxFunctionBlock.IsNewFormat)
                    uuidComboBox.Items.Add(loxFunctionBlock.UUID + " - " + loxFunctionBlock.LatestDescription);
            }
            // disable "convert old format to new" if the file already has the new format 
            if (_fileItem.msStatsType > 0)
                convertOldRadioButton.Enabled = false;

            // get file info text from latest file (highest yyyyMM)
            string newDescription = loxFunctionBlocks.FirstOrDefault(item => item.UUID == _fileItem.UUID)?.LatestDescription;
            if (newDescription.Length > 200)
                newDescription = newDescription.Substring(0, 200);
            newDescriptionTextBox.Text = newDescription;

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
            modifyIntervalGroupBox.Enabled = false;
            newIntervalComboBox.Enabled = false;

            Console.WriteLine("Initializing finished ");
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

            LoxStatFile _newLoxStatFile;
            _loxStatFile = LoxStatFile.Load(fileItem.FileInfo.FullName);
            Console.WriteLine("Reading old format from existing file: {0} ", fileItem.FileInfo.FullName);

            // compose new file name from old name (path, extension) plus UUID_x
            string newFileName = Path.GetDirectoryName(fileItem.FileInfo.FullName) + "\\" + loxFunctionBlock.UUID + "_" + fileNo.ToString() + Path.GetExtension(fileItem.FileInfo.FullName);
            Console.WriteLine("Writing new format to file: {0} ", newFileName);
            
            // try to load file with new format for same yyyyMM - if it does not exist, then create a new file
            bool newFileExists = System.IO.File.Exists(newFileName);
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
                if ((_newLoxStatFile != null) && (_newLoxStatFile.ValueCount != loxFunctionBlock.ValueCount[fileNo]))
                    return 2;
            } else if (newFileExists) {
                // error - new file exists, but overwrite is not allowed
                return 1; 
            } else {
                // get values from old file and some information from latest file with new format
                _newLoxStatFile = new LoxStatFile {
                    FileName = newFileName,
                    Guid = new Guid(),
                    Text = loxFunctionBlock.LatestDescription,
                    ValueCount = loxFunctionBlock.ValueCount[fileNo],
                    Unknown0x02 = _loxStatFile.Unknown0x02,
                    Unknown0x04 = _loxStatFile.Unknown0x04,
                    TextTerminator = _loxStatFile.TextTerminator
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

            // NOTE: Loxone time stamps count seconds since 01.01.2009 in seconds, but reference time zone is MET (UTC+1) 
            // normalize time stamps: set year and months from file, set first day in months, first hour to count for zime zone offset     
            DateTime timeStamp = new DateTime(_loxStatFile.YearMonth.Year, _loxStatFile.YearMonth.Month, 1); //, 0, 0, 0, DateTimeKind.Utc);

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

                if (interval == 0) {
                    // transfer values 1:1 to new format with timestamps from existing data 
                    newDataPoint.TimestampOffset = oldDataPoints[index].TimestampOffset;
                    newDataPoint.Values = new double[_newLoxStatFile.ValueCount];

                    newDataPoint.Values[newValueIndex] = oldDataPoints[index].Values[oldValueIndex];
                } else {
                    // set time stamp to interval and convert old value with time correction
                    newDataPoint.Timestamp = timeStamp;
                    newDataPoint.Values = new double[_newLoxStatFile.ValueCount];

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
                        if (oldDpIndex == 0) {
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
                        } else if (oldDpIndex == oldDataPoints.Length) {
                            // new time stamp is past any existing data - use longest time difference to calculate linear graph
                            xOldDiff = oldDataPoints[oldDataPoints.Length - 1].TimestampOffset - oldDataPoints[0].TimestampOffset;
                            xNewDiff = newDataPoint.TimestampOffset - oldDataPoints[0].TimestampOffset;

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
                        Console.Write("Index: " + index + ", time: " + timeStamp.ToLocalTime().ToString("G"));
                        for (int i = 0; i < _newLoxStatFile.ValueCount; i++)
                            Console.Write(", DP[" + i + "]=" + newDataPoint.Values[i]);
                        Console.WriteLine("");

                        newDataPoint.Padding = new byte[_loxStatFile.GetPaddingLength(8 + _newLoxStatFile.ValueCount * 8)];
                        // add time interval for next loop
                        timeStamp = timeStamp.AddMinutes(interval);
                    }
                } // else - (no 1:1 transfer)
            } // for loop

            // set data points in file to new ones
            _newLoxStatFile.DataPoints = newDataPoints;

            ////// write to new file
            _newLoxStatFile.Save();
            Console.WriteLine("File written!");
            return 0;
        }

        private int ConvertOldToNewFile(MiniserverForm.FileItem fileItem) {

            int errorCode = 0;
            if (fileItem.FileInfo == null) {
                return 2;
            }

            bool allowOverwrite = overwriteCheckBox.Enabled;

            if (uuidComboBox.SelectedItem == null)
                return 2;
            
            string newUUID = uuidComboBox.SelectedItem.ToString().Substring(0, 35);
            LoxFunctionBlock loxFunctionBlock = loxFunctionBlocks.FirstOrDefault(item => item.UUID == newUUID);

            // write values either as as consumption (value0) or feed (value1)
            int valueIndex = value2RadioButton.Checked ? 1 : 0;

            // write power / flow data to _1 file, first data column
            if (powerIntervalComboBox.SelectedIndex < 6)
                errorCode = WriteNewFile(1, fileItem, allowOverwrite, loxFunctionBlock, powerIntervalComboBox.SelectedIndex, 0);
            Console.WriteLine("Error code for _1 : {0} ", errorCode);

            // write meter reading data to _2 file

            // if the new meter only has one column (consumption), column is automatically adjusted from feed to consumption
            if (valueIndex + 1 > loxFunctionBlock.ValueCount[2])
                valueIndex = 0;
            errorCode = WriteNewFile(2, fileItem, allowOverwrite, loxFunctionBlock, meterIntervalComboBox.SelectedIndex, valueIndex);
            Console.WriteLine("Error code for _2 : {0} ", errorCode);

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
            if (modifyInfoNameRadioButton.Checked) 
                errorCode = ModifyInfoNameForFile(_fileItem);

            if (convertOldRadioButton.Checked) 
                errorCode = ConvertOldToNewFile(_fileItem);

            if (modifyIntervalRadioButton.Checked) 
                errorCode = ModifyIntervalForFile(_fileItem);

            if (errorCode == 0)
                this.Close();
        }

        private void changeIntervalLabel_Click(object sender, EventArgs e) {

        }

        private void label4_Click(object sender, EventArgs e) {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {

        }

        private void fileInfoTextBox_TextChanged(object sender, EventArgs e) {

        }

        private void changeIntervalPowerlabel_Click(object sender, EventArgs e) {

        }

        private void toolTip_Popup(object sender, PopupEventArgs e) {

        }

        private void selectUUIDlabel_Click(object sender, EventArgs e) {

        }

        private void selectValueRadioButton_CheckedChanged(object sender, EventArgs e) {

        }

        private void writeValuelabel_Click(object sender, EventArgs e) {

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
                modifyIntervalGroupBox.Enabled = true;
                newIntervalComboBox.Enabled = true;
            }

        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void fileNameLabel_Click(object sender, EventArgs e) {

        }

        private void meterIntervalComboBox_SelectedIndexChanged(object sender, EventArgs e) {

        }
    }
}
