import mapValuesWithKeys from '../mapValuesWithKeys';

describe('mapValuesWithKeys', () => {
  test('map empty object to empty object ', () => {
    expect(mapValuesWithKeys({}, () => null)).toEqual({});
  });

  test('map with key', () => {
    const origObj = {
      firstName: 'John',
      lastName: 'Doe',
    };

    const newObj = mapValuesWithKeys(origObj, key => key.toUpperCase());

    expect(newObj).toEqual({
      firstName: 'FIRSTNAME',
      lastName: 'LASTNAME',
    });
  });

  test('map with value', () => {
    const origObj = {
      firstName: 'John',
      lastName: 'Doe',
    };

    const newObj = mapValuesWithKeys(origObj, (_, value) =>
      value.toUpperCase(),
    );

    expect(newObj).toEqual({
      firstName: 'JOHN',
      lastName: 'DOE',
    });
  });

  test('map with key and value', () => {
    const origObj = {
      firstName: 'John',
      lastName: 'Doe',
    };

    const newObj = mapValuesWithKeys(
      origObj,
      (key, value) => `${key}(${value})`,
    );

    expect(newObj).toEqual({
      firstName: 'firstName(John)',
      lastName: 'lastName(Doe)',
    });
  });
});
