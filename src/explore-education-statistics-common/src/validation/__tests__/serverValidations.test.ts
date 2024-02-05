import { Dictionary } from '@common/types';
import {
  mapFieldErrors,
  FieldMessageMapper,
  mapFallbackFieldError,
  mapServerFieldErrors,
  FieldMessage,
} from '@common/validation/serverValidations';

describe('serverValidations', () => {
  describe('mapFieldErrors', () => {
    test('maps non-field error to `target` field based on its code matching one of its `messages`', () => {
      const mapper = mapFieldErrors({
        target: 'test',
        messages: {
          TestCode: 'Test message',
        },
      });

      expect(
        mapper({
          code: 'TestCode',
          message: '',
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
          TestCode: 'Test message',
        },
      });

      expect(
        mapper({
          sourceField: 'test2',
          code: 'TestCode',
          message: '',
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
          code: '',
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
          TestCode: 'Test message',
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
          TestCode: 'Test message',
        },
      });

      expect(
        mapper({
          sourceField: 'test2',
          code: 'WRONG_CODE',
          message: 'Server error',
        }),
      ).toBeUndefined();

      expect(
        mapper({ code: 'TestCode', message: 'Server error' }),
      ).toBeUndefined();

      expect(
        mapper({
          sourceField: 'test3',
          code: 'TestCode',
          message: 'Another server error',
        }),
      ).toBeUndefined();
    });
  });

  describe('mapServerFieldErrors', () => {
    test('returns field errors without mappers', () => {
      const fieldErrors = mapServerFieldErrors({
        errors: [
          {
            message: 'Test message',
            path: 'test',
          },
        ],
        status: 400,
        title: 'Something went wrong',
        type: '',
      });

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test',
          message: 'Test message',
          mapped: false,
        },
      ]);
    });

    test('returns nested field error without mappers', () => {
      const fieldErrors = mapServerFieldErrors({
        errors: [{ message: 'Test message', path: 'test.field.here' }],
        status: 400,
        title: 'Something went wrong',
        type: '',
      });

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test.field.here',
          message: 'Test message',
          mapped: false,
        },
      ]);
    });

    test('returns array field error without mappers', () => {
      const fieldErrors = mapServerFieldErrors({
        errors: [
          { path: 'test[1]', message: 'Test message' },
          { path: 'testField.here[1]', message: 'Test message 2' },
        ],
        status: 400,
        title: 'Something went wrong',
        type: '',
      });

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test[1]',
          message: 'Test message',
          mapped: false,
        },
        {
          field: 'testField.here[1]',
          message: 'Test message 2',
          mapped: false,
        },
      ]);
    });

    test('returns no field errors for empty server source field without mappers', () => {
      const fieldErrors = mapServerFieldErrors({
        errors: [{ message: 'Test message' }],
        status: 400,
        title: 'Something went wrong',
        type: '',
      });

      expect(fieldErrors).toEqual([]);
    });

    test('returns field error mapped to multiple mappers', () => {
      const fieldErrors = mapServerFieldErrors(
        {
          errors: [{ path: 'test', message: '', code: 'TestCode' }],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'test',
            messages: {
              TestCode: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test',
            messages: {
              TestCode: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test',
          message: 'Test message',
          mapped: true,
        },
        {
          field: 'test',
          message: 'Test message 2',
          mapped: true,
        },
      ]);
    });

    test('returns field errors for multiple server source fields', () => {
      const fieldErrors = mapServerFieldErrors<Dictionary<string>>(
        {
          errors: [
            { path: 'test1', message: '', code: 'TestCode' },
            { path: 'test2', message: '', code: 'TestCode' },
          ],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'test1',
            messages: {
              TestCode: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test2',
            messages: {
              TestCode: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test1',
          message: 'Test message',
          mapped: true,
        },
        {
          field: 'test2',
          message: 'Test message 2',
          mapped: true,
        },
      ]);
    });

    test('returns field errors mapped from empty server source fields', () => {
      const fieldErrors = mapServerFieldErrors<Dictionary<string>>(
        {
          errors: [
            { message: '', code: 'TestCode' },
            { message: '', code: 'TestCode2' },
          ],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'test1',
            messages: {
              TestCode: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test2',
            messages: {
              TestCode2: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test1',
          message: 'Test message',
          mapped: true,
        },
        {
          field: 'test2',
          message: 'Test message 2',
          mapped: true,
        },
      ]);
    });

    test('returns nested field error mapped from from nested server source field', () => {
      const fieldErrors = mapServerFieldErrors(
        {
          errors: [
            { path: 'test.nestedField.here', message: '', code: 'TestCode' },
          ],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'test.nestedField.here',
            messages: {
              TestCode: 'Test message',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test.nestedField.here',
          message: 'Test message',
          mapped: true,
        },
      ]);
    });

    test('returns nested field error mapped from empty server source field', () => {
      const fieldErrors = mapServerFieldErrors(
        {
          errors: [{ message: '', code: 'TestCode' }],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'test.nestedField.here',
            messages: {
              TestCode: 'Test message',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test.nestedField.here',
          message: 'Test message',
          mapped: true,
        },
      ]);
    });

    test('returns array field error mapped from array server source field', () => {
      const fieldErrors = mapServerFieldErrors(
        {
          errors: [{ path: 'test[1]', message: '', code: 'TestCode' }],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'test[1]',
            messages: {
              TestCode: 'Test message',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test[1]',
          message: 'Test message',
          mapped: true,
        },
      ]);
    });

    test('returns array field error mapped from empty server source field', () => {
      const fieldErrors = mapServerFieldErrors(
        {
          errors: [{ message: '', code: 'TestCode' }],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'test[1]',
            messages: {
              TestCode: 'Test message',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test[1]',
          message: 'Test message',
          mapped: true,
        },
      ]);
    });

    test('returns mapped field errors regardless of server source field casing', () => {
      const fieldErrors = mapServerFieldErrors<Dictionary<string>>(
        {
          errors: [
            { path: 'TestField', message: '', code: 'TestCode' },
            { path: 'test_field_2', message: '', code: 'TestCode' },
          ],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'testField',
            messages: {
              TestCode: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'testField2',
            messages: {
              TestCode: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'testField',
          message: 'Test message',
          mapped: true,
        },
        {
          field: 'testField2',
          message: 'Test message 2',
          mapped: true,
        },
      ]);
    });

    test('returns mapped nested field errors regardless of server source field casing', () => {
      const fieldErrors = mapServerFieldErrors<Dictionary<string>>(
        {
          errors: [
            { path: 'Test.Field.Here', message: '', code: 'TestCode' },
            { path: 'test.field_2.here', message: '', code: 'TestCode' },
          ],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'test.field.here',
            messages: {
              TestCode: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test.field2.here',
            messages: {
              TestCode: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test.field.here',
          message: 'Test message',
          mapped: true,
        },
        {
          field: 'test.field2.here',
          message: 'Test message 2',
          mapped: true,
        },
      ]);
    });

    test('returns mapped array field errors regardless of server source field casing', () => {
      const fieldErrors = mapServerFieldErrors<Dictionary<string>>(
        {
          errors: [
            { path: 'TestField[1]', message: '', code: 'TestCode' },
            { path: 'test.field_2[1]', message: '', code: 'TestCode' },
          ],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'testField[1]',
            messages: {
              TestCode: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test.field2[1]',
            messages: {
              TestCode: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'testField[1]',
          message: 'Test message',
          mapped: true,
        },
        {
          field: 'test.field2[1]',
          message: 'Test message 2',
          mapped: true,
        },
      ]);
    });

    test('returns mixture of mapped and unmapped field errors', () => {
      const fieldErrors = mapServerFieldErrors<Dictionary<string>>(
        {
          errors: [
            { path: 'test1', message: '', code: 'TestCode' },
            {
              path: 'test2',
              message: 'Test unmapped message',
              code: 'AnotherCode',
            },
          ],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'test1',
            messages: {
              TestCode: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test2',
            messages: {
              TestCode: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test1',
          message: 'Test message',
          mapped: true,
        },
        {
          field: 'test2',
          message: 'Test unmapped message',
          mapped: false,
        },
      ]);
    });

    test('returns fallback message mapped to field if no other mappings match', () => {
      const fieldErrors = mapServerFieldErrors<Dictionary<string>>(
        {
          errors: [{ message: '', code: 'UnmappedTestCode' }],
          status: 400,
          title: 'Something went wrong',
          type: '',
        },
        [
          mapFieldErrors({
            target: 'test1',
            messages: {
              TestCode: 'Test message',
            },
          }),
          mapFieldErrors({
            target: 'test2',
            messages: {
              TestCode: 'Test message 2',
            },
          }),
        ],
        mapFallbackFieldError({
          target: 'test1',
          fallbackMessage: 'Fallback message',
        }),
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test1',
          message: 'Fallback message',
          mapped: true,
        },
      ]);
    });

    test('returns unmapped field errors when no mappers match', () => {
      const fieldErrors = mapServerFieldErrors(
        {
          errors: [
            {
              path: 'test',
              message: 'Test unmapped message',
              code: 'TestCode',
            },
            {
              path: 'test2',
              message: 'Test unmapped message 2',
              code: 'TestCode2',
            },
          ],
          status: 400,
          title: 'Something went wrong',
          type: '',
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
              TestCode: 'Test message 2',
            },
          }),
        ],
      );

      expect(fieldErrors).toEqual<FieldMessage<unknown>[]>([
        {
          field: 'test',
          message: 'Test unmapped message',
          mapped: false,
        },
        {
          field: 'test2',
          message: 'Test unmapped message 2',
          mapped: false,
        },
      ]);
    });
  });
});
