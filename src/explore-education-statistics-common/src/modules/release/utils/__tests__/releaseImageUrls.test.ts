import {
  insertReleaseIdPlaceholders,
  replaceReleaseIdPlaceholders,
} from '../releaseImageUrls';

describe('replaceReleaseIdPlaceholders', () => {
  test('adds release id to single uri', () => {
    expect(
      replaceReleaseIdPlaceholders(
        '/api/releases/{releaseId}/images/some-image-id',
        'some-release-id',
      ),
    ).toBe('/api/releases/some-release-id/images/some-image-id');
  });

  test('adds release id for multiple uris', () => {
    expect(
      replaceReleaseIdPlaceholders(
        '/api/releases/{releaseId}/images/some-image-id-100 100w, ' +
          '/api/releases/{releaseId}/images/some-image-id-200 200w, ' +
          '/api/releases/{releaseId}/images/some-image-id-300 300w',
        'some-release-id',
      ),
    ).toBe(
      '/api/releases/some-release-id/images/some-image-id-100 100w, ' +
        '/api/releases/some-release-id/images/some-image-id-200 200w, ' +
        '/api/releases/some-release-id/images/some-image-id-300 300w',
    );
  });

  test('does not add release id when no matching placeholder', () => {
    expect(
      replaceReleaseIdPlaceholders(
        '/api/releases/[releaseId]/images/some-image-id',
        'some-release-id',
      ),
    ).toBe('/api/releases/[releaseId]/images/some-image-id');

    expect(
      replaceReleaseIdPlaceholders(
        '/api/releases/not-a-placeholder/images/some-image-id',
        'some-release-id',
      ),
    ).toBe('/api/releases/not-a-placeholder/images/some-image-id');

    expect(
      replaceReleaseIdPlaceholders(
        '/api/releases/images/some-image-id',
        'some-release-id',
      ),
    ).toBe('/api/releases/images/some-image-id');
  });
});

describe('insertReleaseIdPlaceholders', () => {
  test('adds release id placeholder to single uri', () => {
    expect(
      insertReleaseIdPlaceholders(
        '/api/releases/some-release-id/images/some-image-id',
      ),
    ).toBe('/api/releases/{releaseId}/images/some-image-id');
  });

  test('adds release id placeholder for multiple uris', () => {
    expect(
      insertReleaseIdPlaceholders(
        '/api/releases/some-release-id-1/images/some-image-id-100 100w, ' +
          '/api/releases/some-release-id-2/images/some-image-id-200 200w, ' +
          '/api/releases/some-release-id-3/images/some-image-id-300 300w',
      ),
    ).toBe(
      '/api/releases/{releaseId}/images/some-image-id-100 100w, ' +
        '/api/releases/{releaseId}/images/some-image-id-200 200w, ' +
        '/api/releases/{releaseId}/images/some-image-id-300 300w',
    );
  });

  test('does not add release id placeholder when uri does not match pattern', () => {
    expect(
      insertReleaseIdPlaceholders(
        '/api/releases/[releaseId]/images/some-image-id',
      ),
    ).toBe('/api/releases/[releaseId]/images/some-image-id');

    expect(
      insertReleaseIdPlaceholders('/api/releases/images/some-image-id'),
    ).toBe('/api/releases/images/some-image-id');

    expect(insertReleaseIdPlaceholders('/api/images/some-image-id')).toBe(
      '/api/images/some-image-id',
    );
  });
});
