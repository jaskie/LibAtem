﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace LibAtem.Util
{
    public class UniqueQueue<TKey, TVal>: IDisposable
    {
        private readonly BlockingCollection<TKey> idQueue;
        private readonly Dictionary<TKey, TVal> entries;

        public UniqueQueue()
        {
            idQueue = new BlockingCollection<TKey>();
            entries = new Dictionary<TKey, TVal>();
        }

        public int Count => idQueue.Count;
        public bool IsEmpty => Count == 0;

        // TODO return false if item was replaced
        public void Enqueue(TKey key, TVal item)
        {
            lock (entries)
            {
                if (!entries.ContainsKey(key))
                {
                    entries[key] = item;
                    idQueue.Add(key);
                }
                else
                {
                    entries[key] = item;
                }
            }
        }

        public TVal Dequeue()
        {
            TKey key = idQueue.Take();
            lock (entries)
            {
                if (entries.TryGetValue(key, out TVal val))
                {
                    entries.Remove(key);
                    return val;
                }
            }

            // recurse until we find one that isnt corrupt
            Debug.Fail("Failed to find entry for key");
            return default(TVal);
        }

        public void Dispose()
        {
            idQueue.Dispose();
        }
    }
}
