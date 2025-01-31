import subtract from '@common/utils/math/subtract';

describe('subtract', () => {
  test('using integer numbers', () => {
    expect(subtract(10, 10)).toBe(0);
    expect(subtract(10, 4)).toBe(6);
    expect(subtract(20, 2)).toBe(18);
    expect(subtract(-10, 2)).toBe(-12);
    expect(subtract(-20, -4)).toBe(-16);
  });

  test('no floating point inaccuracy using numbers with the same decimal points', () => {
    expect(subtract(6.6, 5.5)).toBe(1.1);
    expect(subtract(6.66, 5.55)).toBe(1.11);
    expect(subtract(6.666, 5.555)).toBe(1.111);

    expect(subtract(6.6, 7.7)).toBe(-1.1);
    expect(subtract(6.66, 7.77)).toBe(-1.11);
    expect(subtract(6.666, 7.777)).toBe(-1.111);

    expect(subtract(-6.6, -5.5)).toBe(-1.1);
    expect(subtract(-6.66, -5.55)).toBe(-1.11);
    expect(subtract(-6.666, -5.555)).toBe(-1.111);
  });

  test('no floating point inaccuracy using numbers with different decimal points', () => {
    expect(subtract(6.6, 5.55)).toBe(1.05);
    expect(subtract(6.66, 5.555)).toBe(1.105);
    expect(subtract(6.666, 5.55552)).toBe(1.11048);

    expect(subtract(6.6, 7.777)).toBe(-1.177);
    expect(subtract(6.66, 7.7777)).toBe(-1.1177);
    expect(subtract(6.666, 7.7)).toBe(-1.034);

    expect(subtract(-6.6, -5.55)).toBe(-1.05);
    expect(subtract(-6.66, -5.555)).toBe(-1.105);
    expect(subtract(-6.666, -5.55552)).toBe(-1.11048);
  });

  test('using NaN', () => {
    expect(subtract(NaN, 2)).toBe(NaN);
    expect(subtract(2, NaN)).toBe(NaN);
  });

  test('using Infinity', () => {
    expect(subtract(Infinity, 2)).toBe(Infinity);
    expect(subtract(Number.POSITIVE_INFINITY, 2)).toBe(Infinity);
    expect(subtract(Number.NEGATIVE_INFINITY, 2)).toBe(-Infinity);

    expect(subtract(2, Infinity)).toBe(-Infinity);
    expect(subtract(2, Number.POSITIVE_INFINITY)).toBe(-Infinity);
    expect(subtract(2, Number.NEGATIVE_INFINITY)).toBe(Infinity);
  });
});
