//-----------------------------------------------------------------------
// <copyright file="QueryFromClause.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.
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
//-----------------------------------------------------------------------
namespace Microsoft.StyleCop.CSharp
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a from clause in a query expression.
    /// </summary>
    public sealed class QueryFromClause : QueryClauseWithExpression
    {
        #region Internal Constructors

        /// <summary>
        /// Initializes a new instance of the QueryFromClause class.
        /// </summary>
        /// <param name="proxy">Proxy object for the clause.</param>
        /// <param name="expression">The range expression.</param>
        internal QueryFromClause(CodeUnitProxy proxy, Expression expression) 
            : base(proxy, QueryClauseType.From, expression)
        {
            Param.AssertNotNull(proxy, "proxy");
            Param.AssertNotNull(expression, "expression");
        }

        #endregion Internal Constructors

        #region Public Override Properties

        /// <summary>
        /// Gets the variables defined within this clause.
        /// </summary>
        public override IList<IVariable> Variables
        {
            get
            {
                IVariable rangeVariable = this.RangeVariable;
                if (rangeVariable != null)
                {
                    return new IVariable[] { rangeVariable };
                }

                return CsParser.EmptyVariableArray;
            }
        }

        #endregion Public Override Properties

        #region Public Properties

        /// <summary>
        /// Gets the variable that ranges over the values in the query result.
        /// </summary>
        public IVariable RangeVariable
        {
            get
            {
                // Find the 'from' keyword.
                FromToken fromToken = this.FindFirstChild<FromToken>();
                if (fromToken == null)
                {
                    return null;
                }

                return ExtractQueryVariable(fromToken.FindNextSibling<Token>(), true, false);
            }
        }

        #endregion Public Properties
    }
}