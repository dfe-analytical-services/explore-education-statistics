import { EinContent } from '@admin/services/educationInNumbersContentService';
import { EducationInNumbersSummaryWithPrevVersion } from '@admin/services/educationInNumbersService';

const testEinPageVersion: EducationInNumbersSummaryWithPrevVersion = {
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
      caption: '',
      heading: 'New section 3',
      content: [
        {
          id: 'content-section-0-content-0',
          body: '',
          type: 'HtmlBlock',
          order: 0,
          comments: [],
        },
        {
          id: 'content-section-0-content-1',
          body: 'Part 2',
          type: 'HtmlBlock',
          order: 0,
          comments: [],
        },
      ],
    },
    {
      id: 'content-section-1',
      order: 1,
      heading: 'New section',
      content: [],
      caption: '',
    },
  ],
};

export default testEinPageVersion;
