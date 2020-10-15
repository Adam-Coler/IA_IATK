using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATK
{
    /// <summary>
    /// Describes a tabular dataset.
    /// </summary>
    [ExecuteInEditMode]
    public class TabularDataSource : MonoBehaviour, IEnumerable<DataDimension>
    {
        #region Unity public variables
        
        [Tooltip("TextAsset containing the tabular data to be used.")]
        public TextAsset Data;
        [Tooltip("Set to true if quotation marks are used to denote strings.")]
        public bool QuotationMarksAsStrings = true;
        [Tooltip("Set a custom separator if required. Leave blank to use default separators (, \\t ; |).")]
        public string CustomSeparator;
        
        #endregion //Unity public variables
        
        
        #region Private variables     
           
        private List<DataDimension> dataDimensions = new List<DataDimension>();
        private bool hasDataLoaded = false;
        private char[] separator = new char[] { ',', '\t', ';', '|' };
        
        #endregion //Private variables


        #region Public properties
        
        /// <summary>
        /// The number of dimensions that this dataset has.
        /// </summary>
        public int DimensionCount
        {
            get { return dataDimensions.Count; }
        }

        /// <summary>
        /// Returns an object that describes the data dimension of the given index.
        /// </summary>
        /// <param name="index">The index of the dimension.</value>
        public DataDimension this[int index]
        {
            get
            { 
                try { return dataDimensions[index]; }
                catch (IndexOutOfRangeException e)
                {
                    Debug.LogError(string.Format("IATK: The requested dimension at the index {0} could not be found.", index));
                    return null;
                }
            }
        }
        
        /// <summary>
        /// Returns an object that describes the data dimension with the given string identifier (header).
        /// </summary>
        /// <param name="identifier">The identifier of the dimension.</value>
        public DataDimension this[string identifier]
        {
            get 
            {  
                foreach (DataDimension d in dataDimensions)
                {
                    if (d.Identifier == identifier)
                        return d;
                }
                
                Debug.LogError(string.Format("IATK: The requested dimension with the idenfier {0} could not be found.", identifier));
                return null;
            }
        }

        public IEnumerator<DataDimension> GetEnumerator()
        {
            for (int i = 0; i < DimensionCount; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < DimensionCount; i++)
            {
                yield return this[i];
            }
        }
        
        #endregion //Public properties
        
        
        #region Unity monobehaviour functions
        
        private void Awake()
        {
            if (!hasDataLoaded && Data != null)
                LoadData();
        }
        
        
        #endregion //Unity monobehaviour functions
        
        #region Public functions
        
        /// <summary>
        /// Loads the dataset as provided in the Data variable.
        /// </summary>
        /// <returns>Returns true if the load succeeded, otherwise returns false.</returns>
        public bool LoadData()
        {
            if (Data == null)
            {
                Debug.LogError("IATK: Could not load the dataset as no TextAsset was provided.");
                return false;
            }
                       
            
            #if UNITY_EDITOR
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            #endif
            
            // Convert the data text file into UTF8
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Data.text);
            string data = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            
            dataDimensions = new List<DataDimension>();
            
            string[] lines = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] headerNames = (CustomSeparator == "") ? lines[0].Split(separator) : lines[0].Split(new [] { CustomSeparator }, StringSplitOptions.None);
            
            // Initialise a 2D array, creating n 1D arrays of m length (n = number of dimensions, m = number of rows)
            string[][] data2DArray = new string[headerNames.Length][];
            for (int i = 0; i < headerNames.Length; i++)
                data2DArray[i] = new string[lines.Length];
                
            // Populate the 2D array to the original data set, except with rows and columns swapped
            if (lines.Length > 1)
            {
                // Reading lines
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] rowValues = (CustomSeparator == "") ? lines[i].Split(separator) : lines[i].Split(new [] { CustomSeparator }, StringSplitOptions.None);
                    // Reading columns (cell by cell)
                    for (int k = 0; k < rowValues.Length; k++)
                    {
                        // Clean the value first, then populate by column index first, row second
                        string value = CleanDataString(rowValues[k]);
                        data2DArray[k][i-1] = value;
                    }
                }
            }
            
            // Initialise data dimensions, which will handle data type checking and normalisation
            for (int i = 0; i  < headerNames.Length; i++)
            {
                DataDimension dimension = new DataDimension(this, headerNames[i], i, data2DArray[i]);
                dataDimensions.Add(dimension);
            }
            
            #if UNITY_EDITOR
            stopwatch.Stop();
            Debug.Log(string.Format("IATK: Dataset {0} loaded in {1} milliseconds.", Data.name, stopwatch.Elapsed.Milliseconds));
            #endif
            
            return true;
        }
        
        #endregion //Public methods
        
        
        #region Private functions
        
        /// <summary>
        /// Cleans a given string value to ensure proper readability and usability.
        /// </summary>
        /// <param name="value">The string to clean.</param>
        /// <returns>A cleaned string.</returns>
        private string CleanDataString(string value)
        {
            if (QuotationMarksAsStrings && value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 1);
            
            return value.Replace("\r", string.Empty);
        }
        
        #endregion //Private functions
    }
}
