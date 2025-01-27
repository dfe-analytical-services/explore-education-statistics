import PublicationEditReleaseSeriesLegacyLinkPage from '@admin/pages/publication/PublicationEditReleaseSeriesLegacyLinkPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import {
  PublicationEditReleaseSeriesLegacyLinkRouteParams,
  publicationEditReleaseSeriesLegacyLinkRoute,
} from '@admin/routes/publicationRoutes';
import _publicationService, {
  PublicationWithPermissions,
  ReleaseSeriesTableEntry,
} from '@admin/services/publicationService';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter, Route } from 'react-router-dom';
import { generatePath } from 'react-router';
import noop from 'lodash/noop';
import render from '@common-test/render';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationEditReleaseSeriesLegacyLinkPage', () => {
  const releaseSeries: ReleaseSeriesTableEntry[] = [
    {
      id: 'release-series-item-1',
      isLegacyLink: true,
      description: 'Legacy link 1',
      releaseSlug: '2000-01',
      legacyLinkUrl: 'https://gov.uk/1',
    },
    {
      id: 'release-series-item-2',
      isLegacyLink: false,
      description: 'Academic Year 2000/01',

      releaseId: 'release-1',
      releaseSlug: 'release-slug',
      isLatest: true,
      isPublished: true,
    },
    {
      id: 'release-series-item-3',
      isLegacyLink: true,
      description: 'Legacy link 2',
      releaseSlug: '2000-01',
      legacyLinkUrl: 'https://gov.uk/2',
    },
  ];

  beforeEach(() => {
    publicationService.getReleaseSeries.mockResolvedValue(releaseSeries);
  });

  test('renders the edit legacy release page', async () => {
    renderPage(testPublication, 'release-series-item-1');

    await waitFor(() => {
      expect(screen.getByText('Edit legacy release')).toBeInTheDocument();
    });
    expect(screen.getByLabelText('Description')).toHaveValue('Legacy link 1');
    expect(screen.getByLabelText('URL')).toHaveValue('https://gov.uk/1');
    expect(
      screen.getByRole('button', { name: 'Save legacy release' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Cancel' })).toHaveAttribute(
      'href',
      '/publication/publication-1/releases/order',
    );
  });

  test('handles successfully submitting the form', async () => {
    renderPage(testPublication, 'release-series-item-1');
    await waitFor(() => {
      expect(screen.getByText('Edit legacy release')).toBeInTheDocument();
    });

    await userEvent.type(screen.getByLabelText('Description'), ' edited');
    await userEvent.type(screen.getByLabelText('URL'), '/edit');
    await userEvent.click(
      screen.getByRole('button', {
        name: 'Save legacy release',
      }),
    );

    await waitFor(() => {
      expect(publicationService.updateReleaseSeries).toHaveBeenCalledWith(
        'publication-1',
        [
          {
            legacyLinkDescription: 'Legacy link 1 edited',
            legacyLinkUrl: 'https://gov.uk/1/edit',
          },
          {
            releaseId: 'release-1',
          },
          {
            legacyLinkDescription: 'Legacy link 2',
            legacyLinkUrl: 'https://gov.uk/2',
          },
        ],
      );
    });
  });
});

function renderPage(
  publication: PublicationWithPermissions,
  releaseSeriesItemId: string,
) {
  render(
    <MemoryRouter
      initialEntries={[
        generatePath<PublicationEditReleaseSeriesLegacyLinkRouteParams>(
          publicationEditReleaseSeriesLegacyLinkRoute.path,
          {
            publicationId: publication.id,
            releaseSeriesItemId,
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
          path={publicationEditReleaseSeriesLegacyLinkRoute.path}
          component={PublicationEditReleaseSeriesLegacyLinkPage}
        />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
