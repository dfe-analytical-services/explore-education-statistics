import generateReleaseContent from '@admin-test/generators/releaseContentGenerators';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import ReleasePageTabExploreData from '@admin/pages/release/content/components/ReleasePageTabExploreData';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import _releaseContentService, {
  DataContent,
  ReleaseContent as ReleaseContentType,
} from '@admin/services/releaseContentService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { noop } from 'lodash';

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

jest.mock('@admin/services/releaseContentService');

const releaseContentService = _releaseContentService as jest.Mocked<
  typeof _releaseContentService
>;

const testReleaseContent = generateReleaseContent({});
const renderWithContext = (
  component: React.ReactNode,
  releaseContent: ReleaseContentType = testReleaseContent,
) =>
  render(
    <TestConfigContextProvider>
      <ReleaseContentProvider
        value={{
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
  const testReleaseDataContent: DataContent = {
    releaseId: 'test-release-id',
    releaseVersionId: 'test-release-version-id',
    dataDashboards: '<h3>Data dashboard text</h3>',
    dataGuidance:
      '<h3>Description</h3><p>---</p><h3>Coverage</h3><p>---</p><h3>File formats and conventions</h3><p>---</p>',
    dataSets: [
      {
        dataSetFileId: 'test-dataset-1-datasetfileid',
        fileId: 'test-dataset-1-fileid',
        subjectId: 'test-dataset-1-subjectid',
        meta: {
          filters: ['Characteristic', 'School type'],
          geographicLevels: [
            'Local authority',
            'Local authority district',
            'National',
          ],
          indicators: [
            'Authorised absence rate',
            'Authorised absence rate exact',
          ],
          numDataFileRows: 1000,
          timePeriodRange: {
            start: '2012/13',
            end: '2016/17',
          },
        },
        title: 'Test dataset 1',
        summary: 'Test dataset 1 summary',
      },
      {
        dataSetFileId: 'test-dataset-2-datasetfileid',
        fileId: 'test-dataset-2-fileid',
        subjectId: 'test-dataset-2-subjectid',
        meta: {
          filters: ['Characteristic', 'School type'],
          geographicLevels: ['Local authority', 'National', 'Regional'],
          indicators: [
            'Authorised absence rate',
            'Number of authorised absence sessions',
          ],
          numDataFileRows: 2000,
          timePeriodRange: {
            start: '2013/14',
            end: '2016/17',
          },
        },
        title: 'Test dataset 2',
        summary: 'Test dataset 2 summary',
      },
    ],
    featuredTables: [
      {
        featuredTableId: 'featured-table-1-id',
        dataBlockId: 'featured-table-1-data-block-id',
        dataBlockParentId: 'featured-table-1-data-block-parent-id',
        title: 'Featured table 1',
        summary: 'Featured table 1 description',
      },
      {
        featuredTableId: 'featured-table-2-id',
        dataBlockId: 'featured-table-2-data-block-id',
        dataBlockParentId: 'featured-table-2-data-block-parent-id',
        title: 'Featured table 2',
        summary: 'Featured table 2 description',
      },
    ],
    supportingFiles: [
      {
        fileId: 'supporting-file-1-file-id',
        extension: 'pdf',
        filename: 'file1.pdf',
        title: 'Supporting file 1',
        summary: 'Supporting file 1 description',
        size: '10 Mb',
      },
    ],
  };

  beforeEach(() => {});

  test('renders correctly with all content sections', async () => {
    releaseContentService.getDataContent.mockResolvedValue(
      testReleaseDataContent,
    );

    renderWithContext(<ReleasePageTabExploreData hidden={false} />);

    await waitFor(() => {
      expect(
        screen.getByRole('heading', {
          name: 'Explore data used in this release',
          level: 2,
        }),
      ).toBeInTheDocument();
    });

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
      within(listItems[6]).getByText(
        'Data catalogue (available when release is published)',
      ),
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
    const featuredTableItems = within(featuredTablesSection).getAllByRole(
      'listitem',
    );
    expect(featuredTableItems).toHaveLength(2);
    expect(within(featuredTableItems[0]).getByRole('link')).toBeInTheDocument();

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
        name: 'Test dataset 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(dataSetItems[0]).getByRole('button', {
        name: /Download Test dataset 1/,
      }),
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

  test('featured table links go to pre-release table tool if isPra', async () => {
    releaseContentService.getDataContent.mockResolvedValue(
      testReleaseDataContent,
    );
    renderWithContext(<ReleasePageTabExploreData hidden={false} isPra />);

    await waitFor(() => {
      expect(
        screen.getByRole('heading', {
          name: 'Explore data used in this release',
          level: 2,
        }),
      ).toBeInTheDocument();
    });

    const featuredTablesSection = screen.getByTestId('featured-tables-section');
    const featuredTableItems = within(featuredTablesSection).getAllByRole(
      'listitem',
    );
    expect(featuredTableItems).toHaveLength(2);
    expect(within(featuredTableItems[0]).getByRole('link')).toHaveAttribute(
      'href',
      '/publication/publication-id/release/Release-title-id/prerelease/table-tool/featured-table-1-data-block-id',
    );
  });

  test('featured table items have buttons instead of links in preview mode (unpublished)', async () => {
    releaseContentService.getDataContent.mockResolvedValue(
      testReleaseDataContent,
    );
    renderWithContext(
      <ReleasePageTabExploreData
        hidden={false}
        isPra
        handleFeaturedTableItemClick={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('heading', {
          name: 'Explore data used in this release',
          level: 2,
        }),
      ).toBeInTheDocument();
    });

    const featuredTablesSection = screen.getByTestId('featured-tables-section');
    const featuredTableItems = within(featuredTablesSection).getAllByRole(
      'listitem',
    );
    expect(featuredTableItems).toHaveLength(2);
    expect(
      within(featuredTableItems[0]).queryByRole('link'),
    ).not.toBeInTheDocument();
    expect(
      within(featuredTableItems[0]).getByRole('button'),
    ).toBeInTheDocument();
  });

  test('does not render optional content sections or links when some data not present', async () => {
    releaseContentService.getDataContent.mockResolvedValue({
      ...testReleaseDataContent,
      featuredTables: [],
      supportingFiles: [],
      dataDashboards: undefined,
    });

    renderWithContext(<ReleasePageTabExploreData hidden={false} />);

    await waitFor(() => {
      expect(
        screen.getByRole('heading', {
          name: 'Explore data used in this release',
          level: 2,
        }),
      ).toBeInTheDocument();
    });

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
      within(listItems[3]).getByText(
        'Data catalogue (available when release is published)',
      ),
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

  test('does not render links grid on mobile', async () => {
    releaseContentService.getDataContent.mockResolvedValue(
      testReleaseDataContent,
    );
    mockIsMedia = true;
    renderWithContext(<ReleasePageTabExploreData hidden={false} />);
    await waitFor(() => {
      expect(
        screen.getByRole('heading', {
          name: 'Explore data used in this release',
          level: 2,
        }),
      ).toBeInTheDocument();
    });

    expect(screen.queryByTestId('links-grid')).not.toBeInTheDocument();
    mockIsMedia = false;
  });

  test('does not render accordion sections on desktop', async () => {
    releaseContentService.getDataContent.mockResolvedValue(
      testReleaseDataContent,
    );
    renderWithContext(<ReleasePageTabExploreData hidden={false} />);
    await waitFor(() => {
      expect(
        screen.getByRole('heading', {
          name: 'Explore data used in this release',
          level: 2,
        }),
      ).toBeInTheDocument();
    });

    expect(screen.queryByTestId('accordion')).not.toBeInTheDocument();
  });

  test('renders some content sections as accordions on mobile', async () => {
    releaseContentService.getDataContent.mockResolvedValue(
      testReleaseDataContent,
    );
    mockIsMedia = true;
    renderWithContext(<ReleasePageTabExploreData hidden={false} />);
    await waitFor(() => {
      expect(
        screen.getByRole('heading', {
          name: 'Explore data used in this release',
          level: 2,
        }),
      ).toBeInTheDocument();
    });

    expect(screen.getByTestId('accordion')).toBeInTheDocument();
    expect(screen.getAllByTestId('accordionSection')).toHaveLength(5);
    mockIsMedia = false;
  });
});
