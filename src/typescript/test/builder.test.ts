import {
  AndFilterExpression,
  ContainsFilterExpression,
  EndsWithFilterExpression,
  EqualsFilterExpression,
  GreaterThanFilterExpression,
  GreaterThanOrEqualFilterExpression,
  LessThanFilterExpression,
  LessThanOrEqualFilterExpression,
  NotFilterExpression,
  OrFilterExpression,
  StartsWithFilterExpression,
} from '../src/expressions';
import { FilterBuilder } from '../src/builder';

describe('FilterBuilder - single conditions', () => {
  it('should build equals filter', () => {
    const result = new FilterBuilder().where('name').eq('Batman').build();
    expect(result).toBeInstanceOf(EqualsFilterExpression);
    expect((result as EqualsFilterExpression).field).toBe('name');
    expect((result as EqualsFilterExpression).value).toBe('Batman');
  });

  it('should build contains filter', () => {
    const result = new FilterBuilder().where('name').contains('bat').build();
    expect(result).toBeInstanceOf(ContainsFilterExpression);
    expect((result as ContainsFilterExpression).value).toBe('bat');
  });

  it('should build startsWith filter', () => {
    const result = new FilterBuilder().where('name').startsWith('Bat').build();
    expect(result).toBeInstanceOf(StartsWithFilterExpression);
  });

  it('should build endsWith filter', () => {
    const result = new FilterBuilder().where('name').endsWith('man').build();
    expect(result).toBeInstanceOf(EndsWithFilterExpression);
  });

  it('should build gt filter', () => {
    const result = new FilterBuilder().where('age').gt(18).build();
    expect(result).toBeInstanceOf(GreaterThanFilterExpression);
    expect((result as GreaterThanFilterExpression).value).toBe(18);
  });

  it('should build gte filter', () => {
    const result = new FilterBuilder().where('age').gte(18).build();
    expect(result).toBeInstanceOf(GreaterThanOrEqualFilterExpression);
  });

  it('should build lt filter', () => {
    const result = new FilterBuilder().where('age').lt(65).build();
    expect(result).toBeInstanceOf(LessThanFilterExpression);
  });

  it('should build lte filter', () => {
    const result = new FilterBuilder().where('age').lte(65).build();
    expect(result).toBeInstanceOf(LessThanOrEqualFilterExpression);
  });

  it('should build not filter', () => {
    const result = new FilterBuilder().where('deleted').not(true).build();
    expect(result).toBeInstanceOf(NotFilterExpression);
    expect((result as NotFilterExpression).filter).toBeInstanceOf(EqualsFilterExpression);
  });
});

describe('FilterBuilder - multiple conditions', () => {
  it('should combine two conditions as AND', () => {
    const result = new FilterBuilder()
      .where('name').contains('bat')
      .andWhere('age').gte(18)
      .build();
    expect(result).toBeInstanceOf(AndFilterExpression);
    expect((result as AndFilterExpression).filters.length).toBe(2);
  });

  it('should build OR filter', () => {
    const result = new FilterBuilder().where('status').or('active', 'pending').build();
    expect(result).toBeInstanceOf(OrFilterExpression);
    expect((result as OrFilterExpression).filters.length).toBe(2);
  });
});

describe('FilterBuilder - errors', () => {
  it('should throw when no conditions added', () => {
    expect(() => new FilterBuilder().build()).toThrow();
  });
});
