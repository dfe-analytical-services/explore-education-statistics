import { Methodology } from '@common/services/methodologyService';

export const testMethodology: Methodology = {
  id: 'methodology-1',
  title: 'Pupil absence statistics: methodology',
  published: '2021-02-16T15:32:01',
  slug: 'pupil-absence-in-schools-in-england',
  content: [
    {
      order: 0,
      heading: 'Section 1',
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
      latestReleaseSlug: 'latest-release-slug-1',
      owner: false,
      contact: {
        teamName: 'Mock Team Name',
        teamEmail: 'mockteammember@gmail.com',
        contactName: 'Mock Team Member',
        contactTelNo: '0161 234 5678',
      },
    },
    {
      id: 'publication-2',
      title: 'Publication 2',
      slug: 'publication-2-slug',
      latestReleaseSlug: 'latest-release-slug-2',
      owner: true,
      contact: {
        teamName: 'Mock Team Name',
        teamEmail: 'mockteammember@gmail.com',
        contactName: 'Mock Team Member',
        contactTelNo: '0161 234 5678',
      },
    },
  ],
};

export default testMethodology;
