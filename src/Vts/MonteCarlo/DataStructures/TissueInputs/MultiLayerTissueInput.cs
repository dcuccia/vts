using System.Collections.Generic;
using System.Runtime.Serialization;
using Vts.Common;
using Vts.MonteCarlo.PhaseFunctionInputs;
using Vts.MonteCarlo.Tissues;

namespace Vts.MonteCarlo
{
    /// <summary>
    /// Implements ITissueInput.  Defines input to MultiLayerTissue class.
    /// </summary>
    [KnownType(typeof(LayerRegion))]
    [KnownType(typeof(OpticalProperties))]
    [KnownType(typeof(HenyeyGreensteinPhaseFunctionInput))]
    [KnownType(typeof(LookupTablePhaseFunctionInput))]
    [KnownType(typeof(BidirectionalPhaseFunctionInput))]
    public class MultiLayerTissueInput : ITissueInput
    {
        public IDictionary<string, IPhaseFunctionInput> RegionPhaseFunctionInputs { get; set; }
        private ITissueRegion[] _regions;

        /// <summary>
        /// constructor for Multi-layer tissue input
        /// </summary>
        /// <param name="regions">list of tissue regions comprising tissue</param>
        public MultiLayerTissueInput(
            ITissueRegion[] regions)
        {
            TissueType = TissueType.MultiLayer;
            _regions = regions;
            RegionPhaseFunctionInputs = new Dictionary<string, IPhaseFunctionInput>();
        }

        /// <summary>
        /// MultiLayerTissue default constructor provides homogeneous tissue
        /// </summary>
        public MultiLayerTissueInput()
            : this(
                new ITissueRegion[]
                { 
                    new LayerRegion(
                        new DoubleRange(double.NegativeInfinity, 0.0),
                        new OpticalProperties( 0.0, 1e-10, 1.0, 1.0),
                        "HenyeyGreensteinKey1"),
                    new LayerRegion(
                        new DoubleRange(0.0, 100.0),
                        new OpticalProperties(0.0, 1.0, 0.8, 1.4),
                        "HenyeyGreensteinKey2"),
                    new LayerRegion(
                        new DoubleRange(100.0, double.PositiveInfinity),
                        new OpticalProperties(0.0, 1e-10, 1.0, 1.0),
                        "HenyeyGreensteinKey3")
                })
        {
            RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey1", new HenyeyGreensteinPhaseFunctionInput());
            RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey2", new HenyeyGreensteinPhaseFunctionInput());
            RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey3", new HenyeyGreensteinPhaseFunctionInput());
        }

        /// <summary>
        /// tissue identifier
        /// </summary>
        public TissueType TissueType { get; set; }
        /// <summary>
        /// list of tissue regions comprising tissue
        /// </summary>
        public ITissueRegion[] Regions { get { return _regions; } set { _regions = value; } }
        
    }
}
