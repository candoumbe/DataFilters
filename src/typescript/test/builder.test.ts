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
import { FilterBuilder } from '../src/builder';

describe('FilterBuilder - single conditions', () => {
  it('should build equals filter', () => {
    const result = new FilterBuilder().where('name').eq('Batman').build();
    expect(result).toBeInstanceOf(EqualsFilter);
    expect((result as EqualsFilter).field).toBe('name');
    expect((result as EqualsFilter).value).toBe('Batman');
  });

  it('should build contains filter', () => {
    const result = new FilterBuilder().where('name').contains('bat').build();
    expect(result).toBeInstanceOf(ContainsFilter);
    expect((result as ContainsFilter).value).toBe('bat');
  });

  it('should build startsWith filter', () => {
    const result = new FilterBuilder().where('name').startsWith('Bat').build();
    expect(result).toBeInstanceOf(StartsWithFilter);
  });

  it('should build endsWith filter', () => {
    const result = new FilterBuilder().where('name').endsWith('man').build();
    expect(result).toBeInstanceOf(EndsWithFilter);
  });

  it('should build gt filter', () => {
    const result = new FilterBuilder().where('age').gt(18).build();
    expect(result).toBeInstanceOf(GreaterThanFilter);
    expect((result as GreaterThanFilter).value).toBe(18);
  });

  it('should build gte filter', () => {
    const result = new FilterBuilder().where('age').gte(18).build();
    expect(result).toBeInstanceOf(GreaterThanOrEqualFilter);
  });

  it('should build lt filter', () => {
    const result = new FilterBuilder().where('age').lt(65).build();
    expect(result).toBeInstanceOf(LessThanFilter);
  });

  it('should build lte filter', () => {
    const result = new FilterBuilder().where('age').lte(65).build();
    expect(result).toBeInstanceOf(LessThanOrEqualFilter);
  });

  it('should build not filter', () => {
    const result = new FilterBuilder().where('deleted').not(true).build();
    expect(result).toBeInstanceOf(NotFilter);
    expect((result as NotFilter).filter).toBeInstanceOf(EqualsFilter);
  });
});

describe('FilterBuilder - multiple conditions', () => {
  it('should combine two conditions as AND', () => {
    const result = new FilterBuilder()
      .where('name').contains('bat')
      .andWhere('age').gte(18)
      .build();
    expect(result).toBeInstanceOf(AndFilter);
    expect((result as AndFilter).filters.length).toBe(2);
  });

  it('should build OR filter', () => {
    const result = new FilterBuilder().where('status').or('active', 'pending').build();
    expect(result).toBeInstanceOf(OrFilter);
    expect((result as OrFilter).filters.length).toBe(2);
  });
});

describe('FilterBuilder - errors', () => {
  it('should throw when no conditions added', () => {
    expect(() => new FilterBuilder().build()).toThrow();
  });
});
