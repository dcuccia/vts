using System;
using System.Collections.Generic;
using Vts.IO;
using Vts.Factories;
using Vts.Modeling.ForwardSolvers;
using System.IO;
using System.Linq;
using Vts.Extensions;
using System.Reflection;

namespace Vts.ReportForwardSolvers.Desktop
{
    class Program
    {
        static void Main(string[] args)
        {
            var projectName = "Vts.ReportForwardSolvers.Desktop";
            var inputPath = @"..\..\Resources\";
            string currentAssemblyDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            inputPath = currentAssemblyDirectoryName + "\\" + inputPath;
            var g = 0.8;
            var n = 1.4;
            var muas = new double[] { 0.001, 0.01, 0.03, 0.1, 0.3 };//[mm-1]
            var musps = new double[] { 0.5, 0.7, 1.0, 1.2, 1.5, 2.0 };//[mm-1]

            var Ops =
                      from musp in musps
                      from mua in muas
                      select new OpticalProperties(mua, musp, g, n);

            var forwardSolverTypes = new ForwardSolverType[]
                      {
                          ForwardSolverType.MonteCarlo,
                          ForwardSolverType.PointSourceSDA,
                          //ForwardSolverType.DistributedPointSDA,
                          //ForwardSolverType.DistributedGaussianSDA,
                          //ForwardSolverType.DeltaPOne,
                          ForwardSolverType.Nurbs,
                      };

            var spatialDomainTypes = new SpatialDomainType[]
                     {
                         SpatialDomainType.Real,
                         //SpatialDomainType.SpatialFrequency,
                     };

            var timeDomainTypes = new TimeDomainType[]
                     {
                         TimeDomainType.SteadyState,
                         TimeDomainType.TimeDomain,
                         //TimeDomainType.FrequencyDomain,   
                     };

            foreach (var sDT in spatialDomainTypes)
            {
                foreach (var tDT in timeDomainTypes)
                {
                    foreach (var op in Ops)
                    {
                        ReportAllForwardSolvers(forwardSolverTypes, sDT, tDT, op, inputPath, projectName);
                    }
                }
            }
        }

        #region methods

        private static void ReportAllForwardSolvers(ForwardSolverType[] forwardSolverTypes,
                                                    SpatialDomainType sDT,
                                                    TimeDomainType tDT,
                                                    OpticalProperties op,
                                                    string inputPath,
                                                    string projectName)
        {
            if (tDT == TimeDomainType.SteadyState)
            {
                ReportSteadyStateForwardSolver(forwardSolverTypes, sDT, op, inputPath, projectName);
            }
            else
            {
                Report2DForwardSolver(forwardSolverTypes, sDT, tDT, op, inputPath, projectName);
            }
        }

        private static void ReportSteadyStateForwardSolver(ForwardSolverType[] forwardSolverTypes,
                                                                   SpatialDomainType sDT,
                                                                   OpticalProperties op,
                                                                   string inputPath,
                                                                   string projectName)
        {
            var filename = "musp" + op.Musp.ToString() + "mua" + op.Mua.ToString();
            filename = filename.Replace(".", "p");
            Console.WriteLine("Looking for file {0} in spatial domain type {1}", filename, sDT.ToString());

            if (File.Exists(inputPath + sDT.ToString() + "/SteadyState/" + filename))
            {
                Console.WriteLine("The file {0} has been found.", filename);
                int sDim = GetSpatialNumberOfPoints(sDT);
                var spatialVariable = (IEnumerable<double>)FileIO.ReadArrayFromBinaryInResources<double>
                                      ("Resources/" + sDT.ToString() + "/SteadyState/" + filename, projectName, sDim);
                foreach (var fST in forwardSolverTypes)
                {
                    var fs = SolverFactory.GetForwardSolver(fST);
                    EvaluateAndWriteForwardSolverSteadyStateResults(fs, sDT, op, spatialVariable);
                }
            }
            else
            {
                Console.WriteLine("The file {0} has not been found.",filename);
            }
        }

