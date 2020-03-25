/* eslint-disable no-template-curly-in-string */
import { addMethod, number, Ref, Schema, TestOptionsMessage } from 'yup';

declare module 'yup' {
  interface NumberSchema extends Schema<number> {
    moreThanOrEqual(limit: number | Ref, message?: TestOptionsMessage): this;
    lessThanOrEqual(limit: number | Ref, message?: TestOptionsMessage): this;
  }
}

addMethod(number, 'moreThanOrEqual', function numberMoreThanOrEqual(
  min: number | Ref,
  message = 'Must be more than or equal to ${path}',
) {
  return this.test('moreThanOrEqual', message, function moreThanOrEqual(value) {
    return !value || value >= this.resolve(min);
  });
});

addMethod(number, 'lessThanOrEqual', function numberLessThanOrEqual(
  max: number | Ref,
  message = 'Must be less than or equal to ${path}',
) {
  return this.test('lessThanOrEqual', message, function lessThanOrEqual(value) {
    return !value || value <= this.resolve(max);
  });
});
