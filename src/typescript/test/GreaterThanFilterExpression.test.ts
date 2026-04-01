import { GreaterThanFilterExpression } from '../src/expressions';

describe('GreaterThanFilterExpression', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new GreaterThanFilterExpression('age', 18);

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'age', op: 'gt', value: 18 });
  });

  it('op field should be gt', () => {
    // Arrange / Act
    const dict = new GreaterThanFilterExpression('f', 0).toDict();

    // Assert
    expect(dict['op']).toBe('gt');
  });
});
