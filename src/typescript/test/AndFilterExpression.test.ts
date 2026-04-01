import { AndFilterExpression, EqualsFilterExpression } from '../src/expressions';

describe('AndFilterExpression', () => {
  it('should serialize with logic and nested filters', () => {
    // Arrange
    const f = new AndFilterExpression([
      new EqualsFilterExpression('a', 1),
      new EqualsFilterExpression('b', 2),
    ]);

    // Act
    const result = f.toDict();

    // Assert
    expect(result['logic']).toBe('and');
    expect((result['filters'] as unknown[]).length).toBe(2);
  });
});
