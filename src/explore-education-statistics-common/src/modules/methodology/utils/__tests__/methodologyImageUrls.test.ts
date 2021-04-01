import {
  insertMethodologyIdPlaceholders,
  replaceMethodologyIdPlaceholders,
} from '../methodologyImageUrls';

describe('replaceMethodologyIdPlaceholders', () => {
  test('adds methodology id to single uri', () => {
    expect(
      replaceMethodologyIdPlaceholders(
        '/api/methodologies/{methodologyId}/images/some-image-id',
        'some-methodology-id',
      ),
    ).toBe('/api/methodologies/some-methodology-id/images/some-image-id');
  });

  test('adds methodology id for multiple uris', () => {
    expect(
      replaceMethodologyIdPlaceholders(
        '/api/methodologies/{methodologyId}/images/some-image-id-100 100w, ' +
          '/api/methodologies/{methodologyId}/images/some-image-id-200 200w, ' +
          '/api/methodologies/{methodologyId}/images/some-image-id-300 300w',
        'some-methodology-id',
      ),
    ).toBe(
      '/api/methodologies/some-methodology-id/images/some-image-id-100 100w, ' +
        '/api/methodologies/some-methodology-id/images/some-image-id-200 200w, ' +
        '/api/methodologies/some-methodology-id/images/some-image-id-300 300w',
    );
  });

  test('does not add methodology id when no matching placeholder', () => {
    expect(
      replaceMethodologyIdPlaceholders(
        '/api/methodologies/[methodologyId]/images/some-image-id',
        'some-methodology-id',
      ),
    ).toBe('/api/methodologies/[methodologyId]/images/some-image-id');

    expect(
      replaceMethodologyIdPlaceholders(
        '/api/methodologies/not-a-placeholder/images/some-image-id',
        'some-methodology-id',
      ),
    ).toBe('/api/methodologies/not-a-placeholder/images/some-image-id');

    expect(
      replaceMethodologyIdPlaceholders(
        '/api/methodologies/images/some-image-id',
        'some-methodology-id',
      ),
    ).toBe('/api/methodologies/images/some-image-id');
  });
});

describe('insertMethodologyIdPlaceholders', () => {
  test('adds methodology id placeholder to single uri', () => {
    expect(
      insertMethodologyIdPlaceholders(
        '/api/methodologies/some-methodology-id/images/some-image-id',
      ),
    ).toBe('/api/methodologies/{methodologyId}/images/some-image-id');
  });

  test('adds methodology id placeholder for multiple uris', () => {
    expect(
      insertMethodologyIdPlaceholders(
        '/api/methodologies/some-methodology-id-1/images/some-image-id-100 100w, ' +
          '/api/methodologies/some-methodology-id-2/images/some-image-id-200 200w, ' +
          '/api/methodologies/some-methodology-id-3/images/some-image-id-300 300w',
      ),
    ).toBe(
      '/api/methodologies/{methodologyId}/images/some-image-id-100 100w, ' +
        '/api/methodologies/{methodologyId}/images/some-image-id-200 200w, ' +
        '/api/methodologies/{methodologyId}/images/some-image-id-300 300w',
    );
  });

  test('does not add methodology id placeholder when uri does not match pattern', () => {
    expect(
      insertMethodologyIdPlaceholders(
        '/api/methodologies/[methodologyId]/images/some-image-id',
      ),
    ).toBe('/api/methodologies/[methodologyId]/images/some-image-id');

    expect(
      insertMethodologyIdPlaceholders(
        '/api/methodologies/images/some-image-id',
      ),
    ).toBe('/api/methodologies/images/some-image-id');

    expect(insertMethodologyIdPlaceholders('/api/images/some-image-id')).toBe(
      '/api/images/some-image-id',
    );
  });
});
