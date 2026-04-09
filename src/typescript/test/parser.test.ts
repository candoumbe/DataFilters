import {
  IFilter,
  AndFilter,
  ContainsFilter,
  EndsWithFilter,
  EqualsFilter,
  GreaterThanFilter,
  GreaterThanOrEqualFilter,
  LessThanFilter,
  LessThanOrEqualFilter,
  NotFilter,
  OrFilter,
  StartsWithFilter,
} from "../src/expressions";
import { parse } from "../src/parser";
import { FilterLogic, FilterOptions } from "../src/filterOptions";

describe("parse - equals", () => {
  const cases: [string, unknown, { field: string; value: unknown }][] = [
    ["name=Batman", EqualsFilter, { field: "name", value: "Batman" }],
    ["name=*bat*", ContainsFilter, { field: "name", value: "bat" }],
    ["name=Bat*", StartsWithFilter, { field: "name", value: "Bat" }],
    ["name=Bat\\*", EqualsFilter, { field: "name", value: "Bat*" }],
  ];

  test.each(cases)(
    'parse("%s") should return expected type with correct field and value',
    (expression, expectedType, expected) => {
      // Arrange / Act

      // Act
      const result: IFilter = parse(expression);

      // Assert
      expect(result).toBeInstanceOf(
        expectedType as unknown as new () => unknown,
      );
      const f = result as unknown as { field: string; value: unknown };
      expect(f.field).toBe(expected.field);
      expect(f.value).toBe(expected.value);
    },
  );
});

describe("parse - contains", () => {
  it("should parse contains expression", () => {
    // Arrange
    const expression = "name=*bat*";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(ContainsFilter);
    expect((result as ContainsFilter).value).toBe("bat");
  });
});

describe("parse - startsWith", () => {
  it("should parse starts-with expression", () => {
    // Arrange
    const expression = "name=Bat*";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(StartsWithFilter);
    expect((result as StartsWithFilter).value).toBe("Bat");
  });
});

describe("parse - endsWith", () => {
  it("should parse ends-with expression", () => {
    // Arrange
    const expression = "name=*man";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(EndsWithFilter);
    expect((result as EndsWithFilter).value).toBe("man");
  });
});

describe("parse - negation", () => {
  it("should parse not-equals expression", () => {
    // Arrange
    const expression = "name=!Batman";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(NotFilter);
    const inner = (result as NotFilter).filter;
    expect(inner).toBeInstanceOf(EqualsFilter);
    expect((inner as EqualsFilter).value).toBe("Batman");
  });
});

describe("parse - range", () => {
  it("should parse closed range [min TO max]", () => {
    // Arrange
    const expression = "age=[18 TO 65]";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(AndFilter);
    const f = result as AndFilter;
    expect(f.filters[0]).toBeInstanceOf(GreaterThanOrEqualFilter);
    expect(f.filters[1]).toBeInstanceOf(LessThanOrEqualFilter);
  });

  it("should parse open range ]min TO max[", () => {
    // Arrange
    const expression = "age=]18 TO 65[";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(AndFilter);
    const f = result as AndFilter;
    expect(f.filters[0]).toBeInstanceOf(GreaterThanFilter);

    const greaterThanFilter = f.filters[0] as GreaterThanFilter;
    expect(greaterThanFilter.field).toBe("age");
    expect(greaterThanFilter.value).toBe("18");

    expect(f.filters[1]).toBeInstanceOf(LessThanFilter);
    const lessThanFilter = f.filters[1] as LessThanFilter;
    expect(lessThanFilter.field).toBe("age");
    expect(lessThanFilter.value).toBe("65");
  });

  it("should parse half-open range [min TO *[", () => {
    // Arrange
    const expression = "age=[18 TO *[";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(GreaterThanOrEqualFilter);
    const greaterThanOrEqualFilter = result as GreaterThanOrEqualFilter;
    expect(greaterThanOrEqualFilter.field).toBe("age");
    expect(greaterThanOrEqualFilter.value).toBe("18");
  });

  it("should parse half-open range ]* TO max]", () => {
    // Arrange
    const expression = "age=]* TO 65]";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(LessThanOrEqualFilter);
    const lessThanOrEqualFilter = result as LessThanOrEqualFilter;
    expect(lessThanOrEqualFilter.field).toBe("age");
    expect(lessThanOrEqualFilter.value).toBe("65");
  });
});

