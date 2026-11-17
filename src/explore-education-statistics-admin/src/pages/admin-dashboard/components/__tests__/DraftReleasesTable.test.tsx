import DraftReleasesTable from '@admin/pages/admin-dashboard/components/DraftReleasesTable';
import DraftReleasesTab from '@admin/pages/admin-dashboard/components/DraftReleasesTab';
import _methodologyService, {
  MethodologyVersionSummary,
} from '@admin/services/methodologyService';
import _releaseVersionService, {
  DashboardReleaseVersionSummary,
} from '@admin/services/releaseVersionService';
import render from '@common-test/render';
import { act, screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React, { useState } from 'react';
import { MemoryRouter } from 'react-router';

jest.mock('@admin/services/methodologyService');
jest.mock('@admin/services/releaseVersionService');

const methodologyService = jest.mocked(_methodologyService);
const releaseVersionService = jest.mocked(_releaseVersionService);

const createRelease = (
  id: string,
  publicationId: string,
  publicationTitle: string,
  year: number,
  overrides: Partial<DashboardReleaseVersionSummary> = {},
): DashboardReleaseVersionSummary => ({
  id,
  releaseId: id,
  slug: `${id}-slug`,
  title: `Release ${id}`,
  year,
  yearTitle: `${year}/${year + 1}`,
  publication: {
    id: publicationId,
    title: publicationTitle,
    slug: `${publicationId}-slug`,
    latestReleaseSlug: `${id}-slug`,
    owner: true,
    contact: {
      teamName: 'Test contact team',
      teamEmail: 'test@example.com',
      contactName: 'Test contact',
    },
  },
  nextReleaseDate: {
    year: '2200',
    month: '1',
    day: '',
  },
  live: false,
  timePeriodCoverage: {
    value: 'AY',
    label: 'Academic year',
  },
  type: 'AccreditedOfficialStatistics',
  approvalStatus: 'Draft',
  amendment: false,
  permissions: {
    canViewReleaseVersion: true,
    canUpdateRelease: true,
    canUpdateReleaseVersion: true,
    canDeleteReleaseVersion: false,
    canMakeAmendmentOfReleaseVersion: false,
  },
  ...overrides,
});

const testReleases: DashboardReleaseVersionSummary[] = [
  createRelease('release-1', 'publication-1', 'Publication 1', 2020),
  createRelease('release-2', 'publication-2', 'Publication 2', 2021, {
    amendment: true,
    previousVersionId: 'previous-version-id',
    permissions: {
      canViewReleaseVersion: true,
      canUpdateRelease: true,
      canUpdateReleaseVersion: true,
      canDeleteReleaseVersion: true,
      canMakeAmendmentOfReleaseVersion: false,
    },
  }),
  createRelease('release-3', 'publication-1', 'Publication 1', 2022, {
    approvalStatus: 'HigherLevelReview',
  }),
  createRelease('release-4', 'publication-3', 'Publication 3', 2023, {
    approvalStatus: 'HigherLevelReview',
    amendment: true,
    previousVersionId: 'previous-version-id',
    permissions: {
      canViewReleaseVersion: true,
      canUpdateRelease: true,
      canUpdateReleaseVersion: true,
      canDeleteReleaseVersion: true,
      canMakeAmendmentOfReleaseVersion: false,
    },
  }),
];

const testMethodology: MethodologyVersionSummary = {
  id: 'methodology-version-1',
  methodologyId: 'methodology-1',
  title: 'Test methodology',
  status: 'Approved',
  amendment: false,
  owned: true,
  permissions: {
    canDeleteMethodology: false,
    canUpdateMethodology: false,
    canApproveMethodology: false,
    canSubmitMethodologyForHigherReview: false,
    canMarkMethodologyAsDraft: false,
    canMakeAmendmentOfMethodology: false,
    canRemoveMethodologyLink: false,
  },
};

interface TableProps {
  releases?: DashboardReleaseVersionSummary[];
  showIssues?: boolean;
  showMethodology?: boolean;
  onChangeRelease?: () => void;
}

const renderTable = ({
  releases = testReleases,
  showIssues = true,
  showMethodology = true,
  onChangeRelease = noop,
}: TableProps = {}) =>
  render(
    <MemoryRouter>
      <DraftReleasesTable
        showIssues={showIssues}
        showMethodology={showMethodology}
        releases={releases}
        onChangeRelease={onChangeRelease}
      />
    </MemoryRouter>,
  );

const getReleaseRowHeaders = () =>
  screen
    .getAllByRole('rowheader')
    .filter(rowHeader => rowHeader.getAttribute('scope') === 'row');

describe('DraftReleasesTable', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    releaseVersionService.getReleaseVersionChecklist.mockResolvedValue({
      errors: [{ code: 'DataFileImportsMustBeCompleted' }],
      valid: false,
      warnings: [{ code: 'NoMethodology' }],
    });
    methodologyService.listLatestMethodologyVersions.mockImplementation(
      async publicationId => [
        {
          ...testMethodology,
          id: `methodology-${publicationId}`,
        },
      ],
    );
  });

  test('renders publication groups, release statuses, issues and methodology actions', async () => {
    renderTable();

    const methodologyLinks = await screen.findAllByRole('link', {
      name: 'View methodology for Publication 1',
    });
    expect(methodologyLinks).toHaveLength(2);
    expect(methodologyLinks[0]).toHaveAttribute(
      'href',
      '/methodology/methodology-publication-1/summary',
    );

    const table = screen.getByRole('table', {
      name: 'Draft releases grouped by publication',
    });
    expect(
      within(table)
        .getAllByRole('columnheader')
        .map(header => header.textContent),
    ).toEqual([
      'Publication title',
      'Release',
      'Draft status',
      'Issues',
      'Methodology status',
      'Actions',
    ]);

    expect(
      within(table).getByRole('link', { name: 'Publication 1' }),
    ).toHaveAttribute('href', '/publication/publication-1/releases');
    expect(
      within(table).getByRole('link', { name: 'Publication 1' }).closest('th'),
    ).toHaveAttribute('rowspan', '2');
    expect(
      within(table).getByRole('link', { name: 'Edit draft Release release-1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );
    expect(
      within(table).getByRole('link', {
        name: 'View issues (2) for Release release-1',
      }),
    ).toBeInTheDocument();
    expect(within(table).getAllByText('Approved')).toHaveLength(4);
    expect(within(table).getByText('Draft Amendment')).toBeInTheDocument();
    expect(within(table).getByText('In Review Amendment')).toBeInTheDocument();
    expect(
      within(table).queryByRole('link', { name: /View existing version/ }),
    ).not.toBeInTheDocument();
    expect(
      methodologyService.listLatestMethodologyVersions,
    ).toHaveBeenCalledTimes(3);
  });

  test('applies the BAU visibility policy in the draft releases tab', async () => {
    render(
      <MemoryRouter>
        <DraftReleasesTab
          isBauUser
          isLoading={false}
          releases={testReleases}
          onChangeRelease={noop}
        />
      </MemoryRouter>,
    );

    expect(
      screen.getAllByRole('columnheader').map(header => header.textContent),
    ).toEqual(['Publication title', 'Release', 'Draft status', 'Actions']);
    expect(screen.getByRole('link', { name: 'Publication 1' })).toHaveAttribute(
      'href',
      '/publication/publication-1/releases',
    );
    expect(
      screen.queryByText(/summary of any outstanding issues/),
    ).not.toBeInTheDocument();

    await Promise.resolve();
    expect(
      releaseVersionService.getReleaseVersionChecklist,
    ).not.toHaveBeenCalled();
    expect(
      methodologyService.listLatestMethodologyVersions,
    ).not.toHaveBeenCalled();
  });

  test('renders only the initial BAU columns and does not request details', async () => {
    renderTable({ showIssues: false, showMethodology: false });

    const table = screen.getByRole('table', {
      name: 'Draft releases grouped by publication',
    });
    expect(
      within(table)
        .getAllByRole('columnheader')
        .map(header => header.textContent),
    ).toEqual(['Publication title', 'Release', 'Draft status', 'Actions']);
    expect(
      within(table).getByRole('link', { name: 'Edit draft Release release-1' }),
    ).toBeInTheDocument();
    expect(
      within(table).queryByRole('link', { name: /View methodology/ }),
    ).not.toBeInTheDocument();

    await Promise.resolve();
    expect(
      releaseVersionService.getReleaseVersionChecklist,
    ).not.toHaveBeenCalled();
    expect(
      methodologyService.listLatestMethodologyVersions,
    ).not.toHaveBeenCalled();
  });

  test('keeps publications with identical titles in separate groups', () => {
    renderTable({
      releases: [
        createRelease('release-1', 'publication-1', 'Same title', 2020),
        createRelease('release-2', 'publication-2', 'Same title', 2021),
      ],
      showIssues: false,
      showMethodology: false,
    });

    const publicationLinks = screen.getAllByRole('link', {
      name: 'Same title',
    });
    expect(publicationLinks).toHaveLength(2);
    expect(publicationLinks[0]).toHaveAttribute(
      'href',
      '/publication/publication-1/releases',
    );
    expect(publicationLinks[1]).toHaveAttribute(
      'href',
      '/publication/publication-2/releases',
    );
  });

  test('sorts publications alphabetically and releases by year descending', () => {
    renderTable({
      releases: [
        createRelease('release-z', 'publication-z', 'Z publication', 2022),
        createRelease('release-a-old', 'publication-a', 'A publication', 2020),
        createRelease('release-a-new', 'publication-a', 'A publication', 2023),
      ],
      showIssues: false,
      showMethodology: false,
    });

    const rows = screen.getAllByRole('row');
    expect(
      within(rows[1]).getByRole('rowheader', { name: 'Release release-a-new' }),
    ).toHaveTextContent('Release release-a-new');
    expect(
      within(rows[2]).getByRole('rowheader', { name: 'Release release-a-old' }),
    ).toHaveTextContent('Release release-a-old');
    expect(
      within(rows[3]).getByRole('rowheader', { name: 'Release release-z' }),
    ).toHaveTextContent('Release release-z');
  });

  test('uses the release permissions for edit and amendment cancellation actions', () => {
    renderTable({
      releases: [
        createRelease(
          'release-readonly',
          'publication-readonly',
          'Read-only publication',
          2020,
          {
            permissions: {
              canViewReleaseVersion: true,
              canUpdateRelease: false,
              canUpdateReleaseVersion: false,
              canDeleteReleaseVersion: false,
              canMakeAmendmentOfReleaseVersion: false,
            },
          },
        ),
        createRelease(
          'release-no-cancel',
          'publication-no-cancel',
          'No-cancel publication',
          2021,
          {
            amendment: true,
            permissions: {
              canViewReleaseVersion: true,
              canUpdateRelease: true,
              canUpdateReleaseVersion: true,
              canDeleteReleaseVersion: false,
              canMakeAmendmentOfReleaseVersion: false,
            },
          },
        ),
      ],
      showIssues: false,
      showMethodology: false,
    });

    expect(
      screen.getByRole('link', { name: 'View draft Release release-readonly' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-readonly/release/release-readonly/summary',
    );
    expect(
      screen.getByRole('link', {
        name: 'Edit draft Release release-no-cancel',
      }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Cancel amendment for Release release-no-cancel',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders the owned methodology instead of adopted methodology entries', async () => {
    methodologyService.listLatestMethodologyVersions.mockResolvedValue([
      {
        ...testMethodology,
        id: 'adopted-methodology',
        owned: false,
      },
      {
        ...testMethodology,
        id: 'owned-methodology-version',
        status: 'HigherLevelReview',
      },
    ]);

    renderTable({
      releases: [testReleases[0]],
      showIssues: false,
    });

    expect(await screen.findByText('In Review')).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'View methodology for Publication 1' }),
    ).toHaveAttribute('href', '/methodology/owned-methodology-version/summary');
    expect(
      screen.queryByRole('link', { name: /adopted-methodology/ }),
    ).not.toBeInTheDocument();
  });

  test('shows explicit absence when there is no owned methodology', async () => {
    methodologyService.listLatestMethodologyVersions.mockResolvedValue([
      {
        ...testMethodology,
        owned: false,
      },
    ]);

    renderTable({
      releases: [testReleases[0]],
      showIssues: false,
    });

    expect(await screen.findByText('No owned methodology')).toBeInTheDocument();
    expect(
      screen.queryByRole('link', { name: /View methodology/ }),
    ).not.toBeInTheDocument();
  });

  test('renders draft, in-review and approved methodology statuses', async () => {
    const statusByPublication: Record<
      string,
      MethodologyVersionSummary['status']
    > = {
      'publication-1': 'Draft',
      'publication-2': 'HigherLevelReview',
      'publication-3': 'Approved',
    };
    methodologyService.listLatestMethodologyVersions.mockImplementation(
      async publicationId => [
        {
          ...testMethodology,
          id: `methodology-${publicationId}`,
          status: statusByPublication[publicationId],
        },
      ],
    );

    renderTable({
      releases: [testReleases[0], testReleases[1], testReleases[3]],
      showIssues: false,
    });

    expect(await screen.findAllByText('Approved')).toHaveLength(1);
    const releaseRows = screen.getAllByRole('row').slice(1);
    expect(within(releaseRows[0]).getAllByRole('cell')[1]).toHaveTextContent(
      'Draft',
    );
    expect(within(releaseRows[1]).getAllByRole('cell')[1]).toHaveTextContent(
      'In Review',
    );
    expect(within(releaseRows[2]).getAllByRole('cell')[1]).toHaveTextContent(
      'Approved',
    );
  });

  test('shows a loading indicator while methodology status is loading', async () => {
    let resolveMethodologies: (
      methodologies: MethodologyVersionSummary[],
    ) => void = noop;
    methodologyService.listLatestMethodologyVersions.mockImplementation(
      () =>
        new Promise(resolve => {
          resolveMethodologies = resolve;
        }),
    );

    renderTable({
      releases: [testReleases[0]],
      showIssues: false,
    });

    expect(screen.getByText('Loading methodology status')).toBeInTheDocument();

    await act(async () => {
      resolveMethodologies([testMethodology]);
    });

    expect(await screen.findByText('Approved')).toBeInTheDocument();
  });

  test('shows methodology lookup failures and retries on request', async () => {
    methodologyService.listLatestMethodologyVersions
      .mockRejectedValueOnce(new Error('Request failed'))
      .mockResolvedValueOnce([testMethodology]);

    const { user } = renderTable({
      releases: [testReleases[0]],
      showIssues: false,
    });

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Could not load methodology status.',
    );
    await user.click(
      screen.getByRole('button', {
        name: 'Retry methodology status for Publication 1',
      }),
    );

    expect(
      await screen.findByRole('link', { name: /View methodology/ }),
    ).toHaveAttribute('href', '/methodology/methodology-version-1/summary');
    expect(
      methodologyService.listLatestMethodologyVersions,
    ).toHaveBeenCalledTimes(2);
  });

  test('shows detail requests only for independently enabled columns', async () => {
    const issuesOnly = renderTable({
      releases: [testReleases[0]],
      showIssues: true,
      showMethodology: false,
    });

    expect(
      await screen.findByRole('link', {
        name: 'View issues (2) for Release release-1',
      }),
    ).toBeInTheDocument();
    expect(
      methodologyService.listLatestMethodologyVersions,
    ).not.toHaveBeenCalled();
    issuesOnly.unmount();

    jest.clearAllMocks();
    releaseVersionService.getReleaseVersionChecklist.mockResolvedValue({
      errors: [],
      valid: true,
      warnings: [],
    });
    methodologyService.listLatestMethodologyVersions.mockResolvedValue([
      testMethodology,
    ]);

    renderTable({
      releases: [testReleases[0]],
      showIssues: false,
      showMethodology: true,
    });

    expect(await screen.findByText('Approved')).toBeInTheDocument();
    expect(
      releaseVersionService.getReleaseVersionChecklist,
    ).not.toHaveBeenCalled();
    expect(
      methodologyService.listLatestMethodologyVersions,
    ).toHaveBeenCalledTimes(1);
  });

  test('shows one page for 25 releases and two pages for 26 releases', async () => {
    const test25Releases = Array.from({ length: 25 }, (_, index) =>
      createRelease(
        `release-${index + 1}`,
        'publication-1',
        'Publication',
        index + 2000,
      ),
    );
    const test26Releases = [
      ...test25Releases,
      createRelease('release-26', 'publication-1', 'Publication', 2026),
    ];

    const { unmount } = renderTable({
      releases: test25Releases,
      showIssues: false,
      showMethodology: false,
    });
    expect(getReleaseRowHeaders()).toHaveLength(25);
    expect(
      screen.queryByRole('navigation', { name: 'Pagination' }),
    ).not.toBeInTheDocument();
    unmount();

    const { user } = renderTable({
      releases: test26Releases,
      showIssues: false,
      showMethodology: false,
    });
    expect(getReleaseRowHeaders()).toHaveLength(25);
    expect(
      screen.getByText('26 results, showing page 1 of 2'),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Next page' }));

    expect(getReleaseRowHeaders()).toHaveLength(1);
    expect(
      screen.getByRole('rowheader', { name: 'Release release-1' }),
    ).toHaveTextContent('Release release-1');
    expect(
      screen.getByText('26 results, showing page 2 of 2'),
    ).toBeInTheDocument();
  });

  test('repeats publication links and recalculates row spans across page boundaries', async () => {
    const testReleases26 = Array.from({ length: 26 }, (_, index) =>
      createRelease(
        `release-${index + 1}`,
        'publication-1',
        'Publication',
        index + 2000,
      ),
    );
    const { user } = renderTable({
      releases: testReleases26,
      showIssues: false,
      showMethodology: false,
    });

    expect(
      screen.getByRole('link', { name: 'Publication' }).closest('th'),
    ).toHaveAttribute('rowspan', '25');

    await user.click(screen.getByRole('button', { name: 'Next page' }));

    expect(
      screen.getByRole('link', { name: 'Publication' }).closest('th'),
    ).toHaveAttribute('rowspan', '1');
  });

  test('resets to a valid page when the release count shrinks', async () => {
    const testReleases26 = Array.from({ length: 26 }, (_, index) =>
      createRelease(
        `release-${index + 1}`,
        'publication-1',
        'Publication',
        index + 2000,
      ),
    );
    const { rerender, user } = renderTable({
      releases: testReleases26,
      showIssues: false,
      showMethodology: false,
    });

    await user.click(screen.getByRole('button', { name: 'Next page' }));
    expect(
      screen.getByRole('rowheader', { name: 'Release release-1' }),
    ).toHaveTextContent('Release release-1');

    rerender(
      <MemoryRouter>
        <DraftReleasesTable
          showIssues={false}
          showMethodology={false}
          releases={testReleases26.slice(0, 25)}
          onChangeRelease={noop}
        />
      </MemoryRouter>,
    );

    expect(getReleaseRowHeaders()).toHaveLength(25);
    expect(
      screen.getByText('25 results, showing page 1 of 1'),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('navigation', { name: 'Pagination' }),
    ).not.toBeInTheDocument();
  });

  test('refreshing after cancelling the last release on a page returns to page one', async () => {
    const testReleases26 = [
      createRelease('release-1', 'publication-1', 'Publication', 2000, {
        amendment: true,
        permissions: {
          canViewReleaseVersion: true,
          canUpdateRelease: true,
          canUpdateReleaseVersion: true,
          canDeleteReleaseVersion: true,
          canMakeAmendmentOfReleaseVersion: false,
        },
      }),
      ...Array.from({ length: 25 }, (_, index) =>
        createRelease(
          `release-${index + 2}`,
          'publication-1',
          'Publication',
          index + 2001,
        ),
      ),
    ];
    const onChangeRelease = jest.fn();
    releaseVersionService.getDeleteReleaseVersionPlan.mockResolvedValue({
      scheduledMethodologies: [],
    });

    const TestTable = () => {
      const [releases, setReleases] = useState(testReleases26);
      return (
        <MemoryRouter>
          <DraftReleasesTable
            showIssues={false}
            showMethodology={false}
            releases={releases}
            onChangeRelease={() => {
              onChangeRelease();
              setReleases(current =>
                current.filter(release => release.id !== 'release-1'),
              );
            }}
          />
        </MemoryRouter>
      );
    };

    const { user } = render(<TestTable />);
    await user.click(screen.getByRole('button', { name: 'Next page' }));
    expect(
      screen.getByRole('rowheader', { name: 'Release release-1' }),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', {
        name: 'Cancel amendment for Release release-1',
      }),
    );
    await user.click(
      within(screen.getByRole('dialog')).getByRole('button', {
        name: 'Confirm',
      }),
    );

    expect(onChangeRelease).toHaveBeenCalled();
    expect(
      await screen.findByText('25 results, showing page 1 of 1'),
    ).toBeInTheDocument();
    expect(getReleaseRowHeaders()).toHaveLength(25);
    expect(
      screen.queryByRole('navigation', { name: 'Pagination' }),
    ).not.toBeInTheDocument();
  });

  test('limits checklist and methodology requests to visible releases', async () => {
    const testReleases26 = Array.from({ length: 26 }, (_, index) =>
      createRelease(
        `release-${index + 1}`,
        `publication-${index + 1}`,
        `Publication ${index + 1}`,
        index + 2000,
      ),
    );
    const { user } = renderTable({ releases: testReleases26 });

    expect(
      await screen.findByRole('link', { name: /View methodology/ }),
    ).toBeInTheDocument();
    expect(
      releaseVersionService.getReleaseVersionChecklist,
    ).toHaveBeenCalledTimes(25);
    expect(
      methodologyService.listLatestMethodologyVersions,
    ).toHaveBeenCalledTimes(25);
    expect(
      releaseVersionService.getReleaseVersionChecklist,
    ).not.toHaveBeenCalledWith('release-9');

    await user.click(screen.getByRole('button', { name: 'Next page' }));

    expect(
      await screen.findByRole('rowheader', { name: 'Release release-9' }),
    ).toHaveTextContent('Release release-9');
    expect(
      releaseVersionService.getReleaseVersionChecklist,
    ).toHaveBeenCalledTimes(26);
    expect(
      methodologyService.listLatestMethodologyVersions,
    ).toHaveBeenCalledTimes(26);
  });

  test('preserves release permissions and refreshes the list after cancelling an amendment', async () => {
    const onChangeRelease = jest.fn();
    releaseVersionService.getDeleteReleaseVersionPlan.mockResolvedValue({
      scheduledMethodologies: [{ id: 'methodology-1', title: 'Methodology 1' }],
    });

    const { user } = renderTable({
      releases: [testReleases[1]],
      showIssues: false,
      showMethodology: false,
      onChangeRelease,
    });

    await user.click(
      screen.getByRole('button', {
        name: 'Cancel amendment for Release release-2',
      }),
    );
    expect(
      await screen.findByText(
        'Confirm you want to cancel this amended release',
      ),
    ).toBeInTheDocument();
    expect(
      releaseVersionService.getDeleteReleaseVersionPlan,
    ).toHaveBeenCalledWith('release-2');

    await user.click(
      within(screen.getByRole('dialog')).getByRole('button', {
        name: 'Confirm',
      }),
    );

    expect(releaseVersionService.deleteReleaseVersion).toHaveBeenCalledWith(
      'release-2',
    );
    expect(onChangeRelease).toHaveBeenCalled();
  });

  test('renders the empty state without pagination', () => {
    renderTable({
      releases: [],
      showIssues: false,
      showMethodology: false,
    });

    expect(
      screen.getByText('There are currently no draft releases'),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('navigation', { name: 'Pagination' }),
    ).not.toBeInTheDocument();
  });
});
