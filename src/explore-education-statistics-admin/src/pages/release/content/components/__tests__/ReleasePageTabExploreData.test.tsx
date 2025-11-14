import generateReleaseContent, {
  generateEditableRelease,
} from '@admin-test/generators/releaseContentGenerators';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import ReleasePageTabExploreData from '@admin/pages/release/content/components/ReleasePageTabExploreData';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseContent as ReleaseContentType } from '@admin/services/releaseContentService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { FeaturedTable } from '@admin/services/featuredTableService';
import { FileInfo } from '@common/services/types/file';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
  useDesktopMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

const testFeaturedTables: FeaturedTable[] = [
  {
    id: 'featured-table-1',
    dataBlockId: 'data-block-1',
    dataBlockParentId: 'data-block-parent-1',
    description: '',
    name: 'Featured table 1',
    order: 0,
  },
  {
    id: 'featured-table-2',
    dataBlockId: 'data-block-2',
    dataBlockParentId: 'data-block-parent-2',
    description: '',
    name: 'Featured table 2',
    order: 0,
  },
];

const testDownloadFiles: FileInfo[] = [
  {
    id: 'download-file-id-1',
    extension: 'csv',
    fileName: 'File name 1',
    name: 'Download file name 1',
    size: '100kb',
    summary: 'Download file summary 1',
    type: 'Data',
  },
  {
    id: 'download-file-id-2',
    extension: 'csv',
    fileName: 'File name 2',
    name: 'Download file name 2',
    size: '80kb',
    summary: 'Download file summary 2',
    type: 'Data',
  },
  {
    id: 'download-file-id-3',
    extension: 'pdf',
    fileName: 'File name 3',
    name: 'Download file name 3',
    size: '200kb',
    summary: 'Download file summary 3',
    type: 'Ancillary',
  },
];

const testReleaseContent = generateReleaseContent({});
const renderWithContext = (
  component: React.ReactNode,
  releaseContent: ReleaseContentType = testReleaseContent,
  featuredTables: FeaturedTable[] = testFeaturedTables,
) =>
  render(
    <TestConfigContextProvider>
      <ReleaseContentProvider
        value={{
          featuredTables,
          ...releaseContent,
          canUpdateRelease: true,
        }}
      >
        <EditingContextProvider editingMode="preview">
          <MemoryRouter>{component}</MemoryRouter>
        </EditingContextProvider>
        ,
      </ReleaseContentProvider>
      ,
    </TestConfigContextProvider>,
  );

describe('ReleasePageTabExploreData', () => {
  test('renders correctly with all content sections', () => {
    renderWithContext(
      <ReleasePageTabExploreData hidden={false} />,
      generateReleaseContent({
        release: generateEditableRelease({
          downloadFiles: testDownloadFiles,
          published: '2025-10-13T00:00:00.0000000Z',
          relatedDashboardsSection: {
            id: 'test-dashboards-section-id',
            heading: 'Data dashboards',
            order: 0,
            content: [
              {
                type: 'HtmlBlock',
                id: 'related-dashboard-block-1',
                comments: [],
                order: 0,
                body: '<p>Related dashboard content</p>',
              },
            ],
          },
        }),
      }),
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
      within(listItems[0]).getByRole('button', {
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
    ).toBeInTheDocument();

    const featuredTablesSection = screen.getByTestId('featured-tables-section');
    expect(
      within(featuredTablesSection).getByRole('heading', {
        name: 'Featured tables',
        level: 2,
      }),
    ).toBeInTheDocument();
    expect(
      within(featuredTablesSection).getByRole('heading', {
        name: '2 featured tables',
        level: 3,
      }),
    ).toBeInTheDocument();
    expect(
      within(featuredTablesSection).getByText(
        "Featured tables are pre-prepared tables created from a statistical release's data sets. They provide statistics that are regularly requested by some users (such as local councils, regional government or government policy teams) and can be adapted to switch between different categories (such as different geographies, time periods or characteristics where available).",
      ),
    ).toBeInTheDocument();
    expect(within(featuredTablesSection).getAllByRole('listitem')).toHaveLength(
      2,
    );

    const datasetsSection = screen.getByTestId('datasets-section');
    expect(
      within(datasetsSection).getByRole('heading', {
        name: 'Data sets: download or create tables',
        level: 2,
      }),
    ).toBeInTheDocument();
    expect(
      within(datasetsSection).getByRole('heading', {
        name: '2 data sets available for download',
        level: 3,
      }),
    ).toBeInTheDocument();
    expect(
      within(datasetsSection).getByRole('button', {
        name: 'Download all (ZIP)',
      }),
    ).toBeInTheDocument();
    expect(
      within(datasetsSection).getByText(
        'Data sets present comprehensive open data from which users can create their own tables using the EES table tool or download a zipped CSV file.',
      ),
    ).toBeInTheDocument();
    const dataSetItems = within(datasetsSection).getAllByTestId(
      'release-data-list-item',
    );
    expect(dataSetItems).toHaveLength(2);
    expect(
      within(dataSetItems[0]).getByRole('heading', {
        name: 'Download file name 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(dataSetItems[0]).getByText(/Download file summary 1/),
    ).toBeInTheDocument();

    expect(
      within(dataSetItems[1]).getByRole('button', { name: /Download/ }),
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
      within(supportingFilesSection).getByRole('heading', {
        name: '1 supporting data file',
        level: 3,
      }),
    ).toBeInTheDocument();
    expect(
      within(supportingFilesSection).getByText(
        'Supporting files provide an area for teams to supply non-standard files for download by users where required.',
      ),
    ).toBeInTheDocument();
    expect(
      within(supportingFilesSection).getAllByRole('listitem'),
    ).toHaveLength(1);

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
    renderWithContext(
      <ReleasePageTabExploreData hidden={false} />,
      generateReleaseContent({
        release: generateEditableRelease({
          downloadFiles: [testDownloadFiles[0]],
          published: '2025-10-13T00:00:00.0000000Z',
        }),
      }),
      [],
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
      within(listItems[0]).getByRole('button', {
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
    ).toBeInTheDocument();

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
    renderWithContext(<ReleasePageTabExploreData hidden={false} />);

    expect(screen.queryByTestId('links-grid')).not.toBeInTheDocument();
    mockIsMedia = false;
  });

  test('does not render accordion sections on desktop', () => {
    renderWithContext(<ReleasePageTabExploreData hidden={false} />);
    expect(screen.queryByTestId('accordion')).not.toBeInTheDocument();
  });

  test('renders some content sections as accordions on mobile', () => {
    mockIsMedia = true;
    renderWithContext(<ReleasePageTabExploreData hidden={false} />);
    expect(screen.getByTestId('accordion')).toBeInTheDocument();
    expect(screen.getAllByTestId('accordionSection')).toHaveLength(3);
    mockIsMedia = false;
  });
});
