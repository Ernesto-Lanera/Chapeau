namespace Chapeau.Models
{
    /// <summary>
    /// Represents a physical table in the restaurant, with occupancy tracking.
    /// </summary>
    public class Table_
    {
        /// <summary>Unique identifier for the table.</summary>
        public int TableId { get; set; }
        /// <summary>Display number of the table (1-10).</summary>
        public int TableNumber { get; set; }
        /// <summary>Whether the table has been manually marked as occupied by staff.</summary>
        public bool IsManuallyOccupied { get; set; }
    }
}
