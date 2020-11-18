import roundNearest, {
  RoundNearestOptions,
} from '@common/utils/number/roundNearest';

describe('roundNearest', () => {
  describe('with default rounding', () => {
    test('returns correct values with multiple of 5', () => {
      const multiple = 5;

      expect(roundNearest(-35, multiple)).toBe(-35);
      expect(roundNearest(-32.51, multiple)).toBe(-35);
      expect(roundNearest(-32.5, multiple)).toBe(-30);
      expect(roundNearest(-32, multiple)).toBe(-30);
      expect(roundNearest(-5, multiple)).toBe(-5);
      expect(roundNearest(0, multiple)).toBe(0);
      expect(roundNearest(5, multiple)).toBe(5);
      expect(roundNearest(32, multiple)).toBe(30);
      expect(roundNearest(32.499, multiple)).toBe(30);
      expect(roundNearest(32.5, multiple)).toBe(35);
      expect(roundNearest(35, multiple)).toBe(35);
    });

    test('returns correct values with multiple of 4', () => {
      const multiple = 4;

      expect(roundNearest(-36, multiple)).toBe(-36);
      expect(roundNearest(-34.01, multiple)).toBe(-36);
      expect(roundNearest(-34, multiple)).toBe(-32);
      expect(roundNearest(-32, multiple)).toBe(-32);
      expect(roundNearest(-4, multiple)).toBe(-4);
      expect(roundNearest(0, multiple)).toBe(0);
      expect(roundNearest(4, multiple)).toBe(4);
      expect(roundNearest(32, multiple)).toBe(32);
      expect(roundNearest(33.99, multiple)).toBe(32);
      expect(roundNearest(34, multiple)).toBe(36);
      expect(roundNearest(36, multiple)).toBe(36);
    });

    test('returns correct values with multiple of 0.01', () => {
      const multiple = 0.01;

      expect(roundNearest(-2.5391, multiple)).toBe(-2.54);
      expect(roundNearest(-2.5349, multiple)).toBe(-2.53);
      expect(roundNearest(-2.532, multiple)).toBe(-2.53);
      expect(roundNearest(-2.5, multiple)).toBe(-2.5);
      expect(roundNearest(0, multiple)).toBe(0);
      expect(roundNearest(2.5, multiple)).toBe(2.5);
      expect(roundNearest(2.532, multiple)).toBe(2.53);
      expect(roundNearest(2.5349, multiple)).toBe(2.53);
      expect(roundNearest(2.5391, multiple)).toBe(2.54);
    });

    test('returns correct values with multiple of 0.33', () => {
      const multiple = 0.33;

      expect(roundNearest(-0.66, multiple)).toBe(-0.66);
      expect(roundNearest(-0.495, multiple)).toBe(-0.33);
      expect(roundNearest(-0.4949, multiple)).toBe(-0.33);
      expect(roundNearest(-0.33, multiple)).toBe(-0.33);
      expect(roundNearest(-0.165, multiple)).toBe(-0);
      expect(roundNearest(-0.16499, multiple)).toBe(-0);
      expect(roundNearest(0, multiple)).toBe(0);
      expect(roundNearest(0.16499, multiple)).toBe(0);
      expect(roundNearest(0.165, multiple)).toBe(0.33);
      expect(roundNearest(0.33, multiple)).toBe(0.33);
      expect(roundNearest(0.4949, multiple)).toBe(0.33);
      expect(roundNearest(0.495, multiple)).toBe(0.66);
      expect(roundNearest(0.66, multiple)).toBe(0.66);
    });

    test('returns correct values with multiple of 1', () => {
      const multiple = 1;

      expect(roundNearest(-5, multiple)).toBe(-5);
      expect(roundNearest(1, multiple)).toBe(1);
      expect(roundNearest(5, multiple)).toBe(5);
    });

    test('returns NaNs with multiple of 0', () => {
      const multiple = 0;

      expect(roundNearest(-5, multiple)).toBeNaN();
      expect(roundNearest(1, multiple)).toBeNaN();
      expect(roundNearest(5, multiple)).toBeNaN();
    });
  });

  describe('with rounding up', () => {
    const options: RoundNearestOptions = {
      direction: 'up',
    };

    test('returns correct values with multiple of 5', () => {
      const multiple = 5;

      expect(roundNearest(-35, multiple, options)).toBe(-35);
      expect(roundNearest(-32.51, multiple, options)).toBe(-30);
      expect(roundNearest(-32.5, multiple, options)).toBe(-30);
      expect(roundNearest(-32, multiple, options)).toBe(-30);
      expect(roundNearest(-5, multiple, options)).toBe(-5);
      expect(roundNearest(0, multiple, options)).toBe(0);
      expect(roundNearest(5, multiple, options)).toBe(5);
      expect(roundNearest(32, multiple, options)).toBe(35);
      expect(roundNearest(32.499, multiple, options)).toBe(35);
      expect(roundNearest(32.5, multiple, options)).toBe(35);
      expect(roundNearest(35, multiple, options)).toBe(35);
    });

    test('returns correct values with multiple of 4', () => {
      const multiple = 4;

      expect(roundNearest(-36, multiple, options)).toBe(-36);
      expect(roundNearest(-34.01, multiple, options)).toBe(-32);
      expect(roundNearest(-34, multiple, options)).toBe(-32);
      expect(roundNearest(-32, multiple, options)).toBe(-32);
      expect(roundNearest(-4, multiple, options)).toBe(-4);
      expect(roundNearest(0, multiple, options)).toBe(0);
      expect(roundNearest(4, multiple, options)).toBe(4);
      expect(roundNearest(32, multiple, options)).toBe(32);
      expect(roundNearest(33.99, multiple, options)).toBe(36);
      expect(roundNearest(34, multiple, options)).toBe(36);
      expect(roundNearest(36, multiple, options)).toBe(36);
    });

    test('returns correct values with multiple of 0.01', () => {
      const multiple = 0.01;

      expect(roundNearest(-2.5391, multiple, options)).toBe(-2.53);
      expect(roundNearest(-2.5349, multiple, options)).toBe(-2.53);
      expect(roundNearest(-2.532, multiple, options)).toBe(-2.53);
      expect(roundNearest(-2.5, multiple, options)).toBe(-2.5);
      expect(roundNearest(0, multiple, options)).toBe(0);
      expect(roundNearest(2.5, multiple, options)).toBe(2.5);
      expect(roundNearest(2.532, multiple, options)).toBe(2.54);
      expect(roundNearest(2.5349, multiple, options)).toBe(2.54);
      expect(roundNearest(2.5391, multiple, options)).toBe(2.54);
    });

    test('returns correct values with multiple of 0.33', () => {
      const multiple = 0.33;

      expect(roundNearest(-0.66, multiple, options)).toBe(-0.66);
      expect(roundNearest(-0.495, multiple, options)).toBe(-0.33);
      expect(roundNearest(-0.4949, multiple, options)).toBe(-0.33);
      expect(roundNearest(-0.33, multiple, options)).toBe(-0.33);
      expect(roundNearest(-0.165, multiple, options)).toBe(-0);
      expect(roundNearest(-0.16499, multiple, options)).toBe(-0);
      expect(roundNearest(0, multiple, options)).toBe(0);
      expect(roundNearest(0.16499, multiple, options)).toBe(0.33);
      expect(roundNearest(0.165, multiple, options)).toBe(0.33);
      expect(roundNearest(0.33, multiple, options)).toBe(0.33);
      expect(roundNearest(0.4949, multiple, options)).toBe(0.66);
      expect(roundNearest(0.495, multiple, options)).toBe(0.66);
      expect(roundNearest(0.66, multiple, options)).toBe(0.66);
    });

    test('returns correct values with multiple of 1', () => {
      const multiple = 1;

      expect(roundNearest(-5, multiple, options)).toBe(-5);
      expect(roundNearest(1, multiple, options)).toBe(1);
      expect(roundNearest(5, multiple, options)).toBe(5);
    });

    test('returns NaN with multiple of 0', () => {
      const multiple = 0;

      expect(roundNearest(-5, multiple, options)).toBeNaN();
      expect(roundNearest(1, multiple, options)).toBeNaN();
      expect(roundNearest(5, multiple, options)).toBeNaN();
    });
  });

  describe('with rounding down', () => {
    const options: RoundNearestOptions = {
      direction: 'down',
    };

    test('returns correct values with multiple of 5', () => {
      const multiple = 5;

      expect(roundNearest(-35, multiple, options)).toBe(-35);
      expect(roundNearest(-32.51, multiple, options)).toBe(-35);
      expect(roundNearest(-32.5, multiple, options)).toBe(-35);
      expect(roundNearest(-32, multiple, options)).toBe(-35);
      expect(roundNearest(-5, multiple, options)).toBe(-5);
      expect(roundNearest(0, multiple, options)).toBe(0);
      expect(roundNearest(5, multiple, options)).toBe(5);
      expect(roundNearest(32, multiple, options)).toBe(30);
      expect(roundNearest(32.499, multiple, options)).toBe(30);
      expect(roundNearest(32.5, multiple, options)).toBe(30);
      expect(roundNearest(35, multiple, options)).toBe(35);
    });

    test('returns correct values with multiple of 4', () => {
      const multiple = 4;

      expect(roundNearest(-36, multiple, options)).toBe(-36);
      expect(roundNearest(-34.01, multiple, options)).toBe(-36);
      expect(roundNearest(-34, multiple, options)).toBe(-36);
      expect(roundNearest(-32, multiple, options)).toBe(-32);
      expect(roundNearest(-4, multiple, options)).toBe(-4);
      expect(roundNearest(0, multiple, options)).toBe(0);
      expect(roundNearest(4, multiple, options)).toBe(4);
      expect(roundNearest(32, multiple, options)).toBe(32);
      expect(roundNearest(33.99, multiple, options)).toBe(32);
      expect(roundNearest(34, multiple, options)).toBe(32);
      expect(roundNearest(36, multiple, options)).toBe(36);
    });

    test('returns correct values with multiple of 0.01', () => {
      const multiple = 0.01;

      expect(roundNearest(-2.5391, multiple, options)).toBe(-2.54);
      expect(roundNearest(-2.5349, multiple, options)).toBe(-2.54);
      expect(roundNearest(-2.532, multiple, options)).toBe(-2.54);
      expect(roundNearest(-2.5, multiple, options)).toBe(-2.5);
      expect(roundNearest(0, multiple, options)).toBe(0);
      expect(roundNearest(2.5, multiple, options)).toBe(2.5);
      expect(roundNearest(2.532, multiple, options)).toBe(2.53);
      expect(roundNearest(2.5349, multiple, options)).toBe(2.53);
      expect(roundNearest(2.5391, multiple, options)).toBe(2.53);
    });

    test('returns correct values with multiple of 0.33', () => {
      const multiple = 0.33;

      expect(roundNearest(-0.66, multiple, options)).toBe(-0.66);
      expect(roundNearest(-0.495, multiple, options)).toBe(-0.66);
      expect(roundNearest(-0.4949, multiple, options)).toBe(-0.66);
      expect(roundNearest(-0.33, multiple, options)).toBe(-0.33);
      expect(roundNearest(-0.165, multiple, options)).toBe(-0.33);
      expect(roundNearest(-0.16499, multiple, options)).toBe(-0.33);
      expect(roundNearest(0, multiple, options)).toBe(0);
      expect(roundNearest(0.16499, multiple, options)).toBe(0);
      expect(roundNearest(0.165, multiple, options)).toBe(0);
      expect(roundNearest(0.33, multiple, options)).toBe(0.33);
      expect(roundNearest(0.4949, multiple, options)).toBe(0.33);
      expect(roundNearest(0.495, multiple, options)).toBe(0.33);
      expect(roundNearest(0.66, multiple, options)).toBe(0.66);
    });

    test('returns correct values with multiple of 1', () => {
      const multiple = 1;

      expect(roundNearest(-5, multiple, options)).toBe(-5);
      expect(roundNearest(1, multiple, options)).toBe(1);
      expect(roundNearest(5, multiple, options)).toBe(5);
    });

    test('returns NaNs with multiple of 0', () => {
      const multiple = 0;

      expect(roundNearest(-5, multiple, options)).toBeNaN();
      expect(roundNearest(1, multiple, options)).toBeNaN();
      expect(roundNearest(5, multiple, options)).toBeNaN();
    });
  });
});
