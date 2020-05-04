using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PriorityQueue<T,U> 
{
    private List<KeyValuePair<T,U>> queue;

    public bool IsEmpty
    {
        get
        {
            return queue.Any();
        }
    }

    internal void Enqueue(T prio, U value)
    {
        queue.Add(new KeyValuePair<T, U>(prio, value));
        queue = queue.OrderBy(predicate => predicate.Key).ToList();
    }

    public U Dequeue()
    {
        U dequeuedItem = queue[0].Value;
        queue.RemoveAt(0);
        return dequeuedItem;
    }
}
