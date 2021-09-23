import { Methodology } from '@common/services/methodologyService';
import { PublicationSummary } from '@common/services/publicationService';

export const testMethodology: Methodology = {
  id: 'methodology-1',
  title: 'Pupil absence statistics: methodology',
  published: '2021-02-16T15:32:01',
  slug: 'pupil-absence-in-schools-in-england',
  content: [
    {
      order: 0,
      heading: 'Section 1',
      caption: 'Section 1 caption',
      content: [
        {
          body: '<p>section 1 content</p>',
          id: 'section-1-content',
          order: 0,
          type: 'HtmlBlock',
        },
      ],
    },
    {
      order: 1,
      heading: 'Section 2',
      caption: 'Section 2 caption',
      content: [
        {
          body: '<p>section 2 content</p>',
          id: 'section-2-content',
          order: 0,
          type: 'HtmlBlock',
        },
      ],
    },
  ],
  annexes: [
    {
      order: 0,
      heading: 'Annex 1',
      caption: 'Annex 1 caption',
      content: [
        {
          body: '<p>annex 1 content</p>',
          id: 'annex-1-content',
          order: 0,
          type: 'HtmlBlock',
        },
      ],
    },
    {
      order: 1,
      heading: 'Annex 2',
      caption: 'Annex 2 caption',
      content: [
        {
          body: '<p>annex 2 content</p>',
          id: 'annex-2-content',
          order: 0,
          type: 'HtmlBlock',
        },
      ],
    },
    {
      order: 2,
      heading: 'Annex 3',
      caption: 'Annex 3 caption',
      content: [
        {
          body: '<p>annex 3 content</p>',
          id: 'annex-3-content',
          order: 0,
          type: 'HtmlBlock',
        },
      ],
    },
  ],
  notes: [
    {
      id: 'note-1',
      displayDate: new Date('2021-09-15T00:00:00'),
      content: 'Latest note',
    },
    {
      id: 'note-2',
      displayDate: new Date('2021-04-19T00:00:00'),
      content: 'Other note',
    },
    {
      id: 'note-3',
      displayDate: new Date('2021-03-01T00:00:00'),
      content: 'Earliest note',
    },
  ],
  publications: [
    {
      id: 'publication-1',
      title: 'Publication 1',
      slug: 'publication-1-slug',
    },
    {
      id: 'publication-2',
      title: 'Publication 2',
      slug: 'publication-2-slug',
    },
  ] as PublicationSummary[],
};

export default testMethodology;
