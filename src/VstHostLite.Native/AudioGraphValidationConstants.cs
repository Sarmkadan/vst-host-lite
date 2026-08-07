namespace VstHostLite.Native;

/// <summary>
/// Constants for AudioGraphValidation.
/// </summary>
internal static class AudioGraphValidationConstants
{
    /// <summary>
    /// Message when audio graph contains no nodes.
    /// </summary>
    public const string AudioGraphMustContainAtLeastOneNode = "AudioGraph must contain at least one node.";

    /// <summary>
    /// Message when node has invalid name.
    /// </summary>
    public const string NodeHasInvalidName = "Node '{0}' has an invalid name: must be non-null, non-empty, and not whitespace.";

    /// <summary>
    /// Message when node has null component pointer.
    /// </summary>
    public const string NodeHasNullComponentPointer = "Node '{0}' has a null component pointer (nint.Zero).";

    /// <summary>
    /// Message when node has self-reference in Prev.
    /// </summary>
    public const string NodeHasSelfReferenceInPrev = "Node '{0}' has a self-reference in Prev.";

    /// <summary>
    /// Message when node has self-reference in Next.
    /// </summary>
    public const string NodeHasSelfReferenceInNext = "Node '{0}' has a self-reference in Next.";

    /// <summary>
    /// Message when audio graph contains a cycle.
    /// </summary>
    public const string AudioGraphContainsCycle = "AudioGraph contains a cycle involving node '{0}'.";

    /// <summary>
    /// Message when node is part of disconnected component.
    /// </summary>
    public const string NodeIsPartOfDisconnectedComponent = "Node '{0}' is part of a disconnected component.";

    /// <summary>
    /// Message when audio graph is invalid with problems.
    /// </summary>
    public const string AudioGraphIsInvalidProblems = "AudioGraph is invalid. Problems:\n{0}";
}