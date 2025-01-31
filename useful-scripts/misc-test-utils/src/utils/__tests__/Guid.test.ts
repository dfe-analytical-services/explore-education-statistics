import Guid from '../Guid';

describe('Guids', () => {
  test('should create a GUID', () => {
    const wrong = 'wrongguid';
    expect(Guid.isGuid(wrong)).toEqual(false);

    const correct = Guid.raw();

    expect(Guid.isGuid(correct)).toEqual(true);
  });

  test('should raw a GUID', () => {
    const wrong = 'wrongguid';
    expect(Guid.isGuid(wrong)).toEqual(false);

    const correct = Guid.raw();

    expect(Guid.isGuid(correct)).toEqual(true);
  });

  test('compares a GUID correctly', () => {
    const wrong = Guid.create();

    expect(wrong.equals(Guid.create())).toEqual(false);

    const correct = Guid.create();

    expect(correct.equals(correct)).toEqual(true);
  });

  test('should compare another GUID empty', () => {
    const wrong = Guid.createEmpty();
    expect(wrong.equals(Guid.create())).toEqual(false);

    const correct = Guid.createEmpty();
    expect(correct.equals(Guid.createEmpty())).toEqual(true);
  });

  test('should verify if a GUID is a valid GUID', () => {
    const wrong = "i'm not a guid";
    expect(Guid.isGuid(wrong)).toEqual(false);

    const correct = Guid.create();
    expect(Guid.isGuid(correct)).toEqual(true);
  });

  test('should parse a GUID', () => {
    const wrong = Guid.raw();
    expect(Guid.parse(wrong).equals(Guid.create())).toEqual(false);

    const correct = Guid.raw();

    expect(Guid.parse(correct).equals(Guid.parse(correct))).toEqual(true);
  });

  test('it should generate a unique value each time', () => {
    const guids: Guid[] = [];

    for (let i = 0; i < 3000; i += 1) {
      guids.push(Guid.create());
    }
    expect(guids.indexOf(guids[0]) < 0).toEqual(false);
    expect(guids.indexOf(Guid.create()) < 0).toEqual(true);
  });

  test('compares empty GUIDs correctly', () => {
    const emptyGuid = Guid.createEmpty();

    const notEmptyGuid = Guid.create();

    expect(Guid.createEmpty()).toEqual(emptyGuid);

    expect(Guid.createEmpty()).not.toEqual(notEmptyGuid);
  });
});