describe("parse - AND combination", () => {
  it("should parse expressions separated by & as AND", () => {
    // Arrange
    const expression = "name=*bat*&age=[18 TO *[";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(AndFilter);
    const andFilter = result as AndFilter;

    expect(andFilter.filters.length).toBe(2);
    expect(andFilter.filters[0]).toBeInstanceOf(ContainsFilter);
    const containsFilter = andFilter.filters[0] as ContainsFilter;
    expect(containsFilter.field).toBe("name");
    expect(containsFilter.value).toBe("bat");

    expect(andFilter.filters[1]).toBeInstanceOf(GreaterThanOrEqualFilter);
    const greatThanOrEqualToFilter = andFilter
      .filters[1] as GreaterThanOrEqualFilter;

    expect(greatThanOrEqualToFilter.field).toBe("age");
    expect(greatThanOrEqualToFilter.value).toBe("18");
  });

  it("should parse comma-separated expressions on same property as AND", () => {
    // Arrange
    const expression = "name=*an,bat*";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(AndFilter);
    const andFilter = result as AndFilter;

    expect(andFilter.filters.length).toBe(2);
    expect(andFilter.filters[0]).toBeInstanceOf(EndsWithFilter);
    const endsWithFilter = andFilter.filters[0] as EndsWithFilter;
    expect(endsWithFilter.field).toBe("name");
    expect(endsWithFilter.value).toBe("an");

    expect(andFilter.filters[1]).toBeInstanceOf(StartsWithFilter);
    const startsWithFilter = andFilter.filters[1] as StartsWithFilter;
    expect(startsWithFilter.field).toBe("name");
    expect(startsWithFilter.value).toBe("bat");
  });
});

describe("parse - OR combination", () => {
  it("should parse pipe-separated expressions as OR", () => {
    // Arrange
    const expression = "status=active|pending";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(OrFilter);
    const orFilter = result as OrFilter;
    expect(orFilter.filters.length).toBe(2);
    expect(orFilter.filters[0]).toBeInstanceOf(EqualsFilter);
    const left = orFilter.filters[0] as EqualsFilter;
    expect(left.field).toBe("status");
    expect(left.value).toBe("active");

    expect(orFilter.filters[1]).toBeInstanceOf(EqualsFilter);
    const right = orFilter.filters[1] as EqualsFilter;
    expect(right.field).toBe("status");
    expect(right.value).toBe("pending");
  });

  it("should keep escaped pipe as literal value", () => {
    // Arrange
    const expression = "status=active\\|pending";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(EqualsFilter);
    const equalsFilter = result as EqualsFilter;
    expect(equalsFilter.field).toBe("status");
    expect(equalsFilter.value).toBe("active|pending");
  });
});

describe("parse - escaped reserved symbols", () => {
  it("should keep escaped top-level comma as literal", () => {
    // Arrange
    const expression = "name=foo\\,bar";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(EqualsFilter);
    const equalsFilter = result as EqualsFilter;
    expect(equalsFilter.field).toBe("name");
    expect(equalsFilter.value).toBe("foo,bar");
  });

  it("should keep escaped ampersand as literal", () => {
    // Arrange
    const expression = "name=foo\\&bar";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(EqualsFilter);
    const equalsFilter = result as EqualsFilter;
    expect(equalsFilter.field).toBe("name");
    expect(equalsFilter.value).toBe("foo&bar");
  });

  it("should not create NotFilter when negation char is escaped", () => {
    // Arrange
    const expression = "name=\\!abc";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(EqualsFilter);
    const equalsFilter = result as EqualsFilter;
    expect(equalsFilter.field).toBe("name");
    expect(equalsFilter.value).toBe("!abc");
  });

  it("should parse escaped wildcard as literal value", () => {
    // Arrange
    const expression = "name=foo\\*";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(EqualsFilter);
    const equalsFilter = result as EqualsFilter;
    expect(equalsFilter.field).toBe("name");
    expect(equalsFilter.value).toBe("foo*");
  });

  it("should parse escaped backslash as literal backslash", () => {
    // Arrange
    const expression = "name=foo\\\\bar";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(EqualsFilter);
    const equalsFilter = result as EqualsFilter;
    expect(equalsFilter.field).toBe("name");
    expect(equalsFilter.value).toBe("foo\\bar");
  });
});

describe("parse - errors", () => {
  it("should throw on empty expression", () => {
    // Arrange / Act / Assert
    expect(() => parse("")).toThrow();
  });

  it("should throw on whitespace-only expression", () => {
    // Arrange / Act / Assert
    expect(() => parse("   ")).toThrow();
  });

  it("should throw on missing field", () => {
    // Arrange / Act / Assert
    expect(() => parse("=value")).toThrow();
  });

  it("should throw on missing equals sign", () => {
    // Arrange / Act / Assert
    expect(() => parse("noequals")).toThrow();
  });
});

describe("parse - parameterized", () => {
  const cases: [string, FilterOptions | undefined, unknown][] = [
    ["name=Batman", undefined, EqualsFilter],
    ["name=*bat*", undefined, ContainsFilter],
    ["name=Bat*", undefined, StartsWithFilter],
    ["name=*man", undefined, EndsWithFilter],
    ["name=!Batman", undefined, NotFilter],
    ["name=!Batman&age=30", { logic: FilterLogic.Or }, OrFilter],
    ["name=!Batman&age=30", undefined, AndFilter],
  ];

  test.each(cases)(
    'parse("%s" with options "%o") should return expected type',
    (expression, options, expectedType) => {
      // Arrange / Act
      const result = parse(expression, options);

      // Assert
      expect(result).toBeInstanceOf(
        expectedType as unknown as new () => unknown,
      );
    },
  );
});

