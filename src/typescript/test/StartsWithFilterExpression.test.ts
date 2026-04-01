import { StartsWithFilterExpression } from '../src/expressions';

describe('StartsWithFilterExpression', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new StartsWithFilterExpression('name', 'Bat');

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'name', op: 'startswith', value: 'Bat' });
  });

  it('op field should be startswith', () => {
    // Arrange / Act
    const dict = new StartsWithFilterExpression('f', 'v').toDict();

    // Assert
    expect(dict['op']).toBe('startswith');
  });
});
