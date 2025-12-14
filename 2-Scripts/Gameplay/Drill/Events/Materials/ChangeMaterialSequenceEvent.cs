using UnityEngine;

public struct ChangeMaterialSequenceEvent
{
    public MaterialSequenceSO sequence;
    public ChangeMaterialSequenceEvent(MaterialSequenceSO seq) => sequence = seq;
}
