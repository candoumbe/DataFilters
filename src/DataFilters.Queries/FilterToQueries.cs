using Queries.Core.Extensions;
using Queries.Core.Parts.Clauses;
using Queries.Core.Parts.Columns;
using System;
using System.Linq;

namespace DataFilters
{
    public static class FilterToQueries
    {
        /// <summary>
        /// Converts <paramref name="filter"/> to <see cref="IWhereClause"/>.
        /// </summary>
        /// <param name="filter">The filter to convert</param>
        /// <returns></returns>
        public static IWhereClause ToWhere(this IFilter filter)
        {
            IWhereClause clause = null;

            switch (filter)
            {
                case Filter f:
                    {
                        ClauseOperator clauseOperator;
                        object value = f.Value;

                        switch (f.Operator)
                        {
                            case FilterOperator.EqualTo:
                                clauseOperator = ClauseOperator.EqualTo;
                                break;
                            case FilterOperator.NotEqualTo:
                                clauseOperator = ClauseOperator.NotEqualTo;
                                break;
                            case FilterOperator.IsNull:
                                clauseOperator = ClauseOperator.IsNull;
                                break;
                            case FilterOperator.IsNotNull:
                                clauseOperator = ClauseOperator.IsNotNull;
                                break;
                            case FilterOperator.LessThan:
                                clauseOperator = ClauseOperator.LessThan;
                                break;
                            case FilterOperator.GreaterThan:
                                clauseOperator = ClauseOperator.GreaterThan;
                                break;
                            case FilterOperator.GreaterThanOrEqual:
                                clauseOperator = ClauseOperator.GreaterThanOrEqualTo;
                                break;
                            case FilterOperator.StartsWith:
                            case FilterOperator.EndsWith:
                            case FilterOperator.Contains:
                                clauseOperator = ClauseOperator.Like;
                                if (f.Operator == FilterOperator.StartsWith)
                                {
                                    value = $"{value}%";
                                }
                                else if (f.Operator == FilterOperator.EndsWith)
                                {
                                    value = $"%{value}";
                                }
                                else
                                {
                                    value = $"%{value}%";
                                }
                                break;
                            case FilterOperator.NotStartsWith:
                            case FilterOperator.NotEndsWith:
                            case FilterOperator.NotContains:
                                clauseOperator = ClauseOperator.NotLike;
                                if (f.Operator == FilterOperator.NotStartsWith)
                                {
                                    value = $"{value}%";
                                }
                                else if (f.Operator == FilterOperator.NotEndsWith)
                                {
                                    value = $"%{value}";
                                }
                                else
                                {
                                    value = $"%{value}%";
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException($"Unsupported '{f.Operator}' operator");
                        }

                        clause = new WhereClause(f.Field.Field(), clauseOperator, new Literal(value));
                        break;
                    }

                case CompositeFilter cf:
                    {
                        clause = new CompositeWhereClause
                        {
                            Logic = cf.Logic == FilterLogic.And
                                ? ClauseLogic.And
                                : ClauseLogic.Or,
                            Clauses = cf.Filters.Select(item => item.ToWhere())
                        };
                        break;
                    }
            }

            return clause;
        }
    }
}
