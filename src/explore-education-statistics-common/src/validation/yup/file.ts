import { addMethod } from 'yup';
import * as Yup from 'yup';

declare module 'yup' {
  interface MixedSchema {
    minSize(min: number, message: string): this;
  }
}

// eslint-disable-next-line func-names
addMethod(Yup.mixed, 'minSize', function (min: number, message: string) {
  // eslint-disable-next-line func-names
  return this.test('minSize', message, function (value) {
    if (!value) {
      return true;
    }
    return (value as File).size > min;
  });
});

/* 
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-ignore - Yup defines 'value' as 'AnyPresentValue' but
    // we know it's a File

*/
