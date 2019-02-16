using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Qazi.Common;
namespace Qazi.DataPreprocessing
{
    public class DataTableEncodingManager
    {
        List<string> _ListOfAttributes;
        DataTable _DataTableToEncode;
        public DataTable _EncodedDataTable;
        Dictionary<string, string> _EncodingDictionary;
        bool _AddBinaryStringAttribute;
        bool _UseBinarySplitMode;
        int sizeOfCodeString = 0;

        public event WorkerProgressUpdateEventHandler EncodingProgress;
        public event WorkerCompletedEventHandler EncodingCompleted;

        private WorkerProgressEventArg ProgressArgs;
        private WorkerCompletedEventArg CompletedArgs;
        
        

        public DataTableEncodingManager(Dictionary<string, string> encodingDictionary, List<string> listOfAttributes, DataTable datatableToEncode, bool addSparseEncodedBinaryStringattribute, bool binarySplitMode)
        {
            _ListOfAttributes = listOfAttributes;
            _DataTableToEncode = datatableToEncode;
            _EncodedDataTable = _DataTableToEncode.Clone();
            _EncodingDictionary = encodingDictionary;
            _AddBinaryStringAttribute = addSparseEncodedBinaryStringattribute;
            _UseBinarySplitMode = binarySplitMode;
            
        }

        public void Run()
        {
            ProgressArgs = new WorkerProgressEventArg();
            int ctr;

            if (_UseBinarySplitMode == true)
            {
                sizeOfCodeString = 0;
                
                foreach (string key in _EncodingDictionary.Keys)
                {
                    if (sizeOfCodeString <= _EncodingDictionary[key].Length)
                    {
                        sizeOfCodeString = _EncodingDictionary[key].Length;
                    }
                }

                
                _EncodedDataTable = new DataTable("Encoded");
                string columnName;
                foreach (DataColumn col in _DataTableToEncode.Columns)
                {
                    columnName = col.ColumnName;
                    if (_ListOfAttributes.Contains(columnName) == false)
                        _EncodedDataTable.Columns.Add(columnName);
                    else
                    {
                        for (ctr = 1; ctr <= sizeOfCodeString; ctr++)
                        {
                            _EncodedDataTable.Columns.Add(columnName + "_" + ctr.ToString());
                        }
                    }
                }
            }
            else
                _EncodedDataTable = _DataTableToEncode.Clone();
            if (_AddBinaryStringAttribute == true)
            {
                _EncodedDataTable.Columns.Add("EncodedString");
                _EncodedDataTable.Columns.Add("EncodedStringLength");
            }

            int attributeIndex;
            string attribute;
            int rowIndex;
            int totalRows = _DataTableToEncode.Rows.Count;
            int totalAttributes = _DataTableToEncode.Columns.Count;
            float progress;
            DataRow row;
            DataRow encodedRow;
            string item;
            string encodeditem;
            string encodedString = "";
            char []encodingStringArray;
            //int ctr;            

            for (rowIndex = 0; rowIndex < totalRows; rowIndex++)
            {
                row = _DataTableToEncode.Rows[rowIndex];
                encodedRow = _EncodedDataTable.NewRow();
                for (attributeIndex = 0; attributeIndex < totalAttributes; attributeIndex++)
                {
                    attribute = _DataTableToEncode.Columns[attributeIndex].ColumnName;
                    item = row[attribute].ToString();
                    if (_ListOfAttributes.Contains(attribute) == true)
                    {
                        encodeditem = _EncodingDictionary[item];
                        if (_UseBinarySplitMode == true)
                        {
                            encodingStringArray = encodeditem.ToCharArray();
                            for (ctr = 1; ctr <= sizeOfCodeString; ctr++)
                            {
                                encodedRow[attribute + "_" + ctr.ToString()] = encodingStringArray[ctr - 1].ToString();
                            }
                        }
                        else
                        {
                            encodedRow[attribute] = encodeditem;
                        }

                        if (_AddBinaryStringAttribute == true)
                        {
                            encodedString = encodedString + encodeditem;
                        }
                    }
                    else
                    {
                        encodedRow[attribute] = item;
                    }
                }
                if (_AddBinaryStringAttribute == true)
                {
                    encodedRow["EncodedString"] = encodedString;
                    encodedRow["EncodedStringLength"] = encodedString.Length;
                    encodedString = "";
                }

                _EncodedDataTable.Rows.Add(encodedRow);
                progress = (((float)(rowIndex+1))/((float)(totalRows)) * 100);
                if (EncodingProgress != null)
                {
                    ProgressArgs.ProgressPercentage = progress;
                    ProgressArgs.UserState = "";
                    EncodingProgress(this,ProgressArgs);
                }
            }

            if (EncodingCompleted != null)
            {
                CompletedArgs = new WorkerCompletedEventArg();
                CompletedArgs.Result = _EncodedDataTable;
                CompletedArgs.UserStateMessage = "Finished";
                EncodingCompleted(this,CompletedArgs);
            }
        }
    }
}
