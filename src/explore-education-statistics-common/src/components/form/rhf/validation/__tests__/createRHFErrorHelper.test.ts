import { FieldError } from 'react-hook-form';
import createRHFErrorHelper from '../createRHFErrorHelper';

describe('createRHFErrorHelper', () => {
  describe('getError/hasError', () => {
    test('gets touched error message', () => {
      const { getError, hasError } = createRHFErrorHelper({
        errors: {
          test: { message: 'Please select' } as FieldError,
        },
        touchedFields: {
          test: true,
        },
      });

      expect(getError('test')).toBe('Please select');
      expect(hasError('test')).toBe(true);
    });

    test('does not get untouched error message', () => {
      const { getError, hasError } = createRHFErrorHelper({
        errors: {
          test: { message: 'Please select' } as FieldError,
        },
        touchedFields: {
          test: false,
        },
      });

      expect(getError('test')).toBe('');
      expect(hasError('test')).toBe(false);
    });

    test('gets untouched error message when the form is submitted', () => {
      const { getError, hasError } = createRHFErrorHelper({
        errors: {
          test: { message: 'Please select' } as FieldError,
        },
        touchedFields: {
          test: false,
        },
        isSubmitted: true,
      });

      expect(getError('test')).toBe('Please select');
      expect(hasError('test')).toBe(true);
    });

    test('gets nested touched error message', () => {
      const { getError } = createRHFErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: { message: 'Please select' } as FieldError,
          },
        },
        touchedFields: {
          test: {
            something: true,
          },
        },
      });

      expect(getError('test.something')).toBe('Please select');
    });

    test('does not get nested untouched error message', () => {
      const { getError, hasError } = createRHFErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: { message: 'Please select' } as FieldError,
          },
        },
        touchedFields: {
          test: {
            something: false,
          },
        },
      });

      expect(getError('test.something')).toBe('');
      expect(hasError('test.something')).toBe(false);
    });

    test('does not get error message when path does not match', () => {
      const { getError, hasError } = createRHFErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: { message: 'Please select' } as FieldError,
          },
        },
        touchedFields: {
          test: {
            something: true,
          },
        },
      });

      expect(getError('invalidPath')).toBe('');
      expect(hasError('invalidPath')).toBe(false);
    });

    test('does not get error message when path only partially matches', () => {
      const { getError, hasError } = createRHFErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: { message: 'Please select' } as FieldError,
          },
        },
        touchedFields: {
          test: {
            something: true,
          },
        },
      });

      expect(getError('test')).toBe('');
      expect(hasError('test')).toBe(false);
    });
  });

  describe('getAllErrors', () => {
    test('gets touched errors', () => {
      const { getAllErrors } = createRHFErrorHelper({
        errors: {
          test: { message: 'Please select' } as FieldError,
        },
        touchedFields: {
          test: true,
        },
      });

      expect(getAllErrors()).toEqual({
        test: 'Please select',
      });
    });

    test('does not get untouched errors', () => {
      const { getAllErrors } = createRHFErrorHelper({
        errors: {
          test: { message: 'Please select' } as FieldError,
        },
        touchedFields: {
          test: false,
        },
      });

      expect(getAllErrors()).toEqual({});
    });

    test('gets untouched errors when the form is submitted', () => {
      const { getAllErrors } = createRHFErrorHelper({
        errors: {
          test: { message: 'Please select' } as FieldError,
        },
        touchedFields: {
          test: false,
        },
        isSubmitted: true,
      });

      expect(getAllErrors()).toEqual({
        test: 'Please select',
      });
    });

    test('gets nested touched errors as single-level object with dot-notation keys', () => {
      const { getAllErrors } = createRHFErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: { message: 'Please select' } as FieldError,
          },
        },
        touchedFields: {
          test: {
            something: true,
          },
        },
      });

      expect(getAllErrors()).toEqual({
        'test.something': 'Please select',
      });
    });

    test('does not get nested untouched errors', () => {
      const { getAllErrors } = createRHFErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: { message: 'Please select' } as FieldError,
          },
        },
        touchedFields: {
          test: {
            something: false,
          },
        },
      });

      expect(getAllErrors()).toEqual({});
    });

    test('gets deeply nested touched errors as single-level object with dot-notation keys', () => {
      const { getAllErrors } = createRHFErrorHelper<{
        a: {
          b: {
            c: {
              d: string;
            };
          };
        };
      }>({
        errors: {
          a: {
            b: {
              c: {
                d: { message: 'Please select' } as FieldError,
              },
            },
          },
        },
        touchedFields: {
          a: {
            b: {
              c: {
                d: true,
              },
            },
          },
        },
      });

      expect(getAllErrors()).toEqual({
        'a.b.c.d': 'Please select',
      });
    });

    test('gets touched error messages for mixed error bag', () => {
      const { getAllErrors } = createRHFErrorHelper<{
        address: {
          line1: string;
          line2: string;
        };
        firstName: string;
        lastName: string;
      }>({
        errors: {
          address: {
            line1: { message: 'Address 1 is required' } as FieldError,
            line2: { message: 'Address 2 is required' } as FieldError,
          },
          firstName: { message: 'First name is required' } as FieldError,
          lastName: { message: 'Last name is required' } as FieldError,
        },
        touchedFields: {
          address: {
            line1: true,
            line2: false,
          },
          firstName: true,
          lastName: false,
        },
      });

      expect(getAllErrors()).toEqual({
        'address.line1': 'Address 1 is required',
        firstName: 'First name is required',
      });
    });

    test('gets all error messages for mixed error bag when form is submitted', () => {
      const { getAllErrors } = createRHFErrorHelper<{
        address: {
          line1: string;
          line2: string;
        };
        firstName: string;
        lastName: string;
      }>({
        errors: {
          address: {
            line1: { message: 'Address 1 is required' } as FieldError,
            line2: { message: 'Address 2 is required' } as FieldError,
          },
          firstName: { message: 'First name is required' } as FieldError,
          lastName: { message: 'Last name is required' } as FieldError,
        },
        touchedFields: {
          address: {
            line1: true,
            line2: false,
          },
          firstName: true,
          lastName: false,
        },
        isSubmitted: true,
      });

      expect(getAllErrors()).toEqual({
        'address.line1': 'Address 1 is required',
        'address.line2': 'Address 2 is required',
        firstName: 'First name is required',
        lastName: 'Last name is required',
      });
    });
  });
});
