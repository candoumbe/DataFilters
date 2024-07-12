using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataFilters.Expressions
{
#if NET6_0
    using DateOnlyTimeOnly.AspNet.Converters;
    using System.ComponentModel;
#endif

    using static Expression;
    using static FilterOperator;
    using static NullableValueBehavior;

    /// <summary>
    /// The `FilterExtensions` class provides extension methods for building expression trees from `IFilter` instances.
    /// It allows for filtering data based on various conditions.
    /// </summary>
    /// <remarks>
    /// Example Usage:
    /// <code>
    /// // Create a filter
    /// IFilter filter = new Filter
    /// {
    ///     Field = "Name",
    ///     Operator = FilterOperator.EqualTo,
    ///     Value = "John"
    /// };
    ///
    /// // Build an expression tree
    /// Expression&lt;Func&lt;Person, bool&gt;&gt; expression = filter.ToExpression&lt;Person&gt;();
    ///
    /// // Use the expression to filter data
    /// var filteredData = data.Where(expression.Compile());
    /// </code>
    /// </remarks>
    public static class FilterExtensions
    {
#if NET6_0
        private readonly static ISet<bool> HackZone = new HashSet<bool>();
#endif

        /// <summary>
        /// List of all primitives types
        /// </summary>
        /// <value></value>
        private static readonly Type[] PrimitiveTypes = [
            typeof(string),
            typeof(Guid),
            typeof(Guid?),
            typeof(int),
            typeof(int?),
            typeof(long?),
            typeof(long?),
            typeof(short?),
            typeof(short?),
            typeof(decimal?),
            typeof(decimal?),
            typeof(double?),
            typeof(double?),
            typeof(ushort?),
            typeof(ushort?),
            typeof(uint?),
            typeof(uint?),
            typeof(ulong?),
            typeof(ulong?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
#if NET6_0_OR_GREATER
            typeof(DateOnly), typeof(DateOnly?),
            typeof(TimeOnly), typeof(TimeOnly?),
#endif
            typeof(bool),
            typeof(bool?),
            typeof(char),
            typeof(char?)
        ];

        /// <summary>
        /// Builds an <see cref="Expression{TDelegate}"/> tree from a <see cref="IFilter"/> instance.
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="filter"><see cref="IFilter"/> instance to build an <see cref="Expression{TDelegate}"/> tree from.</param>
        /// <param name="nullableValueBehavior">Indicates if a "null check" should be added to avoid any potential <see cref="NullReferenceException"/> when accessing a property</param>
        /// <returns><see cref="Expression{TDelegate}"/></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="filter"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Setting <paramref name="nullableValueBehavior"/> to <see cref="AddNullCheck"/> can result in a little overhead
        /// </remarks>
        public static Expression<Func<T, bool>> ToExpression<T>(this IFilter filter, NullableValueBehavior nullableValueBehavior = NoAction)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), $"{nameof(filter)} cannot be null");
            }

#if NET6_0
            // HACK require to handle DateOnly and TimeOnly types.
            if (HackZone.Add(true))
            {
                TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyTypeConverter)));
                TypeDescriptor.AddAttributes(typeof(TimeOnly), new TypeConverterAttribute(typeof(TimeOnlyTypeConverter)));
            }
