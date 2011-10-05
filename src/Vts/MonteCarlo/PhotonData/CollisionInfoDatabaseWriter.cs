﻿using System;
using Vts.IO;
using Vts.MonteCarlo.IO;

namespace Vts.MonteCarlo.PhotonData
{
    /// <summary>
    /// Implements CustomBinaryStreamWriter(OfPhotonDataPoint). Handles writing photon
    /// terminating data to database.
    /// </summary>
    public class CollisionInfoDatabaseWriter : DatabaseWriter<CollisionInfoDatabase, CollisionInfo>
    {
        /// <summary>
        /// constructor for the collision info database writer
        /// </summary>
        /// <param name="virtualBoundaryType">virtual boundary type</param>
        /// <param name="filename">name of database filename</param>
        /// <param name="numberOfSubRegions">number of subregions in tissue</param>
        public CollisionInfoDatabaseWriter(VirtualBoundaryType virtualBoundaryType, string filename, int numberOfSubRegions)
            : base(filename, new CollisionInfoDatabase(numberOfSubRegions), new CollisionInfoSerializer(numberOfSubRegions))
        {
            VirtualBoundaryType = virtualBoundaryType;
        }
        public VirtualBoundaryType VirtualBoundaryType { get; set; }
    }
}
