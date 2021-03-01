import fieldId from '@common/components/form/util/fieldId';

describe('fieldId', () => {
  test('returns correct id from simple name', () => {
    expect(fieldId('testForm', 'name')).toEqual('testForm-name');
  });

  test('returns correct id from name with property', () => {
    expect(fieldId('testForm', 'user.name')).toEqual('testForm-user-name');
  });

  test('returns correct id from array name', () => {
    expect(fieldId('testForm', 'users[0]')).toEqual('testForm-users-0');
  });

  test('returns correct id from array name with property', () => {
    expect(fieldId('testForm', 'users[0].name')).toEqual(
      'testForm-users-0-name',
    );
  });

  test('returns correct id when empty form id', () => {
    expect(fieldId('', 'name')).toEqual('name');
    expect(fieldId('', 'user.name')).toEqual('user-name');
    expect(fieldId('', 'users[0]')).toEqual('users-0');
    expect(fieldId('', 'users[0].name')).toEqual('users-0-name');
  });

  test('throws if name is empty', () => {
    expect(() => {
      fieldId('testForm', '');
    }).toThrow();
  });
});
