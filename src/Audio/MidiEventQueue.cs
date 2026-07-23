using System;
using System.Collections.Generic;
using System.Linq;

public sealed class MidiEventQueue
{
    // ...

    public void Clear()
    {
        // ...
    }

    private sealed class MidiEventComparer : IComparer<MidiEvent>
    {
        // ...
    }
}