describe("parse - FilterOptions", () => {
  it("should use AndFilter by default (no options)", () => {
    // Arrange
    const expression = "name=Batman&age=30";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(AndFilter);
  });

  it("should use AndFilter when options.logic is And", () => {
    // Arrange
    const expression = "name=Batman&age=30";
    const options = new FilterOptions({ logic: FilterLogic.And });

    // Act
    const result: IFilter = parse(expression, options);

    // Assert
    expect(result).toBeInstanceOf(AndFilter);
    const andFilter = result as AndFilter;
    expect(andFilter.filters.length).toBe(2);
    expect(andFilter.filters[0]).toBeInstanceOf(EqualsFilter);
    const left = andFilter.filters[0] as EqualsFilter;
    expect(left.field).toBe("name");
    expect(left.value).toBe("Batman");

    expect(andFilter.filters[1]).toBeInstanceOf(EqualsFilter);
    const right = andFilter.filters[1] as EqualsFilter;
    expect(right.field).toBe("age");
    expect(right.value).toBe("30");
  });

  it("should use OrFilter when options.logic is Or", () => {
    // Arrange
    const expression = "name=Batman&age=30";
    const options = new FilterOptions({ logic: FilterLogic.Or });

    // Act
    const result: IFilter = parse(expression, options);

    // Assert
    expect(result).toBeInstanceOf(OrFilter);
    const orFilter = result as OrFilter;
    expect(orFilter.filters.length).toBe(2);
    expect(orFilter.filters[0]).toBeInstanceOf(EqualsFilter);
    const left = orFilter.filters[0] as EqualsFilter;
    expect(left.field).toBe("name");
    expect(left.value).toBe("Batman");

    expect(orFilter.filters[1]).toBeInstanceOf(EqualsFilter);
    const right = orFilter.filters[1] as EqualsFilter;
    expect(right.field).toBe("age");
    expect(right.value).toBe("30");
  });

  it("should parse query string with '&' as AndFilter by default", () => {
    // Arrange
    const expression = "name=*bat*&age=[18 TO *[";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(AndFilter);
    const andFilter = result as AndFilter;
    expect(andFilter.filters.length).toBe(2);

    expect(andFilter.filters[0]).toBeInstanceOf(ContainsFilter);
    const containsFilter = andFilter.filters[0] as ContainsFilter;
    expect(containsFilter.field).toBe("name");
    expect(containsFilter.value).toBe("bat");

    expect(andFilter.filters[1]).toBeInstanceOf(GreaterThanOrEqualFilter);
    const greaterThanOrEqualFilter = andFilter
      .filters[1] as GreaterThanOrEqualFilter;
    expect(greaterThanOrEqualFilter.field).toBe("age");
    expect(greaterThanOrEqualFilter.value).toBe("18");
  });

  it("should parse URL-encoded query string with '&' as AndFilter", () => {
    // Arrange
    const expression = "name=%2Abat%2A&age=%5B18%20TO%20*%5B";

    // Act
    const result: IFilter = parse(expression);

    // Assert
    expect(result).toBeInstanceOf(AndFilter);
    const andFilter = result as AndFilter;
    expect(andFilter.filters.length).toBe(2);

    expect(andFilter.filters[0]).toBeInstanceOf(ContainsFilter);
    const containsFilter = andFilter.filters[0] as ContainsFilter;
    expect(containsFilter.field).toBe("name");
    expect(containsFilter.value).toBe("bat");

    expect(andFilter.filters[1]).toBeInstanceOf(GreaterThanOrEqualFilter);
    const greaterThanOrEqualFilter = andFilter
      .filters[1] as GreaterThanOrEqualFilter;
    expect(greaterThanOrEqualFilter.field).toBe("age");
    expect(greaterThanOrEqualFilter.value).toBe("18");
  });

  it("should default logic to And when constructed without arguments", () => {
    // Arrange
    const options = new FilterOptions();

    // Assert
    expect(options.logic).toBe(FilterLogic.And);
  });

  it("should apply Or logic on inherited-field comma parts", () => {
    // Arrange
    const expression = "name=*an,bat*";
    const options = new FilterOptions({ logic: FilterLogic.Or });

    // Act
    const result: IFilter = parse(expression, options);

    // Assert
    expect(result).toBeInstanceOf(OrFilter);
    const orFilter = result as OrFilter;
    expect(orFilter.filters.length).toBe(2);
    expect(orFilter.filters[0]).toBeInstanceOf(EndsWithFilter);
    expect(orFilter.filters[1]).toBeInstanceOf(StartsWithFilter);
  });
});
