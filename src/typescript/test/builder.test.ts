import {
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
import { FilterBuilder } from "../src/builder";

describe("FilterBuilder - single conditions", () => {
  it("should build equals filter", () => {
    // Arrange
    const builder = new FilterBuilder().where("name").eq("Batman");

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(EqualsFilter);
    expect((result as EqualsFilter).field).toBe("name");
    expect((result as EqualsFilter).value).toBe("Batman");
  });

  it("should build contains filter", () => {
    // Arrange
    const builder = new FilterBuilder().where("name").contains("bat");

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(ContainsFilter);
    expect((result as ContainsFilter).value).toBe("bat");
  });

  it("should build startsWith filter", () => {
    // Arrange
    const builder = new FilterBuilder().where("name").startsWith("Bat");

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(StartsWithFilter);
  });

  it("should build endsWith filter", () => {
    // Arrange
    const builder = new FilterBuilder().where("name").endsWith("man");

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(EndsWithFilter);
  });

  it("should build gt filter", () => {
    // Arrange
    const builder = new FilterBuilder().where("age").gt(18);

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(GreaterThanFilter);
    expect((result as GreaterThanFilter).value).toBe(18);
  });

  it("should build gte filter", () => {
    // Arrange
    const builder = new FilterBuilder().where("age").gte(18);

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(GreaterThanOrEqualFilter);
  });

  it("should build lt filter", () => {
    // Arrange
    const builder = new FilterBuilder().where("age").lt(65);

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(LessThanFilter);
  });

  it("should build lte filter", () => {
    // Arrange
    const builder = new FilterBuilder().where("age").lte(65);

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(LessThanOrEqualFilter);
  });

  it("should build not filter", () => {
    // Arrange
    const builder = new FilterBuilder().where("deleted").not(true);

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(NotFilter);
    expect((result as NotFilter).filter).toBeInstanceOf(EqualsFilter);
  });
});

describe("FilterBuilder - multiple conditions", () => {
  it("should combine two conditions as AND", () => {
    // Arrange
    const builder = new FilterBuilder()
      .where("name")
      .contains("bat")
      .andWhere("age")
      .gte(18);

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(AndFilter);
    expect((result as AndFilter).filters.length).toBe(2);
  });

  it("should build OR filter", () => {
    // Arrange
    const builder = new FilterBuilder().where("status").or("active", "pending");

    // Act
    const result = builder.build();

    // Assert
    expect(result).toBeInstanceOf(OrFilter);
    const orFilter = result as OrFilter;
    expect(orFilter.left).toBeInstanceOf(EqualsFilter);

    const left = orFilter.left as EqualsFilter;
    expect(left.field).toBe("status");
    expect(left.value).toBe("active");

    const right = orFilter.right as EqualsFilter;
    expect(right.field).toBe("status");
    expect(right.value).toBe("pending");
  });
});

describe("FilterBuilder - errors", () => {
  it("should throw when no conditions added", () => {
    // Arrange
    const builder = new FilterBuilder();

    // Act & Assert
    expect(() => builder.build()).toThrow();
  });
});
