using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AgentMulder.ReSharper.Domain.Elements.Modules;
using AgentMulder.ReSharper.Domain.Elements.Modules.Impl;
using AgentMulder.ReSharper.Domain.Patterns;
using AgentMulder.ReSharper.Domain.Registrations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Services.CSharp.StructuralSearch;
using JetBrains.ReSharper.Psi.Services.CSharp.StructuralSearch.Placeholders;
using JetBrains.ReSharper.Psi.Services.StructuralSearch;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace AgentMulder.Containers.StructureMap.Patterns.Scan
{
    [Export("ComponentRegistration", typeof(IRegistrationPattern))]
    internal sealed class ScanStatements : FromDescriptorPatternBase
    {
        private static readonly IStructuralSearchPattern pattern = 
            new CSharpStructuralSearchPattern("$registry$.Scan($id$ => { $statements$ })",
                new ExpressionPlaceholder("registry", "global::StructureMap.Configuration.DSL.IRegistry"),
                new ArgumentPlaceholder("id"),
                new StatementPlaceholder("statements", -1, -1));

        [ImportingConstructor]
        public ScanStatements([ImportMany] IEnumerable<IBasedOnPattern> basedOnPatterns)
            : base(pattern, basedOnPatterns)
        {
            ModuleExtractor.AddExtractor(new CallingAssemblyExtractor("StructureMap.Graph.IAssemblyScanner", "TheCallingAssembly"));
        }

        public override IEnumerable<IComponentRegistration> GetComponentRegistrations(ITreeNode registrationRootElement)
        {
            IStructuralMatchResult match = Match(registrationRootElement);

            if (match.Matched)
            {
                var invocationExpressions = match.GetMatchedElementList("statements")
                                                 .Cast<IExpressionStatement>()
                                                 .Select(statement => statement.Expression)
                                                 .OfType<IInvocationExpression>();

                IEnumerable<IModule> modules = invocationExpressions.SelectNotNull(expression => ModuleExtractor.GetTargetModule(expression.InvokedExpression));

                foreach (var module in modules)
                {
                    yield break;
                }


                //return from argumentPattern in scanPatterns
                //       from expression in invocationExpressions
                //       from registration in argumentPattern.GetComponentRegistrations(expression)
                //       select registration;
            }
        }
    }
}