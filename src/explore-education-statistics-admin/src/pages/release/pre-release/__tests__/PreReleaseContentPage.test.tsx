import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import PreReleaseContentPage from '@admin/pages/release/pre-release/PreReleaseContentPage';
import { preReleaseContentRoute } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import _releaseContentService, {
  EditableRelease,
  ReleaseContent,
} from '@admin/services/releaseContentService';
import _featuredTableService, {
  FeaturedTable,
} from '@admin/services/featuredTableService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter, Route } from 'react-router';
import { generatePath } from 'react-router-dom';

jest.mock('@admin/services/featuredTableService');
jest.mock('@admin/services/releaseContentService');
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: false,
    };
  },
  useDesktopMedia: () => {
    return {
      isMedia: false,
    };
  },
}));

const releaseContentService = _releaseContentService as jest.Mocked<
  typeof _releaseContentService
>;
const featuredTableService = _featuredTableService as jest.Mocked<
  typeof _featuredTableService
>;

describe('PreReleaseContentPage', () => {
  const testEditableRelease: EditableRelease = {
    approvalStatus: 'Approved',
    content: [
      {
        id: 'section-1',
        order: 0,
        heading: 'Section 1',
        content: [
          {
            id: 'block-1',
            order: 1,
            body: '<p>Section 1 content</p><p><a href="/data-table/fast-track/data-block-parent-1?featuredTable=true" data-featured-table>featured table link</a></p>',
            type: 'HtmlBlock',
            comments: [],
          },
        ],
      },
      {
        id: 'section-2',
        order: 0,
        heading: 'Section 2',
        content: [
          {
            id: 'block-2',
            order: 0,
            body: '<p>Section 2 content</p>',
            type: 'HtmlBlock',
            comments: [],
          },
        ],
      },
    ],
    coverageTitle: 'Academic year',
    downloadFiles: [],
    hasDataGuidance: true,
    hasPreReleaseAccessList: true,
    headlinesSection: {
      id: 'headlines-id',
      heading: '',
      order: 0,
      content: [
        {
          id: 'headllines-id',
          order: 0,
          body: '<p>Headlines content</p>',
          type: 'HtmlBlock',
          comments: [],
        },
      ],
    },

    id: 'release-id',
    keyStatistics: [],
    keyStatisticsSecondarySection: {
      id: '',
      order: 0,
      heading: '',
      content: [],
    },
    latestRelease: false,
    publicationId: 'publication-id',
    publication: {
      id: 'publication-id',
      title: 'Publication 1',
      slug: 'publication-1',
      releaseSeries: [],
      theme: { id: 'theme-1', title: 'Theme 1' },
      contact: {
        contactName: 'John Smith',
        contactTelNo: '0777777777',
        teamEmail: 'john.smith@test.com',
        teamName: 'Team Smith',
      },
      methodologies: [],
    },
    publishScheduled: '2020-03-03',
    relatedInformation: [],
    slug: '2020-21',
    summarySection: {
      id: 'summary-id',
      order: 0,
      content: [],
      heading: '',
    },
    title: 'Academic year 2020/21',
    type: 'OfficialStatistics',
    updates: [],
    yearTitle: '2020/21',
  };

  const testReleaseContent: ReleaseContent = {
    release: testEditableRelease,
    unattachedDataBlocks: [],
  };

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

  test('renders the summary', async () => {
    releaseContentService.getContent.mockResolvedValue(testReleaseContent);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Publication 1' }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('Published-value')).toHaveTextContent(
      '3 March 2020',
    );

    expect(screen.getByTestId('Release type-value')).toHaveTextContent(
      'Official statistics',
    );
  });

  test('renders the content', async () => {
    releaseContentService.getContent.mockResolvedValue(testReleaseContent);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', {
        name: 'Headline facts and figures - 2020/21',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('Headlines content')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Explore data and files used in this release',
      }),
    ).toBeInTheDocument();

    const contentAccordion = screen.getAllByTestId('accordion')[0];
    const contentAccordionSections =
      within(contentAccordion).getAllByTestId('accordionSection');

    expect(contentAccordionSections).toHaveLength(2);

    expect(
      await within(contentAccordionSections[0]).findByRole('button', {
        name: /Section 1/,
      }),
    ).toBeInTheDocument();

    expect(
      within(contentAccordionSections[0]).getByText('Section 1 content'),
    ).toBeInTheDocument();

    expect(
      within(contentAccordionSections[1]).getByRole('button', {
        name: /Section 2/,
      }),
    ).toBeInTheDocument();
    expect(
      within(contentAccordionSections[1]).getByText('Section 2 content'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Help and support',
      }),
    ).toBeInTheDocument();
  });

  test('transforms featured table links', async () => {
    releaseContentService.getContent.mockResolvedValue(testReleaseContent);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
    });

    expect(
      await screen.findByRole('button', {
        name: /Section 1/,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'featured table link' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/prerelease/table-tool/data-block-1',
    );
  });

  test('renders the redesigned content', async () => {
    releaseContentService.getContent.mockResolvedValue(testReleaseContent);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    renderPage();

    window.history.pushState({}, '', '?redesign=true');

    await waitFor(() => {
      expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
    });

    expect(screen.getByTestId('release-page-tabs')).toBeInTheDocument();

    const headlinesSection = screen.getByTestId('headlines-section');
    expect(headlinesSection).toBeInTheDocument();

    expect(
      within(headlinesSection).getByRole('heading', {
        level: 2,
        name: 'Headline facts and figures',
      }),
    ).toBeInTheDocument();

    const content = screen.getByTestId('home-content');
    expect(content).toBeInTheDocument();

    const sections = within(content).getAllByTestId('home-content-section');
    expect(sections).toHaveLength(2);
    expect(sections[0]).toHaveAttribute('id', 'section-section-1');
    expect(
      within(sections[0]).getByRole('heading', { level: 2 }),
    ).toHaveTextContent('Section 1');
  });

  const renderPage = (
    initialEntries: string[] = [
      generatePath<ReleaseRouteParams>(preReleaseContentRoute.path, {
        publicationId: 'publication-1',
        releaseVersionId: 'release-1',
      }),
    ],
  ) => {
    return render(
      <MemoryRouter initialEntries={initialEntries}>
        <TestConfigContextProvider>
          <Route
            component={PreReleaseContentPage}
            path={preReleaseContentRoute.path}
          />
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  };
});
