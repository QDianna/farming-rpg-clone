namespace DefaultNamespace
{
    public class InventoryEntry
    {
        public InventoryItem item;
        public int quantity;

        public InventoryEntry(InventoryItem item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }
}