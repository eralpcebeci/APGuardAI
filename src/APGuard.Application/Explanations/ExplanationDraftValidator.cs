namespace APGuard.Application.Explanations;

public sealed class ExplanationDraftValidator
{
    public ExplanationFailure Validate(ExplanationDraft draft, IReadOnlyList<AuthoritativeValue> facts,
        IReadOnlyList<ExplanationSource> sources, ExplanationLimits limits)
    {
        if (draft is null || !Enum.IsDefined(draft.ProposedCoverage) || draft.Claims is null || draft.Claims.Count == 0)
            return ExplanationFailure.InvalidDraft;
        if (draft.Claims.Count > limits.MaximumClaims) return ExplanationFailure.BudgetExceeded;
        var ids = new HashSet<string>(StringComparer.Ordinal); long size = 0;
        foreach (var c in draft.Claims)
        {
            if (c is null || !SafeId(c.Id) || !ids.Add(c.Id) || !Enum.IsDefined(c.Kind) || string.IsNullOrWhiteSpace(c.UntrustedText) ||
                c.FinancialReferences is null || c.SourceReferences is null || c.SourceReferences.Count == 0)
                return ExplanationFailure.InvalidDraft;
            size += c.Id.Length + c.UntrustedText.Length;
            if (c.FinancialReferences.Count > 32 || c.SourceReferences.Count > limits.MaximumSources) return ExplanationFailure.BudgetExceeded;
            foreach (var reference in c.FinancialReferences)
                if (reference is null || !Enum.IsDefined(reference.Field) || !facts.Any(f => f.Reference == reference))
                    return ExplanationFailure.InvalidFinancialReference;
            foreach (var support in c.SourceReferences)
            {
                if (support is null || !SafeId(support.SourceId)) return ExplanationFailure.UnknownReference;
                var source = sources.SingleOrDefault(s => s.Id == support.SourceId);
                if (source is null) return ExplanationFailure.UnknownReference;
                if (string.IsNullOrWhiteSpace(support.ExactQuote) || !source.Chunk.Content.Contains(support.ExactQuote, StringComparison.Ordinal))
                    return ExplanationFailure.QuoteMismatch;
                size += support.ExactQuote.Length + support.SourceId.Length;
            }
            if (size > limits.MaximumDraftCharacters) return ExplanationFailure.BudgetExceeded;
        }
        // Exact quote containment and valid IDs do NOT establish entailment, cause or adequacy.
        // UntrustedText (including written-out wrong numbers) is NEVER rendered as an answer.
        return ExplanationFailure.None;
    }
    internal static bool SafeId(string? id) => id is { Length: > 0 and <= 64 } && id.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
}
