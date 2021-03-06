﻿using System.Linq;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Plugins.Unity.Daemon.Stages.Highlightings;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

[assembly: RegisterConfigurableSeverity(InvalidSignatureWarning.HIGHLIGHTING_ID,
    UnityHighlightingGroupIds.INCORRECT_METHOD_SIGNATURE,
    UnityHighlightingGroupIds.Unity,
    "Incorrect method parameters",
    "Incorrect parameters for expected method signature.",
    Severity.WARNING)]

namespace JetBrains.ReSharper.Plugins.Unity.Daemon.Stages.Highlightings
{
    [ConfigurableSeverityHighlighting(HIGHLIGHTING_ID, CSharpLanguage.Name,
        OverlapResolve = OverlapResolveKind.WARNING,
        ToolTipFormatString = MESSAGE)]
    public class InvalidSignatureWarning : IHighlighting, IUnityHighlighting
    {
        public const string HIGHLIGHTING_ID = "Unity.InvalidParameters";
        public const string MESSAGE = "Incorrect method parameters. Expected '{0}'";

        public InvalidSignatureWarning(IMethodDeclaration methodDeclaration, MethodSignature expectedMethodSignature)
        {
            MethodDeclaration = methodDeclaration;
            ExpectedMethodSignature = expectedMethodSignature;

            ToolTip = string.Format(MESSAGE,
                $"({string.Join(", ", expectedMethodSignature.Parameters.Select(p => p.Type.GetPresentableName(methodDeclaration.Language) + " " + p.Name))})");
        }

        public bool IsValid()
        {
            return MethodDeclaration != null && MethodDeclaration.IsValid();
        }

        public DocumentRange CalculateRange()
        {
            var nameRange = MethodDeclaration.GetNameDocumentRange();
            if (!nameRange.IsValid())
                return DocumentRange.InvalidRange;

            var @params = MethodDeclaration.Params;
            if (@params == null)
                return nameRange;

            var paramsRange = @params.GetDocumentRange();
            if (!paramsRange.IsValid())
                return nameRange;

            if (!paramsRange.IsEmpty)
                return paramsRange;

            var lparRange = MethodDeclaration.LPar?.GetDocumentRange();
            var rparRange = MethodDeclaration.RPar?.GetDocumentRange();
            var startOffset = lparRange != null && lparRange.Value.IsValid()
                ? lparRange.Value
                : paramsRange;
            var endOffset = rparRange != null && rparRange.Value.IsValid()
                ? rparRange.Value
                : paramsRange;

            return new DocumentRange(startOffset.StartOffset, endOffset.EndOffset);
        }

        public string ToolTip { get; }
        public string ErrorStripeToolTip => ToolTip;

        public IMethodDeclaration MethodDeclaration { get; }
        public MethodSignature ExpectedMethodSignature { get; }
    }
}