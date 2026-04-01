import { EqualsFilterExpression, OrFilter } from '../src/expressions';

describe('OrFilter', () => {
  it('should serialize with logic and nested filters', () => {
    // Arrange
    const f = new OrFilter([
      new EqualsFilterExpression('status', 'active'),
      new EqualsFilterExpression('status', 'pending'),
    ]);

    // Act
    const result = f.toDict();

    // Assert
    expect(result['logic']).toBe('or');
    expect((result['filters'] as unknown[]).length).toBe(2);
  });
});
