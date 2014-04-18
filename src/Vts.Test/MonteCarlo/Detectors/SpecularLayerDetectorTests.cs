using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.PhaseFunctionInputs;
using Vts.MonteCarlo.Tissues;

namespace Vts.Test.MonteCarlo.Detectors
{
    /// <summary>
    /// These tests execute an MC simulation with 100 photons and verify
    /// that all photons tallie to specular
    /// </summary>
    [TestFixture]
    public class SpecularLayerDetectorTests
    {
        private SimulationOutput _output;
        private double _specularReflectance;

        /// <summary>
        /// Setup input to the MC, SimulationInput, and execute MC
        /// </summary>
        [TestFixtureSetUp]
        public void execute_Monte_Carlo()
        {
            MultiLayerTissueInput ti = new MultiLayerTissueInput(
                     new ITissueRegion[]
                    { 
                        new LayerRegion(
                            new DoubleRange(double.NegativeInfinity, 0.0),
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0),
                        "HenyeyGreensteinKey1"),
                        new LayerRegion(
                            new DoubleRange(0.0, 20.0),
                            new OpticalProperties(0.01, 1.0, 0.8, 1.4),
                        "HenyeyGreensteinKey2"),
                        new LayerRegion(
                            new DoubleRange(20.0, double.PositiveInfinity),
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0),
                        "HenyeyGreensteinKey3")
                    }
                 );
            ti.RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey1", new HenyeyGreensteinPhaseFunctionInput());
            ti.RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey2", new HenyeyGreensteinPhaseFunctionInput());
            ti.RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey3", new HenyeyGreensteinPhaseFunctionInput());
            var input = new SimulationInput(
                 100,
                 "Output",
                 new SimulationOptions(
                     0,
                     RandomNumberGeneratorType.MersenneTwister,
                     AbsorptionWeightingType.Analog,
                //PhaseFunctionType.HenyeyGreenstein,
                     new List<DatabaseType>() { }, // databases to be written
                     true, // tally SecondMoment
                     false, // track statistics
                     0.0, // RR threshold -> 0 = no RR performed
                     0),
                 new DirectionalPointSourceInput(
                     new Position(0.0, 0.0, 0.0),
                     new Direction(0.0, 0.0, 1.0),
                     0 // start in air
                 ),
                 ti,
                new List<IDetectorInput>
                {
                    new RSpecularDetectorInput(), 
                }
            );

            _specularReflectance = Optics.Specular(input.TissueInput.Regions[0].RegionOP.N,
               input.TissueInput.Regions[1].RegionOP.N);
            _output = new MonteCarloSimulation(input).Run();
        }

        // Specular Reflectance
        [Test]
        public void validate_RSpecular()
        {
            Assert.Less(Math.Abs(_output.Rspec - _specularReflectance), 0.003);
        }
    }
}
