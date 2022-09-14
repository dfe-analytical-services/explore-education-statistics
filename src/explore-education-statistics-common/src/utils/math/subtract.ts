import countDecimalPlaces from '@common/utils/number/countDecimalPlaces';
import max from 'lodash/max';

/**
 * Safely subtract {@param a} from {@param b} to return an accurate
 * number that is not affected by floating point inaccuracy.
 *
 * For example, if the numbers 6.6 and 5.5 are subtracted in normal
 * JavaScript, you actually get 1.0999999999999996. Comparatively,
 * this function will return the expected value of 1.1.
 */
export default function subtract(a: number, b: number) {
  const decimalPlaces = max([
    countDecimalPlaces(a),
    countDecimalPlaces(b),
  ]) as number;

  const correctionFactor = 10 ** decimalPlaces;

  return (a * correctionFactor - b * correctionFactor) / correctionFactor;
}