        private static void Report2DForwardSolver(ForwardSolverType[] forwardSolverTypes,
                                                   SpatialDomainType sDT,
                                                   TimeDomainType tDT,
                                                   OpticalProperties op,
                                                   string inputPath,
                                                   string projectName)
        {
            var filename = "musp" + op.Musp.ToString() + "mua" + op.Mua.ToString();
            filename = filename.Replace(".", "p");
            Console.WriteLine("Looking for file {0} in spatial domain type {1} and temporal domain type{2}",
                                                                   filename, sDT.ToString(), tDT.ToString());
            if (File.Exists(inputPath + sDT.ToString() + "/SteadyState/" + filename) ||
                File.Exists(inputPath + sDT.ToString() + "/" + tDT.ToString() + "/" + filename))
            {
                Console.WriteLine("The file {0} has been found.", filename);
                int sDim = GetSpatialNumberOfPoints(sDT);
                int tDim = GetTemporalNumberOfPoints(sDT, tDT);
                int[] dims = { sDim, tDim };


                var spatialVariable = (IEnumerable<double>)FileIO.ReadArrayFromBinaryInResources<double>
                                      ("Resources/" + sDT.ToString() + "/" + "SteadyState/" + filename, projectName, sDim);
                var temporalVariable = (double[,])FileIO.ReadArrayFromBinaryInResources<double>
                                      ("Resources/" + sDT.ToString() + "/" + tDT.ToString() + "/" + filename, projectName, dims);
                foreach (var fST in forwardSolverTypes)
                {
                    var fs = SolverFactory.GetForwardSolver(fST);
                    EvaluateAndWriteForwardSolver2DResults(fs, sDT, tDT, op, spatialVariable, temporalVariable);
                }
            }
            else
            {
                Console.WriteLine("The file {0} has not been found", filename);
            }
        }

        private static void EvaluateAndWriteForwardSolverSteadyStateResults(IForwardSolver fs,
                                                                    SpatialDomainType sDT,
                                                                    OpticalProperties op,
                                                                    IEnumerable<double> spatialVariable)
        {
            double[] reflectanceValues;

            var ReflectanceFunction = GetSteadyStateReflectanceFunction(fs, sDT);

            MakeDirectoryIfNonExistent(sDT.ToString(), "SteadyState", fs.LocalToString());

            reflectanceValues = ReflectanceFunction(op.AsEnumerable(), spatialVariable).ToArray();

            LocalWriteArrayToBinary<double>(reflectanceValues,@"Output/" + sDT.ToString() +
                                              "/SteadyState/" + fs.LocalToString() + "/" +
                                              "musp" + op.Musp.ToString() + "mua" + op.Mua.ToString(),FileMode.Create);
        }

        private static void EvaluateAndWriteForwardSolver2DResults(IForwardSolver fs,
                                                                   SpatialDomainType sDT,
                                                                   TimeDomainType tDT,
                                                                   OpticalProperties op,
                                                                   IEnumerable<double> spatialVariable,
                                                                   double[,] temporalVariable)
        {
            double[] reflectanceValues;
            var ReflectanceFunction = Get2DReflectanceFunction(fs, sDT, tDT);

            MakeDirectoryIfNonExistent(sDT.ToString(), tDT.ToString(), fs.LocalToString());

            var sV = spatialVariable.First();
            var tV = temporalVariable.Row(0);

            reflectanceValues = ReflectanceFunction(op.AsEnumerable(), sV.AsEnumerable(), tV).ToArray();

            LocalWriteArrayToBinary<double>(reflectanceValues, @"Output/" + sDT.ToString() +
                                            "/" + tDT.ToString() + "/" + fs.LocalToString() + "/" +
                                            "musp" + op.Musp.ToString() + "mua" + op.Mua.ToString(),
                                            FileMode.Create);

            for (int spaceInd = 1; spaceInd < spatialVariable.Count(); spaceInd++)
            {
                sV = spatialVariable.ElementAt(spaceInd);
                tV = temporalVariable.Row(spaceInd);

                reflectanceValues = ReflectanceFunction(op.AsEnumerable(), sV.AsEnumerable(), tV).ToArray();

                LocalWriteArrayToBinary<double>(reflectanceValues, @"Output/" + sDT.ToString() + "/" +
                                                tDT.ToString() + "/" + fs.LocalToString() + "/" +
                                                "musp" + op.Musp.ToString() + "mua" + op.Mua.ToString(),
                                                FileMode.Append);
            }
        }

        private static int GetSpatialNumberOfPoints(SpatialDomainType sDT)
        {
            int sDim;
            if (sDT == SpatialDomainType.Real)
            {
                sDim = 200;
            }
            else if (sDT == SpatialDomainType.SpatialFrequency)
            {
                sDim = 200;
            }
            else
            {
                throw new ArgumentException("Non valid spatial domain.");
            }
            return sDim;
        }

        private static int GetTemporalNumberOfPoints(SpatialDomainType sDT, TimeDomainType tDT)
        {
            int tDim;
            if (sDT == SpatialDomainType.Real)
            {
                if (tDT == TimeDomainType.TimeDomain)
                {
                    tDim = 201;
                }
                else if (tDT == TimeDomainType.FrequencyDomain)
                {
                    tDim = 201;
                }
                else
                {
                    throw new ArgumentException("Non valid temporal domain type.");
                }
            }
            else if (sDT == SpatialDomainType.SpatialFrequency)
            {
                if (tDT == TimeDomainType.TimeDomain)
                {
                    tDim = 201;
                }
                else if (tDT == TimeDomainType.FrequencyDomain)
                {
                    tDim = 201;
                }
                else
                {
                    throw new ArgumentException("Non valid temporal domain type.");
                }
            }
            else
            {
                throw new ArgumentException("Non valid spatial domain type.");
            }

            return tDim;
        }

