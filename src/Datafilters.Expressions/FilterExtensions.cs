using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using static DataFilters.FilterOperator;
using static System.Linq.Expressions.Expression;

namespace DataFilters
{
    public static class FilterExtensions
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
            static object ConvertObjectToDateTime(object source, Type targetType)
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

            static Expression ComputeIsNotEmpty(MemberExpression property) => IsNotAStringAndIsEnumerable(property.Type)
                    ? Not(ComputeIsEmpty(property))
                    : (Expression)NotEqual(property, Constant(string.Empty));

            static bool IsNotAStringAndIsEnumerable(Type propertyType) => propertyType != typeof(string)
                                                                          && propertyType.IsAssignableToGenericType(typeof(IEnumerable<>));

            static Expression ComputeIsEmpty(MemberExpression property) => IsNotAStringAndIsEnumerable(property.Type)
                    ? Not(Call(typeof(Enumerable),
                           nameof(Enumerable.Any),
                           new Type[] { property.Type.GenericTypeArguments[0] },
                           property))
                    : (Expression)Equal(property, Constant(string.Empty));

            static Expression ComputeEquals(MemberExpression property, object value)
            {
                Expression equals = null;
                ConstantExpression constantExpression = ComputeConstantExpressionBasedOnPropertyExpressionTargetTypeAndValue(property.Type, value);

                if (IsNotAStringAndIsEnumerable(property.Type))
                {
                    ParameterExpression pe = Parameter(property.Type.GenericTypeArguments[0]);
                    equals = Call(typeof(Enumerable),
                                  nameof(Enumerable.Any),
                                  new Type[] { property.Type.GenericTypeArguments[0] },
                                  property,
                                  Lambda(Equal(pe, constantExpression), new[] { pe }));
                }
                else
                {
                    equals = Equal(property, constantExpression);
                }

                return equals;
            }

            static bool IsDatetimeMember(Type memberType) => memberType == typeof(DateTime)
                                                             || memberType == typeof(DateTime?)
                                                             || memberType == typeof(DateTimeOffset)
                                                             || memberType == typeof(DateTimeOffset?);

            static ConstantExpression ComputeConstantExpressionBasedOnPropertyExpressionTargetTypeAndValue(Type memberType, object value)
            {
                ConstantExpression ce;

                if (IsDatetimeMember(memberType))
                {
                    ce = Constant(ConvertObjectToDateTime(value, memberType), memberType);
                }
                else if (IsNotAStringAndIsEnumerable(memberType))
                {
                    Type parameterType = memberType.GenericTypeArguments?.SingleOrDefault() ?? typeof(object);
                    ce = Constant(value, parameterType);
                }
                else
                {
                    ce = Constant(value, memberType);
                }

                return ce;
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

                            Type memberType = (property.Member as PropertyInfo)?.PropertyType;
                            ConstantExpression constantExpression = ComputeConstantExpressionBasedOnPropertyExpressionTargetTypeAndValue(memberType, df.Value);

                            Expression body = df.Operator switch
                            {
                                NotEqualTo => NotEqual(property, constantExpression),
                                IsNull => Equal(property, Constant(null)),
                                IsNotNull => NotEqual(property, Constant(null)),
                                FilterOperator.LessThan => LessThan(property, constantExpression),
                                FilterOperator.GreaterThan => GreaterThan(property, constantExpression),
                                FilterOperator.GreaterThanOrEqual => GreaterThanOrEqual(property, constantExpression),
                                LessThanOrEqualTo => LessThanOrEqual(property, constantExpression),
                                StartsWith => Call(property, typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) }), constantExpression),
                                NotStartsWith => Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) }), constantExpression)),
                                EndsWith => Call(property, typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) }), constantExpression),
                                NotEndsWith => Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) }), constantExpression)),
                                Contains => Call(property, typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) }), constantExpression),
                                NotContains => Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) }), constantExpression)),
                                IsEmpty => ComputeIsEmpty(property),
                                IsNotEmpty => ComputeIsNotEmpty(property),
                                EqualTo => ComputeEquals(property, df.Value),
                                _ => throw new ArgumentOutOfRangeException(nameof(filter), df.Operator, "Unsupported operator")
                            };
                            filterExpression = Lambda<Func<T, bool>>(body, pe);
                        }

                        break;
                    }

                case MultiFilter dcf:
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