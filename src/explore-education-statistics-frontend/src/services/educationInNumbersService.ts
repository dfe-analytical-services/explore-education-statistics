import { ContentBlock } from '@common/services/types/blocks';

export interface EducationInNumbersPage {
  id: string;
  title: string;
  slug: string;
  description: string;
  published: string;
  content: {
    order: number;
    heading: string;
    caption: string;
    content: ContentBlock[];
  }[];
}

export interface EducationInNumbersNavItem {
  id: string;
  title: string;
  order: number;
  slug: string;
  published: string;
}

const educationInNumbersService = {
  getEducationInNumbersPage(slug: string): Promise<EducationInNumbersPage> {
    // return contentApi.get(`/education-in-numbers/${slug}`);
    return new Promise(resolve => {
      resolve({
        id: '1',
        title: 'Sample Education in Numbers Page',
        slug,
        description:
          'This is a sample description for the Education in Numbers page.',
        published: new Date().toISOString(),
        content: [
          {
            order: 1,
            heading: 'Sample Heading',
            caption: 'This is a sample caption.',
            content: [
              {
                id: 'block-1',
                order: 1,
                body: '<p>This is the first block of content in section 1.</p>',
                type: 'HtmlBlock',
              },
            ],
          },
          {
            order: 2,
            heading: 'Lorem Ipsum Dolor',
            caption: 'Sample caption for section 2.',
            content: [
              {
                id: 'block-2',
                order: 1,
                body: '<p>This is the first block of content in section 2.</p><h3>Subheading</h3><p>More content here.</p>',
                type: 'HtmlBlock',
              },
              {
                id: 'block-3',
                order: 2,
                body: '<p>This is the second block of content in section 2.</p>',
                type: 'HtmlBlock',
              },
            ],
          },
        ],
      });
    });
  },
  listEducationInNumbersPages(): Promise<EducationInNumbersNavItem[]> {
    // return contentApi.get('/education-in-numbers-nav');
    return new Promise(resolve => {
      resolve([
        {
          id: '1',
          slug: '',
          order: 1,
          title: 'Education in numbers',
          published: '2025-07-15T10:55:09.4902182+00:00',
        },
        {
          id: '2',
          order: 2,
          title: 'Key Statstics',
          slug: 'key-statstics',
          published: '2025-08-16T10:55:09.4902182+00:00',
        },
      ]);
    });
  },
};

export default educationInNumbersService;
