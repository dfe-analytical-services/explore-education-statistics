import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import _methodologyService, {
  MethodologyVersion,
} from '@admin/services/methodologyService';
import _permissionService from '@admin/services/permissionService';
import { generatePath, MemoryRouter } from 'react-router';
import MethodologyStatusPage from '@admin/pages/methodology/edit-methodology/status/MethodologyStatusPage';
import { MethodologyContextProvider } from '@admin/pages/methodology/contexts/MethodologyContext';
import {
  MethodologyRouteParams,
  methodologyStatusRoute,
} from '@admin/routes/methodologyRoutes';
import userEvent from '@testing-library/user-event';
import { Route } from 'react-router-dom';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';

jest.mock('@admin/services/methodologyService');
jest.mock('@admin/services/permissionService');

const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;

describe('MethodologyStatusPage', () => {
  const testMethodology: MethodologyVersion = {
    id: 'm1',
    amendment: false,
    title: 'Test methodology',
    slug: 'test-methodology',
    owningPublication: {
      id: 'p1',
      title: 'Owning publication title',
    },
  } as MethodologyVersion;

  const testMethodologyWithOtherPublications = {
    ...testMethodology,
    otherPublications: [
      {
        id: 'p2',
        title: 'Other publication 1',
      },
      {
        id: 'p3',
        title: 'Other publication 2',
      },
    ],
  };

  test('renders Draft status details', async () => {
    permissionService.canApproveMethodology.mockResolvedValue(false);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage({
      ...testMethodology,
      status: 'Draft',
      publishingStrategy: 'WithRelease',
    });

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Status')).toHaveTextContent('In Draft');
    expect(screen.queryByTestId('Internal note-key')).not.toBeInTheDocument();
    expect(screen.queryByTestId('When to publish-key')).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('Publish with release-key'),
    ).not.toBeInTheDocument();

    expect(screen.queryByText('Edit status')).not.toBeInTheDocument();
  });

  test('renders Approved details for publishing immediately', async () => {
    permissionService.canApproveMethodology.mockResolvedValue(false);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage({
      ...testMethodology,
      status: 'Approved',
      internalReleaseNote: 'Test internal release note',
      publishingStrategy: 'Immediately',
    });

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Status')).toHaveTextContent('Approved');

    expect(screen.getByTestId('Internal note-key')).toHaveTextContent(
      'Internal note',
    );
    expect(screen.getByTestId('Internal note-value')).toHaveTextContent(
      'Test internal release note',
    );

    expect(screen.getByTestId('When to publish-key')).toHaveTextContent(
      'When to publish',
    );
    expect(screen.getByTestId('When to publish-value')).toHaveTextContent(
      'Immediately',
    );

    expect(
      screen.queryByTestId('Publish with release-key'),
    ).not.toBeInTheDocument();

    expect(screen.queryByText('Edit status')).not.toBeInTheDocument();
  });

  test('renders Approved details for publishing with release', async () => {
    permissionService.canApproveMethodology.mockResolvedValue(false);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage({
      ...testMethodology,
      status: 'Approved',
      internalReleaseNote: 'Test internal release note',
      publishingStrategy: 'WithRelease',
      scheduledWithRelease: {
        id: 'dependant-release',
        title: 'Dependant Release',
      },
    });

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Status')).toHaveTextContent('Approved');

    expect(screen.getByTestId('Internal note-key')).toHaveTextContent(
      'Internal note',
    );
    expect(screen.getByTestId('Internal note-value')).toHaveTextContent(
      'Test internal release note',
    );

    expect(screen.getByTestId('When to publish-key')).toHaveTextContent(
      'When to publish',
    );
    expect(screen.getByTestId('When to publish-value')).toHaveTextContent(
      'With a specific release',
    );

    expect(screen.getByTestId('Publish with release-key')).toHaveTextContent(
      'Publish with release',
    );
    expect(screen.getByTestId('Publish with release-value')).toHaveTextContent(
      'Dependant Release',
    );

    expect(screen.queryByText('Edit status')).not.toBeInTheDocument();
  });

  test('renders Edit status button if user can approve methodology', async () => {
    permissionService.canApproveMethodology.mockResolvedValue(true);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Edit status' }),
      ).toBeInTheDocument();
    });
  });

  test('renders Edit status button if user can mark methodology as draft', async () => {
    permissionService.canApproveMethodology.mockResolvedValue(false);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(true);

    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Edit status' }),
      ).toBeInTheDocument();
    });
  });

  test('renders status form when Edit button is clicked', async () => {
    methodologyService.getUnpublishedReleases.mockResolvedValue([]);
    permissionService.canApproveMethodology.mockResolvedValue(true);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Edit status' }),
      ).toBeInTheDocument();
    });

    userEvent.click(screen.getByRole('button', { name: 'Edit status' }));

    await waitFor(() => {
      expect(screen.getByText('Edit methodology status')).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Update status' }),
      ).toBeInTheDocument();
    });
  });

  test('renders the owning publication', async () => {
    methodologyService.getUnpublishedReleases.mockResolvedValue([]);
    permissionService.canApproveMethodology.mockResolvedValue(true);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(screen.getByTestId('Owning publication-key')).toHaveTextContent(
        'Owning publication',
      );
      expect(screen.getByTestId('Owning publication-value')).toHaveTextContent(
        'Owning publication title',
      );
    });
  });

  test('renders the other publications', async () => {
    methodologyService.getUnpublishedReleases.mockResolvedValue([]);
    permissionService.canApproveMethodology.mockResolvedValue(true);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage(testMethodologyWithOtherPublications);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(screen.getByTestId('Other publications-key')).toHaveTextContent(
        'Other publications',
      );
      const otherPublications = screen.queryAllByTestId(
        'other-publication-item',
      );
      expect(otherPublications.length).toBe(2);
      expect(otherPublications[0]).toHaveTextContent('Other publication 1');
      expect(otherPublications[1]).toHaveTextContent('Other publication 2');
    });
  });

  test('does not render the other publications section if there are none', async () => {
    methodologyService.getUnpublishedReleases.mockResolvedValue([]);
    permissionService.canApproveMethodology.mockResolvedValue(true);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(
        screen.queryByTestId('Other publications-key'),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('Other publications-value'),
      ).not.toBeInTheDocument();
    });
  });

  test('renders the publicly accessible url of the methodology', async () => {
    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByTestId('public-methodology-url')).toHaveValue(
        'http://localhost/methodology/test-methodology',
      );
    });
  });

  function renderPage(methodology: MethodologyVersion) {
    render(
      <MemoryRouter
        initialEntries={[
          generatePath<MethodologyRouteParams>(methodologyStatusRoute.path, {
            methodologyId: testMethodology.id,
          }),
        ]}
      >
        <TestConfigContextProvider>
          <MethodologyContextProvider methodology={methodology}>
            <Route
              path={methodologyStatusRoute.path}
              component={MethodologyStatusPage}
            />
          </MethodologyContextProvider>
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  }
});
