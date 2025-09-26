import testTile from '@admin/pages/education-in-numbers/content/__tests__/__data__/testTile';
import { EinContent } from '@admin/services/educationInNumbersContentService';
import { EinSummaryWithPrevVersion } from '@admin/services/educationInNumbersService';

const testEinPageVersion: EinSummaryWithPrevVersion = {
  id: 'test-ein-page',
  title: 'Test Education in Numbers Page',
  slug: 'test-ein-page',
  description: 'Test description for the Education in Numbers page',
  version: 2,
  previousVersionId: '',
};

export const testEinPageContent: EinContent = {
  id: 'ein-page-0',
  title: 'Education in Numbers',
  slug: '2020-21',
  content: [
    {
      id: 'content-section-0',
      order: 0,
      heading: 'New section 3',
      content: [
        {
          id: 'content-section-0-content-0',
          body: '',
          type: 'HtmlBlock',
          order: 0,
        },
        {
          id: 'content-section-0-content-1',
          body: 'Part 2',
          type: 'HtmlBlock',
          order: 1,
        },
      ],
    },
    {
      id: 'content-section-1',
      order: 1,
      heading: 'New section',
      content: [],
    },
    {
      id: 'content-section-2',
      order: 2,
      heading: 'New section',
      content: [
        {
          id: 'content-section-0-content-0',
          title: 'Test tile group',
          type: 'TileGroupBlock',
          order: 0,
          tiles: [
            testTile,
            {
              id: 'tile-2',
              type: 'FreeTextStatTile',
              order: 1,
              title: 'Tile 2 title',
              statistic: '2000',
              trend: 'Tile 2 trend',
              linkText: 'Tile 2 link text',
              linkUrl: 'https://example.com/tile-2',
            },
          ],
        },
      ],
    },
  ],
};

export default testEinPageVersion;
