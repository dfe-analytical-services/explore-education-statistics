import ReleaseStatusChecklistSummary from '@admin/pages/release/components/ReleaseStatusChecklistSummary';
import _releaseVersionService, {
  ReleaseVersionChecklist,
} from '@admin/services/releaseVersionService';
import { screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import render from '@common-test/render';

jest.mock('@admin/services/releaseVersionService');
const releaseVersionService = jest.mocked(_releaseVersionService);

describe('ReleaseStatusChecklistSummary', () => {
  describe('default format', () => {
    test('renders correctly with errors and warnings', async () => {
      const testChecklist: ReleaseVersionChecklist = {
        valid: false,
        warnings: [{ code: 'NoMethodology' }, { code: 'NoNextReleaseDate' }],
        errors: [
          { code: 'PublicDataGuidanceRequired' },
          { code: 'ReleaseNoteRequired' },
          { code: 'SummarySectionContainsEmptyHtmlBlock' },
        ],
      };

      releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
        testChecklist,
      );

      render(
        <MemoryRouter>
          <ReleaseStatusChecklistSummary
            publicationId="publication-id"
            releaseVersionId="release-id"
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText(
          'There are 3 errors (and 2 warnings), please resolve these before assigning for higher review.',
        ),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'There are 3 errors (and 2 warnings), please resolve these before assigning for higher review.',
        }),
      ).toHaveAttribute(
        'href',
        '/publication/publication-id/release/release-id/checklist',
      );
    });

    test('renders correctly with errors only', async () => {
      const testChecklist: ReleaseVersionChecklist = {
        valid: false,
        warnings: [],
        errors: [{ code: 'PublicDataGuidanceRequired' }],
      };

      releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
        testChecklist,
      );

      render(
        <MemoryRouter>
          <ReleaseStatusChecklistSummary
            publicationId="publication-id"
            releaseVersionId="release-id"
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText(
          'There is 1 error, please resolve this before assigning for higher review.',
        ),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'There is 1 error, please resolve this before assigning for higher review.',
        }),
      ).toHaveAttribute(
        'href',
        '/publication/publication-id/release/release-id/checklist',
      );
    });

    test('renders correctly with warnings only', async () => {
      const testChecklist: ReleaseVersionChecklist = {
        valid: false,
        warnings: [{ code: 'NoMethodology' }, { code: 'NoNextReleaseDate' }],
        errors: [],
      };

      releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
        testChecklist,
      );

      render(
        <MemoryRouter>
          <ReleaseStatusChecklistSummary
            publicationId="publication-id"
            releaseVersionId="release-id"
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText(
          'There are 2 warnings, please check these before assigning for higher review.',
        ),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'There are 2 warnings, please check these before assigning for higher review.',
        }),
      ).toHaveAttribute(
        'href',
        '/publication/publication-id/release/release-id/checklist',
      );
    });

    test('renders correctly with no errors or warnings', async () => {
      const testChecklist: ReleaseVersionChecklist = {
        valid: true,
        warnings: [],
        errors: [],
      };

      releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
        testChecklist,
      );

      render(
        <MemoryRouter>
          <ReleaseStatusChecklistSummary
            publicationId="publication-id"
            releaseVersionId="release-id"
          />
        </MemoryRouter>,
      );

      expect(
        await screen.findByText(
          'No issues to resolve. This release can be published.',
        ),
      ).toBeInTheDocument();
      expect(screen.queryByRole('link')).not.toBeInTheDocument();
    });
  });

  describe('simple format', () => {
    test('renders correctly with errors and warnings', async () => {
      const testChecklist: ReleaseVersionChecklist = {
        valid: false,
        warnings: [{ code: 'NoMethodology' }, { code: 'NoNextReleaseDate' }],
        errors: [
          { code: 'PublicDataGuidanceRequired' },
          { code: 'ReleaseNoteRequired' },
          { code: 'SummarySectionContainsEmptyHtmlBlock' },
        ],
      };

      releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
        testChecklist,
      );

      render(
        <MemoryRouter>
          <ReleaseStatusChecklistSummary
            publicationId="publication-id"
            releaseVersionId="release-id"
            releaseTitle="Release 1"
            simple
          />
        </MemoryRouter>,
      );

      expect(await screen.findByText(/View issues/)).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'View issues (5) for Release 1',
        }),
      ).toHaveAttribute(
        'href',
        '/publication/publication-id/release/release-id/checklist',
      );
    });

    test('renders correctly with no errors or warnings', async () => {
      const testChecklist: ReleaseVersionChecklist = {
        valid: true,
        warnings: [],
        errors: [],
      };

      releaseVersionService.getReleaseVersionChecklist.mockResolvedValue(
        testChecklist,
      );

      render(
        <MemoryRouter>
          <ReleaseStatusChecklistSummary
            publicationId="publication-id"
            releaseVersionId="release-id"
            releaseTitle="Release 1"
            simple
          />
        </MemoryRouter>,
      );

      expect(await screen.findByText('No issues')).toBeInTheDocument();
      expect(screen.queryByRole('link')).not.toBeInTheDocument();
    });
  });
});
