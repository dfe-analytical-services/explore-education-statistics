import { render, screen, within } from '@testing-library/react';
import React from 'react';
import ReleaseExploreDataPage from '../ReleaseExploreDataPage';
import {
  testPublicationSummary,
  testReleaseDataContent,
  testReleaseVersionSummary,
} from './__data__/testReleaseData';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

describe('ReleaseExploreDataPage', () => {
  test('renders correctly with all content sections', () => {
    render(
      <ReleaseExploreDataPage
        releaseVersionSummary={testReleaseVersionSummary}
        publicationSummary={testPublicationSummary}
        dataContent={testReleaseDataContent}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Explore data used in this release',
        level: 2,
      }),
    ).toBeInTheDocument();

    const linksList = screen.getByTestId('links-grid');
    const listItems = within(linksList).getAllByRole('listitem');
    expect(listItems).toHaveLength(7);
    expect(
      within(listItems[0]).getByRole('link', {
        name: 'Download all data from this release (ZIP)',
      }),
    ).toBeInTheDocument();
    expect(
      within(listItems[1]).getByRole('link', {
        name: 'Featured tables',
      }),
    ).toHaveAttribute('href', '#featured-tables-section');
    expect(
      within(listItems[1]).getByText(
        "Featured tables are pre-prepared tables created from a statistical release's data sets. They provide statistics that are regularly requested by users and can be adapted to switch between different categories.",
      ),
    ).toBeInTheDocument();
    expect(
      within(listItems[2]).getByRole('link', {
        name: 'Data sets: download or create tables',
      }),
    ).toHaveAttribute('href', '#datasets-section');
    expect(
      within(listItems[3]).getByRole('link', {
        name: 'Supporting files',
      }),
    ).toHaveAttribute('href', '#supporting-files-section');
    expect(
      within(listItems[4]).getByRole('link', {
        name: 'Data dashboards',
      }),
    ).toHaveAttribute('href', '#data-dashboards-section');
    expect(
      within(listItems[5]).getByRole('link', {
        name: 'Data guidance',
      }),
    ).toHaveAttribute('href', '#data-guidance-section');
    expect(
      within(listItems[6]).getByRole('link', {
        name: 'Data catalogue',
      }),
    ).toHaveAttribute(
      'href',
      '/data-catalogue?themeId=test-theme-id&publicationId=publication-summary-1&releaseVersionId=release-version-summary-1',
    );

    const featuredTablesSection = screen.getByTestId('featured-tables-section');
    expect(
      within(featuredTablesSection).getByRole('heading', {
        name: 'Featured tables',
        level: 2,
      }),
    ).toBeInTheDocument();
    expect(
      within(featuredTablesSection).getByText(
        "Featured tables are pre-prepared tables created from a statistical release's data sets. They provide statistics that are regularly requested by some users (such as local councils, regional government or government policy teams) and can be adapted to switch between different categories (such as different geographies, time periods or characteristics where available).",
      ),
    ).toBeInTheDocument();

    const datasetsSection = screen.getByTestId('datasets-section');
    expect(
      within(datasetsSection).getByRole('heading', {
        name: 'Data sets: download or create tables',
        level: 2,
      }),
    ).toBeInTheDocument();
    expect(
      within(datasetsSection).getByText(
        'Data sets present comprehensive open data from which users can create their own tables using the EES table tool or download a zipped CSV file.',
      ),
    ).toBeInTheDocument();

    const supportingFilesSection = screen.getByTestId(
      'supporting-files-section',
    );
    expect(
      within(supportingFilesSection).getByRole('heading', {
        name: 'Supporting files',
        level: 2,
      }),
    ).toBeInTheDocument();
    expect(
      within(supportingFilesSection).getByText(
        'Supporting files provide an area for teams to supply non-standard files for download by users where required.',
      ),
    ).toBeInTheDocument();

    const dataDashboardsSection = screen.getByTestId('data-dashboards-section');
    expect(
      within(dataDashboardsSection).getByRole('heading', {
        name: 'Data dashboards',
        level: 2,
      }),
    ).toBeInTheDocument();
    expect(
      within(dataDashboardsSection).getByText(
        "Data dashboards provide an alternative route to explore a statistical release's data, presenting key statistics and further insights, often via graphical visualisations.",
      ),
    ).toBeInTheDocument();

    const dataGuidanceSection = screen.getByTestId('data-guidance-section');
    expect(
      within(dataGuidanceSection).getByRole('heading', {
        name: 'Data guidance',
        level: 2,
      }),
    ).toBeInTheDocument();
    expect(
      within(dataGuidanceSection).getByText(
        'Description of the data included in this release, this is a methodology document, providing information on data sources, their coverage and quality and how the data is produced.',
      ),
    ).toBeInTheDocument();
  });

  test('does not render optional content sections or links when not present', () => {
    render(
      <ReleaseExploreDataPage
        releaseVersionSummary={testReleaseVersionSummary}
        publicationSummary={testPublicationSummary}
        dataContent={{
          ...testReleaseDataContent,
          supportingFiles: [],
          featuredTables: [],
          dataDashboards: undefined,
        }}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Explore data used in this release',
        level: 2,
      }),
    ).toBeInTheDocument();

    const linksList = screen.getByTestId('links-grid');
    const listItems = within(linksList).getAllByRole('listitem');
    expect(listItems).toHaveLength(4);
    expect(
      within(listItems[0]).getByRole('link', {
        name: 'Download all data from this release (ZIP)',
      }),
    ).toBeInTheDocument();
    expect(
      within(listItems[1]).getByRole('link', {
        name: 'Data sets: download or create tables',
      }),
    ).toHaveAttribute('href', '#datasets-section');
    expect(
      within(listItems[2]).getByRole('link', {
        name: 'Data guidance',
      }),
    ).toHaveAttribute('href', '#data-guidance-section');
    expect(
      within(listItems[3]).getByRole('link', {
        name: 'Data catalogue',
      }),
    ).toHaveAttribute(
      'href',
      '/data-catalogue?themeId=test-theme-id&publicationId=publication-summary-1&releaseVersionId=release-version-summary-1',
    );

    expect(
      screen.queryByTestId('featured-tables-section'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('supporting-files-section'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('data-dashboards-section'),
    ).not.toBeInTheDocument();
  });

  test('does not render links grid on mobile', () => {
    mockIsMedia = true;
    render(
      <ReleaseExploreDataPage
        releaseVersionSummary={testReleaseVersionSummary}
        publicationSummary={testPublicationSummary}
        dataContent={testReleaseDataContent}
      />,
    );

    expect(screen.queryByTestId('links-grid')).not.toBeInTheDocument();
    mockIsMedia = false;
  });
});
