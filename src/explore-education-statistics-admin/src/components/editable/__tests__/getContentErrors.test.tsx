import { render } from '@testing-library/react';
import getContentErrors from '@admin/components/editable/utils/getContentErrors';
import getInvalidContent from '@admin/components/editable/utils/getInvalidContent';
import getInvalidLinks from '@admin/components/editable/utils/getInvalidLinks';
import getInvalidImages from '@admin/components/editable/utils/getInvalidImages';
import { Element } from '@admin/types/ckeditor';

jest.mock('@admin/components/editable/utils/getInvalidImages');
jest.mock('@admin/components/editable/utils/getInvalidLinks');
jest.mock('@admin/components/editable/utils/getInvalidContent');

const mockGetInvalidContent = getInvalidContent as jest.MockedFunction<
  typeof getInvalidContent
>;
const mockGetInvalidLinks = getInvalidLinks as jest.MockedFunction<
  typeof getInvalidLinks
>;
const mockGetInvalidImages = getInvalidImages as jest.MockedFunction<
  typeof getInvalidImages
>;

describe('getContentErrors', () => {
  const elements = [] as Element[];

  beforeEach(() => {
    jest.clearAllMocks();
    mockGetInvalidContent.mockReturnValue([]);
    mockGetInvalidLinks.mockReturnValue([]);
    mockGetInvalidImages.mockReturnValue([]);
  });

  test('runs all validators when content, links and images checks are enabled', () => {
    mockGetInvalidContent.mockReturnValue([
      { type: 'boldAsHeading', details: 'content error' },
    ]);
    mockGetInvalidLinks.mockReturnValue([
      { text: 'Broken link', url: 'http://invalid' },
    ]);
    mockGetInvalidImages.mockReturnValue([{ name: 'image.png' }]);

    const result = getContentErrors(elements, {
      checkContent: true,
      checkLinks: true,
      checkImages: true,
    });

    expect(mockGetInvalidContent).toHaveBeenCalledWith(elements);
    expect(mockGetInvalidLinks).toHaveBeenCalledWith(elements);
    expect(mockGetInvalidImages).toHaveBeenCalledWith(elements);

    expect(result).toBeDefined();
    expect(result?.errorMessage).toContain(
      'Content errors have been found: 1 image does not have alternative text.  1 link has an invalid URL. 1 accessibility error.',
    );
    expect(result?.contentErrorDetails).toBeTruthy();
  });

  describe('content validation', () => {
    test('returns undefined when no invalid content', () => {
      const result = getContentErrors(elements, {
        checkContent: true,
        checkLinks: false,
        checkImages: false,
      });
      expect(result).toBeUndefined();
    });

    test('does not run when checkContent is false', () => {
      const result = getContentErrors(elements, {
        checkContent: false,
        checkLinks: false,
        checkImages: false,
      });
      expect(mockGetInvalidContent).not.toHaveBeenCalled();
      expect(result).toBeUndefined();
    });

    test.each([
      [1, '1 accessibility error.'],
      [2, '2 accessibility errors.'],
    ])(
      'returns correct error message for %i invalid content items',
      (count, expectedMessage) => {
        mockGetInvalidContent.mockReturnValue(
          Array.from({ length: count }, () => ({
            type: 'repeatedLinkText',
            details: 'dummy',
          })),
        );

        const result = getContentErrors(elements, {
          checkContent: true,
          checkLinks: false,
          checkImages: false,
        });

        expect(result?.errorMessage).toContain(expectedMessage);
        expect(result?.contentErrorDetails).toBeTruthy();
      },
    );

    test('matches snapshot for content error details', () => {
      mockGetInvalidContent.mockReturnValue([
        {
          type: 'repeatedLinkText',
          details: 'dummy details',
        },
      ]);

      const result = getContentErrors(elements, {
        checkContent: true,
        checkLinks: false,
        checkImages: false,
      });

      expect(result?.contentErrorDetails).toBeTruthy();

      const { container } = render(result!.contentErrorDetails);

      expect(container).toMatchSnapshot();
    });
  });

  describe('link validation', () => {
    test('returns undefined when no invalid links', () => {
      const result = getContentErrors(elements, {
        checkLinks: true,
        checkImages: false,
        checkContent: false,
      });
      expect(result).toBeUndefined();
    });

    test('does not run when checkLinks is false', () => {
      const result = getContentErrors(elements, {
        checkLinks: false,
        checkImages: false,
        checkContent: false,
      });
      expect(mockGetInvalidLinks).not.toHaveBeenCalled();
      expect(result).toBeUndefined();
    });

    test.each([
      [1, '1 link has an invalid URL.'],
      [2, '2 links have invalid URLs.'],
    ])(
      'returns correct error message for %i invalid links',
      (count, expectedMessage) => {
        mockGetInvalidLinks.mockReturnValue(
          Array.from({ length: count }, () => ({
            text: 'text',
            url: 'http://invalid',
          })),
        );

        const result = getContentErrors(elements, {
          checkLinks: true,
          checkImages: false,
          checkContent: false,
        });

        expect(result?.errorMessage).toContain(expectedMessage);
        expect(result?.contentErrorDetails).toBeTruthy();
      },
    );

    test('matches snapshot for link error details', () => {
      mockGetInvalidLinks.mockReturnValue([
        { text: 'Example link', url: 'http://invalid-url' },
      ]);

      const result = getContentErrors(elements, {
        checkLinks: true,
        checkImages: false,
        checkContent: false,
      });

      const { container } = render(result!.contentErrorDetails);

      expect(container).toMatchSnapshot();
    });
  });

  describe('image validation', () => {
    test('returns undefined when no invalid images', () => {
      const result = getContentErrors(elements, {
        checkImages: true,
        checkContent: false,
        checkLinks: false,
      });
      expect(result).toBeUndefined();
    });

    test('does not run when checkImages is false', () => {
      const result = getContentErrors(elements, {
        checkImages: false,
        checkContent: false,
        checkLinks: false,
      });
      expect(mockGetInvalidImages).not.toHaveBeenCalled();
      expect(result).toBeUndefined();
    });

    test.each([
      [1, '1 image does not have alternative text.'],
      [2, '2 images do not have alternative text.'],
    ])(
      'returns correct error message for %i invalid images',
      (count, expectedMessage) => {
        mockGetInvalidImages.mockReturnValue(
          Array.from({ length: count }, () => ({ name: 'image' })),
        );

        const result = getContentErrors(elements, {
          checkImages: true,
          checkContent: false,
          checkLinks: false,
        });

        expect(result?.errorMessage).toContain(expectedMessage);
        expect(result?.contentErrorDetails).toBeTruthy();
      },
    );

    test('matches snapshot for image error details', () => {
      mockGetInvalidImages.mockReturnValue([{ name: 'image.png' }]);

      const result = getContentErrors(elements, {
        checkImages: true,
        checkContent: false,
        checkLinks: false,
      });

      const { container } = render(result!.contentErrorDetails);

      expect(container).toMatchSnapshot();
    });
  });
});
