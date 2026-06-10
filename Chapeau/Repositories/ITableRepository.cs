namespace Chapeau.Repositories
{
    public interface ITableRepository
    {
        void EnsureColumnExists();
        void SetOccupied(int tableId, bool occupied);
        bool HasActiveOrders(int tableId);
    }
}
