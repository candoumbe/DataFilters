import { EndsWithFilterExpression } from '../src/expressions';

describe('EndsWithFilterExpression', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new EndsWithFilterExpression('name', 'man');

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'name', op: 'endswith', value: 'man' });
  });

  it('op field should be endswith', () => {
    // Arrange / Act
    const dict = new EndsWithFilterExpression('f', 'v').toDict();

    // Assert
    expect(dict['op']).toBe('endswith');
  });
});
