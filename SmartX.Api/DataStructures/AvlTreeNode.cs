
namespace SmartX.Api.DataStructures
{
    public class AvlTreeNode<T>
    {
        // Timestamp used to organise the tree.
        public DateTime Timestamp { get; set; }

        // Readings sharing the same timestamp.
        public List<T> Values { get; set; }

        // Left and right child nodes.
        public AvlTreeNode<T>? Left { get; set; }
        public AvlTreeNode<T>? Right { get; set; }

        // Height used for AVL balancing.
        public int Height { get; set; }

        public AvlTreeNode(DateTime timestamp, T value)
        {
            Timestamp = timestamp;
            Values = new List<T> { value };
            Height = 1;
        }
    }
}
