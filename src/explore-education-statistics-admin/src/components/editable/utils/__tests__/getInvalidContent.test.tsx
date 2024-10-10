import getInvalidContent from '@admin/components/editable/utils/getInvalidContent';
import { JsonElement } from '@admin/types/ckeditor';

describe('getInvalidContent', () => {
  test('returns errors for paragraphs which are entirely bold', () => {
    const testContent: JsonElement[] = [
      {
        name: 'paragraph',
        children: [
          {
            data: 'A line of normal text',
          },
        ],
      },
      {
        name: 'paragraph',
        children: [
          {
            attributes: {
              bold: true,
            },
            data: 'A line of bold text',
          },
        ],
      },
      {
        name: 'paragraph',
        children: [
          {
            data: 'A line with a mix of ',
          },
          {
            attributes: {
              bold: true,
            },
            data: 'bold',
          },
          {
            data: ' and normal text',
          },
        ],
      },
    ];

    const result = getInvalidContent(testContent);
    expect(result).toEqual([
      {
        type: 'boldAsHeading',
        message: 'A line of bold text',
      },
    ]);
  });

  test('returns errors for empty headings', () => {
    const testContent: JsonElement[] = [
      {
        name: 'heading3',
        children: [
          {
            data: 'a heading',
          },
        ],
      },
      {
        name: 'heading3',
      },
    ];

    const result = getInvalidContent(testContent);
    expect(result).toEqual([
      {
        type: 'emptyHeading',
      },
    ]);
  });

  test('returns errors for tables without headers', () => {
    const testContent: JsonElement[] = [
      {
        attributes: {
          headingColumns: 1,
        },
        name: 'table',
        children: [
          {
            name: 'tableRow',
            children: [
              {
                name: 'tableCell',
                children: [
                  {
                    name: 'paragraph',
                    children: [
                      {
                        data: 'th',
                      },
                    ],
                  },
                ],
              },
              {
                name: 'tableCell',
                children: [
                  {
                    name: 'paragraph',
                    children: [
                      {
                        data: 'tcd',
                      },
                    ],
                  },
                ],
              },
            ],
          },
        ],
      },
      {
        attributes: {
          headingRows: 1,
        },
        name: 'table',
        children: [
          {
            name: 'tableRow',
            children: [
              {
                name: 'tableCell',
                children: [
                  {
                    name: 'paragraph',
                    children: [
                      {
                        data: 'th',
                      },
                    ],
                  },
                ],
              },
            ],
          },
          {
            name: 'tableRow',
            children: [
              {
                name: 'tableCell',
                children: [
                  {
                    name: 'paragraph',
                    children: [
                      {
                        data: 'td',
                      },
                    ],
                  },
                ],
              },
            ],
          },
        ],
      },
      {
        name: 'table',
        children: [
          {
            name: 'tableRow',
            children: [
              {
                name: 'tableCell',
                children: [
                  {
                    name: 'paragraph',
                    children: [
                      {
                        data: 'td',
                      },
                    ],
                  },
                ],
              },
              {
                name: 'tableCell',
                children: [
                  {
                    name: 'paragraph',
                    children: [
                      {
                        data: 'td',
                      },
                    ],
                  },
                ],
              },
            ],
          },
        ],
      },
    ];

    const result = getInvalidContent(testContent);
    expect(result).toEqual([
      {
        type: 'missingTableHeaders',
      },
    ]);
  });

  test('returns errors for skipped heading levels', () => {
    const testContent: JsonElement[] = [
      {
        name: 'heading3',
        children: [
          {
            data: 'heading 3',
          },
        ],
      },
      {
        name: 'heading4',
        children: [
          {
            data: 'heading 4',
          },
        ],
      },
      {
        name: 'heading3',
        children: [
          {
            data: 'heading3',
          },
        ],
      },
      {
        name: 'heading5',
        children: [
          {
            data: 'heading 5',
          },
        ],
      },
      {
        name: 'heading4',
        children: [
          {
            data: 'heading 4',
          },
        ],
      },
    ];

    const result = getInvalidContent(testContent);
    expect(result).toEqual([
      {
        type: 'skippedHeadingLevel',
        message: 'h3 (heading3) to h5 (heading 5)',
      },
    ]);
  });

  test('returns an error when the first heading is higher than h3', () => {
    const testContent: JsonElement[] = [
      {
        name: 'heading4',
        children: [
          {
            data: 'heading 4',
          },
        ],
      },
    ];

    const result = getInvalidContent(testContent);
    expect(result).toEqual([
      {
        type: 'skippedHeadingLevel',
        message: 'h2 (section title) to h4 (heading 4)',
      },
    ]);
  });

  test('returns an error when there is repeated link text within the same page where the links are different', () => {
    const testContent: JsonElement[] = [
      {
        name: 'paragraph',
        children: [
          {
            data: 'words ',
          },
          {
            attributes: {
              linkHref: 'https://gov.uk',
              linkOpenInNewTab: true,
            },
            data: 'link to something',
          },
          {
            data: ' words',
          },
        ],
      },
      {
        name: 'paragraph',
        children: [
          {
            data: 'words ',
          },
          {
            attributes: {
              linkHref: 'https://bbc.co.uk',
              linkOpenInNewTab: true,
            },
            data: 'link to something',
          },
          {
            data: ' words',
          },
        ],
      },
    ];

    const result = getInvalidContent(testContent);

    expect(result).toEqual([
      {
        type: 'repeatedLinkText',
        message: 'link to something',
        details: 'https://gov.uk, https://bbc.co.uk',
      },
      {
        type: 'repeatedLinkText',
        message: 'link to something',
        details: 'https://bbc.co.uk, https://gov.uk',
      },
    ]);
  });

  test('returns an error when the link text is an inaccessible word or phrase', () => {
    const testContent: JsonElement[] = [
      {
        name: 'paragraph',
        children: [
          {
            data: 'words ',
          },
          {
            attributes: {
              linkHref: 'https://gov.uk',
              linkOpenInNewTab: true,
            },
            data: 'link to something',
          },
          {
            data: ' words',
          },
        ],
      },
      {
        name: 'paragraph',
        children: [
          {
            data: 'words ',
          },
          {
            attributes: {
              linkHref: 'https://bbc.co.uk',
              linkOpenInNewTab: true,
            },
            data: 'learn more',
          },
          {
            data: ' words',
          },
        ],
      },
      {
        name: 'paragraph',
        children: [
          {
            data: 'words ',
          },
          {
            attributes: {
              linkHref: 'https://bbc.co.uk',
              linkOpenInNewTab: true,
            },
            data: ' learn more ',
          },
          {
            data: ' words',
          },
        ],
      },
    ];

    const result = getInvalidContent(testContent);

    expect(result).toEqual([
      {
        type: 'badLinkText',
        message: 'learn more',
      },
      {
        type: 'badLinkText',
        message: ' learn more ',
      },
    ]);
  });

  test('returns an error when the link text is the same as the url', () => {
    const testContent: JsonElement[] = [
      {
        name: 'paragraph',
        children: [
          {
            data: 'words ',
          },
          {
            attributes: {
              linkHref: 'https://gov.uk',
              linkOpenInNewTab: true,
            },
            data: 'link to something',
          },
          {
            data: ' words',
          },
        ],
      },
      {
        name: 'paragraph',
        children: [
          {
            data: 'words ',
          },
          {
            attributes: {
              linkHref: 'https://bbc.co.uk',
              linkOpenInNewTab: true,
            },
            data: 'https://bbc.co.uk',
          },
          {
            data: ' words',
          },
        ],
      },
    ];

    const result = getInvalidContent(testContent);

    expect(result).toEqual([
      {
        type: 'urlLinkText',
        message: 'https://bbc.co.uk',
      },
    ]);
  });
});
