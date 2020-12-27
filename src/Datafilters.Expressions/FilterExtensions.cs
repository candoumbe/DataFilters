using DataFilters.Grammar.Exceptions;
using System;
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
        /// List of all primitives types
        /// </summary>
        /// <value></value>
        private static readonly Type[] PrimitiveTypes = {
            typeof(string),
            typeof(Guid), typeof(Guid?),
            typeof(int), typeof(int?),
            typeof(long?), typeof(long?),
            typeof(short?), typeof(short?),
            typeof(decimal?), typeof(decimal?),
            typeof(double?), typeof(double?),
            typeof(ushort?), typeof(ushort?),
            typeof(uint?), typeof(uint?),
            typeof(ulong?), typeof(ulong?),
            typeof(DateTime), typeof(DateTime?),
            typeof(DateTimeOffset), typeof(DateTimeOffset?),
            typeof(bool), typeof(bool?),
            typeof(char), typeof(char?)
        };

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
                    : NotEqual(property, Constant(string.Empty));

            static bool IsNotAStringAndIsEnumerable(Type propertyType) => propertyType != typeof(string)
                                                                          && propertyType.IsAssignableToGenericType(typeof(IEnumerable<>));

            static Expression ComputeIsEmpty(MemberExpression property) => IsNotAStringAndIsEnumerable(property.Type)
                    ? Not(Call(typeof(Enumerable),
                          nameof(Enumerable.Any),
                          new Type[] { property.Type.GenericTypeArguments[0] },
                          property))
                    : Equal(property, Constant(string.Empty));

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

            static Expression ComputeContains(MemberExpression property, object value)
            {
                Expression contains = null;
                ConstantExpression constantExpression = ComputeConstantExpressionBasedOnPropertyExpressionTargetTypeAndValue(property.Type, value);

                if (IsNotAStringAndIsEnumerable(property.Type))
                {
                    Type genericArgType = property.Type.GenericTypeArguments[0];
                    ParameterExpression pe = Parameter(genericArgType);

                    if (typeof(string).Equals(genericArgType))
                    {
                        contains = Call(typeof(Enumerable),
                                        nameof(Enumerable.Any),
                                        new Type[] { typeof(string) },
                                        property,
                                        Lambda(
                                            Call(pe,
                                                typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) }),
                                                constantExpression), new[] { pe }));
                    }
                    else
                    {
                        contains = Call(typeof(Enumerable),
                                        nameof(Enumerable.Any),
                                        new Type[] { property.Type.GenericTypeArguments[0] },
                                        property,
                                        Lambda(Equal(pe, constantExpression), new[] { pe }));
                    }
                }
                else
                {
                    contains = Call(property,
                                  typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) }),
                                  constantExpression);
                }

                return contains;
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

            static Expression ComputeBodyExpression(MemberExpression property, FilterOperator @operator, object value)
            {
                ConstantExpression constantExpression = ComputeConstantExpressionBasedOnPropertyExpressionTargetTypeAndValue(((PropertyInfo)property.Member).PropertyType, value);

                return @operator switch
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
                    Contains => ComputeContains(property, value),
                    NotContains => Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) }), constantExpression)),
                    IsEmpty => ComputeIsEmpty(property),
                    IsNotEmpty => ComputeIsNotEmpty(property),
                    EqualTo => ComputeEquals(property, value),
                    _ => throw new ArgumentOutOfRangeException(nameof(@operator), @operator, "Unsupported operator")
                };
            }

            static Expression ComputeExpression(ParameterExpression pe, IEnumerable<string> fields, Type targetType, FilterOperator @operator, object value, MemberExpression property = null)
            {
                Expression body = null;
                int i = 0;

                if (fields.Once())
                {
                    if (IsNotAStringAndIsEnumerable(targetType))
                    {
                        Type enumerableGenericType = targetType.GenericTypeArguments[0];
                        TypeInfo typeInfo = enumerableGenericType.GetTypeInfo();
                        ParameterExpression localParameter = Parameter(enumerableGenericType);
                        Expression localBody = null;

                        if (typeInfo.IsPrimitive || PrimitiveTypes.Contains(enumerableGenericType))
                        {
                            localBody = ComputeBodyExpression(property, @operator, value);
                        }
                        else
                        {
                            MemberExpression localProperty = Property(localParameter, fields.Single());
                            localBody = ComputeBodyExpression(localProperty, @operator, value);
                            body = Call(typeof(Enumerable),
                                         nameof(Enumerable.Any),
                                         new[] { enumerableGenericType },
                                         property,
                                         Lambda(localBody, new[] { localParameter })
                            );
                        }
                    }
                    else
                    {
                        body = ComputeBodyExpression(Property((Expression)property ?? pe, fields.Single()), @operator, value);
                    }
                }
                else
                {
                    bool stopComputingExpression = false;
                    while (!stopComputingExpression && i < fields.Count())
                    {
                        if (IsNotAStringAndIsEnumerable(targetType))
                        {
                            stopComputingExpression = true;
                            Type enumerableGenericType = targetType.GenericTypeArguments[0];
                            ParameterExpression localParameter = Parameter(enumerableGenericType);
                            Expression localBody;

                            fields = fields.Skip(i)
                                           .ToArray();
                            if (fields.Any())
                            {
                                localBody = ComputeExpression(localParameter, fields.ToArray(), enumerableGenericType, @operator, value, property);
                            }
                            else
                            {
                                localBody = ComputeBodyExpression(property, @operator, value);
                            }

                            body = Call(typeof(Enumerable),
                                        nameof(Enumerable.Any),
                                        new[] { enumerableGenericType },
                                        property,
                                        Lambda(localBody, new[] { localParameter })
                                    );
                        }
                        else
                        {
                            property = Property(body ?? property ?? (Expression)pe, fields.ElementAt(i));
                            PropertyInfo pi = (PropertyInfo)property.Member;
                            TypeInfo propertyTypeInfo = pi.PropertyType.GetTypeInfo();

                            if (propertyTypeInfo.IsPrimitive || PrimitiveTypes.Contains(pi.PropertyType))
                            {
                                body = ComputeBodyExpression(property, @operator, value);
                            }
                            else
                            {
                                stopComputingExpression = true;
                                body = ComputeExpression(pe, fields.Skip(i + 1).ToArray(), pi.PropertyType, @operator, value, property);
                            }
                        }

                        i++;
                    }
                }

                return body;
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

                            string[] fields = df.Field.Replace(@"[""", ".")
                                                      .Replace(@"""]", string.Empty)
                                                      .Split(new[] { '.' })
                                                      .ToArray();

                            Expression body = ComputeExpression(pe, fields, type, df.Operator, df.Value);

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