/* eslint-disable no-template-curly-in-string */
import { NumberSchema, Schema, addMethod, number } from 'yup';

declare module 'yup' {
  interface NumberSchema extends Schema<number> {
    moreThanOrEqual(limit: number, message?: string): this;
    lessThanOrEqual(limit: number, message?: string): this;
  }
}

addMethod<NumberSchema>(
  number,
  'moreThanOrEqual',
  function numberMoreThanOrEqual(
    min: number,
    message = 'Must be more than or equal to ${path}',
  ) {
    return this.test(
      'moreThanOrEqual',
      message,
      function moreThanOrEqual(value) {
        return !value || value >= this.resolve(min);
      },
    );
  },
);

addMethod<NumberSchema>(
  number,
  'lessThanOrEqual',
  function numberLessThanOrEqual(
    max: number,
    message = 'Must be less than or equal to ${path}',
  ) {
    return this.test(
      'lessThanOrEqual',
      message,
      function lessThanOrEqual(value) {
        return !value || value <= this.resolve(max);
      },
    );
  },
);
