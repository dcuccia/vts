﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.PhotonData;

namespace Vts.MonteCarlo.Factories
{
    /// <summary>
    /// Factory methods to provide the PhotonDatabaseWriter (or list of PhotonDatabaseWriters)
    /// or CollisionInfoDatabaseWriter (or list).
    /// </summary>
    public class DatabaseWriterFactory
    {
        /// <summary>
        /// Static method to provide a list of PhotonDatabaseWriters.  It calls the method
        /// to instantiate one PhotonDatabaseWriter, GetSurfaceVirtualBoundaryDatabaseWriter,
        /// for all elements in the list of VirtualBoundaryType.
        /// </summary>
        /// <param name="virtualBoundaryTypes">list of VirtualBoundaryType</param>
        /// <param name="filePath">path string for database output</param>
        /// <param name="outputName">name string of output</param>
        /// <returns></returns>
        public static IList<PhotonDatabaseWriter> GetSurfaceVirtualBoundaryDatabaseWriters(
            IList<VirtualBoundaryType> virtualBoundaryTypes, string filePath, string outputName)
        {
            return virtualBoundaryTypes.Select(v => GetSurfaceVirtualBoundaryDatabaseWriter(v,
                filePath, outputName)).ToList();
        
        }
        /// <summary>
        /// Static method to instantiate correct PhotonDatabaseWriter given a 
        /// VirtualBoundaryType, path to where to output database and database filename.
        /// </summary>
        /// <param name="virtualBoundaryType">Enum designating virtual boundary type</param>
        /// <param name="filePath">path string of database output</param>
        /// <param name="outputName">filename string of database file</param>
        /// <returns></returns>
        public static PhotonDatabaseWriter GetSurfaceVirtualBoundaryDatabaseWriter(
            VirtualBoundaryType virtualBoundaryType, string filePath, string outputName)
        {
            switch (virtualBoundaryType)
            {
                default:
                case VirtualBoundaryType.DiffuseReflectance:
                    return new PhotonDatabaseWriter(VirtualBoundaryType.DiffuseReflectance,
                        Path.Combine(filePath, outputName, "DiffuseReflectanceDatabase"));
                case VirtualBoundaryType.DiffuseTransmittance:
                    return new PhotonDatabaseWriter(VirtualBoundaryType.DiffuseTransmittance,
                        Path.Combine(filePath, outputName, "DiffuseTransmittanceDatabase"));
                case VirtualBoundaryType.SpecularReflectance:
                    return new PhotonDatabaseWriter(VirtualBoundaryType.SpecularReflectance,
                        Path.Combine(filePath, outputName, "SpecularReflectanceDatabase"));
                case VirtualBoundaryType.pMCDiffuseReflectance:
                    return new PhotonDatabaseWriter(VirtualBoundaryType.pMCDiffuseReflectance,
                        Path.Combine(filePath, outputName, "DiffuseReflectanceDatabase"));
            }
        }
        /// <summary>
        /// Static method to provide list of CollisionInfoDatabaseWriters.  It calls the method
        /// to instantiate one CollisionInfoDatabaseWriter, GetCollisionInfoDatabaseWriter,
        /// for all elements in the list of VirtualBoundaryType. 
        /// </summary>
        /// <param name="virtualBoundaryTypes">list of VirtualBoundaryTypes</param>
        /// <param name="tissue">ITissue needed to instantiate Writer to know how many regions</param>
        /// <param name="filePath">path string of database output</param>
        /// <param name="outputName">filename string of output file</param>
        /// <returns></returns>
        public static IList<CollisionInfoDatabaseWriter> GetCollisionInfoDatabaseWriters(
            IList<VirtualBoundaryType> virtualBoundaryTypes, ITissue tissue, string filePath, string outputName)
        {
            return virtualBoundaryTypes.Select(v => GetCollisionInfoDatabaseWriter(v, tissue,
                filePath, outputName)).ToList();

        }
        /// <summary>
        /// Static method to instantiate correct CollisionInfoDatabaseWriter given a 
        /// VirtualBoundaryType, path to where to output database and database filename.
        /// </summary>
        /// <param name="virtualBoundaryType">VirtualBoundaryType enum</param>
        /// <param name="tissue">ITissue to know how many regions</param>
        /// <param name="filePath">path string of database output</param>
        /// <param name="outputName">filename string of database file</param>
        /// <returns></returns>
        public static CollisionInfoDatabaseWriter GetCollisionInfoDatabaseWriter(
            VirtualBoundaryType virtualBoundaryType, ITissue tissue, string filePath, string outputName)
        {
            switch (virtualBoundaryType)
            {
                default:
                case VirtualBoundaryType.pMCDiffuseReflectance:
                    return new CollisionInfoDatabaseWriter(VirtualBoundaryType.pMCDiffuseReflectance,
                        Path.Combine(filePath, outputName, "CollisionInfoDatabase"), 
                        tissue.Regions.Count());
            }
        }

    }
} 
