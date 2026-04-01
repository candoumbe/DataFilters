import { EqualsFilterExpression, OrFilterExpression } from '../src/expressions';

describe('OrFilterExpression', () => {
  it('should serialize with logic and nested filters', () => {
    // Arrange
    const f = new OrFilterExpression([
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
