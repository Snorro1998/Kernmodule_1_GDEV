using System;
using System.Collections.Generic;

public class Pool<T>
{
    Stack<T> unclaimed, claimed;
    Func<T, bool> checkInUse;
    // drempelwaarde voor het hergebruiken van elementen
    int minClaim; 

    public Pool(int minClaim, int size, Func<T> factory, Func<T, bool> inUse)
    {
        this.minClaim = minClaim;
        unclaimed = new Stack<T>(size);
        claimed = new Stack<T>();
        checkInUse = inUse;

        // reserveer vrije elementen
        for (int i = 0; i < size; ++i)
        {
            unclaimed.Push(factory());
        }
    }

    public T Acquire()
    {
        // hergebruik ongebruikte elementen
        while (claimed.Count > minClaim)
        {
            if (checkInUse(claimed.Peek()))
                break;

            unclaimed.Push(claimed.Pop());
        }

        // gebruik een beschikbaar element
        if (unclaimed.Count > 0)
        {
            T top = unclaimed.Pop();
            claimed.Push(top);
            return top;
        }

        // er is geen element beschikbaar
        return default;
    }
}