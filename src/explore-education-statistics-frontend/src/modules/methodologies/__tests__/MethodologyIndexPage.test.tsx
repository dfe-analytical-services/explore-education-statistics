import MethodologyIndexPage from '@frontend/modules/methodologies/MethodologyIndexPage';
import { MethodologyTheme } from '@common/services/themeService';
import { render, screen, within } from '@testing-library/react';
import React from 'react';

const testThemes: MethodologyTheme[] = [
  {
    id: 'theme-1',
    summary: 'Theme 1 summary',
    title: 'Theme 1',
    topics: [
      {
        id: 'topic-1',
        publications: [
          {
            id: 'publication-1',
            methodologies: [
              {
                id: 'methodology-1',
                slug: 'methodology-1-slug',
                title: 'Methodology 1',
              },
            ],
            title: 'Publication 1',
          },
          {
            id: 'publication-2',
            methodologies: [
              {
                id: 'methodology-2',
                slug: 'methodology-2-slug',
                title: 'Methodology 2',
              },
              {
                id: 'methodology-3',
                slug: 'methodology-3-slug',
                title: 'Methodology 3',
              },
            ],
            title: 'Publication 2',
          },
        ],
        title: 'Topic 1',
      },
      {
        id: 'topic-2',
        publications: [
          {
            id: 'publication-3',
            methodologies: [
              {
                id: 'methodology-4',
                slug: 'methodology-4-slug',
                title: 'Methodology 4',
              },
            ],
            title: 'Publication 3',
          },
        ],
        title: 'Topic 2',
      },
    ],
  },
  {
    id: 'theme-2',
    summary: 'Theme 2 summary',
    title: 'Theme 2',
    topics: [
      {
        id: 'topic-3',
        publications: [],
        title: 'Topic 3',
      },
      {
        id: 'topic-4',
        publications: [
          {
            id: 'publication-4',
            methodologies: [],
            title: 'Publication 4',
          },
        ],
        title: 'Topic 4',
      },
      {
        id: 'topic-5',
        publications: [
          {
            id: 'publication-5',
            methodologies: [
              {
                id: 'methodology-5',
                slug: 'methodology-5-slug',
                title: 'Methodology 5',
              },
            ],
            title: 'Publication 5',
          },
        ],
        title: 'Topic 5',
      },
    ],
  },
];

describe('MethodologyIndexPage', () => {
  test('renders methodology index page with themes', async () => {
    render(<MethodologyIndexPage themes={testThemes} />);

    expect(screen.getByTestId('page-title')).toHaveTextContent('Methodologies');

    expect(
      screen.getByLabelText(
        'Search to find the methodology behind specific education statistics and data.',
      ),
    ).toBeInTheDocument();

    const themes = screen.getAllByTestId('accordionSection');
    expect(themes).toHaveLength(2);

    const theme1Methodologies = within(themes[0]).getAllByRole('listitem');

    expect(theme1Methodologies).toHaveLength(4);
    expect(
      within(theme1Methodologies[0]).getByRole('link', {
        name: 'Methodology 1',
      }),
    ).toHaveAttribute('href', '/methodology/methodology-1-slug');
    expect(
      within(theme1Methodologies[1]).getByRole('link', {
        name: 'Methodology 2',
      }),
    ).toHaveAttribute('href', '/methodology/methodology-2-slug');
    expect(
      within(theme1Methodologies[2]).getByRole('link', {
        name: 'Methodology 3',
      }),
    ).toHaveAttribute('href', '/methodology/methodology-3-slug');
    expect(
      within(theme1Methodologies[3]).getByRole('link', {
        name: 'Methodology 4',
      }),
    ).toHaveAttribute('href', '/methodology/methodology-4-slug');

    const theme2Methodologies = within(themes[1]).getAllByRole('listitem');
    expect(theme2Methodologies).toHaveLength(1);
    expect(
      within(theme2Methodologies[0]).getByRole('link', {
        name: 'Methodology 5',
      }),
    ).toHaveAttribute('href', '/methodology/methodology-5-slug');
  });

  test('renders methodology index page without themes', async () => {
    render(<MethodologyIndexPage themes={[]} />);

    expect(
      screen.queryByLabelText(
        'Search to find the methodology behind specific education statistics and data.',
      ),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('No data currently published.'),
    ).toBeInTheDocument();
  });
});
