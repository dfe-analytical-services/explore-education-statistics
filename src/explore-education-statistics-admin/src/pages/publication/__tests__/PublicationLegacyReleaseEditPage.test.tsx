import PublicationLegacyReleaseEditPage from '@admin/pages/publication/PublicationLegacyReleaseEditPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import {
  PublicationEditLegacyReleaseRouteParams,
  publicationEditLegacyReleaseRoute,
} from '@admin/routes/publicationRoutes';
import _publicationService, {
  PublicationWithPermissions,
  ReleaseSeriesItem,
} from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter, Route } from 'react-router-dom';
import { generatePath } from 'react-router';
import noop from 'lodash/noop';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationLegacyReleaseEditPage', () => {
  const releaseSeries: ReleaseSeriesItem[] = [
    {
      id: 'legacy-release-1',
      isLegacyLink: true,
      description: 'Legacy release 1',

      legacyLinkUrl: 'https://gov.uk/1',
    },
    {
      id: 'release-1',
      isLegacyLink: false,
      description: 'Academic Year 2000/01',

      releaseParentId: 'release-parent-1',
      publicationSlug: 'publication-slug',
      releaseSlug: 'release-slug',
    },
    {
      id: 'legacy-release-2',
      isLegacyLink: true,
      description: 'Legacy release 2',

      legacyLinkUrl: 'https://gov.uk/2',
    },
  ];

  beforeEach(() => {
    publicationService.getReleaseSeriesView.mockResolvedValue(releaseSeries);
  });

  test('renders the edit legacy release page', async () => {
    renderPage(testPublication, 'legacy-release-1');

    await waitFor(() => {
      expect(screen.getByText('Edit legacy release')).toBeInTheDocument();
    });
    expect(screen.getByLabelText('Description')).toHaveValue(
      'Legacy release 1',
    );
    expect(screen.getByLabelText('URL')).toHaveValue('https://gov.uk/1');
    expect(
      screen.getByRole('button', { name: 'Save legacy release' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Cancel' })).toHaveAttribute(
      'href',
      '/publication/publication-1/legacy',
    );
  });

  test('handles successfully submitting the form', async () => {
    renderPage(testPublication, 'legacy-release-1');
    await waitFor(() => {
      expect(screen.getByText('Edit legacy release')).toBeInTheDocument();
    });

    userEvent.type(screen.getByLabelText('Description'), ' edited');
    userEvent.type(screen.getByLabelText('URL'), '/edit');
    userEvent.click(
      screen.getByRole('button', {
        name: 'Save legacy release',
      }),
    );

    await waitFor(() => {
      expect(publicationService.updateReleaseSeriesView).toHaveBeenCalledWith(
        'publication-1',
        [
          {
            id: 'legacy-release-1',

            legacyLinkDescription: 'Legacy release 1 edited',
            legacyLinkUrl: 'https://gov.uk/1/edit',
          },
          {
            id: 'release-1',

            releaseParentId: 'release-parent-1',
          },
          {
            id: 'legacy-release-2',

            legacyLinkDescription: 'Legacy release 2',
            legacyLinkUrl: 'https://gov.uk/2',
          },
        ],
      );
    });
  });
});

// @MarkFix more tests

function renderPage(
  publication: PublicationWithPermissions,
  legacyReleaseId: string,
) {
  render(
    <MemoryRouter
      initialEntries={[
        generatePath<PublicationEditLegacyReleaseRouteParams>(
          publicationEditLegacyReleaseRoute.path,
          {
            publicationId: publication.id,
            legacyReleaseId,
          },
        ),
      ]}
    >
      <PublicationContextProvider
        publication={publication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <Route
          path={publicationEditLegacyReleaseRoute.path}
          component={PublicationLegacyReleaseEditPage}
        />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
