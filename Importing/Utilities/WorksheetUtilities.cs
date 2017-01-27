using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.Kingpin.WCF2.Utilities;
using Amazon.Kingpin.WCF2.Classes.Importing.Entities;
using Amazon.Kingpin.WCF2.Data.Providers;
using Amazon.Kingpin.WCF2.Data.Access;
using CsvHelper;

namespace Amazon.Kingpin.WCF2.Classes.Importing.Utilities
{
    public class WorksheetUtilities
    {
        private List<char> Letters = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', ' ' };

        //private SPDataProvider spDataProvider;
        //private SPDataAccess spDataAccess;
        //string subSiteUrl;
        //string libraryName;
        //string fileName;
        private MemoryStream fileStream;

        private SPDataAccess spDataAccess;

        private bool containsWorksheets = false;

        /// <summary>
        /// Default Ctor
        /// </summary>
        public WorksheetUtilities() { }

        public WorksheetUtilities(SPDataAccess dataAccess)
        {
            this.spDataAccess = dataAccess;
        }

        /// <summary>
        /// Automoatically generates the column letters
        /// </summary>
        private void GenerateColumnLetters()
        {
            this.Letters = new List<char>();
            for (int i = 65; i < 91; i++)
            {
                this.Letters.Add(Convert.ToChar(i));
            }
        }

        public void LoadFile(string listName, string fileName)
        {
            MappingData mappingData = new MappingData() {
                LibraryName = "/Common",
                ListName = listName,
                FileName =  fileName
            };
            this.LoadFile(mappingData);
        }

        public void LoadFile(MappingData mappingData)
        {
            if (mappingData.FileName.Contains("xlsx"))
                this.containsWorksheets = true;

            try
            {
                //this.fileStream = spDataAccess.GetFileStream(mappingData.Team.SiteUrl, mappingData.LibraryName, mappingData.FileName);
                this.fileStream = spDataAccess.GetFileStream(string.Empty, mappingData.LibraryName, mappingData.FileName);
            }
            catch(Exception ex)
            {
                string errMsg = string.Format("Cannot open file: {0}/{1}; {3}", mappingData.LibraryName, mappingData.FileName, ex);
                throw new Exception(errMsg, ex.InnerException);
            }
        }

