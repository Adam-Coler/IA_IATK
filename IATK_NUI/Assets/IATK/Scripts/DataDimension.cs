using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IATK
{
    /// <summary>
    /// Transformations which apply to a single dimension in the data.
    /// </summary>
    public enum DataTransformation
    {
        Identity,
        Square,
        Cube,
        Sqrt,
        LogTen,
        LogNatural,
        Reciprocal,
        OneMinus
    }
    
    /// <summary>
    /// Describes a single dimension in a DataSource without any normalisation.
    /// </summary>
    public class DataDimension
    {
        #region Public properties
        
        /// <summary>
        /// The DataSource object which this dimension belongs to.
        /// </summary>
        public TabularDataSource ParentDataSource { get; private set; }
        /// <summary>
        /// The textual identifier for this dimension.
        /// </summary>
        public string Identifier { get; private set; }
        /// <summary>
        /// The integer identifier for this dimension, relative to the DataSource.
        /// </summary>
        public int Index { get; private set; }
        /// <summary>
        /// The raw data of this dimension as found in the original dataset.
        /// </summary>
        public string[] RawData { get; private set; }
        /// <summary>
        /// The inferred type of this dimension (e.g., float, date, string).
        /// </summary>
        public DataType DataType { get; private set; }
        /// <summary>
        /// The data of this dimension in a normalised format (0 to 1) for cached access.
        /// </summary>
        public float[] NormalisedData { get; private set; }
        
        /// <summary>
        /// The minimum value of this dimension, as a string.
        /// </summary>
        public string MinValue { get; private set; }
        /// <summary>
        /// The maximum value of this dimension, as a string.
        /// </summary>
        public string MaxValue { get; private set; }
        
        
        /// <summary>
        /// The number of values in this dimension.
        /// </summary>
        public int Length { get { return RawData.Length; }}
        
        
        #endregion //Public properties
        
        
        #region Private variables
        
        private const int ROWS_TO_CONFIRM_DATA_TYPE = 25;
        
        #endregion //Private variables
        
        /// <summary>
        /// Initialises a new DataDimension.
        /// </summary>
        /// <param name="parentDataSource">The DataSource object which this dimension belongs to.</param>
        /// <param name="identifier">The textual identifier for this dimension.</param>
        /// <param name="index">The integer identifier for this dimension, relative to the DataSource.</param>
        /// <param name="rawData">The raw data of this dimension as found in the original dataset.</param>
        /// <param name="type">The inferred type of this dimension (e.g., float, date, string).</param>
        public DataDimension(TabularDataSource parentDataSource, string identifier, int index, string[] rawData)
        {
            ParentDataSource = parentDataSource;
            Identifier = identifier;
            Index = index;
            RawData = rawData;
            
            InferDataType();
            NormaliseData();
        }
        
                        
        /// <summary>
        /// Applies a data transformation (e.g., x square, reciprocal, square root) to a this dimension's data. Note that this is only allowed when the data type is numeric.
        /// </summary>
        /// <param name="dimension">The data dimension to apply the transformation onto.</param>
        /// <param name="dataTransformation">The type of transformation to apply. Defaults to identity (i.e., no transformation).</param>
        /// <returns>An array of floats with the transformation applied to it.</returns>
        public float[] GetNumericData(DataTransformation dataTransformation = DataTransformation.Identity)
        {
            if (DataType != DataType.Float || DataType != DataType.Int)
            {
                Debug.LogError("IATK: Data transformation can only be applied to dimensions with numeric types.");
                return null;
            }
            
            float[] values = new float[Length];
            
            // TODO: Right now this function has to convert strings into floats. Change this to pre-process this parsing if it is too performance intensive
            switch (dataTransformation)
            {
                case DataTransformation.Identity:
                    for (int i = 0; i < Length; i++)
                    {
                        values[i] = float.Parse(RawData[i]);
                    }
                    break;
                
                case DataTransformation.Square:
                    for (int i = 0; i < Length; i++)
                    {
                        values[i] = Mathf.Pow(float.Parse(RawData[i]), 2);
                    }
                    break;
                    
                case DataTransformation.Cube:
                    for (int i = 0; i < Length; i++)
                    {
                        values[i] = Mathf.Pow(float.Parse(RawData[i]), 3);
                    }
                    break;
                    
                case DataTransformation.Sqrt:
                    for (int i = 0; i < Length; i++)
                    {
                        values[i] = Mathf.Sqrt(float.Parse(RawData[i]));
                    }
                    break;
                    
                case DataTransformation.LogTen:
                    for (int i = 0; i < Length; i++)
                    {
                        values[i] = Mathf.Log10(float.Parse(RawData[i]));
                    }
                    break;
                    
                case DataTransformation.LogNatural:
                    for (int i = 0; i < Length; i++)
                    {
                        values[i] = Mathf.Log(float.Parse(RawData[i]));
                    }
                    break;
                    
                case DataTransformation.Reciprocal:
                    for (int i = 0; i < Length; i++)
                    {
                        values[i] = 1f / float.Parse(RawData[i]);
                    }
                    break;
                    
                case DataTransformation.OneMinus:
                    for (int i = 0; i < Length; i++)
                    {
                        values[i] = 1f - float.Parse(RawData[i]);
                    }
                    break;
            }
            
            return values;            
        }
        
        private void NormaliseData()
        {
            /*
            if (Type == DataType.Undefined)
            {
                Debug.LogError(string.Format("IATK: Cannot normalise dimension with identifier {0} as its inferred data type is undefined.", Identifier));
            }
            
            NormalisedData = new float[Length];
            
            switch (Type)
            {
                case DataType.Bool:
                {
                    bool 
                }
                
                
                case DataType.Date:
                
                
                
                case DataType.Time:
                
                
                case DataType.Int:
                
                
                case DataType.Float:
                
                
                case DataType.String:
                
                
                default:
                
            }
            */
        }
        
        /// <summary>
        /// Infers the DataType of this dimension using a very simple heuristic.
        /// </summary>
        private void InferDataType()
        {
            DataType result = DataType.Undefined;
            
            int floatCount = 0;
            int intCount = 0;
            int boolCount = 0;
            
            for (int i = 0; i < Length; i++)
            {
                DataType type = DataTypeExtension.InferTypeFromString(RawData[i]);
                
                // If the first type checked is a date, time, or string, we can be reasonably confident in its guess
                if (type == DataType.Date ||
                    type == DataType.Time ||
                    type == DataType.String)
                {
                    DataType = type;
                    break;
                }
                // Otherwise, keep incrementing the other data types until one of the passes a set threshold
                else if (type == DataType.Float && floatCount++ < ROWS_TO_CONFIRM_DATA_TYPE)
                {
                    DataType = type;
                    break;
                }
                else if (type == DataType.Int && intCount++ < ROWS_TO_CONFIRM_DATA_TYPE)
                {
                    DataType = type;
                    break;
                }
                else if (type == DataType.Bool && boolCount++ < ROWS_TO_CONFIRM_DATA_TYPE)
                {
                    DataType = type;
                    break;
                }
                
            }
            
            DataType = result;
        }        
    }
}
