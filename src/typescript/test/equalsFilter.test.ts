import { EqualsFilter } from '../src/expressions';

describe('EqualsFilter', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new EqualsFilter('name', 'Batman');

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'name', op: 'eq', value: 'Batman' });
  });

  it('should store field and value', () => {
    // Arrange / Act
    const f = new EqualsFilter('age', 30);

    // Assert
    expect(f.field).toBe('age');
    expect(f.value).toBe(30);
  });

  it('op field should be eq', () => {
    // Arrange / Act
    const dict = new EqualsFilter('f', 'v').toDict();

    // Assert
    expect(dict['op']).toBe('eq');
  });
});
