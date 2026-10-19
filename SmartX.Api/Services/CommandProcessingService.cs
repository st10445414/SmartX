
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class CommandProcessingService
    {
        private readonly object _sync = new();

        // Standard commands: first in, first out.
        private readonly Queue<DeviceCommand> _normalQueue = new();

        // Critical commands: lower priority number runs first.
        private readonly PriorityQueue<DeviceCommand, (int, long)>
            _priorityQueue = new();

        // Undo history: last executed manual override first.
        private readonly Stack<CommandHistoryEntry> _undoStack = new();

        // Simulated current device state.
        private readonly Dictionary<string, string> _deviceStates = new();

        // Completed command history.
        private readonly List<DeviceCommand> _processedCommands = new();

        private long _sequence = 0;

        private static string StateKey(DeviceCommand command) =>
            $"{command.SensorId}:{command.Action.Trim().ToUpperInvariant()}";

        public DeviceCommand Enqueue(DeviceCommand command)
        {
            lock (_sync)
            {
                if (command.IsCritical)
                {
                    _priorityQueue.Enqueue(command, (0, _sequence++));
                }
                else
                {
                    _normalQueue.Enqueue(command);
                }

                return command;
            }
        }

        public DeviceCommand? ProcessNext()
        {
            lock (_sync)
            {
                DeviceCommand command;

                if (_priorityQueue.Count > 0)
                {
                    command = _priorityQueue.Dequeue();
                }
                else if (_normalQueue.Count > 0)
                {
                    command = _normalQueue.Dequeue();
                }
                else
                {
                    return null;
                }

                string key = StateKey(command);

                string previousValue =
                    _deviceStates.TryGetValue(key, out var existing)
                        ? existing
                        : "OFF";

                _deviceStates[key] = command.Value;

                command.Status = "Executed";
                command.ProcessedAt = DateTime.UtcNow;

                _undoStack.Push(new CommandHistoryEntry
                {
                    CommandId = command.Id,
                    SensorId = command.SensorId,
                    Action = command.Action,
                    PreviousValue = previousValue,
                    NewValue = command.Value,
                    ExecutedAt = command.ProcessedAt.Value
                });

                _processedCommands.Add(command);

                return command;
            }
        }

        public CommandHistoryEntry? UndoLast()
        {
            lock (_sync)
            {
                if (_undoStack.Count == 0)
                    return null;

                var entry = _undoStack.Pop();

                string key =
                    $"{entry.SensorId}:{entry.Action.Trim().ToUpperInvariant()}";

                _deviceStates[key] = entry.PreviousValue;
                entry.UndoneAt = DateTime.UtcNow;

                var original = _processedCommands
                    .FirstOrDefault(c => c.Id == entry.CommandId);

                if (original != null)
                    original.Status = "Undone";

                return entry;
            }
        }

        public object GetStatus()
        {
            lock (_sync)
            {
                return new
                {
                    NormalQueueCount = _normalQueue.Count,
                    CriticalQueueCount = _priorityQueue.Count,
                    UndoCount = _undoStack.Count,
                    ProcessedCount = _processedCommands.Count,

                    DeviceStates = _deviceStates.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value),

                    RecentCommands = _processedCommands
                        .TakeLast(20)
                        .Reverse()
                        .Select(c => new
                        {
                            c.Id,
                            c.SensorId,
                            c.Action,
                            c.Value,
                            c.IsCritical,
                            c.Status,
                            c.ProcessedAt
                        })
                        .ToList()
                };
            }
        }
    }
}
