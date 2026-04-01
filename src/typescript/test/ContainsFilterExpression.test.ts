import { ContainsFilterExpression } from '../src/expressions';

describe('ContainsFilterExpression', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new ContainsFilterExpression('name', 'bat');

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'name', op: 'contains', value: 'bat' });
  });

  it('op field should be contains', () => {
    // Arrange / Act
    const dict = new ContainsFilterExpression('f', 'v').toDict();

    // Assert
    expect(dict['op']).toBe('contains');
  });
});
