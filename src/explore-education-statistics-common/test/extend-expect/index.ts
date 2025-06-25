import toBeAriaDisabled from './toBeAriaDisabled';
import toHaveNumericValue from './toHaveNumericValue';
import toHaveScrolledIntoView from './toHaveScrolledIntoView';

expect.extend({
  toBeAriaDisabled,
  toHaveScrolledIntoView,
  toHaveNumericValue,
});
