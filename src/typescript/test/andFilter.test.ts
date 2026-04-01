import { AndFilter, EqualsFilter } from '../src/expressions';

describe('AndFilter', () => {
  it('should serialize with logic and nested filters', () => {
    // Arrange
    const f = new AndFilter([
      new EqualsFilter('a', 1),
      new EqualsFilter('b', 2),
    ]);

    // Act
    const result = f.toDict();

    // Assert
    expect(result['logic']).toBe('and');
    expect((result['filters'] as unknown[]).length).toBe(2);
  });
});
