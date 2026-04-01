import { GreaterThanOrEqualFilter } from '../src/expressions';

describe('GreaterThanOrEqualFilter', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new GreaterThanOrEqualFilter('age', 18);

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'age', op: 'gte', value: 18 });
  });

  it('op field should be gte', () => {
    // Arrange / Act
    const dict = new GreaterThanOrEqualFilter('f', 0).toDict();

    // Assert
    expect(dict['op']).toBe('gte');
  });
});
