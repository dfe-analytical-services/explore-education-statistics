import generateIdFromHeading from '@common/components/util/generateIdFromHeading';

describe('generateIdFromHeading', () => {
  test('Returns expected result from normal text', () => {
    expect(generateIdFromHeading('Test Heading')).toEqual(
      'section-test-heading',
    );
  });

  test('Handles prefix if provided', () => {
    expect(generateIdFromHeading('Test Heading', 'prefix')).toEqual(
      'prefix-test-heading',
    );
  });

  test('Handles special characters', () => {
    expect(generateIdFromHeading('#Test@ "Heading"/')).toEqual(
      'section-test-heading',
    );
  });
});
