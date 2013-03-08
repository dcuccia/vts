using System;
using System.Linq;
using System.Collections.Generic;
using Vts.Common;
using Vts.MonteCarlo.DataStructuresValidation;
using Vts.MonteCarlo.Tissues;
using Vts.MonteCarlo.Extensions;

namespace Vts.MonteCarlo
{
    /// <summary>
    /// This sanity checks SimulationInput
    /// </summary>
    public class SimulationInputValidation
    {
        /// <summary>
        /// Master of call validation methods. Calls methods to validate source,
        /// tissue and detector definitions.
        /// </summary>
        /// <param name="input">SimulationInput to be validated</param>
        /// <returns>ValidationResult with IsValid bool set and message about error if false</returns>
        public static ValidationResult ValidateInput(SimulationInput input)
        {
            var validations = new Func<SimulationInput, ValidationResult>[]
                {
                    si => ValidateN(si.N),
                    si => ValidateSourceInput(si.SourceInput, si.TissueInput),
                    si => ValidateTissueInput(si.TissueInput),
                    si => ValidateDetectorInput(si),
                    si => ValidateCombinedInputParameters(si),
                    si => ValidateCurrentIncapabilities(si)
                };

            foreach (var validation in validations)
            {
                var tempResult = validation(input);
                if (!tempResult.IsValid)
                {
                    return tempResult;
                }
            }
            
            return new ValidationResult( true, "Simulation input is valid");

        }

        private static ValidationResult ValidateN(long N)
        {
            return new ValidationResult(
                N >= 10,
                "Number of photons must be greater than 9",
                "This is an implementation detail of the MC simulation");
        }

        private static ValidationResult ValidateSourceInput(ISourceInput sourceInput, ITissueInput tissueInput)
        {
            if ((sourceInput.InitialTissueRegionIndex < 0) ||
                (sourceInput.InitialTissueRegionIndex > tissueInput.Regions.Length - 1))
            {
                return new ValidationResult(
                    false,
                    "Source input not valid given tissue definition",
                    "Alter sourceInput.InitialTissueRegionIndex to be consistent with tissue definition");
            }
            else
            {
                return new ValidationResult(
                    true,
                    "Starting photons in region " + sourceInput.InitialTissueRegionIndex);
            }
        }

        private static ValidationResult ValidateTissueInput(ITissueInput tissueInput)
        {
            if (tissueInput is MultiLayerTissueInput)
            {
                return MultiLayerTissueInputValidation.ValidateInput(tissueInput);
            }
            if (tissueInput is SingleEllipsoidTissueInput)
            {
                return SingleEllipsoidTissueInputValidation.ValidateInput(tissueInput);
            }

            return new ValidationResult(
                true,
                "Tissue input must be valid",
                "Validation skipped for tissue input " + tissueInput + ". No matching validation rules were found.");
        }
        private static ValidationResult ValidateDetectorInput(SimulationInput si)
        {
            if (((si.Options.Databases == null) || (si.Options.Databases.Count() < 1)) && (si.DetectorInputs.Count() < 1))
            {
                return new ValidationResult(
                    false,
                    "No detector inputs specified and no database to be written",
                    "Make sure list of DetectorInputs is not empty or null if no databases are to be written");
            }
            // black list of unimplemented detectors
            foreach (var detectorInput in si.DetectorInputs)
            {
                if (detectorInput.TallyType.IsNotImplementedYet())
                {
                    return new ValidationResult(
                        false,
                        "DetectorInput not implemented yet:" + detectorInput.ToString(),
                        "Please omit " + detectorInput.ToString() + " from DetectorInput list");
                }
            }
            return new ValidationResult(
                true,
                "DetectorInput must be valid",
                "");
        }

