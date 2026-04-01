import { EqualsFilterExpression, NotFilter } from '../src/expressions';

describe('NotFilter', () => {
  it('should serialize with logic not', () => {
    // Arrange
    const f = new NotFilter(new EqualsFilterExpression('deleted', true));

    // Act
    const result = f.toDict();

    // Assert
    expect(result['logic']).toBe('not');
    expect((result['filters'] as unknown[]).length).toBe(1);
  });

  it('should wrap the inner filter', () => {
    // Arrange
    const inner = new EqualsFilterExpression('active', false);

    // Act
    const f = new NotFilter(inner);

    // Assert
    expect(f.filter).toBe(inner);
  });
});