        /// <summary>
        /// Returns all the names of the worksheets in
        /// the excel workbook supplied as a MemoryStream
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<string> GetWorksheetNames(string listName, string fileName)
        {
            List<string> sheetNames = new List<string>();

            if (fileName.Contains("xlsx"))
            {
                this.LoadFile(listName, fileName);
                this.containsWorksheets = true;
                if (this.fileStream == null) { 
                    throw new NullReferenceException("File stream is null. Please initialize the worksheet with a Stream."); 
                }
            }
          
            if(this.containsWorksheets)
            {
                try
                {
                    using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(this.fileStream, false))
                    {
                        Workbook wb = spreadsheet.WorkbookPart.Workbook;
                        sheetNames = spreadsheet.WorkbookPart.Workbook.Descendants<Sheet>().Select(s => s.Name.ToString()).ToList();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            return sheetNames;
        }

        /// <summary>
        /// Get the specified worksheet from the workbook
        /// </summary>
        /// <param name="workbookPart"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        private Worksheet GetWorkSheet(WorkbookPart workbookPart, string sheetName)
        {
            //Get the relationship id of the sheetname
            string relId = string.Empty;
            Worksheet worksheet = null;

            try
            {
                relId = workbookPart.Workbook.Descendants<Sheet>()
                 .Where(s => s.Name.Value.Equals(sheetName)).First().Id;
                worksheet = ((WorksheetPart)workbookPart.GetPartById(relId)).Worksheet;
            }
            catch (Exception)
            {
                worksheet = ((WorksheetPart)workbookPart.WorksheetParts.First()).Worksheet;
            }

            return worksheet;
        }

        /// <summary>
        /// Overloaded method to make loading the sheet
        /// easier from the API using the MappingData object
        /// </summary>
        /// <param name="mappingData"></param>
        /// <returns></returns>
        public List<string> GetWorksheetColumns(MappingData mappingData)
        {
            this.LoadFile(mappingData);
            return this.GetWorksheetColumns(mappingData.SheetName);
        }

        /// <summary>
        /// Gets the worksheet columns
        /// </summary>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public List<string> GetWorksheetColumns(string sheetName)
        {
            List<string> columns = new List<string>();
            List<string> exclusionColumns = new List<string>() { "Attachment", "GUID", "Path", "Version" };

            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(this.fileStream, false))
            {
                Worksheet sheet = GetWorkSheet(spreadsheet.WorkbookPart, sheetName);
                SheetData sd = (SheetData)sheet.Where(x => x.LocalName == "sheetData").First();
                SharedStringTablePart sstp = spreadsheet.WorkbookPart.SharedStringTablePart;

                //columns!                
                Row header = sd.Elements<Row>().First();
                foreach (Cell c in header.Elements<Cell>())
                {
                    string cellValue = GetValue(c, sstp).Trim();
                    if(!exclusionColumns.Contains(cellValue))
                    {
                        columns.Add(cellValue);
                    }
                }
            }

            return columns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="library"></param>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public DataTable GetDataTable(MappingData mappingData)
        {
            DataTable dt = null;
            bool isFirst = true;
            bool isImportTemplate = false;
            bool headersSet = false;

            if(this.containsWorksheets)
            {
                using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(this.fileStream, false))
                {
                    Worksheet sheet = GetWorkSheet(spreadsheet.WorkbookPart, mappingData.SheetName);
                    SheetData sd = (SheetData)sheet.Where(x => x.LocalName == "sheetData").First();
                    dt = new DataTable(mappingData.SheetName);
                    SharedStringTablePart sstp = spreadsheet.WorkbookPart.SharedStringTablePart;

                    // create columns                
                    Row header = sd.Elements<Row>().First();
                    foreach (Cell cell in header.Elements<Cell>())
                    {
                        DataColumn dc = new DataColumn();
                        dc.Caption = GetValue(cell, sstp).Trim();
                        dc.ColumnName = dc.Caption.Trim();
                        dt.Columns.Add(dc);
                    }

                    // need to manually inject the team path here for Templates since they do
                    // not have a teamId column and we get this value from the client request
                    if (mappingData.TemplateType == ImportType.Import || mappingData.TemplateType == ImportType.Current)
                    {
                        DataColumn dc = new DataColumn();
                        dc.ColumnName = "KPTeam";
                        dt.Columns.Add(dc);
                        isImportTemplate = true;
                    }

                    //rows!
                    foreach (Row row in sd.Elements<Row>())
                    {
                        if (isFirst)
                            //this is to keep the row headers from showing up. Ugh.
                            isFirst = false;
                        else
                        {
                            DataRow dataRow = dt.NewRow();
                            List<Cell> cells = row.Elements<Cell>().ToList();
                            string value;
                            int? indexedColumn;

                            for (int index = 0; index < cells.Count; index++)
                            {
                                indexedColumn = GetColumnIndexFromName(GetColumnName(cells[index].CellReference));
                                if (indexedColumn.HasValue)
                                {
                                    value = GetValue(cells[index], sstp);
                                    dataRow[indexedColumn.Value] = value;
                                }
                            }

                            // add the last column value = "Path"
                            if (isImportTemplate)
                            {
                                // need to get the name of the Team
                                dataRow["KPTeam"] = mappingData.Team.Nick;
                            }
                            dt.Rows.Add(dataRow);
                        }
                    }
                }
            }
            else
            {
                // CSV doesn't have sheets so we give it an arbitrary name 
                string sheetName = "Project";
                int rowIndex = 0;
                int STATUS_COLUMN_INDEX = 7;
                // create new datatable
                dt = new DataTable(sheetName);
                // generate column header fields
                // this must be a csv if no worksheets
                TextReader textReader = new StreamReader(this.fileStream);
                CsvReader csv = new CsvReader(textReader);

                while(csv.Read())
                {
                    if(!headersSet)
                    {
                        this.SetHeaders(csv, dt);
                        headersSet = true;
                    }
                    // totally messed up sheet includes headers before each data row
                    // so we need to skip 3 rows because there is also an 'empty' row
                    if(rowIndex % 3 == 0)
                    {
                        DataRow dataRow = dt.NewRow();
                        foreach (string fieldName in csv.FieldHeaders)
                        {
                            string value = csv.GetField<string>(fieldName);
                            dataRow[fieldName.Trim()] = string.IsNullOrEmpty(value.Trim()) ? null : value.Trim();
                        }
                        
                        // multiple status columns 
                        // need to get the first one as that is the 'latest' value
                        dataRow["Status"] = csv.GetField<string>(STATUS_COLUMN_INDEX);
                        
                        // setting team here using supplied 'global' teamId
                        dataRow["KPTeam"] = mappingData.Team.Nick;
                        
                        // setting type here using hard-coded value
                        dataRow["ProjectType"] = "Client Project";
                        
                        dt.Rows.Add(dataRow);
                    }
                    rowIndex++;
                }
            }

            return dt;
        }

        private void SetHeaders(CsvReader csv, DataTable dt)
        {
            //create columns                
            foreach (string fieldName in csv.FieldHeaders)
            {
                DataColumn dc = new DataColumn();
                dc.Caption = fieldName.Trim();
                dc.ColumnName = dc.Caption;
                // column names may be duplicated in Current
                if(!dt.Columns.Contains(fieldName.Trim()))
                    dt.Columns.Add(dc);

            }
            // column not labeled on sheet
            // multiple status columns showing weekly history
            dt.Columns.Add("Status");
            // not included on sheet - which column to use for this?
            // use the supplied teamId for now
            dt.Columns.Add("KPTeam");
            // project type not included in sheet - will manually set
            dt.Columns.Add("ProjectType");

        }

        /// <summary>
        /// Given a cell name, parses the specified cell to get the column name.
        /// </summary>
        /// <param name="cellReference">Address of the cell (ie. B2)</param>
        /// <returns>Column Name (ie. B)</returns>
        private string GetColumnName(string cellReference)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);

