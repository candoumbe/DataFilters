using DataFilters;
using System;
using static System.Linq.Expressions.Expression;
using static DataFilters.FilterOperator;
using System.Reflection;
using System.Linq.Expressions;

namespace Datafilters
{
    public static class FilterToExpressions
    {
        /// <summary>
        /// Builds an <see cref="Expression{Func{T}}"/> tree from a <see cref="IFilter"/> instance.
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="filter"><see cref="IFilter"/> instance to build an <see cref="Expression{TDelegate}"/> tree from.</param>
        /// <returns><see cref="Expression{TDelegate}"/></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="filter"/> is <c>null</c>.</exception>
        public static Expression<Func<T, bool>> ToExpression<T>(this IFilter filter)
        {
            object ConvertObjectToDateTime(object source, Type targetType)
            {
                object dateTime = null;

                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                {
                    if (DateTime.TryParse(source?.ToString(), out DateTime result))
                    {
                        dateTime = result;
                    }
                    else if (targetType == typeof(DateTime?))
                    {
                        dateTime = null;
                    }
                }
                else if (targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?))
                {
                    if (DateTimeOffset.TryParse(source?.ToString(), out DateTimeOffset result))
                    {
                        dateTime = result;
                    }
                    else if (targetType == typeof(DateTimeOffset?))
                    {
                        dateTime = null;
                    }
                }

                return dateTime;
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), $"{nameof(filter)} cannot be null");
            }
            Expression<Func<T, bool>> filterExpression = null;

            switch (filter)
            {
                case Filter df:
                    {
                        if (df.Field == null)
                        {
                            filterExpression = _ => true;
                        }
                        else
                        {
                            Type type = typeof(T);
                            ParameterExpression pe = Parameter(type, "item");

                            string[] fields = df.Field.Split(new[] { '.' });
                            MemberExpression property = null;
                            foreach (string field in fields)
                            {
                                property = property == null
                                    ? Property(pe, field)
                                    : Property(property, field);
                            }

                            Expression body;
                            Type memberType = (property.Member as PropertyInfo)?.PropertyType;
                            ConstantExpression constantExpression = memberType == typeof(DateTime) || memberType == typeof(DateTime?) || memberType == typeof(DateTimeOffset) || memberType == typeof(DateTimeOffset?)
                                ? Constant(ConvertObjectToDateTime(df.Value, memberType), memberType)
                                : Constant(df.Value, memberType);

                            switch (df.Operator)
                            {
                                case NotEqualTo:
                                    body = NotEqual(property, constantExpression);
                                    break;
                                case IsNull:
                                    body = Equal(property, Constant(null));
                                    break;
                                case IsNotNull:
                                    body = NotEqual(property, Constant(null));
                                    break;
                                case FilterOperator.LessThan:
                                    body = LessThan(property, constantExpression);
                                    break;
                                case FilterOperator.GreaterThan:
                                    body = GreaterThan(property, constantExpression);
                                    break;
                                case FilterOperator.GreaterThanOrEqual:
                                    body = GreaterThanOrEqual(property, constantExpression);
                                    break;
                                case StartsWith:
                                    body = Call(property, typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) }), constantExpression);
                                    break;
                                case NotStartsWith:
                                    body = Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) }), constantExpression));
                                    break;
                                case EndsWith:
                                    body = Call(property, typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) }), constantExpression);
                                    break;
                                case NotEndsWith:
                                    body = Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) }), constantExpression));
                                    break;
                                case Contains:
                                    body = Call(property, typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) }), constantExpression);
                                    break;
                                case NotContains:
                                    body = Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) }), constantExpression));
                                    break;
                                case IsEmpty:
                                    body = Equal(property, Constant(string.Empty));
                                    break;
                                case IsNotEmpty:
                                    body = NotEqual(property, Constant(string.Empty));
                                    break;
                                case EqualTo:
                                    body = Equal(property, constantExpression);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(filter), df.Operator, "Unsupported operator");
                            }

                            filterExpression = Lambda<Func<T, bool>>(body, pe);
                        }

                        break;
                    }

                case CompositeFilter dcf:
                    {
                        Expression<Func<T, bool>> expression = null;
                        // local function that can combine two expressions using either AND or OR operators
                        Func<Expression<Func<T, bool>>, Expression<Func<T, bool>>, Expression<Func<T, bool>>> expressionMerger;

                        if (dcf.Logic == FilterLogic.And)
                        {
                            expressionMerger = (first, second) => first.AndAlso(second);
                        }
                        else
                        {
                            expressionMerger = (first, second) => first.OrElse(second);
                        }

                        foreach (IFilter item in dcf.Filters)
                        {
                            expression = expression == null
                                ? item.ToExpression<T>()
                                : expressionMerger(expression, item.ToExpression<T>());
                        }

                        filterExpression = expression;
                        break;
                    }
            }

            return filterExpression;
        }
    }
}