        private static Func<IEnumerable<OpticalProperties>, IEnumerable<double>, IEnumerable<double>, IEnumerable<double>>
                       Get2DReflectanceFunction(IForwardSolver fs, SpatialDomainType sD, TimeDomainType tD)
        {
            Func<IEnumerable<OpticalProperties>, IEnumerable<double>, IEnumerable<double>, IEnumerable<double>> ReflectanceFunction;

            switch (sD)
            {
                case SpatialDomainType.Real:
                    if (tD == TimeDomainType.TimeDomain)
                    {
                        ReflectanceFunction = fs.RofRhoAndT;
                    }
                    else if (tD == TimeDomainType.FrequencyDomain)
                    {
                        ReflectanceFunction = (op,rho,ft) => fs.RofRhoAndFt(op,rho,ft).Select(rComplex => rComplex.Magnitude);
                    }
                    else 
                    {
                        throw new ArgumentException("Non valid temporal domain.");
                    }
                    break;
                case SpatialDomainType.SpatialFrequency:
                    if (tD == TimeDomainType.TimeDomain)
                    {
                        ReflectanceFunction = fs.RofFxAndT;
                    }
                    else if (tD == TimeDomainType.FrequencyDomain)
                    {
                        ReflectanceFunction = (op, fx, ft) => fs.RofFxAndFt(op, fx, ft).Select(rComplex => rComplex.Magnitude);
                    }
                    else 
                    {
                        throw new ArgumentException("Non valid temporal domain.");
                    }
                    break;
                default:
                    throw new ArgumentException("Non valid spatial domain.");
                    break;
            }
            return ReflectanceFunction;
        }

        private static Func<IEnumerable<OpticalProperties>, IEnumerable<double>, IEnumerable<double>>
                       GetSteadyStateReflectanceFunction(IForwardSolver fs, SpatialDomainType sd)
        {
            Func<IEnumerable<OpticalProperties>, IEnumerable<double>, IEnumerable<double>> ReflectanceFunction;
            switch (sd)
            {
                case SpatialDomainType.Real:
                    ReflectanceFunction = fs.RofRho;
                    break;
                case SpatialDomainType.SpatialFrequency:
                    ReflectanceFunction = fs.RofFx;
                    break;
                default:
                    throw new ArgumentException("Non valid solution domain!");
            }
            return ReflectanceFunction;
        }

        private static void LocalWriteArrayToBinary<T>(Array dataIN, string filename, FileMode mode) where T : struct
        {
            // Create a file to write binary data 
            using (Stream s = StreamFinder.GetFileStream(filename, mode))
            {
                using (BinaryWriter bw = new BinaryWriter(s))
                {
                    new ArrayCustomBinaryWriter<T>().WriteToBinary(bw, dataIN);
                }
            }
        }

        private static void MakeDirectoryIfNonExistent(string sDTFolder, string tDTFolder, string fsFolder)
        {
            if (!(Directory.Exists(@"Output/" + sDTFolder + "/" + tDTFolder + "/" + fsFolder)))
            {
                Directory.CreateDirectory(@"Output/" + sDTFolder + "/" + tDTFolder + "/" + fsFolder);
            }
        }

        #endregion methods
    }

    /// <summary>
    /// This class defines some local extensions methods.
    /// </summary>
    public static class LocalExtensions
    {
        /// <summary>
        /// Returns a string with a name representing the forward solver.
        /// </summary>
        /// <param name="forwardSolver">forward solver</param>
        /// <returns>string with forward solver name</returns>
        public static string LocalToString(this IForwardSolver forwardSolver)
        {
            if (forwardSolver as NurbsForwardSolver != null)
            {
                return "Nurbs";
            }
            else if (forwardSolver as MonteCarloForwardSolver != null)
            {
                return "MonteCarlo";
            }
            else if (forwardSolver as DistributedPointSourceSDAForwardSolver != null)
            {
                return "DistributedPointSDA";
            }
            else if (forwardSolver as DistributedGaussianSourceSDAForwardSolver
                != null)
            {
                return "DistributedGaussianSDA";
            }
            else if (forwardSolver as PointSourceSDAForwardSolver != null)
            {
                return "PointSDA";
            }
            else if (forwardSolver as DeltaPOneForwardSolver != null)
            {
                return "DeltaPOne";
            }
            else
            {
                throw new Exception("Unknown forward solver type.");
            }
        }
    }
}

