import ReleaseStatusChecklist from '@admin/pages/release/components/ReleaseStatusChecklist';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import _releaseVersionService, {
  ReleaseVersionChecklist,
} from '@admin/services/releaseVersionService';
import { AuthContextTestProvider, User } from '@admin/contexts/AuthContext';
import { GlobalPermissions } from '@admin/services/authService';
import { screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import render from '@common-test/render';

jest.mock('@admin/services/releaseVersionService');
const releaseVersionService = jest.mocked(_releaseVersionService);

describe('ReleaseStatusChecklist', () => {
  const testChecklist: ReleaseVersionChecklist = {
    valid: false,
    warnings: [],
    errors: [
      { code: 'DataFileImportsMustBeCompleted' },
      { code: 'DataFileReplacementsMustBeCompleted' },
      { code: 'PublicDataGuidanceRequired' },
      { code: 'ReleaseNoteRequired' },
      { code: 'SummarySectionContainsEmptyHtmlBlock' },
      { code: 'EmptyContentSectionExists' },
      { code: 'GenericSectionsContainEmptyHtmlBlock' },
      { code: 'RelatedDashboardsSectionContainsEmptyHtmlBlock' },
      { code: 'ReleaseMustContainKeyStatOrNonEmptyHeadlineBlock' },
      { code: 'PublicApiDataSetImportsMustBeCompleted' },
      { code: 'PublicApiDataSetCancellationsMustBeResolved' },
      { code: 'PublicApiDataSetFailuresMustBeResolved' },
      { code: 'PublicApiDataSetMappingsMustBeCompleted' },
    ],
  };

  test('renders correctly with errors', async () => {
    const bauUser: User = {
      id: 'user-id',
      name: 'BAU',
      permissions: {
        isBauUser: true,
      } as GlobalPermissions,
    };

    releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
      testChecklist,
    );

    render(
      <AuthContextTestProvider user={bauUser}>
        <MemoryRouter>
          <ReleaseStatusChecklist releaseVersion={testRelease} />
        </MemoryRouter>
        ,
      </AuthContextTestProvider>,
    );

    expect(await screen.findByText('Errors')).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'All checks passed' }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Errors' })).toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Warnings' }),
    ).not.toBeInTheDocument();

    expect(screen.getByText('13 issues')).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'All data imports must be completed',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#data-uploads',
    );

    expect(
      screen.getByRole('link', {
        name: 'All data file replacements must be completed',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#data-uploads',
    );

    expect(
      screen.getByRole('link', {
        name: 'All summary information must be completed on the data guidance page',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#data-guidance',
    );

    expect(
      screen.getByRole('link', {
        name: 'A public release note for this amendment is required, add this near the top of the content page',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/content',
    );

    expect(
      screen.getByRole('link', {
        name: 'Release content should not contain an empty summary section',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/content',
    );

    expect(
      screen.getByRole('link', {
        name: 'Release content should not contain any empty sections',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/content',
    );

    expect(
      screen.getByRole('link', {
        name: 'Release content should not contain empty text blocks',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/content',
    );

    expect(
      screen.getByRole('link', {
        name: 'Release content should not contain an empty related dashboards section',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/content',
    );

    expect(
      screen.getByRole('link', {
        name: 'Release must contain a key statistic or a non-empty headline text block',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/content',
    );

    expect(
      screen.getByRole('link', {
        name: 'All public API data set processing must be completed',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#api-data-sets',
    );

    expect(
      screen.getByRole('link', {
        name: 'All cancelled public API data sets must be removed or completed',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#api-data-sets',
    );

    expect(
      screen.getByRole('link', {
        name: 'All failed public API data sets must be retried or removed',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#api-data-sets',
    );

    expect(
      screen.getByRole('link', {
        name: 'All public API data set mappings must be completed',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#api-data-sets',
    );
  });

  test('does not render api data set links for non-BAU users', async () => {
    const nonBauUser: User = {
      id: 'user-id',
      name: 'BAU',
      permissions: {
        isBauUser: false,
      } as GlobalPermissions,
    };

    releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
      testChecklist,
    );

    render(
      <AuthContextTestProvider user={nonBauUser}>
        <MemoryRouter>
          <ReleaseStatusChecklist releaseVersion={testRelease} />
        </MemoryRouter>
      </AuthContextTestProvider>,
    );

    expect(await screen.findByText('Errors')).toBeInTheDocument();

    expect(
      screen.getByText('All public API data set processing must be completed'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'All public API data set processing must be completed',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText(
        'All cancelled public API data sets must be removed or completed',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'All cancelled public API data sets must be removed or completed',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText(
        'All failed public API data sets must be retried or removed',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'All failed public API data sets must be retried or removed',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('All public API data set mappings must be completed'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'All public API data set mappings must be completed',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with warnings', async () => {
    const testChecklistWithWarnings: ReleaseVersionChecklist = {
      valid: true,
      warnings: [
        { code: 'NoMethodology' },
        {
          code: 'MethodologyNotApproved',
          methodologyId: 'methodology-1',
        },
        {
          code: 'MethodologyNotApproved',
          methodologyId: 'methodology-2',
        },
        { code: 'NoNextReleaseDate' },
        { code: 'NoFootnotesOnSubjects', totalSubjects: 3 },
        { code: 'NoFeaturedTables' },
        { code: 'NoDataFiles' },
        { code: 'NoPublicPreReleaseAccessList' },
      ],
      errors: [],
    };

    releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
      testChecklistWithWarnings,
    );

    render(
      <MemoryRouter>
        <ReleaseStatusChecklist releaseVersion={testRelease} />
      </MemoryRouter>,
    );

    expect(await screen.findByText('Warnings')).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'All checks passed' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Errors' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Warnings' }),
    ).toBeInTheDocument();

    expect(screen.getByText('8 things')).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'An in-EES methodology page has not been linked to this publication',
      }),
    ).toHaveAttribute('href', '/publication/publication-1/methodologies');

    expect(
      screen.getAllByRole('link', {
        name: 'A methodology for this publication is not yet approved',
      })[0],
    ).toHaveAttribute('href', '/methodology/methodology-1/status');

    expect(
      screen.getAllByRole('link', {
        name: 'A methodology for this publication is not yet approved',
      })[1],
    ).toHaveAttribute('href', '/methodology/methodology-2/status');

    expect(
      screen.getByRole('link', {
        name: 'No next expected release date has been added',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/status',
    );

    expect(
      screen.getByRole('link', {
        name: "3 data files don't have any footnotes",
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/footnotes',
    );

    expect(
      screen.getByRole('link', {
        name: 'No data blocks have been saved as featured tables',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data-blocks',
    );

    expect(
      screen.getByRole('link', {
        name: 'No data files uploaded',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#data-uploads',
    );

    expect(
      screen.getByRole('link', {
        name: 'A public pre-release access list has not been created',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/prerelease-access#preReleaseAccess-publicList',
    );
  });

  test('renders correctly with both warnings and errors', async () => {
    const testChecklistWithErrorsAndWarnings: ReleaseVersionChecklist = {
      valid: true,
      warnings: [{ code: 'NoMethodology' }],
      errors: [{ code: 'PublicDataGuidanceRequired' }],
    };

    releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
      testChecklistWithErrorsAndWarnings,
    );

    render(
      <MemoryRouter>
        <ReleaseStatusChecklist releaseVersion={testRelease} />
      </MemoryRouter>,
    );

    expect(await screen.findByText('Errors')).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'All checks passed' }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Errors' })).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Warnings' }),
    ).toBeInTheDocument();

    expect(screen.getByText('1 issue')).toBeInTheDocument();
    expect(screen.getByText('1 thing')).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'All summary information must be completed on the data guidance page',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'An in-EES methodology page has not been linked to this publication',
      }),
    ).toBeInTheDocument();
  });

  test('renders correctly with no warnings or errors', async () => {
    const testChecklistWithNoErrorsOrWarnings: ReleaseVersionChecklist = {
      valid: true,
      warnings: [],
      errors: [],
    };

    releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
      testChecklistWithNoErrorsOrWarnings,
    );

    render(
      <MemoryRouter>
        <ReleaseStatusChecklist releaseVersion={testRelease} />
      </MemoryRouter>,
    );

    expect(await screen.findByText('All checks passed')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'All checks passed' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Errors' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Warnings' }),
    ).not.toBeInTheDocument();
  });
});
