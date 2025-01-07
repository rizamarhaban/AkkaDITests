using System;
using System.Threading;

namespace AkkaDI.Examples.CustomMailbox;

internal class ConcurrentLinkedList<T>
{
    private class Node
    {
        public T Value;
        public Node? Next;

        public Node(T value)
        {
            Value = value;
        }
    }

    private Node? _head;

    public ConcurrentLinkedList()
    {
        _head = null;
    }

    /// Add a value to the front of the list (thread-safe)
    public void AddFirst(T value)
    {
        var newNode = new Node(value);

        do
        {
            newNode.Next = _head; // Set the new node's next pointer to the current head
        }
        while (!CompareAndSwap(ref _head, newNode.Next, newNode));
    }

    /// Add a value to the end of the list (thread-safe)
    public void AddLast(T value)
    {
        var newNode = new Node(value);

        while (true)
        {
            var current = _head;
            if (current == null) // Empty list
            {
                if (CompareAndSwap(ref _head, null, newNode))
                    break;
            }
            else
            {
                Node? prev = null;
                while (current != null)
                {
                    prev = current;
                    current = current.Next;
                }

                if (prev != null && CompareAndSwap(ref prev.Next, null, newNode))
                    break;
            }
        }
    }

    /// Traverse the list (thread-safe)
    public void Traverse(Action<T> action)
    {
        var current = _head;
        while (current != null)
        {
            action(current.Value);
            current = current.Next;
        }
    }

    /// Remove the first element (thread-safe)
    public bool RemoveFirst(out T? value)
    {
        Node? oldHead;

        do
        {
            oldHead = _head;
            if (oldHead == null)
            {
                value = default;
                return false; // List is empty
            }
        }
        while (!CompareAndSwap(ref _head, oldHead, oldHead.Next));

        value = oldHead.Value;
        return true;
    }

    /// Remove the last element (not fully lock-free; could be optimized further)
    public bool RemoveLast(out T? value)
    {
        Node? current, prev = null;

        while (true)
        {
            current = _head;
            if (current == null) // Empty list
            {
                value = default;
                return false;
            }

            while (current.Next != null)
            {
                prev = current;
                current = current.Next;
            }

            if (prev == null) // Only one element
            {
                if (CompareAndSwap(ref _head, current, null))
                {
                    value = current.Value;
                    return true;
                }
            }
            else if (CompareAndSwap(ref prev.Next, current, null))
            {
                value = current.Value;
                return true;
            }
        }
    }

    /// Atomic Compare-And-Swap (CAS) operation
    private static bool CompareAndSwap(ref Node? location, Node? expected, Node? newValue)
    {
        return Interlocked.CompareExchange(ref location, newValue, expected) == expected;
    }
}
