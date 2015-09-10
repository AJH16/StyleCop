﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StyleCopCodeCleanupModule.cs" company="http://stylecop.codeplex.com">
//   MS-PL
// </copyright>
// <license>
//   This source code is subject to terms and conditions of the Microsoft 
//   Public License. A copy of the license can be found in the License.html 
//   file at the root of this distribution. If you cannot locate the  
//   Microsoft Public License, please send an email to dlr@microsoft.com. 
//   By using this source code in any fashion, you are agreeing to be bound 
//   by the terms of the Microsoft Public License. You must not remove this 
//   notice, or any other, from this software.
// </license>
// <summary>
//   Custom StyleCop CodeCleanUp module to fix StyleCop violations.
//   We ensure that most of the ReSharper modules are run before we are.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace StyleCop.ReSharper.CodeCleanup
{
    using System.Collections.Generic;

    using JetBrains.DocumentModel;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Daemon.CSharp.CodeCleanup;
    using JetBrains.ReSharper.Feature.Services.CodeCleanup;
    using JetBrains.ReSharper.Feature.Services.CSharp.CodeCleanup;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.CSharp;
    using JetBrains.ReSharper.Psi.CSharp.Tree;
    using JetBrains.ReSharper.Psi.Files;

    using StyleCop.Diagnostics;
    using StyleCop.ReSharper.CodeCleanup.Rules;
    using StyleCop.ReSharper.Core;
    using StyleCop.ReSharper.ShellComponents;

    /// <summary>
    ///   Custom StyleCop CodeCleanUp module to fix StyleCop violations.
    ///   We ensure that most of the ReSharper modules are run before we are.
    /// </summary>
    [CodeCleanupModule(ModulesBefore = new[] { typeof(UpdateFileHeader), typeof(HighlightingCleanupModule), typeof(ReplaceByVar), typeof(ReformatCode) })]
    public class StyleCopCodeCleanupModule : ICodeCleanupModule
    {
        /// <summary>
        ///   StyleCop descriptor.
        /// </summary>
        private static readonly StyleCopDescriptor Descriptor = new StyleCopDescriptor();

        private readonly StyleCopSettings styleCopSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleCopCodeCleanupModule"/> class.
        /// </summary>
        /// <param name="bootstrapper">
        /// The entry point to the StyleCop API.
        /// </param>
        public StyleCopCodeCleanupModule(StyleCopBootstrapper bootstrapper)
        {
            this.styleCopSettings = bootstrapper.Settings;
        }

        /// <summary>
        /// Gets the collection of option descriptors.
        /// </summary>
        /// <value>
        /// The descriptors.
        /// </value>
        public ICollection<CodeCleanupOptionDescriptor> Descriptors
        {
            get
            {
                return new CodeCleanupOptionDescriptor[] { Descriptor };
            }
        }

        /// <summary>
        /// Gets a value indicating whether the module is available on selection, or on the whole file.
        /// </summary>
        /// <value>
        /// The is available on selection.
        /// </value>
        public bool IsAvailableOnSelection
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the language this module can operate.
        /// </summary>
        /// <value>
        /// The language type.
        /// </value>
        public PsiLanguageType LanguageType
        {
            get
            {
                return CSharpLanguage.Instance;
            }
        }

        /// <summary>
        /// Check if this module can handle given project file.
        /// </summary>
        /// <param name="projectFile">
        /// The project file to check.
        /// </param>
        /// <returns>
        /// <c>True.</c>if the project file is available; otherwise 
        /// <c>False.</c>.
        /// </returns>
        public bool IsAvailable(IPsiSourceFile projectFile)
        {
            return projectFile.GetDominantPsiFile<CSharpLanguage>() != null;
        }

        /// <summary>
        /// Process clean-up on file.
        /// </summary>
        /// <param name="projectFile">
        /// The project file to process.
        /// </param>
        /// <param name="rangeMarker">
        /// The range marker to process.
        /// </param>
        /// <param name="profile">
        /// The code cleanup settings to use.
        /// </param>
        /// <param name="progressIndicator">
        /// The progress indicator.
        /// </param>
        public void Process(
            IPsiSourceFile projectFile, IRangeMarker rangeMarker, CodeCleanupProfile profile, JetBrains.Application.Progress.IProgressIndicator progressIndicator)
        {
            if (!this.IsAvailable(projectFile))
            {
                return;
            }

            ISolution solution = projectFile.GetSolution();

            ICSharpFile file = projectFile.GetDominantPsiFile<CSharpLanguage>() as ICSharpFile;

            if (file == null)
            {
                return;
            }

            if (!profile.GetSetting(Descriptor))
            {
                return;
            }

            var services = solution.GetPsiServices(); 
            services.Transactions.Execute("Code cleanup", () => this.InternalProcess(projectFile.ToProjectFile(), file));

            StyleCopTrace.Out();
        }

        /// <summary>
        /// Get default setting for given profile type.
        /// </summary>
        /// <param name="profile">
        /// The code cleanup profile to use.
        /// </param>
        /// <param name="profileType">
        /// Determine if it is a full or reformat <see cref="CodeCleanup.DefaultProfileType"/>.
        /// </param>
        public void SetDefaultSetting(CodeCleanupProfile profile, CodeCleanup.DefaultProfileType profileType)
        {
            profile.SetSetting(Descriptor, true);
        }

        /// <summary>
        /// Processes all the cleanup.
        /// </summary>
        /// <param name="projectFile">
        /// The project file to clean.
        /// </param>
        /// <param name="file">
        /// The PSI file to clean.
        /// </param>
        private void InternalProcess(IProjectFile projectFile, ICSharpFile file)
        {
            // Process the file for all the different Code Cleanups we have here
            // we do them in a very specific order. Do not change it.
            Settings settings = this.styleCopSettings.GetSettings(projectFile);

            ReadabilityRules.ExecuteAll(file, settings);
            MaintainabilityRules.ExecuteAll(file, settings);
            DocumentationRules.ExecuteAll(file, settings);
            LayoutRules.ExecuteAll(file, settings);
            SpacingRules.ExecuteAll(file, settings);
            OrderingRules.ExecuteAll(file, settings);
        }
    }
}