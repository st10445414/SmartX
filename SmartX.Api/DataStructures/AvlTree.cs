
namespace SmartX.Api.DataStructures
{
    public class AvlTree<T>
    {
        private AvlTreeNode<T>? _root;

        // Number of readings stored, including duplicate timestamps.
        public int Count { get; private set; }

        // Insert a reading using its timestamp.
        public void Insert(DateTime timestamp, T value)
        {
            _root = InsertNode(_root, timestamp, value);
            Count++;
        }

        private AvlTreeNode<T> InsertNode(
            AvlTreeNode<T>? node,
            DateTime timestamp,
            T value)
        {
            if (node == null)
            {
                return new AvlTreeNode<T>(timestamp, value);
            }

            int comparison = timestamp.CompareTo(node.Timestamp);

            if (comparison < 0)
            {
                node.Left = InsertNode(
                    node.Left, timestamp, value);
            }
            else if (comparison > 0)
            {
                node.Right = InsertNode(
                    node.Right, timestamp, value);
            }
            else
            {
                // Preserve readings with identical timestamps.
                node.Values.Add(value);
                return node;
            }

            UpdateHeight(node);

            int balance = GetBalance(node);

            // Left-Left case
            if (balance > 1 &&
                timestamp < node.Left!.Timestamp)
            {
                return RotateRight(node);
            }

            // Right-Right case
            if (balance < -1 &&
                timestamp > node.Right!.Timestamp)
            {
                return RotateLeft(node);
            }

            // Left-Right case
            if (balance > 1 &&
                timestamp > node.Left!.Timestamp)
            {
                node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            // Right-Left case
            if (balance < -1 &&
                timestamp < node.Right!.Timestamp)
            {
                node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            return node;
        }

        // Return the height of a node.
        private static int GetHeight(AvlTreeNode<T>? node)
        {
            return node?.Height ?? 0;
        }

        // Update height after insertion or rotation.
        private static void UpdateHeight(AvlTreeNode<T> node)
        {
            node.Height = 1 + Math.Max(
                GetHeight(node.Left),
                GetHeight(node.Right));
        }

        // Balance factor = left height - right height.
        private static int GetBalance(AvlTreeNode<T>? node)
        {
            if (node == null)
                return 0;

            return GetHeight(node.Left) -
                   GetHeight(node.Right);
        }

        // Right rotation.
        private static AvlTreeNode<T> RotateRight(
            AvlTreeNode<T> y)
        {
            var x = y.Left!;
            var temporarySubtree = x.Right;

            x.Right = y;
            y.Left = temporarySubtree;

            UpdateHeight(y);
            UpdateHeight(x);

            return x;
        }

        // Left rotation.
        private static AvlTreeNode<T> RotateLeft(
            AvlTreeNode<T> x)
        {
            var y = x.Right!;
            var temporarySubtree = y.Left;

            y.Left = x;
            x.Right = temporarySubtree;

            UpdateHeight(x);
            UpdateHeight(y);

            return y;
        }

        // Find all readings within a timestamp range.
        public List<T> SearchRange(
            DateTime start,
            DateTime end)
        {
            if (start > end)
            {
                throw new ArgumentException(
                    "Start date must be before or equal to end date.");
            }

            var results = new List<T>();

            SearchRangeNode(_root, start, end, results);

            return results;
        }

        private static void SearchRangeNode(
            AvlTreeNode<T>? node,
            DateTime start,
            DateTime end,
            List<T> results)
        {
            if (node == null)
                return;

            // Only search left when earlier timestamps
            // could still fall within the requested range.
            if (start < node.Timestamp)
            {
                SearchRangeNode(
                    node.Left, start, end, results);
            }

            // Include all readings at matching timestamps.
            if (node.Timestamp >= start &&
                node.Timestamp <= end)
            {
                results.AddRange(node.Values);
            }

            // Only search right when later timestamps
            // could still fall within the requested range.
            if (end > node.Timestamp)
            {
                SearchRangeNode(
                    node.Right, start, end, results);
            }
        }

        // Return all readings in chronological order.
        public List<T> GetAllInOrder()
        {
            var results = new List<T>();

            TraverseInOrder(_root, results);

            return results;
        }

        private static void TraverseInOrder(
            AvlTreeNode<T>? node,
            List<T> results)
        {
            if (node == null)
                return;

            TraverseInOrder(node.Left, results);

            results.AddRange(node.Values);

            TraverseInOrder(node.Right, results);
        }

        // Expose tree height for testing and reporting.
        public int Height => GetHeight(_root);
    }
}
