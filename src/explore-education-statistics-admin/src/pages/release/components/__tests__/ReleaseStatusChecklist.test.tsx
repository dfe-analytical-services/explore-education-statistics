import ReleaseStatusChecklist from '@admin/pages/release/components/ReleaseStatusChecklist';
import { Release } from '@admin/services/releaseService';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';

describe('ReleaseStatusChecklist', () => {
  const testRelease: Release = {
    id: 'release-1',
    slug: 'release-1-slug',
    status: 'Draft',
    latestRelease: false,
    live: false,
    amendment: false,
    releaseName: 'Release 1',
    publicationId: 'publication-1',
    publicationTitle: 'Publication 1',
    publicationSlug: 'publication-1-slug',
    timePeriodCoverage: { value: 'W51', label: 'Week 51' },
    title: 'Release Title',
    type: {
      id: 'type-1',
      title: 'Official Statistics',
    },
    contact: {
      id: 'contact-1',
      teamName: 'Test name',
      teamEmail: 'test@test.com',
      contactName: 'Test contact name',
      contactTelNo: '1111 1111 1111',
    },
    previousVersionId: '',
    preReleaseAccessList: '',
  };

  test('renders correctly with errors', () => {
    render(
      <MemoryRouter>
        <ReleaseStatusChecklist
          checklist={{
            valid: false,
            warnings: [],
            errors: [
              { code: 'DataFileImportsMustBeCompleted' },
              { code: 'DataFileReplacementsMustBeCompleted' },
              {
                code: 'MethodologyMustBeApproved',
                methodologyId: 'methodology-1',
              },
              { code: 'PublicMetaGuidanceRequired' },
              { code: 'PublicPreReleaseAccessListRequired' },
              { code: 'ReleaseNoteRequired' },
            ],
          }}
          release={testRelease}
        />
      </MemoryRouter>,
    );

    expect(
      screen.queryByRole('heading', { name: 'All checks passed' }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Errors' })).toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Warnings' }),
    ).not.toBeInTheDocument();

    expect(screen.getByText('6 issues')).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'All data file imports must be completed',
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
        name: 'Methodology must be approved',
      }),
    ).toHaveAttribute('href', '/methodologies/methodology-1/status');

    expect(
      screen.getByRole('link', {
        name: 'All public metadata guidance must be populated',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#metadata-guidance',
    );

    expect(
      screen.getByRole('link', {
        name: 'Public pre-release access list is required',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/prerelease-access#preReleaseAccess-publicList',
    );

    expect(
      screen.getByRole('link', {
        name: 'Public release note for this amendment is required',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/content',
    );
  });

  test('renders correctly with warnings', () => {
    render(
      <MemoryRouter>
        <ReleaseStatusChecklist
          checklist={{
            valid: true,
            warnings: [
              { code: 'NoMethodology' },
              { code: 'NoNextReleaseDate' },
              { code: 'NoFootnotesOnSubjects', totalSubjects: 3 },
              { code: 'NoTableHighlights' },
              { code: 'NoDataFiles' },
            ],
            errors: [],
          }}
          release={testRelease}
        />
      </MemoryRouter>,
    );

    expect(
      screen.queryByRole('heading', { name: 'All checks passed' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Errors' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Warnings' }),
    ).toBeInTheDocument();

    expect(screen.getByText('5 potential issues')).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'No methodology attached to publication',
      }),
    ).toHaveAttribute('href', '/publication/publication-1/edit');

    expect(
      screen.getByRole('link', {
        name: 'No next release expected date',
      }),
    ).toHaveAttribute('href', '#releaseStatusForm-nextReleaseDate-month');

    expect(
      screen.getByRole('link', {
        name: 'No footnotes for 3 subjects',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/footnotes',
    );

    expect(
      screen.getByRole('link', {
        name: 'No table highlights',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/datablocks',
    );

    expect(
      screen.getByRole('link', {
        name: 'No data files uploaded',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#data-uploads',
    );
  });

  test('renders correctly with both warnings and errors', () => {
    render(
      <MemoryRouter>
        <ReleaseStatusChecklist
          checklist={{
            valid: true,
            warnings: [{ code: 'NoMethodology' }],
            errors: [{ code: 'PublicMetaGuidanceRequired' }],
          }}
          release={testRelease}
        />
      </MemoryRouter>,
    );

    expect(
      screen.queryByRole('heading', { name: 'All checks passed' }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Errors' })).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Warnings' }),
    ).toBeInTheDocument();

    expect(screen.getByText('1 issue')).toBeInTheDocument();
    expect(screen.getByText('1 potential issue')).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'All public metadata guidance must be populated',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'No methodology attached to publication',
      }),
    ).toBeInTheDocument();
  });

  test('renders correctly with no warnings or errors', () => {
    render(
      <MemoryRouter>
        <ReleaseStatusChecklist
          checklist={{
            valid: true,
            warnings: [],
            errors: [],
          }}
          release={testRelease}
        />
      </MemoryRouter>,
    );

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