#endif

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

                            string[] fields =
                            [
                                .. df.Field.Replace(@"[""", ".")
                                           .Replace(@"""]", string.Empty)
                                           .Split(['.'])
                            ];

                            Expression body = nullableValueBehavior switch
                            {
                                NoAction => ComputeExpression(pe, fields, type, df.Operator, df.Value),
                                AddNullCheck => ComputeNullSafeExpression(pe, fields, type, df.Operator, df.Value),
                                _ => throw new NotSupportedException($"Unsupported '{nullableValueBehavior}' behavior")
                            };

                            filterExpression = Lambda<Func<T, bool>>(body, pe);
                        }

                        break;
                    }

                case MultiFilter dcf:
                    {
                        Expression<Func<T, bool>> expression = null;
                        foreach (IFilter item in dcf.Filters)
                        {
                            expression = expression == null
                                ? item.ToExpression<T>()
                                : MergeExpressions(expression, dcf.Logic, item.ToExpression<T>());
                        }

                        filterExpression = expression;
                        break;
                    }
            }

            return filterExpression;
        }

        private static object ConvertObjectToDateTime(object source, Type targetType)
        {
            object dateTime = null;

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                if (DateTime.TryParse(source?.ToString(), out DateTime result))
                {
                    dateTime = result;
                }
            }
            else if (targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?))
            {
                if (DateTimeOffset.TryParse(source?.ToString(), out DateTimeOffset result))
                {
                    dateTime = result;
                }
            }
#if NET6_0_OR_GREATER
            else if (targetType == typeof(DateOnly) || targetType == typeof(DateOnly?))
            {
                if (DateOnly.TryParse(source?.ToString(), out DateOnly result))
                {
                    dateTime = result;
                }
            }
#endif

            return dateTime;
        }

        private static Expression ComputeBodyExpression(MemberExpression property, FilterOperator @operator, object value)
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
#if NET6_0_OR_GREATER
                StartsWith => Call(property, typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [constantExpression.Value?.GetType() ?? typeof(string)])!, constantExpression),
                NotStartsWith => Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [constantExpression.Value?.GetType() ?? typeof(string)])!, constantExpression)),
                EndsWith => Call(property, typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [value?.GetType() ?? typeof(string)])!, constantExpression),
                NotEndsWith => Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [value?.GetType() ?? typeof(string)])!, constantExpression)),
#else
                StartsWith => Call(property, typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!, constantExpression),
                NotStartsWith => Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [value?.GetType() ?? typeof(string)])!, constantExpression)),
                EndsWith => Call(property, typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [value?.GetType() ?? typeof(string)])!, constantExpression),
                NotEndsWith => Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [value?.GetType() ?? typeof(string)])!, constantExpression)),
#endif
                Contains => ComputeContains(property, value),
                NotContains => Not(Call(property, typeof(string).GetRuntimeMethod(nameof(string.Contains), [value?.GetType() ?? typeof(string)])!, constantExpression)),
                IsEmpty => ComputeIsEmpty(property),
                IsNotEmpty => ComputeIsNotEmpty(property),
                EqualTo => ComputeEquals(property, value),
                _ => throw new NotSupportedException($"Unsupported {@operator} operator when computing a body expression")
            };
        }

        private static Expression ComputeNullSafeBodyExpression(MemberExpression property, FilterOperator @operator, object value)
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
                StartsWith => AndAlso(NotEqual(property, Constant(null)),
                                      Call(property,
                                           typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!,
                                           constantExpression)
                                      ),
                NotStartsWith => AndAlso(NotEqual(property, Constant(null)),
                                         Not(
                                             Call(property,
                                                  typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [value?.GetType() ?? typeof(string)])!,
                                                  constantExpression)
                                             )
                                         ),
                EndsWith => AndAlso(NotEqual(property, Constant(null)),
                                    Call(property,
                                         typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(string)])!,
                                         constantExpression)
                                    ),
                NotEndsWith => AndAlso(NotEqual(property, Constant(null)),
                                       Not(
                                           Call(property,
                                                typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(string)])!,
                                                constantExpression)
                                           )
                                       ),
                Contains => ComputeNullSafeContains(property, value),
                NotContains => Not(ComputeNullSafeContains(property, value)),
                IsEmpty => ComputeNullSafeIsEmpty(property),
                IsNotEmpty => Not(ComputeNullSafeIsEmpty(property)),
                EqualTo => ComputeNullSafeEquals(property, value),
                _ => throw new NotSupportedException($"Unsupported {@operator} operator when computing a body expression")
            };
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Computes and returns a <see cref="ConstantExpression"/> based on the property expression target type and value.
        /// Handles different scenarios based on the member type, including <see cref="DateTime"/>, <see cref="DateOnly"/> (for .NET 6.0 or greater), enumerable and other types. 
        /// </summary>
#else
        /// <summary>
        /// Computes and returns a <see cref="ConstantExpression"/> based on the property expression target type and value.
        /// Handles different scenarios based on the member type, including <see cref="DateTime"/>, enumerable and other types.
        /// </summary>
#endif
        private static ConstantExpression ComputeConstantExpressionBasedOnPropertyExpressionTargetTypeAndValue(Type memberType, object value)
        {
            ConstantExpression ce;

            if (IsDatetimeMember(memberType))
            {
                ce = Constant(ConvertObjectToDateTime(value, memberType), memberType);
            }
#if NET6_0_OR_GREATER
            else if (IsDateOnly(memberType))
            {
                ce = Constant(ConvertObjectToDateTime(value, memberType), memberType);
            }
#endif
            else if (IsNotAStringAndIsEnumerable(memberType))
            {
                Type parameterType = memberType.GenericTypeArguments?.SingleOrDefault() ?? typeof(object);
                ce = Constant(value, parameterType);
            }
            else
            {
                Type valueType = value switch
                {
                    null => memberType,
                    _ => value.GetType()
                };
                ce = Constant(value, valueType);
            }

            return ce;
        }

        private static Expression ComputeIsEmpty(MemberExpression property) => IsNotAStringAndIsEnumerable(property.Type)
                            ? Not(Call(typeof(Enumerable),
                                  nameof(Enumerable.Any),
                                  new[] { property.Type.GenericTypeArguments[0] },
                                  property))
                            : Equal(property, Constant(string.Empty));

        private static Expression ComputeNullSafeIsEmpty(MemberExpression property)
        {
            Expression isEmpty = null;
            if (IsNotAStringAndIsEnumerable(property.Type))
            {
                Expression left = NotEqual(property, Constant(null));
                Expression right = null;
                Type genericType = null;
#if !NETSTANDARD1_3
                if (property.Type.IsGenericType)
                {
                    genericType = property.Type.GenericTypeArguments[0];
                }
#endif

                right = Not(Call(typeof(Enumerable),
                                nameof(Enumerable.Any),
                                genericType is not null
                                    ? [genericType]
                                    : null,
                                property));

                isEmpty = AndAlso(left, right);
            }
            else
            {
                isEmpty = Not(Equal(property, Constant(string.Empty)));
            }

            return isEmpty;
        }

        private static Expression ComputeContains(MemberExpression property, object value)
        {
            Expression contains = null;
            ConstantExpression constantExpression = ComputeConstantExpressionBasedOnPropertyExpressionTargetTypeAndValue(property.Type, value);

            if (IsNotAStringAndIsEnumerable(property.Type))
            {
                Type genericArgType = property.Type.GenericTypeArguments[0];
                ParameterExpression pe = Parameter(genericArgType);

                contains = typeof(string) == genericArgType
                    ? Call(typeof(Enumerable),
                                    nameof(Enumerable.Any),
                                    [typeof(string)],
                                    property,
                                    Lambda(
                                        Call(pe,
                                            typeof(string).GetRuntimeMethod(nameof(string.Contains), [constantExpression.Value?.GetType() ?? typeof(string)])!,
                                            constantExpression), [pe]))
                    : Call(typeof(Enumerable),
                                    nameof(Enumerable.Any),
                                    [property.Type.GenericTypeArguments[0]],
                                    property,
                                    Lambda(Equal(pe, constantExpression), [pe]));
            }
            else
            {
                contains = Call(property,
                                typeof(string).GetRuntimeMethod(nameof(string.Contains), [constantExpression.Value?.GetType() ?? typeof(string)])!,
                                constantExpression);
            }

            return contains;
        }

        private static Expression ComputeNullSafeContains(MemberExpression property, object value)
        {
            Expression contains = null;
            ConstantExpression constantExpression = ComputeConstantExpressionBasedOnPropertyExpressionTargetTypeAndValue(property.Type, value);

            if (IsNotAStringAndIsEnumerable(property.Type))
            {
                Type genericArgType = property.Type.GenericTypeArguments[0];
                ParameterExpression pe = Parameter(genericArgType);

                contains = typeof(string) == genericArgType
                    ? Call(typeof(Enumerable),
                                    nameof(Enumerable.Any),
                                    [typeof(string)],
                                    property,
                                    Lambda(
                                        AndAlso(NotEqual(pe, Constant(null)),
                                                Call(pe,
                                                     typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string)])!,
                                                     constantExpression)), [pe]))
                    : Call(typeof(Enumerable),
                           nameof(Enumerable.Any),
                           [property.Type.GenericTypeArguments[0]],
                           property,
                           Lambda(Equal(pe, constantExpression), [pe]));
            }
            else
            {
                contains = Call(property,
                              typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string)])!,
                              constantExpression);
            }

            return contains;
        }

        private static Expression ComputeEquals(MemberExpression property, object value)
        {
            Expression equals = null;
            ConstantExpression constantExpression = ComputeConstantExpressionBasedOnPropertyExpressionTargetTypeAndValue(property.Type, value);

            if (IsNotAStringAndIsEnumerable(property.Type))
            {
                ParameterExpression pe = Parameter(property.Type.GenericTypeArguments[0]);
                equals = Call(typeof(Enumerable),
                              nameof(Enumerable.Any),
                              [property.Type.GenericTypeArguments[0]],
                              property,
                              Lambda(Equal(pe, constantExpression), [pe]));
            }
            else
            {
                equals = Equal(property, constantExpression);
            }

            return equals;
        }

        private static Expression ComputeNullSafeEquals(MemberExpression property, object value)
        {
            Expression equals = null;
            ConstantExpression constantExpression = ComputeConstantExpressionBasedOnPropertyExpressionTargetTypeAndValue(property.Type, value);

            if (IsNotAStringAndIsEnumerable(property.Type))
            {
                ParameterExpression pe = Parameter(property.Type.GenericTypeArguments[0]);
                equals = Call(typeof(Enumerable),
                              nameof(Enumerable.Any),
                              [property.Type.GenericTypeArguments[0]],
                              property,
                              Lambda(Equal(pe, constantExpression), [pe]));
            }
            else
            {
                equals = Equal(property, constantExpression);
            }

            return equals;
        }

        private static Expression ComputeIsNotEmpty(MemberExpression property) => IsNotAStringAndIsEnumerable(property.Type)
                            ? Not(ComputeIsEmpty(property))
                            : NotEqual(property, Constant(string.Empty));

        private static bool IsNotAStringAndIsEnumerable(Type propertyType) => propertyType != typeof(string)
                                                                              && propertyType.IsAssignableToGenericType(typeof(IEnumerable<>));

        private static Expression ComputeExpression(ParameterExpression pe, IReadOnlyList<string> fields, Type targetType, FilterOperator @operator, object value, MemberExpression property = null)
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
                    Expression localBody;

                    if (!(typeInfo.IsPrimitive || PrimitiveTypes.Contains(enumerableGenericType)))
                    {
                        MemberExpression localProperty = Property(localParameter, fields.Single());
                        localBody = ComputeBodyExpression(localProperty, @operator, value);
                        body = Call(typeof(Enumerable),
                                     nameof(Enumerable.Any),
                                     [enumerableGenericType],
                                     property,
                                     Lambda(localBody, [localParameter])
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
                while (!stopComputingExpression && i < fields.Count)
                {
                    if (IsNotAStringAndIsEnumerable(targetType))
                    {
                        stopComputingExpression = true;
                        Type enumerableGenericType = targetType.GenericTypeArguments[0];
                        ParameterExpression localParameter = Parameter(enumerableGenericType);
                        Expression localBody;

                        fields = fields.Skip(i)
                                       .ToArray();
                        localBody = fields.Any()
                            ? ComputeExpression(localParameter, fields, enumerableGenericType, @operator, value, property)
                            : ComputeBodyExpression(property, @operator, value);

                        body = Call(typeof(Enumerable),
                                    nameof(Enumerable.Any),
                                    [enumerableGenericType],
                                    property,
                                    Lambda(localBody, [localParameter])
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

        private static Expression ComputeNullSafeExpression(ParameterExpression pe, IReadOnlyList<string> fields, Type targetType, FilterOperator @operator, object value, MemberExpression property = null)
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
                    Expression localBody;

                    if (!(typeInfo.IsPrimitive || PrimitiveTypes.Contains(enumerableGenericType)))
                    {
                        MemberExpression localProperty = Property(localParameter, fields.Single());
                        localBody = ComputeNullSafeBodyExpression(localProperty, @operator, value);
                        body = AndAlso(NotEqual(property, Constant(null)),
                                       Call(typeof(Enumerable),
                                            nameof(Enumerable.Any),
                                            [enumerableGenericType],
                                            property,
                                            Lambda(localBody, [localParameter]))
                        );
                    }
                }
                else
                {
                    body = ComputeNullSafeBodyExpression(Property((Expression)property ?? pe, fields.Single()), @operator, value);
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
                        localBody = fields.Any()
                            ? ComputeNullSafeExpression(localParameter, fields.ToArray(), enumerableGenericType, @operator, value, property)
                            : ComputeNullSafeBodyExpression(property, @operator, value);

                        body = AndAlso(NotEqual(property, Constant(null)),
                                       Call(typeof(Enumerable),
                                            nameof(Enumerable.Any),
                                            [enumerableGenericType],
                                            property,
                                            Lambda(localBody, [localParameter])));
                    }
                    else
                    {
                        property = Property(body ?? property ?? (Expression)pe, fields.ElementAt(i));
                        PropertyInfo pi = (PropertyInfo)property.Member;
                        TypeInfo propertyTypeInfo = pi.PropertyType.GetTypeInfo();

                        if (propertyTypeInfo.IsPrimitive || PrimitiveTypes.Contains(pi.PropertyType))
                        {
                            body = ComputeNullSafeBodyExpression(property, @operator, value);
                        }
                        else
                        {
                            stopComputingExpression = true;
                            body = ComputeNullSafeExpression(pe, fields.Skip(i + 1).ToArray(), pi.PropertyType, @operator, value, property);
                        }
                    }

                    i++;
                }
            }

            return body;
        }

        private static Expression<Func<T, bool>> MergeExpressions<T>(Expression<Func<T, bool>> left, FilterLogic logic, Expression<Func<T, bool>> right)
                => logic switch
                {
                    FilterLogic.And => left.AndAlso(right),
                    FilterLogic.Or => left.OrElse(right),
                    _ => throw new NotSupportedException("Unsupported filter logic"),
                };

        // Tests if membertype is a "DateTime" type
        private static bool IsDatetimeMember(Type memberType) => memberType == typeof(DateTime)
                                                         || memberType == typeof(DateTime?)
                                                         || memberType == typeof(DateTimeOffset)
                                                         || memberType == typeof(DateTimeOffset?);

#if NET6_0_OR_GREATER
        private static bool IsDateOnly(Type memberType) => memberType == typeof(DateOnly)
            || memberType == typeof(DateOnly?);
#endif
    }
}