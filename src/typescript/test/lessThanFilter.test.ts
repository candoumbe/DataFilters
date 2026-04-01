import { LessThanFilter } from '../src/expressions';

describe('LessThanFilter', () => {
  it('should serialize to correct dict', () => {
    // Arrange
    const f = new LessThanFilter('age', 65);

    // Act
    const result = f.toDict();

    // Assert
    expect(result).toEqual({ field: 'age', op: 'lt', value: 65 });
  });

  it('op field should be lt', () => {
    // Arrange / Act
    const dict = new LessThanFilter('f', 0).toDict();

    // Assert
    expect(dict['op']).toBe('lt');
  });
});
