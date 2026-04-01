import { EqualsFilterExpression } from '../src/expressions';

describe('EqualsFilterExpression', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new EqualsFilterExpression('name', 'Batman');

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'name', op: 'eq', value: 'Batman' });
  });

  it('should store field and value', () => {
    // Arrange / Act
    const f = new EqualsFilterExpression('age', 30);

    // Assert
    expect(f.field).toBe('age');
    expect(f.value).toBe(30);
  });

  it('op field should be eq', () => {
    // Arrange / Act
    const dict = new EqualsFilterExpression('f', 'v').toDict();

    // Assert
    expect(dict['op']).toBe('eq');
  });
});
