import typedKeys from '@common/utils/object/typedKeys';

describe('typedKeys', () => {
  test('returns object literal keys', () => {
    const keys = typedKeys({
      firstName: 'First name',
      lastName: 'Last name',
    });

    expect(keys).toEqual<typeof keys>(['firstName', 'lastName']);
  });

  test('returns class keys', () => {
    const keys = typedKeys(new TestPerson('First name', 'Last name'));

    expect(keys).toEqual<typeof keys>(['firstName', 'lastName']);
  });

  class TestPerson {
    constructor(
      public firstName: string,
      public lastName: string,
    ) {}
  }
});
