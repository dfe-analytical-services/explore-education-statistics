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
  const mockElements = [] as Element[];

  describe('getInvalidContent', () => {
    beforeEach(() => {
      jest.clearAllMocks();
      mockGetInvalidContent.mockReturnValue([]);
    });
    test('returns undefined when no invalid content', () => {
      mockGetInvalidContent.mockReturnValue([]);
      const result = getContentErrors(mockElements, { checkContent: true });
      expect(result).toBeUndefined();
    });

    test('does not check content when checkContent is false', () => {
      const result = getContentErrors(mockElements, { checkContent: false });
      expect(mockGetInvalidContent).not.toHaveBeenCalled();
      expect(result).toBeUndefined();
    });

    test('returns error message for single invalid content', () => {
      mockGetInvalidContent.mockReturnValue([
        {
          type: 'repeatedLinkText',
          details: 'dummy details',
        },
      ]);
      const result = getContentErrors(mockElements, { checkContent: true });
      expect(result?.errorMessage).toContain('1 accessibility error.');
    });

    test('returns error message for multiple invalid content', () => {
      mockGetInvalidContent.mockReturnValue([
        {
          type: 'repeatedLinkText',
          details: 'dummy details',
        },
        {
          type: 'boldAsHeading',
          details: 'dummy details',
        },
      ]);
      const result = getContentErrors(mockElements, { checkContent: true });
      expect(result?.errorMessage).toContain('2 accessibility errors.');
    });

    test('renders InvalidContentDetails component when invalid content exists', () => {
      mockGetInvalidContent.mockReturnValue([
        {
          type: 'repeatedLinkText',
          details: 'dummy details',
        },
      ]);
      const result = getContentErrors(mockElements, { checkContent: true });
      expect(result?.contentErrorDetails).toBeTruthy();
    });
  });

  describe('getInvalidLinks', () => {
    beforeEach(() => {
      jest.clearAllMocks();
      mockGetInvalidLinks.mockReturnValue([]);
    });
    test('returns undefined when no invalid content', () => {
      mockGetInvalidLinks.mockReturnValue([]);
      const result = getContentErrors(mockElements, { checkLinks: true });
      expect(result).toBeUndefined();
    });

    test('does not check content when checkLinks is false', () => {
      const result = getContentErrors(mockElements, { checkLinks: false });
      expect(mockGetInvalidLinks).not.toHaveBeenCalled();
      expect(result).toBeUndefined();
    });

    test('returns error message for single invalid content', () => {
      mockGetInvalidLinks.mockReturnValue([
        {
          text: 'string',
          url: 'http://localhost/url',
        },
      ]);
      const result = getContentErrors(mockElements, { checkLinks: true });
      expect(result?.errorMessage).toContain('1 link has an invalid URL.');
      expect(result?.contentErrorDetails).toBeTruthy();
    });

    test('returns error message for multiple invalid content', () => {
      mockGetInvalidLinks.mockReturnValue([
        {
          text: 'string',
          url: 'http://localhost/url',
        },
        {
          text: 'string',
          url: 'http://www.google.com',
        },
      ]);
      const result = getContentErrors(mockElements, { checkLinks: true });
      expect(result?.errorMessage).toContain('2 links have invalid URLs.');
      expect(result?.contentErrorDetails).toBeTruthy();
    });
  });

  describe('getInvalidImages', () => {
    beforeEach(() => {
      jest.clearAllMocks();
      mockGetInvalidImages.mockReturnValue([]);
    });
    test('returns undefined when no invalid content', () => {
      mockGetInvalidImages.mockReturnValue([]);
      const result = getContentErrors(mockElements, { checkImages: true });
      expect(result).toBeUndefined();
    });

    test('does not check content when checkImages is false', () => {
      const result = getContentErrors(mockElements, { checkImages: false });
      expect(mockGetInvalidImages).not.toHaveBeenCalled();
      expect(result).toBeUndefined();
    });

    test('returns error message for single invalid content', () => {
      mockGetInvalidImages.mockReturnValue([
        {
          name: 'string',
        },
      ]);
      const result = getContentErrors(mockElements, { checkImages: true });
      expect(result?.errorMessage).toContain(
        '1 image does not have alternative text.',
      );
      expect(result?.contentErrorDetails).toBeTruthy();
    });

    test('returns error message for multiple invalid content', () => {
      mockGetInvalidImages.mockReturnValue([
        {
          name: 'string',
        },
        {
          name: 'string',
        },
      ]);
      const result = getContentErrors(mockElements, { checkImages: true });
      expect(result?.errorMessage).toContain(
        '2 images do not have alternative text.',
      );
      expect(result?.contentErrorDetails).toBeTruthy();
    });
  });
});
