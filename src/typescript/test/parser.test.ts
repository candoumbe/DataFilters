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

describe("parse - equals", () => {
    it("should parse simple equals expression", () => {
        // Arrange
        const expression = "name=Batman";

        // Act
        const result: IFilter = parse(expression);

        // Assert
        expect(result).toBeInstanceOf(EqualsFilter);
        const f = result as EqualsFilter;
        expect(f.field).toBe("name");
        expect(f.value).toBe("Batman");
    });
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
    it("should parse comma-separated expressions as AND", () => {
        // Arrange
        const expression = "name=*bat*,age=[18 TO *[";

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
        const equalsFilter1 = orFilter.filters[0] as EqualsFilter;
        expect(equalsFilter1.field).toBe("status");
        expect(equalsFilter1.value).toBe("active");

        expect(orFilter.filters[1]).toBeInstanceOf(EqualsFilter);
        const equalsFilter2 = orFilter.filters[1] as EqualsFilter;
        expect(equalsFilter2.field).toBe("status");
        expect(equalsFilter2.value).toBe("pending");
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
    const cases: [string, unknown][] = [
        ["name=Batman", EqualsFilter],
        ["name=*bat*", ContainsFilter],
        ["name=Bat*", StartsWithFilter],
        ["name=*man", EndsWithFilter],
        ["name=!Batman", NotFilter],
    ];

    test.each(cases)(
        'parse("%s") should return expected type',
        (expression, expectedType) => {
            // Arrange / Act
            const result = parse(expression);

            // Assert
            expect(result).toBeInstanceOf(
                expectedType as unknown as new () => unknown,
            );
        },
    );
});
