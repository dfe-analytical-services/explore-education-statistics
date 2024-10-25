import MethodologyIndexPage from '@frontend/modules/methodologies/MethodologyIndexPage';
import { MethodologyTheme } from '@common/services/themeService';
import { render, screen, within } from '@testing-library/react';
import React from 'react';

const testThemes: MethodologyTheme[] = [
  {
    id: 'theme-1',
    summary: 'Theme 1 summary',
    title: 'Theme 1',
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
  },
  {
    id: 'theme-2',
    summary: 'Theme 2 summary',
    title: 'Theme 2',
    publications: [
      {
        id: 'publication-4',
        methodologies: [],
        title: 'Publication 4',
      },
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
  },
  {
    id: 'theme-3',
    summary: 'Theme 3 summary',
    title: 'Theme 3',
    publications: [
      {
        id: 'publication-6',
        methodologies: [
          {
            id: 'methodology-6',
            slug: 'methodology-6-slug',
            title: 'Methodology 6',
          },
        ],
        title: 'Publication 6',
      },
      {
        id: 'publication-7',
        methodologies: [
          {
            id: 'methodology-6',
            slug: 'methodology-6-slug',
            title: 'Methodology 6',
          },
        ],
        title: 'Publication 7',
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
    expect(themes).toHaveLength(3);

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

    const theme3Methodologies = within(themes[2]).getAllByRole('listitem');
    expect(theme3Methodologies).toHaveLength(1); // Duplicates should be removed
    expect(
      within(theme3Methodologies[0]).getByRole('link', {
        name: 'Methodology 6',
      }),
    ).toHaveAttribute('href', '/methodology/methodology-6-slug');
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
