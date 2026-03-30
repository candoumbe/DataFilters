import {
  AndFilter,
  ContainsFilter,
  EndsWithFilter,
  EqualsFilter,
  GreaterThanFilter,
  GreaterThanOrEqualFilter,
  LessThanFilter,
  LessThanOrEqualFilter,
  NotFilter,
  OrFilter,
  StartsWithFilter,
} from '../src/expressions';
import { parse } from '../src/parser';

describe('parse - equals', () => {
  it('should parse simple equals expression', () => {
    const result = parse('name=Batman');
    expect(result).toBeInstanceOf(EqualsFilter);
    const f = result as EqualsFilter;
    expect(f.field).toBe('name');
    expect(f.value).toBe('Batman');
  });
});

describe('parse - contains', () => {
  it('should parse contains expression', () => {
    const result = parse('name=*bat*');
    expect(result).toBeInstanceOf(ContainsFilter);
    expect((result as ContainsFilter).value).toBe('bat');
  });
});

describe('parse - startsWith', () => {
  it('should parse starts-with expression', () => {
    const result = parse('name=Bat*');
    expect(result).toBeInstanceOf(StartsWithFilter);
    expect((result as StartsWithFilter).value).toBe('Bat');
  });
});

describe('parse - endsWith', () => {
  it('should parse ends-with expression', () => {
    const result = parse('name=*man');
    expect(result).toBeInstanceOf(EndsWithFilter);
    expect((result as EndsWithFilter).value).toBe('man');
  });
});

describe('parse - negation', () => {
  it('should parse not-equals expression', () => {
    const result = parse('name=!Batman');
    expect(result).toBeInstanceOf(NotFilter);
    const inner = (result as NotFilter).filter;
    expect(inner).toBeInstanceOf(EqualsFilter);
    expect((inner as EqualsFilter).value).toBe('Batman');
  });
});

describe('parse - range', () => {
  it('should parse closed range [min,max]', () => {
    const result = parse('age=[18,65]');
    expect(result).toBeInstanceOf(AndFilter);
    const f = result as AndFilter;
    expect(f.filters[0]).toBeInstanceOf(GreaterThanOrEqualFilter);
    expect(f.filters[1]).toBeInstanceOf(LessThanOrEqualFilter);
  });

  it('should parse open range (min,max)', () => {
    const result = parse('age=(18,65)');
    expect(result).toBeInstanceOf(AndFilter);
    const f = result as AndFilter;
    expect(f.filters[0]).toBeInstanceOf(GreaterThanFilter);
    expect(f.filters[1]).toBeInstanceOf(LessThanFilter);
  });

  it('should parse half-open range [min,]', () => {
    const result = parse('age=[18,]');
    expect(result).toBeInstanceOf(GreaterThanOrEqualFilter);
    expect((result as GreaterThanOrEqualFilter).value).toBe('18');
  });

  it('should parse half-open range [,max]', () => {
    const result = parse('age=[,65]');
    expect(result).toBeInstanceOf(LessThanOrEqualFilter);
    expect((result as LessThanOrEqualFilter).value).toBe('65');
  });
});

describe('parse - AND combination', () => {
  it('should parse comma-separated expressions as AND', () => {
    const result = parse('name=*bat*,age=[18,]');
    expect(result).toBeInstanceOf(AndFilter);
    expect((result as AndFilter).filters.length).toBe(2);
  });
});

describe('parse - OR combination', () => {
  it('should parse pipe-separated expressions as OR', () => {
    const result = parse('status=active|status=pending');
    expect(result).toBeInstanceOf(OrFilter);
    expect((result as OrFilter).filters.length).toBe(2);
  });
});

describe('parse - errors', () => {
  it('should throw on empty expression', () => {
    expect(() => parse('')).toThrow();
  });

  it('should throw on whitespace-only expression', () => {
    expect(() => parse('   ')).toThrow();
  });

  it('should throw on missing field', () => {
    expect(() => parse('=value')).toThrow();
  });

  it('should throw on missing equals sign', () => {
    expect(() => parse('noequals')).toThrow();
  });
});

describe('parse - parameterized', () => {
  const cases: [string, unknown][] = [
    ['name=Batman', EqualsFilter],
    ['name=*bat*', ContainsFilter],
    ['name=Bat*', StartsWithFilter],
    ['name=*man', EndsWithFilter],
    ['name=!Batman', NotFilter],
  ];

  test.each(cases)('parse("%s") should return expected type', (expression, expectedType) => {
    expect(parse(expression)).toBeInstanceOf(expectedType as unknown as new () => unknown);
  });
});
