using DataFilters.AspNetCore.Filters;

using FluentAssertions;

using FsCheck;
using FsCheck.Xunit;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

using Moq;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

using Xunit;
using Xunit.Abstractions;

using static Microsoft.AspNetCore.Http.HttpMethods;

namespace DataFilters.AspNetCore.UnitTests.Filters
{
    public class SelectPropertiesActionFilterAttributeTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public SelectPropertiesActionFilterAttributeTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Property]
        public void Ctor_should_set_properties_accordingly(bool onGet, bool onPost, bool onPut, bool onPatch)
        {
            // Act
            SelectPropertiesActionFilterAttribute attribute = new(onGet, onPost, onPut, onPatch);

            // Assert
            attribute.OnGet.Should()
                           .Be(onGet);
            attribute.OnPost.Should()
                            .Be(onPost);
            attribute.OnPatch.Should()
                             .Be(onPatch);
            attribute.OnPut.Should()
                           .Be(onPut);
        }

        [Fact]
        public void Type_should_be_an_ActionFilterAttribute()
        {
            Type selectPropertiesAttribute = typeof(SelectPropertiesActionFilterAttribute);

            // Assert
            selectPropertiesAttribute.Should()
                                     .NotBeAbstract().And
                                     .NotBeStatic().And
                                     .HaveConstructor(new[] { typeof(bool), typeof(bool), typeof(bool), typeof(bool) }).And
                                     .HaveAccessModifier(FluentAssertions.Common.CSharpAccessModifier.Public);

            selectPropertiesAttribute.Should()
                                     .BeDerivedFrom<ActionFilterAttribute>();
        }

        [Property]
        public void Given_inputs_constructor_should_set_properties_accordingly(bool onGet, bool onPost, bool onPut, bool onPatch)
        {
            _outputHelper.WriteLine($"{nameof(onGet)} : {onGet}");
            _outputHelper.WriteLine($"{nameof(onPost)} : {onPost}");
            _outputHelper.WriteLine($"{nameof(onPut)} : {onPut}");
            _outputHelper.WriteLine($"{nameof(onPatch)} : {onPatch}");

            // Act
            SelectPropertiesActionFilterAttribute attribute = new(onGet: onGet, onPost: onPost, onPatch: onPatch, onPut: onPut);

            // Assert
            attribute.OnGet.Should().Be(onGet);
            attribute.OnPost.Should().Be(onPost);
            attribute.OnPut.Should().Be(onPut);
            attribute.OnPatch.Should().Be(onPatch);
        }

        public static IEnumerable<object[]> OkObjectResultCases
        {
            get
            {
                yield return new object[]
                {
                    Get,
                    new HeaderDictionary(new Dictionary<string, StringValues>
                    {
                        [SelectPropertiesActionFilterAttribute.FieldSelectorHeaderName] = new StringValues("prop")
                    }),
                    new
                    {
                        prop = "value",
                        prop2 = new
                        {
                            subProp = 1,
                            subProp2 = 2
                        }
                    },
                    (Expression<Func<IEnumerable<string>, bool>>)(props => props.Exactly(1)
                                                                           && props.Once(propName => propName =="prop")
                    ),
                    $"The filter is configured to support HTTP verb '{Get}' is supported and '{SelectPropertiesActionFilterAttribute.FieldSelectorHeaderName}' header is set to 'prop'"
                };
            }
        }

        [Theory]
        [MemberData(nameof(OkObjectResultCases))]
        public void Given_request_with_custom_selection_headers_and_controller_returned_OkObjectResult_filter_should_perform_selection(string method,
                                                                                                                                       IHeaderDictionary headers,
                                                                                                                                       object okResultValue,
                                                                                                                                       Expression<Func<IEnumerable<string>, bool>> expectation,
                                                                                                                                       string reason)
        {
            // Arrange
            DefaultHttpContext httpContext = new();
            httpContext.Request.Method = method;
            headers.ForEach(header => httpContext.Request.Headers.TryAdd(header.Key, header.Value));

            ActionContext actionContext = new(
               httpContext,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               new ModelStateDictionary());

            ActionExecutedContext actionExecutedContext = new(actionContext,
                                                              new List<IFilterMetadata>(),
                                                              new Mock<object>())
            {
                Result = new OkObjectResult(okResultValue)
            };

            SelectPropertiesActionFilterAttribute sut = new();

            // Act
            sut.OnActionExecuted(actionExecutedContext);

            // Assert
            IActionResult result = actionExecutedContext.Result;

            IEnumerable<string> fieldNames = result.Should()
                                         .BeAssignableTo<ObjectResult>().Which.Value.Should()
                                         .BeAssignableTo<ExpandoObject>().Which
                                         .Select(kv => kv.Key);

            fieldNames.Should()
                      .Match(expectation, reason);
        }

        [Property]
        public void Given_request_without_custom_selection_headers_and_controller_returned_OkObjectResult_filter_should_perform_no_action(NonWhiteSpaceString method)
        {
            // Arrange
            DefaultHttpContext httpContext = new();
            httpContext.Request.Method = method.Item;

            ActionContext actionContext = new(
               httpContext,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               new ModelStateDictionary());

            OkObjectResult okObjectResult = new OkObjectResult(new
            {
                prop = "value",
                prop2 = new
                {
                    subProp = 1,
                    subProp2 = 2
                }
            });
            ActionExecutedContext actionExecutedContext = new(actionContext,
                                                              new List<IFilterMetadata>(),
                                                              new Mock<object>())
            {
                Result = okObjectResult
            };

            SelectPropertiesActionFilterAttribute sut = new();

            // Act
            sut.OnActionExecuted(actionExecutedContext);

            // Assert
            IActionResult result = actionExecutedContext.Result;

            result.Should()
                  .Be(okObjectResult);
        }
    }
}
