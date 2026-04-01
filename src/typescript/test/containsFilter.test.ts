import { ContainsFilter } from '../src/expressions';

describe('ContainsFilter', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new ContainsFilter('name', 'bat');

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'name', op: 'contains', value: 'bat' });
  });

  it('op field should be contains', () => {
    // Arrange / Act
    const dict = new ContainsFilter('f', 'v').toDict();

    // Assert
    expect(dict['op']).toBe('contains');
  });
});
