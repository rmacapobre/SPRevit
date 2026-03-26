using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Specpoint.Revit2026
{
    public class GridReportExporter
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public GridReportExporter()
            : this(string.Empty, string.Empty)
        {
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileNamePrefix">Output filename without extension.</param>
        /// <param name="worksheetName">Name to use for the Excel worksheet tab (31 chars max).</param>
        public GridReportExporter(string fileNamePrefix, string worksheetName)
        {
            this.fileNamePrefix = fileNamePrefix;
            this.nameOfWorksheet = worksheetName;
            this.includeColNames = true;
            this.boldFirstOutputRow = true;
        }

        /// <summary>
        /// IncludeColumnNameRow property.
        /// </summary>
        /// <value>Whether or not to include a row showing the column names as the first row in the exported report.</value>
        public bool IncludeColumnNameRow
        {
            get
            {
                return this.includeColNames;
            }
            set
            {
                this.includeColNames = value;
            }
        }
        private bool includeColNames;

        /// <summary>
        /// BoldFirstRow property.
        /// </summary>
        /// <value>Whether to bold the first row of the exported report (HTML format only).</value>
        public bool BoldFirstRow
        {
            get
            {
                return this.boldFirstOutputRow;
            }
            set
            {
                this.boldFirstOutputRow = true;
            }
        }
        private bool boldFirstOutputRow;

        /// <summary>
        /// Filename prefix for any exported file.
        /// </summary>
        public string FileNameBeforeExtension
        {
            get
            {
                return StringUtilities.MakeValidFileName(fileNamePrefix);
            }
            set
            {
                this.fileNamePrefix = (value == null) ? string.Empty : value;
            }
        }
        private string fileNamePrefix;

        /// <summary>
        /// Name for Excel spreadsheet tab in exported XML file. 31 characters max.
        /// </summary>
        public string WorksheetName
        {
            get
            {
                return this.nameOfWorksheet;
            }
            set
            {
                this.nameOfWorksheet = (value == null) ? string.Empty : value;
            }
        }
        private string nameOfWorksheet;

        /// <summary>
        /// Exports a data grid view to a spreadsheet or webpage.
        /// </summary>
        /// <param name="dataGridView">DataGridView to export.</param>
        public void ExportGrid(DataGridView dataGridView)
        {
            try
            {
                if (this.FileNameBeforeExtension == string.Empty)
                    throw new ApplicationException("Spreadsheet must have a name.");

                DataTable dt = dataGridView.DataSource as DataTable;
                if (dt == null)
                    throw new ApplicationException("Spreadsheet does not contain any data.");

                SaveFileDialog dlg = new SaveFileDialog();
                dlg.AddExtension = true;
                dlg.FileName = this.FileNameBeforeExtension
                    + " - "
                    + System.DateTime.Today.Month.ToString("D2")
                    + "-"
                    + System.DateTime.Today.Day.ToString("D2")
                    + "-"
                    + System.DateTime.Today.Year.ToString("D4");
                dlg.Filter = "Comma-separated Values (*.csv)|*.csv|HTML file (*.html)|*.html|Excel Spreadsheet (*.xml)|*.xml|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.Cancel)
                    return;

                switch (dlg.FilterIndex)
                {
                    // csv format
                    case 1:
                        ExportToCsv(dataGridView, dlg.FileName);
                        break;
                    // html format
                    case 2:
                        ExportToHtml(dataGridView, dlg.FileName);
                        break;
                    // excel spread sheet
                    case 3:
                    default:
                        ExportToXml(dataGridView, dlg.FileName);
                        break;
                }
            }
            catch (IOException ex)
            {
                ErrorReporter.ReportError(ex.Message);
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

        }

        private void ExportToXml(DataGridView dataGridView, string fileName)
        {
            string title = "Export to XML";
            try
            {
                if (File.Exists(fileName))
                {
                    // Replace the existing copy
                    File.Delete(fileName);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0}", ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get data table
            DataTable dt = dataGridView.DataSource as DataTable;

            // writer settings
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "   ";
            settings.Encoding = Encoding.UTF8;

            // write XML
            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                // processing info
                writer.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
                writer.WriteProcessingInstruction("mso-application", "progid = 'Excel.Sheet'");

                // root
                writer.WriteStartElement("Workbook", "urn:schemas-microsoft-com:office:spreadsheet");

                // namespaces
                writer.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");
                writer.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
                writer.WriteAttributeString("xmlns", "ss", null, "urn:schemas-microsoft-com:office:spreadsheet");
                writer.WriteAttributeString("xmlns", "html", null, "http://www.w3.org/TR/REC-html40");

                // DocumentProperties
                writer.WriteStartElement("DocumentProperties", "urn:schemas-microsoft-com:office:office");
                writer.WriteElementString("LastAuthor", "Revit User");
                writer.WriteElementString("Created", DateTime.UtcNow.ToString("o"));
                writer.WriteElementString("LastSaved", DateTime.UtcNow.ToString("o"));
                writer.WriteElementString("Version", "11.5606");
                writer.WriteEndElement();

                // ExcelWorkbook
                writer.WriteStartElement("ExcelWorkbook", "urn:schemas-microsoft-com:office:excel");
                writer.WriteElementString("WindowHeight", "10830");
                writer.WriteElementString("WindowWidth", "15480");
                writer.WriteElementString("WindowTopX", "360");
                writer.WriteElementString("WindowTopY", "75");
                writer.WriteElementString("AcceptLabelsInFormulas", string.Empty);
                writer.WriteElementString("ProtectStructure", "False");
                writer.WriteElementString("ProtectWindows", "False");
                writer.WriteEndElement();

                // Worksheet
                writer.WriteStartElement("Worksheet");
                writer.WriteAttributeString("ss", "Name", "urn:schemas-microsoft-com:office:spreadsheet", this.WorksheetName);

                // Table
                writer.WriteStartElement("Table");

                // write column widths
                List<int> hiddenCols = GetHiddenCols(dataGridView);
                for (int i = 0; i < dataGridView.Columns.Count; i++)
                {
                    // Filter out hidden cols
                    if (hiddenCols.Contains(i)) continue;

                    writer.WriteStartElement("Column");
                    writer.WriteAttributeString("ss", "Width", "urn:schemas-microsoft-com:office:spreadsheet", OurConvert.ToString(dataGridView.Columns[i].Width));
                    writer.WriteEndElement();
                }

                // write header row
                if (this.IncludeColumnNameRow)
                {
                    int i = 0;
                    writer.WriteStartElement("Row");
                    foreach (DataColumn dc in dt.Columns)
                    {
                        // Filter out hidden cols
                        if (hiddenCols.Contains(i)) continue;

                        writer.WriteStartElement("Cell");
                        writer.WriteStartElement("Data");
                        writer.WriteAttributeString("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet", "String");
                        writer.WriteValue(dc.ColumnName);
                        writer.WriteEndElement();
                        writer.WriteEndElement();

                        i++;
                    }
                    writer.WriteEndElement();

                }

                // write rows
                int row = 0;
                List<int> hiddenRows = GetHiddenRows(dataGridView);
                foreach (DataRow dr in dt.Rows)
                {
                    // Filter out hidden rows
                    if (hiddenRows.Contains(row))
                    {
                        row++;
                        continue;
                    }

                    writer.WriteStartElement("Row");
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        // Filter out hidden cols
                        if (hiddenCols.Contains(i)) continue;

                        if (dr[i].Equals(DBNull.Value)) continue;
                        writer.WriteStartElement("Cell");
                        writer.WriteStartElement("Data");
                        writer.WriteAttributeString("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet", "String");
                        writer.WriteValue(dr[i]);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    row++;
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        private void ExportToHtml(DataGridView dataGridView, string fileName)
        {
            string title = "Export to HTML";
            try
            {
                if (File.Exists(fileName))
                {
                    // Replace the existing copy
                    File.Delete(fileName);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0}", ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get data table
            DataTable dt = dataGridView.DataSource as DataTable;

            // write Html
            using (StreamWriter writer = new StreamWriter(File.OpenWrite(fileName)))
            {
                writer.Write(
                    @"<html>
                            <head>
                            <style type=string.Emptytext/cssstring.Empty>
                            table { border:1px solid black; border-collapse:collapse; empty-cells:show;}
                            td { border:1px solid black; border-collapse:collapse; padding-left:5px; padding-right:5px; }
                            tr.highlight { background-color:LightCyan; }
                            </style>
                            </head>
                            <body><table>");

                // highlight alternate rows
                bool highlight = false;

                // write header row
                List<int> hiddenCols = GetHiddenCols(dataGridView);
                if (this.IncludeColumnNameRow)
                {
                    int i = 0;
                    writer.Write("<tr>");
                    foreach (DataColumn dc in dt.Columns)
                    {
                        // Filter off hidden cols
                        if (hiddenCols.Contains(i)) continue;

                        string headerColData;
                        if (this.BoldFirstRow)
                        {
                            headerColData = "<td><b>" + dc.ColumnName + "</b></td>";
                        }
                        else
                        {
                            headerColData = "<td>" + dc.ColumnName + "</td>";
                        }
                        writer.Write(headerColData);
                        i++;
                    }
                    writer.WriteLine("</tr>");
                    highlight = !highlight;
                }

                // write data rows
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    bool dataWritten = false;

                    // write remaining rows
                    List<int> hiddenRows = GetHiddenRows(dataGridView);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        // Filter off hidden rows
                        if (hiddenRows.Contains(i)) continue;

                        row = dt.Rows[i];
                        if (highlight)
                            writer.Write("<tr class=\"highlight\">");
                        else
                            writer.Write("<tr>");
                        dataWritten = false;
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            // Filter off hidden cols
                            if (hiddenCols.Contains(j)) continue;

                            string colData = row[j].ToString();
                            writer.Write("<td>" + colData + "</td>");
                            if (!dataWritten && colData.Length > 0)
                            {
                                // we wrote at least some text this row, so toggle highlight
                                // for the next row.
                                dataWritten = true;
                            }
                        }
                        writer.WriteLine("</tr>");

                        // toggle highlight if row wasn't empty
                        if (dataWritten)
                            highlight = !highlight;
                    }
                }

                writer.Write("</table></body></html>");
            }
        }

        private void ExportToCsv(DataGridView dataGridView, string fileName)
        {
            string title = "Export to CSV";
            try
            {
                if (File.Exists(fileName))
                {
                    // Replace the existing copy
                    File.Delete(fileName);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0}", ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get data table
            DataTable dt = dataGridView.DataSource as DataTable;

            // write CSV file.
            List<int> hiddenCols = GetHiddenCols(dataGridView);
            using (StreamWriter writer = new StreamWriter(File.OpenWrite(fileName)))
            {
                // Write column headers.
                if (this.IncludeColumnNameRow)
                {
                    string headers = string.Empty;
                    for (int colIndex = 0; colIndex < dataGridView.Columns.Count; colIndex++)
                    {
                        // Filter out hidden cols
                        if (hiddenCols.Contains(colIndex)) continue;

                        // Enclose values in double-quotes to preserve commas.
                        string headerText = dataGridView.Columns[colIndex].HeaderText;
                        headers += StringUtilities.DoubleQuote(headerText) + ",";
                    }
                    writer.WriteLine(headers);
                }

                // Write data.
                List<int> hiddenRows = GetHiddenRows(dataGridView);
                for (int rowIndex = 0; rowIndex < dataGridView.RowCount; rowIndex++)
                {
                    // Filter out hidden rows
                    if (hiddenRows.Contains(rowIndex)) continue;

                    string line = string.Empty;
                    for (int colIndex = 0; colIndex < dataGridView.Rows[rowIndex].Cells.Count; colIndex++)
                    {
                        // Filter out hidden cols
                        if (hiddenCols.Contains(colIndex)) continue;

                        string cellText = dataGridView.Rows[rowIndex].Cells[colIndex].Value as string;

                        // If the value contains one double quotes
                        if (cellText.Contains("\""))
                        {
                            // Replace with two double quotes
                            cellText = cellText.Replace("\"", "\"\"");
                        }

                        line += StringUtilities.DoubleQuote(cellText) + ",";
                    }
                    writer.WriteLine(line);
                }
            }
        }

        private List<int> GetHiddenCols(DataGridView grid)
        {
            List<int> result = new List<int>();

            int i = 0;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.Visible == false)
                {
                    result.Add(i);
                }

                i++;
            }
            return result;
        }

        private List<int> GetHiddenRows(DataGridView grid)
        {
            List<int> result = new List<int>();

            int i = 0;
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Visible == false)
                {
                    result.Add(i);
                }

                i++;
            }

            return result;
        }
    }
}