        /// <summary>
        /// This method checks the input against combined combinations of options
        /// and source, tissue, detector definitions.   
        /// </summary>
        /// <param name="input">input to be validated</param>
        /// <returns>ValidationResult with IsValid set and error message if false</returns>
        private static ValidationResult ValidateCombinedInputParameters(SimulationInput input)
        {
            // check that absorption weighting type set to analog and RR weight threshold != 0.0
            if ((input.Options.AbsorptionWeightingType == AbsorptionWeightingType.Analog) &&
                input.Options.RussianRouletteWeightThreshold != 0.0)
            {
                return new ValidationResult(
                    false,
                    "Russian Roulette cannot be employed with Analog absorption weighting is specified",
                    "With Analog absorption weighting, set Russian Roulette weight threshold = 0.0");
            }
            // check that if single ellipsoid tissue specified and (r,z) detector specified,
            // that ellipsoid is centered at x=0, y=0
            if (input.TissueInput is SingleEllipsoidTissueInput)
            {
                foreach (var detectorInput in input.DetectorInputs)
                {
                    var ellipsoid = (EllipsoidRegion)((SingleEllipsoidTissueInput)input.TissueInput).
                        EllipsoidRegion;
                    if (detectorInput.TallyType.IsCylindricalTally() &&
                        (ellipsoid.Center.X != 0.0) && (ellipsoid.Center.Y != 0.0))
                    {
                        return new ValidationResult(
                            false,
                            "Ellipsoid must be centered at (x,y)=(0,0) for cylindrical tallies",
                            "Change ellipsoid center to (0,0) or specify non-cylindrical type tally");            
                    }

                }

            }
            // check that if non-normal source defined, that detectors defined are not cylindrical tallies
            // this could be greatly expanded, just an initial start 
            if (input.SourceInput is DirectionalPointSourceInput)
            {
                var source = (DirectionalPointSourceInput) input.SourceInput;
                if (source.Direction != new Direction(0,0,1))
                {
                    foreach (var detectorInput in input.DetectorInputs)
                    {
                        if (detectorInput.TallyType.IsCylindricalTally())
                        {
                            return new ValidationResult(
                                false,
                                "If source is angled, cannot define cylindrically symmetric detectors",
                                "Change detector to Cartesian equivalent or define source to be normal"); 
                        }
                    }
                }
            }
            return new ValidationResult(
                true,
                "Input options or tissue/detector combinations are valid",
                "");

        }
        /// <summary>
        /// Method checks SimulationInput against current incapabilities of the code.
        /// </summary>
        /// <param name="input">SimulationInput</param>
        /// <returns>ValidationResult</returns>
        private static ValidationResult ValidateCurrentIncapabilities(SimulationInput input)
        {
            if (input.Options.AbsorptionWeightingType == AbsorptionWeightingType.Continuous)
            {
                foreach (var detectorInput in input.DetectorInputs)
                {
                    if (detectorInput.TallyType.IsNotImplementedForCAW())
                    {
                        return new ValidationResult(
                            false,
                            "The use of Continuous Absorption Weighting with cylindrical volume detectors not implemented yet",
                            "Modify AbsorptionWeightingType to Discrete");
                    }
                }

            }
            if (input.Options.AbsorptionWeightingType == AbsorptionWeightingType.Discrete)
            {
                foreach (var detectorInput in input.DetectorInputs)
                {
                    if (detectorInput.TallyType.IsNotImplementedForDAW())
                    {
                        return new ValidationResult(
                            false,
                            "The use of Discrete Absorption Weighting with path length type detectors not implemented yet",
                            "Modify AbsorptionWeightingType to Continuous");
                    }
                }
            }
            // can only run dMC detectors with 1 perturbed region for the present
            foreach (var detectorInput in input.DetectorInputs)
            {
                if (detectorInput is dMCdROfRhodMuaDetectorInput)
                {
                    return dMCdROfRhodMuaDetectorInputValidation.ValidateInput(detectorInput);
                }
                if (detectorInput is dMCdROfRhodMusDetectorInput)
                {
                    return dMCdROfRhodMusDetectorInputValidation.ValidateInput(detectorInput);
                }
            }         
            return new ValidationResult(
                true,
                "Detector definitions are consistent with current capabilities");
        }
    }
}
