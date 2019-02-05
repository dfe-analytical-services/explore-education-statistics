import { addMethod, number, Ref, Schema, TestOptionsMessage } from 'yup';

declare module 'yup' {
  interface NumberSchema extends Schema<number> {
    moreThanOrEqual(limit: number | Ref, message?: TestOptionsMessage): this;
    lessThanOrEqual(limit: number | Ref, message?: TestOptionsMessage): this;
  }
}

addMethod(number, 'moreThanOrEqual', function(
  min: number | Ref,
  message = 'Must be more than or equal to ${path}',
) {
  return this.test('moreThanOrEqual', message, function(value) {
    return !value || value >= this.resolve(min);
  });
});

addMethod(number, 'lessThanOrEqual', function(
  max: number | Ref,
  message = 'Must be less than or equal to ${path}',
) {
  return this.test('lessThanOrEqual', message, function(value) {
    return !value || value <= this.resolve(max);
  });
});
