import PublicationAdoptMethodologyPage from '@admin/pages/publication/PublicationAdoptMethodologyPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import { MethodologyVersion } from '@admin/services/methodologyService';
import _publicationService, {
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Router } from 'react-router-dom';
import { createMemoryHistory } from 'history';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@admin/services/publicationService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationAdoptMethodologyPage', () => {
  const testMethodology1: MethodologyVersion = {
    amendment: false,
    id: 'methodology-v1',
    internalReleaseNote: 'this is the release note',
    methodologyId: 'methodology-1',
    published: '2021-06-08T00:00:00',
    slug: 'methodology-slug-1',
    status: 'Approved',
    title: 'Methodology 1',
    owningPublication: {
      id: 'publication-2',
      title: 'Publication 2',
    },
  };
  const testMethodology2: MethodologyVersion = {
    amendment: false,
    id: 'methodology-v2',
    internalReleaseNote: 'this is the release note',
    methodologyId: 'methodology-2',
    published: '2021-06-08T00:00:00',
    slug: 'methodology-slug-2',
    status: 'Approved',
    title: 'Methodology 2',
    owningPublication: {
      id: 'publication-3',
      title: 'Publication 3',
    },
  };
  const testMethodology3: MethodologyVersion = {
    amendment: false,
    id: 'methodology-v3',
    internalReleaseNote: 'this is the release note',
    methodologyId: 'methodology-3',
    published: '2021-06-08T00:00:00',
    slug: 'methodology-slug-3',
    status: 'Approved',
    title: 'Methodology 3',
    owningPublication: {
      id: 'publication-4',
      title: 'Publication 4',
    },
  };

  const testMethodologies: MethodologyVersion[] = [
    testMethodology1,
    testMethodology2,
    testMethodology3,
  ];

  test('renders the adopt a methodology page correctly', async () => {
    publicationService.getAdoptableMethodologies.mockResolvedValue(
      testMethodologies,
    );
    renderPage(testPublication);

    expect(screen.getByText('Adopt a methodology')).toBeInTheDocument();

    await waitFor(() =>
      expect(screen.getByText('Select a methodology')).toBeInTheDocument(),
    );

    expect(
      screen.getByLabelText('Search for a methodology'),
    ).toBeInTheDocument();

    const radios = screen.getAllByRole('radio');
    expect(radios.length).toBe(3);
    expect(screen.getByLabelText('Methodology 1')).toHaveAttribute(
      'value',
      'methodology-1',
    );
    expect(screen.getByLabelText('Methodology 2')).toHaveAttribute(
      'value',
      'methodology-2',
    );
    expect(screen.getByLabelText('Methodology 3')).toHaveAttribute(
      'value',
      'methodology-3',
    );

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('shows a message if there are no methodologies', async () => {
    publicationService.getAdoptableMethodologies.mockResolvedValue([]);
    renderPage(testPublication);

    await waitFor(() =>
      expect(
        screen.getByText('No methodologies available.'),
      ).toBeInTheDocument(),
    );

    expect(screen.queryByText('Select a methodology')).not.toBeInTheDocument();

    expect(
      screen.queryByLabelText('Search for a methodology'),
    ).not.toBeInTheDocument();

    expect(screen.queryByRole('radio')).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Save' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Cancel' }),
    ).not.toBeInTheDocument();
  });

  test('handles successful form submission', async () => {
    const history = createMemoryHistory();
    publicationService.getAdoptableMethodologies.mockResolvedValue(
      testMethodologies,
    );
    render(
      <Router history={history}>
        <PublicationContextProvider
          publication={testPublication}
          onPublicationChange={noop}
          onReload={noop}
        >
          <PublicationAdoptMethodologyPage />
        </PublicationContextProvider>
      </Router>,
    );

    await waitFor(() =>
      expect(screen.getByText('Select a methodology')).toBeInTheDocument(),
    );

    userEvent.click(screen.getByLabelText('Methodology 2'));

    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(publicationService.adoptMethodology).toHaveBeenCalledWith(
        'publication-1',
        'methodology-2',
      );
    });

    expect(history.location.pathname).toBe(
      `/publication/publication-1/methodologies`,
    );
  });

  test('handles clicking the cancel button', async () => {
    const history = createMemoryHistory();
    publicationService.getAdoptableMethodologies.mockResolvedValue(
      testMethodologies,
    );
    render(
      <Router history={history}>
        <PublicationContextProvider
          publication={testPublication}
          onPublicationChange={noop}
          onReload={noop}
        >
          <PublicationAdoptMethodologyPage />
        </PublicationContextProvider>
      </Router>,
    );

    await waitFor(() =>
      expect(screen.getByText('Select a methodology')).toBeInTheDocument(),
    );

    userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(publicationService.adoptMethodology).not.toHaveBeenCalled();

    expect(history.location.pathname).toBe(
      `/publication/publication-1/methodologies`,
    );
  });
});

function renderPage(publication: PublicationWithPermissions) {
  render(
    <MemoryRouter>
      <PublicationContextProvider
        publication={publication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <PublicationAdoptMethodologyPage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