            return match.Value;
        }

        /// <summary>
        /// Given just the column name (no row index), it will return the zero based column index.
        /// Note: This method will only handle columns with a length of up to two (ie. A to Z and AA to ZZ). 
        /// A length of three can be implemented when needed.
        /// </summary>
        /// <param name="columnName">Column Name (ie. A or AB)</param>
        /// <returns>Zero based index if the conversion was successful; otherwise null</returns>
        private int? GetColumnIndexFromName(string columnName)
        {
            int? columnIndex = null;

            string[] colLetters = Regex.Split(columnName, "([A-Z]+)");
            colLetters = colLetters.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            if (colLetters.Count() <= 2)
            {
                int index = 0;
                foreach (string col in colLetters)
                {
                    List<char> col1 = colLetters.ElementAt(index).ToCharArray().ToList();
                    int? indexValue = Letters.IndexOf(col1.ElementAt(index));

                    if (indexValue != -1)
                    {
                        // The first letter of a two digit column needs some extra calculations
                        if (index == 0 && colLetters.Count() == 2)
                        {
                            columnIndex = columnIndex == null ? (indexValue + 1) * 26 : columnIndex + ((indexValue + 1) * 26);
                        }
                        else
                        {
                            columnIndex = columnIndex == null ? indexValue : columnIndex + indexValue;
                        }
                    }

                    index++;
                }
            }

            return columnIndex;
        }

        /// <summary>
        /// Get the value from the worksheet cell
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="stringTablePart"></param>
        /// <returns></returns>
        private string GetValue(Cell cell, SharedStringTablePart stringTablePart)
        {
            string value = string.Empty;

            if (cell.ChildElements.Count == 0)
                return null;

            // raw sheets can have null DataTypes for values?
            if (cell.DataType == null && cell.CellValue != null)
                value = cell.CellValue.Text.Trim();

            // look up real value from shared string table 
            if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                value = cell.CellValue.InnerText;
                value = stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }

            // exported sheets don't use shared strings - they're InlineString
            if ((cell.DataType != null) && cell.DataType == CellValues.InlineString)
                value = cell.FirstChild.InnerText.Trim();

            // exported sheet saves values with specific types
            if ((cell.DataType != null) && cell.DataType == CellValues.Number)
                value = cell.CellValue.Text.Trim();

            return value;
        }
    }
}
