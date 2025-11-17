import getNavItemsFromContentSections from '@common/components/util/getNavItemsFromContentSections';

describe('getNavItemsFromContentSections', () => {
  test('Returns nav items with nested items correctly', () => {
    const output = [
      {
        id: 'section-section-1-heading',
        text: 'Section 1 heading',
        subNavItems: [],
      },
      {
        id: 'section-section-2-heading',
        text: 'Section 2 heading',
        subNavItems: [
          {
            id: 'test-section-2-subheading',
            text: 'Section 2 subheading',
          },
        ],
      },
      {
        id: 'section-section-3-heading',
        text: 'Section 3 heading',
        subNavItems: [],
      },
    ];

    const result = getNavItemsFromContentSections([
      {
        id: 'test-section-1-id',
        heading: 'Section 1 heading',
        content: [
          {
            type: 'HtmlBlock',
            body: '<p>Section 1 block 1 text</p>',
            id: 'test-section-1-block-1-id',
          },
          {
            type: 'DataBlock',
            dataBlockVersion: {
              dataBlockVersionId:
                'test-section-1-block-2-data-block-version-id',
              dataBlockParentId: 'test-section-1-block-2-data-block-parent-id',
              charts: [],
              heading: 'Data block heading',
              name: 'Data block name',
              query: {
                subjectId: 'test-subject-id',
                locationIds: ['test-location-id'],
                timePeriod: {
                  startYear: 2025,
                  startCode: 'AY',
                  endYear: 2025,
                  endCode: 'AY',
                },
                filters: ['test-filter-id'],
                indicators: ['test-indicator-id'],
              },
              source: 'Source',
              table: {
                tableHeaders: {
                  columnGroups: [],
                  columns: [
                    {
                      value: '2025_AY',
                      type: 'TimePeriod',
                    },
                  ],
                  rowGroups: [
                    [
                      {
                        value: 'filter-id',
                        type: 'Filter',
                      },
                    ],
                    [
                      {
                        level: 'localAuthority',
                        value: 'location-id',
                        type: 'Location',
                      },
                    ],
                  ],
                  rows: [
                    {
                      value: 'indicator-id',
                      type: 'Indicator',
                    },
                  ],
                },
              },
            },
            id: 'test-section-1-block-2-id',
          },
          {
            type: 'HtmlBlock',
            body: '<p>Section 1 block 3 text</p>',
            id: 'test-section-1-block-3-id',
          },
        ],
      },
      {
        id: 'test-section-2-id',
        heading: 'Section 2 heading',
        content: [
          {
            type: 'HtmlBlock',
            body: '<h3>Section 2 subheading</h3><p>Section 2 block 1 text</p>',
            id: 'test-section-2-block-1-id',
          },
        ],
      },
      {
        id: 'test-section-3-id',
        heading: 'Section 3 heading',
        content: [
          {
            type: 'EmbedBlock',
            embedBlock: {
              embedBlockId: 'test-embed-block-id',
              title: 'Embedded dashboard title',
              url: 'https://department-for-education.shinyapps.io/test-dashboard',
            },
            id: 'test-section-3-block-1-id',
          },
        ],
      },
    ]);
    expect(result).toEqual(output);
  });
});
