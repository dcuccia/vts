﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Vts.Common;
using Vts.IO;
using Vts.MonteCarlo.Detectors;
using AutoMapper;
using Vts.MonteCarlo.IO;

namespace Vts.MonteCarlo.Factories
{
    // interfaces

    public interface IDetectorProvider<TDetector, TDetectorInput, TDetectorOutput> : IProvider<IDetector>
        where TDetector : IDetector
        where TDetectorInput : IDetectorInput
        where TDetectorOutput : IDetectorOutput
    {
        Func<string, TDetectorInput> ReadInputFromXML { get; set; }
        Func<string, string, TDetectorInput> ReadInputFromXMLInResources { get; set; }
        Action<TDetectorInput, string> WriteInputToXML { get; set; }
        Func<TDetectorInput, TDetector> CreateDetector { get; set; }
        Func<TDetector, TDetectorOutput> CreateOutput { get; set; }
    }

    public interface IProvider<IDetector>
    {
        Type TargetType { get; set; }
    }


    //public interface IOutput<TDetector>
    //    where TDetector : IDetector
    //{
    //    string Name { get; set; }
    //}

    public interface IDetectorOutput
    {
        int[] Dimensions { get; set; }
        string Name { get; set; }
        TallyType TallyType { get; set; }
    }

    public interface IDetectorOutput<T> : IDetectorOutput
    {
        /// <summary>
        /// Mean of detector tally
        /// </summary>
        T Mean { get; set; }
        /// <summary>
        /// Second moment of detector tally
        /// </summary>
        T SecondMoment { get; set; }
    }


    // base class implementations

    /// <summary>
    /// Base class for all detectors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DetectorBase<T> : IDetector<T>
    {
        private T _mean;
        private T _secondMoment;
        private int[] _dimensions;

        protected DetectorBase()
        {
            TallySecondMoment = false;
            Name = "";

            IsReflectanceTally = false;
            IsTransmittanceTally = false;
            IsSpecularReflectanceTally = false;
            IsInternalSurfaceTally = false;
            IspMCReflectanceTally = false;
            IsVolumeTally = false;
            IsCylindricalTally = false;
            IsNotImplementedForCAW = false;
            IsNotImplementedYet = false;
        }

        public string Name { get; set; } // shouldn't have public set_Name
        public TallyType TallyType { get; set; } // shouldn't have public set_TallyType

        public T Mean
        {
            get
            {
                if (_mean == null != null)
                {
                    _dimensions = GetDimensions();
                    _mean = (T)((dynamic)Array.CreateInstance(typeof(T).GetElementType(), _dimensions));
                }
                return _mean;
            }
        }

        public T SecondMoment
        {
            get
            {
                if (_secondMoment == null && TallySecondMoment)
                {
                    _dimensions = GetDimensions();
                    _secondMoment = (T)((dynamic)Array.CreateInstance(typeof(T).GetElementType(), _dimensions));
                }
                return _secondMoment;
            }
        }

        public int[] Dimensions
        {
            get { return _dimensions; }
        }

        public bool TallySecondMoment { get; set; }
        public long TallyCount { get; set; } // shouldn't have public set_TallyCount

        public bool IsReflectanceTally { get; protected set; }
        public bool IsTransmittanceTally { get; protected set; }
        public bool IsSpecularReflectanceTally { get; protected set; }
        public bool IsInternalSurfaceTally { get; protected set; }
        public bool IspMCReflectanceTally { get; protected set; }
        public bool IsVolumeTally { get; protected set; }
        public bool IsCylindricalTally { get; protected set; }
        public bool IsNotImplementedForCAW { get; protected set; }
        public bool IsNotImplementedYet { get; protected set; }

        protected abstract int[] GetDimensions();
        public abstract void Tally(Photon photon);
        public abstract void Normalize(long numPhotons);
    }

