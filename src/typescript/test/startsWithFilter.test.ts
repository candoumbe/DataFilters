import { StartsWithFilter } from '../src/expressions';

describe('StartsWithFilter', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new StartsWithFilter('name', 'Bat');

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'name', op: 'startswith', value: 'Bat' });
  });

  it('op field should be startswith', () => {
    // Arrange / Act
    const dict = new StartsWithFilter('f', 'v').toDict();

    // Assert
    expect(dict['op']).toBe('startswith');
  });
});
