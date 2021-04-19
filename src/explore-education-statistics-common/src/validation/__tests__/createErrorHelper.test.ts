import createErrorHelper from '../createErrorHelper';

describe('createErrorHelper', () => {
  describe('getError/hasError', () => {
    test('gets touched error message', () => {
      const { getError, hasError } = createErrorHelper({
        errors: {
          test: 'Please select',
        },
        touched: {
          test: true,
        },
      });

      expect(getError('test')).toBe('Please select');
      expect(hasError('test')).toBe(true);
    });

    test('does not get untouched error message', () => {
      const { getError, hasError } = createErrorHelper({
        errors: {
          test: 'Please select',
        },
        touched: {
          test: false,
        },
      });

      expect(getError('test')).toBe('');
      expect(hasError('test')).toBe(false);
    });

    test('gets nested touched error message', () => {
      const { getError } = createErrorHelper<{ test: { something: string } }>({
        errors: {
          test: {
            something: 'Please select',
          },
        },
        touched: {
          test: {
            something: true,
          },
        },
      });

      expect(getError('test.something')).toBe('Please select');
    });

    test('does not get nested untouched error message', () => {
      const { getError, hasError } = createErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: 'Please select',
          },
        },
        touched: {
          test: {
            something: false,
          },
        },
      });

      expect(getError('test.something')).toBe('');
      expect(hasError('test.something')).toBe(false);
    });

    test('does not get error message when path does not match', () => {
      const { getError, hasError } = createErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: 'Please select',
          },
        },
        touched: {
          test: {
            something: true,
          },
        },
      });

      expect(getError('invalidPath')).toBe('');
      expect(hasError('invalidPath')).toBe(false);
    });

    test('does not get error message when path only partially matches', () => {
      const { getError, hasError } = createErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: 'Please select',
          },
        },
        touched: {
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
      const { getAllErrors } = createErrorHelper({
        errors: {
          test: 'Please select',
        },
        touched: {
          test: true,
        },
      });

      expect(getAllErrors()).toEqual({
        test: 'Please select',
      });
    });

    test('does not get untouched errors', () => {
      const { getAllErrors } = createErrorHelper({
        errors: {
          test: 'Please select',
        },
        touched: {
          test: false,
        },
      });

      expect(getAllErrors()).toEqual({});
    });

    test('gets nested touched errors as single-level object with dot-notation keys', () => {
      const { getAllErrors } = createErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: 'Please select',
          },
        },
        touched: {
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
      const { getAllErrors } = createErrorHelper<{
        test: { something: string };
      }>({
        errors: {
          test: {
            something: 'Please select',
          },
        },
        touched: {
          test: {
            something: false,
          },
        },
      });

      expect(getAllErrors()).toEqual({});
    });

    test('gets deeply nested touched errors as single-level object with dot-notation keys', () => {
      const { getAllErrors } = createErrorHelper<{
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
                d: 'Please select',
              },
            },
          },
        },
        touched: {
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
      const { getAllErrors } = createErrorHelper<{
        address: {
          line1: string;
          line2: string;
        };
        firstName: string;
        lastName: string;
      }>({
        errors: {
          address: {
            line1: 'Address 1 is required',
            line2: 'Address 2 is required',
          },
          firstName: 'First name is required',
          lastName: 'Last name is required',
        },
        touched: {
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

    test('handle single undefined, touched value in array in errors object', () => {
      const { getAllErrors } = createErrorHelper<{
        subjects: undefined[];
      }>({
        errors: {
          subjects: [undefined],
        } as never,
        touched: {
          subjects: [true],
        } as never,
      });
      expect(getAllErrors()).toEqual({});
    });

    test('handle undefined in array in errors object', () => {
      const { getAllErrors } = createErrorHelper<{
        subjects: ({ content: string } | undefined)[];
      }>({
        errors: {
          subjects: [
            undefined,
            { content: 'Error two' },
            { content: 'Error three' },
          ],
        } as never,
        touched: {
          subjects: [{ content: false }, { content: true }, { content: true }],
        } as never,
      });
      expect(getAllErrors()).toEqual({
        'subjects.1.content': 'Error two',
        'subjects.2.content': 'Error three',
      });
    });
  });
});
