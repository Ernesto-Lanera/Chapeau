namespace Chapeau.Repositories
{
    /// <summary>
    /// Repository interface for table occupancy management.
    /// </summary>
    public interface ITableRepository
    {
        /// <summary>Ensures the IsManuallyOccupied column exists on the Table_ table (runtime migration).</summary>
        void EnsureColumnExists();
        /// <summary>Sets the manual occupancy status of a table.</summary>
        void SetOccupied(int tableId, bool occupied);
        /// <summary>Checks whether a table has any active (non-paid) orders.</summary>
        bool HasActiveOrders(int tableId);
    }
}
