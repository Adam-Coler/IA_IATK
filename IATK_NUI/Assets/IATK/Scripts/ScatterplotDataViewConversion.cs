using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATK
{
    /// <summary>
    /// Provides functionality for transforming and apply aggregations to data in a scatterplot.
    /// </summary>
    public class ScatterplotDataViewConversion : DataViewConversion
    {
        /// <summary>
        /// The X dimension of this data view conversion.
        /// </summary>
        public DataDimension XDimension { get; set; }
        /// <summary>
        /// The Y dimension of this data view conversion.
        /// </summary>
        public DataDimension YDimension { get; set; }
        /// <summary>
        /// The Z dimension of this data view conversion.
        /// </summary>
        public DataDimension ZDimension { get; set; }
        
        /// <summary>
        /// The data transformation to apply to the X dimension.
        /// </summary>
        public DataTransformation XDimensionTransformation = DataTransformation.Identity;
        /// <summary>
        /// The data transformation to apply to the Y dimension.
        /// </summary>
        public DataTransformation YDimensionTransformation = DataTransformation.Identity;
        /// <summary>
        /// The data transformation to apply to the Z dimension.
        /// </summary>
        public DataTransformation ZDimensionTransformation = DataTransformation.Identity;
        
        /// <summary>
        /// The dimension to group (aggregate) points together. Note that this is the only type of statistical transformation available to scatterplots as they don't otherwise work.
        /// </summary>
        public DataDimension AggregationDimension { get; set; }
        
        /// <summary>
        /// The type of aggregation to apply. Does not do anything if AggregationDimension is set to null.
        /// </summary>
        public Aggregation Aggregation { get; set; }
        
        #region Public functions
        
        /// <summary>
        /// Calculates the normalised positions (within the range 0 to 1) of the data points to be used by the scatterplot View.
        /// The number of returned positions may vary based on given aggregations.
        /// </summary>
        /// <returns>A Vector3 array of normalised positions.</returns>
        public override Vector3[] GetViewPositions()
        {
            // If there is no aggregation to apply
            if (AggregationDimension == null)
            {
                // Get the number of points in the original data. If all dimensions are not defined, return a 0 length array.
                int length = 0;
                if (XDimension != null)
                    length = XDimension.Length;
                else if (YDimension != null)
                    length = YDimension.Length;
                else if (ZDimension != null)
                    length = ZDimension.Length;
                else
                    return new Vector3[0];
                                
                // Get the positions of each dimension with the appropriate transformation applied
                float[] xPositions = XDimension != null ? XDimension.GetNumericData(XDimensionTransformation) : new float[length];
                float[] yPositions = YDimension != null ? YDimension.GetNumericData(YDimensionTransformation) : new float[length];
                float[] zPositions = ZDimension != null ? ZDimension.GetNumericData(ZDimensionTransformation) : new float[length];
               
                // Normalise these positions within the range 0 to 1
                xPositions = NormaliseValues(xPositions);
                yPositions = NormaliseValues(yPositions);
                zPositions = NormaliseValues(zPositions);
               
                 // Combine the three arrays together into a Vector3 array                
                Vector3[] result = new Vector3[length];
                for (int i = 0; i < length; i++)
                    result[i] = new Vector3(xPositions[i], yPositions[i], zPositions[i]);
                
                return result;
            }
            else
            {
                // Make sure that the aggregating dimension is categorical. The simplest case is that this is anything except for floats (disregarding binning)
                if (AggregationDimension.DataType == DataType.Float)
                {
                    Debug.Log(string.Format("IATK: Cannot aggregate by {0} on scatterplot as it is a continuous dimension.", AggregationDimension.Identifier));
                    return new Vector3[0];
                }
                
                // Create required input arrays
                List<DataDimension> dimensions = new List<DataDimension>();
                List<DataTransformation> transformations = new List<DataTransformation>();
                if (XDimension != null)
                {
                    dimensions.Add(XDimension);
                    transformations.Add(XDimensionTransformation);
                }
                if (YDimension != null)
                {
                    dimensions.Add(YDimension);
                    transformations.Add(YDimensionTransformation);
                }
                if (ZDimension != null)
                {
                    dimensions.Add(ZDimension);
                    transformations.Add(ZDimensionTransformation);
                }
                
                // If there are no actual dimensions, we can just return an empty Vector3 array here without needing to do any calculations
                if (dimensions.Count == 0)
                    return new Vector3[0];
                
                // Apply the aggregation
                AggregationTransformation aggTransform = ApplyAggregation(new DataDimension[] { AggregationDimension }, Aggregation, dimensions.ToArray(), transformations.ToArray());
                
                // Retrieve and normalise the dimensions
                int length = aggTransform.Length;
                float[] xPositions = (aggTransform.AggregatedValues.ContainsKey(XDimension)) ? NormaliseValues(aggTransform.AggregatedValues[XDimension], XDimension.DataType) : new float[length];
                float[] yPositions = (aggTransform.AggregatedValues.ContainsKey(YDimension)) ? NormaliseValues(aggTransform.AggregatedValues[YDimension], YDimension.DataType) : new float[length];
                float[] zPositions = (aggTransform.AggregatedValues.ContainsKey(ZDimension)) ? NormaliseValues(aggTransform.AggregatedValues[ZDimension], ZDimension.DataType) : new float[length];
                
                
                // Combine the three arrays together into a Vector3 array                
                Vector3[] result = new Vector3[length];
                for (int i = 0; i < length; i++)
                    result[i] = new Vector3(xPositions[i], yPositions[i], zPositions[i]);
                
                return result;
            }
        }
        
        
        // public bool IsTransformationValid(Aggregation transformation)
        // {
        //     if (transformation == Aggregation.Sort)
        //     {
        //         return true;
        //     }
            
        //     Debug.LogError(string.Format("IATK: Scatterplots do not support data transformation {0}", transformation.ToString()));
        //     return false;
        // }
        
        
        #endregion //Public functions
    }    
}
