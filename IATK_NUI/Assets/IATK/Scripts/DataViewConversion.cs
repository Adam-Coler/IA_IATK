using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IATK
{    
    /// <summary>
    /// Transformations which aggregate the data and apply to the overall visualisation. This either needs to be accompanied by an identifying dimension to aggregate against, or be
    /// pre-defined by the visualisation itself (e.g., categorical values on x-axis for bar charts).
    /// </summary>
    public enum Aggregation
    {
        None,
        Sum,
        Count,
        DistinctCount,
        Average,
        Median,
        Min,
        Max
    }
    
    public enum Dimension
    {
        X,
        Y,
        Z
    }
        
    /// <summary>
    /// Base class for handling conversion from the data to the view (visual representation).
    /// </summary>
    public abstract class DataViewConversion : MonoBehaviour
    {
        /// <summary>
        /// A struct specifically for the data view conversion to return multiple objects at once.
        /// </summary>
        protected struct AggregationTransformation
        {
            /// <summary>
            /// A dictionary where each key is a data dimension, and the corresponding value is the processed data as a string (in order to ensure compatibility).
            /// </summary>
            public Dictionary<DataDimension, string[]> AggregatedValues;
            /// <summary>
            /// A dictionary mapping aggregated indices with the original raw indices in the dataset.
            /// </summary>
            public Dictionary<int, int[]> AggregatedToRawIndices;
            /// <summary>
            /// A dictionary mapping original raw indices in the dataset with aggregated indices.
            /// </summary>
            public Dictionary<int, int> RawToAggregatedIndices;
            /// <summary>
            /// The number of rows in the newly aggregated data.
            /// </summary>
            public int Length;
        }

        public abstract Vector3[] GetViewPositions();
                
        /// <summary>
        /// Applies an aggregation to a set of dimensions based on an aggregating dimension.
        /// </summary>
        /// <param name="aggregatingDimension">The data dimensions which the aggregations are based on. The dimensions must not have continuous values.</param>
        /// <param name="aggregation">The type of aggregation to apply.</param>
        /// <param name="dataDimensions">The dimensions of the visualisation which the aggregation is being applied to, as an array.</param>
        /// <param name="dataTransformations">The data transformations that each dimension defined in dataDimensions has, as an array. This must be the same length and in the same order.</param>
        /// <returns>A struct containing multiple values associated with aggregation.</returns>
        protected AggregationTransformation ApplyAggregation(DataDimension[] aggregatingDimensions, Aggregation aggregation, DataDimension[] dataDimensions, DataTransformation[] dataTransformations)
        {
            // Error checking
            if (aggregation == Aggregation.None)
            {
                Debug.LogError("IATK: Cannot apply a statistcal transformation of type \"None\".");
                return new AggregationTransformation();
            }
            
            if (aggregatingDimensions.Length == 0 || dataDimensions.Length == 0)
            {
                Debug.LogError(string.Format("IATK: No data dimensions have been given to aggregate."));
                return new AggregationTransformation();
            }
            
            if (dataDimensions.Length != dataTransformations.Length)
            {
                Debug.LogError(string.Format("IATK: Aggregation requires the same number of data dimensions ({0} given) and data transformations ({1} given).", dataDimensions.Length, dataTransformations.Length));
                return new AggregationTransformation();
            }
            
            // Make sure that the aggregating dimension is not continuous. The simplest case is that this is anything except for floats (disregarding binning)
            foreach (var agg in aggregatingDimensions)
            {
                if (agg.DataType == DataType.Float)
                {
                    Debug.LogError(string.Format("IATK: Cannot aggregate by {0} as it is a continuous dimension.", agg.Identifier));
                    return new AggregationTransformation();
                }
            }
            
            // To ensure maximum compatibility, we sort the given dimensions here in order to prevent inconsistent aggregation group numbers that are based on the order of the given arrays
            aggregatingDimensions = aggregatingDimensions.OrderBy(x => x.Identifier).ToArray();
            var zip = dataDimensions.Zip(dataTransformations, (x, y) => new { x, y } )
                .OrderBy(pair => pair.x.Identifier)
                .ToList();
            dataDimensions = zip.Select(pair => pair.x).ToArray();
            dataTransformations = zip.Select(pair => pair.y).ToArray();            
            
            // Create groups for each combinations of aggregating values 
            List<List<string>> uniqueCategories = new List<List<string>>();
            foreach (var agg in aggregatingDimensions)
            {
                uniqueCategories.Add(agg.RawData.Distinct().ToList());
            }
            List<List<string>> aggregationGroups = GetCombinations(uniqueCategories, new List<string>()).ToList();
            
            // Go through each row in the dataset and assign an index to mapping dictionaries based on its aggregation group
            Dictionary<int, List<int>> aggToRaw = new Dictionary<int, List<int>>();
            Dictionary<int, int> rawToAgg = new Dictionary<int, int>();
            for (int i = 0; i < aggregatingDimensions[0].Length; i++)
            {
                // Check to see what group the row is in
                int groupIndex = GetAggregationGroupIndexOfRow(aggregatingDimensions, aggregationGroups, i);
                
                // Store the mapping
                if (!aggToRaw.ContainsKey(groupIndex))
                    aggToRaw.Add(groupIndex, new List<int>());
                aggToRaw[groupIndex].Add(i);
                
                rawToAgg.Add(i, groupIndex);
            }
            
            // Now we can finally aggregate values together based on the groupings
            // Do this for each of the given dimensions (can be done independent from one another)
            Dictionary<DataDimension, string[]> aggValues = new Dictionary<DataDimension, string[]>();
            for (int i = 0; i < dataDimensions.Length; i++)
            {
                DataDimension dimension = dataDimensions[i];
                DataTransformation transformation = dataTransformations[i];
                
                aggValues[dimension] = new string[dimension.Length];
                float[] dimensionValues = dimension.GetNumericData(transformation);
                
                // The outer array refers to an aggregation group, the inner lists are the float values of rows associated with that group
                List<float>[] groupedValues = new List<float>[aggregationGroups.Count];
                
                // Assign raw values to their respective lists
                for (int j = 0; j < dimension.Length; j++)
                {
                    groupedValues[rawToAgg[j]].Add(dimensionValues[j]);
                }
                
                // Now we aggregate the actual values based on the given statistical transformation
                string[] aggregatedValues = new string[aggregationGroups.Count];
                for (int j = 0; j < aggregationGroups.Count; j++)
                {
                    float value = AggregateValues(groupedValues[j], aggregation);
                    aggregatedValues[j] = value.ToString();
                }
                
                // Assign these aggregated values back to the full length list of values (one element per row)
                for (int j = 0; j < dimension.Length; j++)
                {
                    aggValues[dimension][j] = aggregatedValues[rawToAgg[j]];
                }
            }
            
            // Initialise and set values of return struct
            Dictionary<int, int[]> aggToRaw2 = new Dictionary<int, int[]>();
            foreach (int key in aggToRaw.Keys)
            {
                aggToRaw2.Add(key, aggToRaw[key].ToArray());
            }
            
            AggregationTransformation retVal = new AggregationTransformation();
            retVal.AggregatedValues = aggValues;
            retVal.AggregatedToRawIndices = aggToRaw2;
            retVal.RawToAggregatedIndices = rawToAgg;
            retVal.Length = aggValues.Values.First().Length;
            
            return retVal;
        }
        
        /// <summary>
        /// Recursive function to generate all combinations from a list of lists.
        /// </summary>
        /// <param name="lists">A list of list. The outer list is each set, the inner list are unique values.</param>
        /// <param name="selected">Recursive parameter. Set as empty list if not a recursive call.</param>
        /// <returns></returns>
        protected IEnumerable<List<string>> GetCombinations(IEnumerable<List<string>> lists, IEnumerable<string> selected)
        {
            if (lists.Any())
            {
                var remainingLists = lists.Skip(1);
                foreach (var item in lists.First())
                    foreach (var combo in GetCombinations(remainingLists, selected.Concat(new string[] { item })))
                        yield return combo;
            }
            else
            {
                yield return selected.ToList();
            }
        }
        
        /// <summary>
        /// Calculates the aggregation group index that the given row (from the original data) belongs to.
        /// </summary>
        /// <param name="aggregatingDimensions">The data dimensions which the aggregations are based on.</param>
        /// <param name="aggregationGroups">The groups of categorical raw values which the aggregation groups are based on.</param>
        /// <param name="rowIndex">The row index from the raw data to determine the aggregation group index of.</param>
        /// <returns>The aggregation group index of the given row.</returns>
        protected int GetAggregationGroupIndexOfRow(DataDimension[] aggregatingDimensions, List<List<string>> aggregationGroups, int rowIndex)
        {
            // Get all of the values at this row in the aggregating dimensions
            List<string> values = new List<string>();            
            foreach (var agg in aggregatingDimensions)
            {
                values.Add(agg.RawData[rowIndex]);
            }
            
            // Determine which one of the aggregation groups this belongs to
            for (int i = 0; i < aggregationGroups.Count; i++)
            {
                if (CompareListEquality(aggregationGroups[i], values))
                    return i;
            }
            
            Debug.LogError("IATK: No aggregation group could be found for the given row. This should not normally happen, and if so, it is a bug!");            
            return 0;
        }
        
        /// <summary>
        /// Compares to lists to check if the strings in both of them are equal.
        /// </summary>
        /// <param name="list1">The first list to compare.</param>
        /// <param name="list2">The second list to compare.</param>
        /// <returns>True if both lists have the same strings, otherwise returns false.</returns>
        protected bool CompareListEquality(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                    return false;
            }

            return true;
        }
        
        /// <summary>
        /// Computes an aggregation of a list of values based on a given transformation.
        /// </summary>
        /// <param name="values">The list of values to aggregate.</param>
        /// <param name="statisticalTransformation">The type of transformation to apply.</param>
        /// <returns></returns>
        protected float AggregateValues(List<float> values, Aggregation statisticalTransformation)
        {
            switch (statisticalTransformation)
            {
                case Aggregation.Sum:
                    return values.Sum();
                    
                case Aggregation.Count:
                    return values.Count;
                
                case Aggregation.DistinctCount:
                    return values.Distinct().Count();
                
                case Aggregation.Average:
                    return values.Average();
                    
                case Aggregation.Median:
                    var sorted = values.OrderBy(x => x).ToList();
                    double mid = (sorted.Count - 1) / 2.0;
                    return (sorted[(int)(mid)] + sorted[(int)(mid + 0.5)]) / 2;
                
                case Aggregation.Min:
                    return values.Min();
                    
                case Aggregation.Max:
                    return values.Max();
                
                default:
                    Debug.LogError(string.Format("IATK: Statistical transformation of type {0} is either not supported or not yet implemented.", statisticalTransformation.ToString()));
                    return 0;
            }
        }
        
        /// <summary>
        /// Normalises a float array into a fixed range. Defaults to 0..1.
        /// </summary>
        /// <param name="values">The float array to normalise.</param>
        /// <param name="minNorm">The minimum normalisation range. Default is 0.</param>
        /// <param name="maxNorm">The maximum normalisation range. Default is 1.</param>
        /// <returns></returns>
        protected float[] NormaliseValues(float[] values, float minNorm = 0, float maxNorm = 0)
        {
            float minValue = values.Min();
            float maxValue = values.Max();
            
            return values.Select(x => NormaliseValue(x, minValue, maxValue, minNorm, maxNorm)).ToArray();
        }
        
        /// <summary>
        /// Normalises a string array into a fixed range. This also handles conversion of categorical values. Defaults to 0..1.
        /// </summary>
        /// <param name="values">The string array to normalise.</param>
        /// <param name="minNorm">The minimum normalisation range. Default is 0.</param>
        /// <param name="maxNorm">The maximum normalisation range. Default is 1.</param>
        /// <returns></returns>
        protected float[] NormaliseValues(string[] values, DataType dataType, float minNorm = 0, float maxNorm = 1)
        {
            int count = values.Length;
            float[] normalisedValues = new float[count];
            
            switch (dataType)
            {
                case DataType.Int:
                case DataType.Float:
                    float[] parsedValues = values.Select(x => float.Parse(x)).ToArray();
                    float minValue = parsedValues.Min();
                    float maxValue = parsedValues.Max();
                    
                    normalisedValues = parsedValues.Select(x => NormaliseValue(x, minValue, maxValue, minNorm, maxNorm)).ToArray();
                    break;
            }
            
            return normalisedValues;
        }
        
        protected float NormaliseValue(float value, float min, float max, float minNorm, float maxNorm)
        {
            var a = (maxNorm - minNorm) / (max - min);
            var b = max - a * max;
            return a * value + b;
        }
    }
}
