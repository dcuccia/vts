﻿using Vts.Common;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.Interfaces;
using Vts.MonteCarlo.Sources.SourceProfiles;

namespace Vts.MonteCarlo.Sources
{
    public class CustomVolumetricEllipsoidalSourceInput : ISourceInput
    {
        // this handles custom ellipsoidal (volumetric)
        public CustomVolumetricEllipsoidalSourceInput(
            double aParameter,
            double bParameter,
            double cParameter,
            ISourceProfile sourceProfile,
            DoubleRange polarAngleEmissionRange,
            DoubleRange azimuthalAngleEmissionRange,
            Direction newDirectionOfPrincipalSourceAxis,
            Position translationFromOrigin,
            int initialTissueRegionIndex)
        {
            SourceType = SourceType.CustomVolumetricEllipsoidal;
            AParameter = aParameter;
            BParameter = bParameter;
            CParameter = cParameter;
            SourceProfile = sourceProfile;
            PolarAngleEmissionRange = polarAngleEmissionRange;
            AzimuthalAngleEmissionRange = azimuthalAngleEmissionRange;
            NewDirectionOfPrincipalSourceAxis = newDirectionOfPrincipalSourceAxis;
            TranslationFromOrigin = translationFromOrigin;
            InitialTissueRegionIndex = initialTissueRegionIndex;
        }

        public CustomVolumetricEllipsoidalSourceInput(
            double aParameter,
            double bParameter,
            double cParameter,
            ISourceProfile sourceProfile,
            DoubleRange polarAngleEmissionRange,
            DoubleRange azimuthalAngleEmissionRange)
            : this(
                aParameter,
                bParameter,
                cParameter,
                sourceProfile,
                polarAngleEmissionRange,
                azimuthalAngleEmissionRange,
                SourceDefaults.DefaultDirectionOfPrincipalSourceAxis.Clone(),
                SourceDefaults.DefaultPosition.Clone(),
                0) { }

        public CustomVolumetricEllipsoidalSourceInput()
            : this(
                1.0,
                1.0,
                2.0,
                new FlatSourceProfile(),
                SourceDefaults.DefaultFullPolarAngleRange.Clone(),
                SourceDefaults.DefaultAzimuthalAngleRange.Clone(),
                SourceDefaults.DefaultDirectionOfPrincipalSourceAxis.Clone(),
                SourceDefaults.DefaultPosition.Clone(),
                0) { }

        public SourceType SourceType { get; set; }
        public double AParameter { get; set; }
        public double BParameter { get; set; }
        public double CParameter { get; set; }
        public ISourceProfile SourceProfile { get; set; }
        public DoubleRange PolarAngleEmissionRange { get; set; }
        public DoubleRange AzimuthalAngleEmissionRange { get; set; }       
        public Direction NewDirectionOfPrincipalSourceAxis { get; set; }
        public Position TranslationFromOrigin { get; set; }
        public int InitialTissueRegionIndex { get; set; }
    }
}