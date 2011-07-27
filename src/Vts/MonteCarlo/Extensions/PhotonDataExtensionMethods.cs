using System.Collections.Generic;
using Vts.MonteCarlo.PhotonData;
using Vts.MonteCarlo.VirtualBoundaries;

namespace Vts.MonteCarlo.Extensions
{
    /// <summary>
    /// Methods used to write to surface or volume virtual boundary databases
    /// </summary>
    public static class PhotonDataExtensionMethods
    {


        public static void WriteToPMCSurfaceVirtualBoundaryDatabases(
            this IList<CollisionInfoDatabaseWriter> collisionInfoDatabaseWriters, PhotonDataPoint dp, CollisionInfo collisionInfo)
        {
            foreach (var writer in collisionInfoDatabaseWriters)
            {
                WriteToPMCSurfaceVirtualBoundaryDatabase(writer, dp, collisionInfo);
            };
        }
        public static void WriteToPMCSurfaceVirtualBoundaryDatabase(
            this CollisionInfoDatabaseWriter collisionInfoDatabaseWriter, PhotonDataPoint dp, CollisionInfo collisionInfo)
        {
            if (dp.BelongsToSurfaceVirtualBoundary(collisionInfoDatabaseWriter))
            {
                collisionInfoDatabaseWriter.Write(collisionInfo);
            }
        }
        public static bool BelongsToSurfaceVirtualBoundary(this PhotonDataPoint dp,
            CollisionInfoDatabaseWriter collisionInfoDatabaseWriter)
        {
            if ((dp.StateFlag.Has(PhotonStateType.PseudoDiffuseReflectanceVirtualBoundary) &&
                 collisionInfoDatabaseWriter.VirtualBoundaryType == VirtualBoundaryType.DiffuseReflectance) ||
                (dp.StateFlag.Has(PhotonStateType.PseudoDiffuseTransmittanceVirtualBoundary) &&
                 collisionInfoDatabaseWriter.VirtualBoundaryType == VirtualBoundaryType.DiffuseTransmittance) ||
                (dp.StateFlag.Has(PhotonStateType.PseudoDiffuseReflectanceVirtualBoundary) && // pMC uses regular PST
                 collisionInfoDatabaseWriter.VirtualBoundaryType == VirtualBoundaryType.pMCDiffuseReflectance))
            {
                return true;
            }
            return false;
        }
        


    }
}