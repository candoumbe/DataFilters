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

describe('EqualsFilter', () => {
  it('should serialize to correct dict', () => {
    const f = new EqualsFilter('name', 'Batman');
    expect(f.toDict()).toEqual({ field: 'name', op: 'eq', value: 'Batman' });
  });

  it('should store field and value', () => {
    const f = new EqualsFilter('age', 30);
    expect(f.field).toBe('age');
    expect(f.value).toBe(30);
  });
});

describe('ContainsFilter', () => {
  it('should serialize to correct dict', () => {
    const f = new ContainsFilter('name', 'bat');
    expect(f.toDict()).toEqual({ field: 'name', op: 'contains', value: 'bat' });
  });
});

describe('StartsWithFilter', () => {
  it('should serialize to correct dict', () => {
    const f = new StartsWithFilter('name', 'Bat');
    expect(f.toDict()).toEqual({ field: 'name', op: 'startswith', value: 'Bat' });
  });
});

describe('EndsWithFilter', () => {
  it('should serialize to correct dict', () => {
    const f = new EndsWithFilter('name', 'man');
    expect(f.toDict()).toEqual({ field: 'name', op: 'endswith', value: 'man' });
  });
});

describe('GreaterThanFilter', () => {
  it('should serialize to correct dict', () => {
    const f = new GreaterThanFilter('age', 18);
    expect(f.toDict()).toEqual({ field: 'age', op: 'gt', value: 18 });
  });
});

describe('GreaterThanOrEqualFilter', () => {
  it('should serialize to correct dict', () => {
    const f = new GreaterThanOrEqualFilter('age', 18);
    expect(f.toDict()).toEqual({ field: 'age', op: 'gte', value: 18 });
  });
});

describe('LessThanFilter', () => {
  it('should serialize to correct dict', () => {
    const f = new LessThanFilter('age', 65);
    expect(f.toDict()).toEqual({ field: 'age', op: 'lt', value: 65 });
  });
});

describe('LessThanOrEqualFilter', () => {
  it('should serialize to correct dict', () => {
    const f = new LessThanOrEqualFilter('age', 65);
    expect(f.toDict()).toEqual({ field: 'age', op: 'lte', value: 65 });
  });
});

describe('AndFilter', () => {
  it('should serialize with logic and nested filters', () => {
    const f = new AndFilter([new EqualsFilter('a', 1), new EqualsFilter('b', 2)]);
    const result = f.toDict();
    expect(result['logic']).toBe('and');
    expect((result['filters'] as unknown[]).length).toBe(2);
  });
});

describe('OrFilter', () => {
  it('should serialize with logic and nested filters', () => {
    const f = new OrFilter([
      new EqualsFilter('status', 'active'),
      new EqualsFilter('status', 'pending'),
    ]);
    const result = f.toDict();
    expect(result['logic']).toBe('or');
    expect((result['filters'] as unknown[]).length).toBe(2);
  });
});

describe('NotFilter', () => {
  it('should serialize with logic not', () => {
    const f = new NotFilter(new EqualsFilter('deleted', true));
    const result = f.toDict();
    expect(result['logic']).toBe('not');
    expect((result['filters'] as unknown[]).length).toBe(1);
  });

  it('should wrap the inner filter', () => {
    const inner = new EqualsFilter('active', false);
    const f = new NotFilter(inner);
    expect(f.filter).toBe(inner);
  });
});

describe('filter operator values', () => {
  const cases: [ReturnType<(typeof EqualsFilter.prototype)['toDict']>, string][] = [
    [new EqualsFilter('f', 'v').toDict(), 'eq'],
    [new ContainsFilter('f', 'v').toDict(), 'contains'],
    [new StartsWithFilter('f', 'v').toDict(), 'startswith'],
    [new EndsWithFilter('f', 'v').toDict(), 'endswith'],
    [new GreaterThanFilter('f', 0).toDict(), 'gt'],
    [new GreaterThanOrEqualFilter('f', 0).toDict(), 'gte'],
    [new LessThanFilter('f', 0).toDict(), 'lt'],
    [new LessThanOrEqualFilter('f', 0).toDict(), 'lte'],
  ];

  test.each(cases)('op field should be %s', (dict, expectedOp) => {
    expect(dict['op']).toBe(expectedOp);
  });
});