    public class DetectorProvider<TDetectorInput, TDetector, TDetectorOutput>
        where TDetector : IDetector
        where TDetectorOutput : IDetectorOutput
    {
        static DetectorProvider()
        {
            Mapper.CreateMap<TDetectorInput, TDetector>();
            Mapper.CreateMap<TDetector, TDetectorOutput>();

            KnownTypes.Add(typeof(TDetectorInput));
            KnownTypes.Add(typeof(TDetectorOutput));
        }

        public DetectorProvider()
        {
            CreateDetector = input => Mapper.Map<TDetectorInput, TDetector>(input);
            CreateOutput = detector => Mapper.Map<TDetector, TDetectorOutput>(detector);

            ReadInputFromFile = filename => Vts.IO.FileIO.ReadFromXML<TDetectorInput>(filename);
            WriteInputToFile = (input, filename) => Vts.IO.FileIO.WriteToXML(input, filename);
            ReadInputFromResources = (filename, projectName) => Vts.IO.FileIO.ReadFromXMLInResources<TDetectorInput>(filename, projectName);

            WriteOutputToFile = (output, filename) => DetectorIO.WriteDetectorOutputToFile(output, filename);
            ReadOutputFromFile = (filename, folderPath) => DetectorIO.ReadDetectorOutputFromFile<TDetectorOutput>(filename, folderPath);

            TargetType = typeof(TDetector);
        }

        public Type TargetType { get; set; }
        public Func<TDetectorInput, TDetector> CreateDetector { get; set; }
        public Func<TDetector, TDetectorOutput> CreateOutput { get; set; }
        public Func<string, TDetectorInput> ReadInputFromFile { get; set; }
        public Func<string, string, TDetectorInput> ReadInputFromResources { get; set; }
        public Action<TDetectorInput, string> WriteInputToFile { get; set; }
        public Func<string, string, TDetectorOutput> ReadOutputFromFile { get; set; }
        public Action<TDetectorOutput, string> WriteOutputToFile { get; set; }

    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////

    // user code

    /// <summary>
    /// Class to hold information necessary for creating detector
    /// </summary>
    public class SampleDetectorInput : IDetectorInput
    {
        public SampleDetectorInput()
        {
            TallyType = TallyType.ROfFx;
            Name = "ROfFx";
            QRange = new DoubleRange(0, 1, 10);
        }

        public TallyType TallyType { get; set; }
        public string Name { get; set; }
        public DoubleRange QRange { get; set; }
    }

    /// <summary>
    /// Acutal detector class implementation
    /// </summary>
    public class SampleDetector : DetectorBase<double[]>
    {
        private static int _tempIndex = -1;

        public SampleDetector()
        {
            QRange = new DoubleRange(0, 1, 10);
            TallySecondMoment = false;
            Name = "SampleDetector";

            // todo: I've created a monster...
            IsReflectanceTally = true;
            IsTransmittanceTally = false;
            IsSpecularReflectanceTally = false;
            IsInternalSurfaceTally = false;
            IspMCReflectanceTally = false;
            IsVolumeTally = false;
            IsCylindricalTally = false;
            IsNotImplementedForCAW = false;
            IsNotImplementedYet = false;
        }

        public DoubleRange QRange { get; set; }

        public override void Tally(Photon photon)
        {
            Mean[(_tempIndex++) % Dimensions[0]] += photon.DP.Weight;
            TallyCount++;
        }

        public override void Normalize(long numPhotons)
        {
            for (int i = 0; i < Dimensions[0]; i++)
            {
                Mean[i] /= TallyCount;
            }
        }

        protected override int[] GetDimensions()
        {
            return new int[] { QRange.Count - 1 };
        }
    }

    /// <summary>
    /// Class representing detector data to save/store
    /// </summary>
    public class DetectorOutput<T> : IDetectorOutput<T>
    {
        [IgnoreDataMember]
        public T Mean { get; set; }

        [IgnoreDataMember]
        public T SecondMoment { get; set; }

        public int[] Dimensions { get; set; }
        public string Name { get; set; }
        public TallyType TallyType { get; set; }
    }

    public class SampleDetectorOutput : DetectorOutput<double[]>
    {
        public DoubleRange QRange { get; set; }
    }


    /// <summary>
    /// Class that glues all the pieces together. In most cases, there shouldn't be any extra work to do here
    /// </summary>
    public class SampleDetectorProvider
        : DetectorProvider<SampleDetectorInput, SampleDetector, SampleDetectorOutput>
    {
    }
}
