﻿using System;
using Vts.Common;
using Vts.MonteCarlo.Helpers;

namespace Vts.MonteCarlo.Sources
{
    /// <summary>
    /// Abstract class for CircularSourceBase
    /// </summary>
    public abstract class CircularSourceBase : ISource
    {
        /// <summary>
        /// Source beam diameter FWHM (-1 for flat beam)
        /// </summary>
        protected double _beamDiameterFWHM;
        /// <summary>
        /// New source axis direction 
        /// </summary>
        protected Direction _newDirectionOfPrincipalSourceAxis;
        /// <summary>
        /// New source location
        /// </summary>
        protected Position _translationFromOrigin;
        /// <summary>
        /// Beam rotation from inward normal
        /// </summary>
        protected PolarAzimuthalAngles _beamRotationFromInwardNormal;
        /// <summary>
        /// Source rotation and translation flags
        /// </summary>
        protected SourceFlags _rotationAndTranslationFlags;
        /// <summary>
        /// The outer radius of the circular source
        /// </summary>
        protected double _outerRadius;
        /// <summary>
        /// The inner radius of the circular source
        /// </summary>
        protected double _innerRadius;
        /// <summary>
        /// Initial tissue region index
        /// </summary>
        protected int _initialTissueRegionIndex;

        /// <summary>
        /// Defines CircularSourceBase class
        /// </summary>
        /// <param name="innerRadius">The inner radius of the circular source</param>
        /// <param name="outerRadius">The outer radius of the circular source</param>
        /// <param name="beamDiameterFWHM">Beam diameter FWHM (-1 for flat beam)</param>
        /// <param name="newDirectionOfPrincipalSourceAxis">New source axis direction</param> 
        /// <param name="translationFromOrigin">New source location</param>
        /// <param name="beamRotationFromInwardNormal">Polar Azimuthal Rotational Angle of inward Normal</param>
        /// <param name="initialTissueRegionIndex">Initial tissue region index</param>
        protected CircularSourceBase(            
            double outerRadius,
            double innerRadius,
            double beamDiameterFWHM,
            Direction newDirectionOfPrincipalSourceAxis,
            Position translationFromOrigin,
            PolarAzimuthalAngles beamRotationFromInwardNormal,
            int initialTissueRegionIndex)
        {
            _rotationAndTranslationFlags = new SourceFlags(
                newDirectionOfPrincipalSourceAxis != SourceDefaults.DefaultDirectionOfPrincipalSourceAxis.Clone(),
                translationFromOrigin != SourceDefaults.DefaultPosition.Clone(),
                beamRotationFromInwardNormal != SourceDefaults.DefaultBeamRoationFromInwardNormal.Clone());

            _outerRadius = outerRadius;
            _innerRadius = innerRadius;
            _beamDiameterFWHM = beamDiameterFWHM;
            _newDirectionOfPrincipalSourceAxis = newDirectionOfPrincipalSourceAxis.Clone();
            _translationFromOrigin = translationFromOrigin.Clone();
            _beamRotationFromInwardNormal = beamRotationFromInwardNormal.Clone();
            _initialTissueRegionIndex = initialTissueRegionIndex;
        }

        /// <summary>
        /// Implements Get next photon
        /// </summary>
        /// <param name="tissue">tissue</param>
        /// <returns>photon</returns>
        public Photon GetNextPhoton(ITissue tissue)
        {
            //Source starts from anywhere in the circle
            Position finalPosition = GetFinalPosition(_beamDiameterFWHM, _innerRadius, _outerRadius, Rng);

            // sample angular distribution
            Direction finalDirection = GetFinalDirection(finalPosition);

            //Find the relevent polar and azimuthal pair for the direction
            PolarAzimuthalAngles _rotationalAnglesOfPrincipalSourceAxis = SourceToolbox.GetPolarAzimuthalPairFromDirection(_newDirectionOfPrincipalSourceAxis);

            //Rotation and translation
            SourceToolbox.UpdateDirectionPositionAfterGivenFlags(
                ref finalPosition,
                ref finalDirection,
                _rotationalAnglesOfPrincipalSourceAxis,
                _translationFromOrigin,
                _beamRotationFromInwardNormal,                
                _rotationAndTranslationFlags);

            var photon = new Photon(finalPosition, finalDirection, tissue, _initialTissueRegionIndex, Rng);

            return photon;
        }

        /// <summary>
        /// Returns final direction for a given position
        /// </summary>
        /// <param name="position">Current position</param>
        /// <returns>new direction</returns>
        protected abstract Direction GetFinalDirection(Position position); // position may or may not be needed

        private static Position GetFinalPosition(double beamDiameterFWHM, double innerRadius, double outerRadius, Random rng)
        {
            return beamDiameterFWHM < 0.0
                ? SourceToolbox.GetPositionInACircleRandomFlat(
                        SourceDefaults.DefaultPosition.Clone(),
                        innerRadius,
                        outerRadius,
                        rng)
                : SourceToolbox.GetPositionInACircleRandomGaussian(
                        SourceDefaults.DefaultPosition.Clone(),
                        outerRadius,   
                        innerRadius,
                        beamDiameterFWHM,
                        rng);
        }

        #region Random number generator code (copy-paste into all sources)
        /// <summary>
        /// The random number generator used to create photons. If not assigned externally,
        /// a Mersenne Twister (MathNet.Numerics.Random.MersenneTwister) will be created with
        /// a seed of zero.
        /// </summary>
        public Random Rng
        {
            get
            {
                if (_rng == null)
                {
                    _rng = new MathNet.Numerics.Random.MersenneTwister(0);
                }
                return _rng;
            }
            set { _rng = value; }
        }
        private Random _rng;
        #endregion
    }
}
