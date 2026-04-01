import { EndsWithFilter } from '../src/expressions';

describe('EndsWithFilter', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new EndsWithFilter('name', 'man');

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'name', op: 'endswith', value: 'man' });
  });

  it('op field should be endswith', () => {
    // Arrange / Act
    const dict = new EndsWithFilter('f', 'v').toDict();

    // Assert
    expect(dict['op']).toBe('endswith');
  });
});
