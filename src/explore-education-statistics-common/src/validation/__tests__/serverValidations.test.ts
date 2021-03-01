import { Dictionary } from '@common/types';
import {
  convertServerFieldErrors,
  mapFieldErrors,
  FieldMessageMapper,
} from '@common/validation/serverValidations';

describe('serverValidations', () => {
  describe('mapFieldErrors', () => {
    test('maps non-field error to `target` field based on its message matching one of its `messages`', () => {
      const mapper = mapFieldErrors({
        target: 'test',
        messages: {
          TEST_CODE: 'Test message',
        },
      });

      expect(
        mapper({
          message: 'TEST_CODE',
        }),
      ).toEqual<ReturnType<FieldMessageMapper>>({
        targetField: 'test',
        message: 'Test message',
      });
    });

    test('maps field error to a `target` field when `source` and one of its `messages` match', () => {
      const mapper = mapFieldErrors({
        target: 'test',
        source: 'test2',
        messages: {
          TEST_CODE: 'Test message',
        },
      });

      expect(
        mapper({
          sourceField: 'test2',
          message: 'TEST_CODE',
        }),
      ).toEqual<ReturnType<FieldMessageMapper>>({
        targetField: 'test',
        message: 'Test message',
      });
    });

    test('maps field error to `target` field with original message if `source` matches and no `messages` are provided', () => {
      const mapper = mapFieldErrors({
        target: 'test',
        source: 'test2',
      });

      expect(
        mapper({
          sourceField: 'test2',
          message: 'The original error message',
        }),
      ).toEqual<ReturnType<FieldMessageMapper>>({
        targetField: 'test',
        message: 'The original error message',
      });
    });

    test('does not map non-field error with message that does not match any of its `messages`', () => {
      const mapper = mapFieldErrors({
        target: 'test',
        messages: {
          TEST_CODE: 'Test message',
        },
      });

      expect(mapper({ message: 'WRONG_CODE' })).toBeUndefined();
    });

    test('does not map non-field error when there are no `messages`', () => {
      const mapper = mapFieldErrors({
        target: 'test',
      });

      expect(mapper({ message: 'WRONG_CODE' })).toBeUndefined();
    });

    test('does not map any errors that do not match the `source`', () => {
      const mapper = mapFieldErrors({
        target: 'test',
        source: 'test2',
        messages: {
          TEST_CODE: 'Test message',
        },
      });

      expect(
        mapper({
          sourceField: 'test2',
          message: 'WRONG_CODE',
        }),
      ).toBeUndefined();

      expect(
        mapper({
          message: 'TEST_CODE',
        }),
      ).toBeUndefined();

      expect(
        mapper({
          sourceField: 'test3',
          message: 'TEST_CODE',
        }),
      ).toBeUndefined();
    });
  });

  describe('convertServerFieldErrors', () => {
    test('returns field errors without mappers', () => {
      const fieldErrors = convertServerFieldErrors({
        errors: {
          test: ['Test message'],
        },
        status: 400,
        title: 'Something went wrong',
      });

      expect(fieldErrors).toEqual({
        test: 'Test message',
      });
    });

    test('returns nested field error without mappers', () => {
      const fieldErrors = convertServerFieldErrors({
        errors: {
          'test.field.here': ['Test message'],
        },
        status: 400,
        title: 'Something went wrong',
      });

      expect(fieldErrors).toEqual({
        test: {
          field: {
            here: 'Test message',
          },
        },
      });
    });

    test('returns array field error without mappers', () => {
      const fieldErrors = convertServerFieldErrors({
        errors: {
          'test[1]': ['Test message'],
          'testField.here[1]': ['Test message 2'],
        },
        status: 400,
        title: 'Something went wrong',
      });

      expect(fieldErrors).toEqual({
        test: [undefined, 'Test message'],
        testField: {
          here: [undefined, 'Test message 2'],
        },
      });
    });

    test('returns no field errors for empty server source field without mappers', () => {
      const fieldErrors = convertServerFieldErrors({
        errors: {
          '': ['Test message'],
        },
        status: 400,
        title: 'Something went wrong',
      });

      expect(fieldErrors).toEqual({});
    });

    test('returns field error mapped by last candidate mapper', () => {
      const fieldErrors = convertServerFieldErrors(
        {
          errors: {
            test: ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'test',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test',
            messages: {
              TEST_CODE: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        test: 'Test message 2',
      });
    });

    test('returns field errors for multiple server source fields', () => {
      const fieldErrors = convertServerFieldErrors<Dictionary<string>>(
        {
          errors: {
            test1: ['TEST_CODE'],
            test2: ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'test1',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test2',
            messages: {
              TEST_CODE: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        test1: 'Test message',
        test2: 'Test message 2',
      });
    });

    test('returns field errors mapped from empty server source field', () => {
      const fieldErrors = convertServerFieldErrors<Dictionary<string>>(
        {
          errors: {
            '': ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'test1',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test2',
            messages: {
              TEST_CODE: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        test1: 'Test message',
        test2: 'Test message 2',
      });
    });

    test('returns nested field error mapped from from nested server source field', () => {
      const fieldErrors = convertServerFieldErrors(
        {
          errors: {
            'test.nestedField.here': ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'test.nestedField.here',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        test: {
          nestedField: {
            here: 'Test message',
          },
        },
      });
    });

    test('returns nested field error mapped from empty server source field', () => {
      const fieldErrors = convertServerFieldErrors(
        {
          errors: {
            '': ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'test.nestedField.here',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        test: {
          nestedField: {
            here: 'Test message',
          },
        },
      });
    });

    test('returns array field error mapped from array server source field', () => {
      const fieldErrors = convertServerFieldErrors(
        {
          errors: {
            'test[1]': ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'test[1]',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        test: [undefined, 'Test message'],
      });
    });

    test('returns array field error mapped from empty server source field', () => {
      const fieldErrors = convertServerFieldErrors(
        {
          errors: {
            '': ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'test[1]',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        test: [undefined, 'Test message'],
      });
    });

    test('returns mapped field errors regardless of server source field casing', () => {
      const fieldErrors = convertServerFieldErrors<Dictionary<string>>(
        {
          errors: {
            TestField: ['TEST_CODE'],
            test_field_2: ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'testField',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'testField2',
            messages: {
              TEST_CODE: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        testField: 'Test message',
        testField2: 'Test message 2',
      });
    });

    test('returns mapped nested field errors regardless of server source field casing', () => {
      const fieldErrors = convertServerFieldErrors<Dictionary<string>>(
        {
          errors: {
            'Test.Field.Here': ['TEST_CODE'],
            'test.field_2.here': ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'test.field.here',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test.field2.here',
            messages: {
              TEST_CODE: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        test: {
          field: {
            here: 'Test message',
          },
          field2: {
            here: 'Test message 2',
          },
        },
      });
    });

    test('returns mapped array field errors regardless of server source field casing', () => {
      const fieldErrors = convertServerFieldErrors<Dictionary<string>>(
        {
          errors: {
            'TestField[1]': ['TEST_CODE'],
            'test.field_2[1]': ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'testField[1]',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test.field2[1]',
            messages: {
              TEST_CODE: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        testField: [undefined, 'Test message'],
        test: {
          field2: [undefined, 'Test message 2'],
        },
      });
    });

    test('returns mixture of mapped and unmapped field errors', () => {
      const fieldErrors = convertServerFieldErrors<Dictionary<string>>(
        {
          errors: {
            test1: ['TEST_CODE'],
            test2: ['Not a test code'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'test1',
            messages: {
              TEST_CODE: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test2',
            messages: {
              TEST_CODE: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        test1: 'Test message',
        test2: 'Not a test code',
      });
    });

    test('returns unmapped field errors when no mappers match', () => {
      const fieldErrors = convertServerFieldErrors(
        {
          errors: {
            test: ['TEST_CODE'],
          },
          status: 400,
          title: 'Something went wrong',
        },
        [
          mapFieldErrors({
            target: 'test',
            messages: {
              WRONG_CODE: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test',
            source: 'wrongField',
            messages: {
              TEST_CODE: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual({
        test: 'TEST_CODE',
      });
    });
  });
});
