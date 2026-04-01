import { LessThanOrEqualFilterExpression } from '../src/expressions';

describe('LessThanOrEqualFilterExpression', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new LessThanOrEqualFilterExpression('age', 65);

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'age', op: 'lte', value: 65 });
  });

  it('op field should be lte', () => {
    // Arrange / Act
    const dict = new LessThanOrEqualFilterExpression('f', 0).toDict();

    // Assert
    expect(dict['op']).toBe('lte');
  });
});
