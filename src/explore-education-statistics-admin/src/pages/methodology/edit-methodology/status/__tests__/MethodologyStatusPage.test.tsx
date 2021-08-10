import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import _methodologyService, {
  BasicMethodology,
} from '@admin/services/methodologyService';
import _permissionService from '@admin/services/permissionService';
import { generatePath, MemoryRouter } from 'react-router';
import MethodologyStatusPage from '@admin/pages/methodology/edit-methodology/status/MethodologyStatusPage';
import {
  MethodologyRouteParams,
  methodologyStatusRoute,
} from '@admin/routes/methodologyRoutes';
import userEvent from '@testing-library/user-event';
import { Route } from 'react-router-dom';

jest.mock('@admin/services/methodologyService');
jest.mock('@admin/services/permissionService');

const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;

describe('MethodologyStatusPage', () => {
  const testMethodology: BasicMethodology = {
    id: 'm1',
    amendment: false,
    title: 'Test methodology',
    slug: 'test-methodology',
    owningPublication: {
      id: 'p1',
      title: 'Owning publication title',
    },
  } as BasicMethodology;

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
    methodologyService.getMethodology.mockResolvedValue({
      ...testMethodology,
      status: 'Draft',
      publishingStrategy: 'WithRelease',
    });
    permissionService.canApproveMethodology.mockResolvedValue(false);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(screen.getByTestId('Status-key')).toHaveTextContent('Status');
      expect(screen.getByTestId('Status-value')).toHaveTextContent('In Draft');
    });

    expect(screen.queryByTestId('When to publish-key')).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('Publish with release-key'),
    ).not.toBeInTheDocument();

    expect(screen.queryByText('Edit status')).not.toBeInTheDocument();
  });

  test('renders Approved details for publishing immediatately', async () => {
    methodologyService.getMethodology.mockResolvedValue({
      ...testMethodology,
      status: 'Approved',
      publishingStrategy: 'Immediately',
    });
    permissionService.canApproveMethodology.mockResolvedValue(false);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(screen.getByTestId('Status-key')).toHaveTextContent('Status');
      expect(screen.getByTestId('Status-value')).toHaveTextContent('Approved');

      expect(screen.getByTestId('When to publish-key')).toHaveTextContent(
        'When to publish',
      );
      expect(screen.getByTestId('When to publish-value')).toHaveTextContent(
        'Immediately',
      );
    });

    expect(
      screen.queryByTestId('Publish with release-key'),
    ).not.toBeInTheDocument();

    expect(screen.queryByText('Edit status')).not.toBeInTheDocument();
  });

  test('renders Approved details for publishing with release', async () => {
    methodologyService.getMethodology.mockResolvedValue({
      ...testMethodology,
      status: 'Approved',
      publishingStrategy: 'WithRelease',
      scheduledWithRelease: {
        id: 'dependant-release',
        title: 'Dependant Release',
      },
    });
    permissionService.canApproveMethodology.mockResolvedValue(false);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(screen.getByTestId('Status-key')).toHaveTextContent('Status');
      expect(screen.getByTestId('Status-value')).toHaveTextContent('Approved');

      expect(screen.getByTestId('When to publish-key')).toHaveTextContent(
        'When to publish',
      );
      expect(screen.getByTestId('When to publish-value')).toHaveTextContent(
        'With a specific release',
      );

      expect(screen.getByTestId('Publish with release-key')).toHaveTextContent(
        'Publish with release',
      );
      expect(
        screen.getByTestId('Publish with release-value'),
      ).toHaveTextContent('Dependant Release');
    });

    expect(screen.queryByText('Edit status')).not.toBeInTheDocument();
  });

  test('renders Edit status button if user can approve methodology', async () => {
    methodologyService.getMethodology.mockResolvedValue(testMethodology);
    permissionService.canApproveMethodology.mockResolvedValue(true);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Edit status' }),
      ).toBeInTheDocument();
    });
  });

  test('renders Edit status button if user can mark methodology as draft', async () => {
    methodologyService.getMethodology.mockResolvedValue(testMethodology);
    permissionService.canApproveMethodology.mockResolvedValue(false);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(true);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Edit status' }),
      ).toBeInTheDocument();
    });
  });

  test('renders status form when Edit button is clicked', async () => {
    methodologyService.getMethodology.mockResolvedValue(testMethodology);
    methodologyService.getUnpublishedReleases.mockResolvedValue([]);
    permissionService.canApproveMethodology.mockResolvedValue(true);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage();

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
    methodologyService.getMethodology.mockResolvedValue(testMethodology);
    methodologyService.getUnpublishedReleases.mockResolvedValue([]);
    permissionService.canApproveMethodology.mockResolvedValue(true);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage();

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
    methodologyService.getMethodology.mockResolvedValue(
      testMethodologyWithOtherPublications,
    );
    methodologyService.getUnpublishedReleases.mockResolvedValue([]);
    permissionService.canApproveMethodology.mockResolvedValue(true);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage();

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
    methodologyService.getMethodology.mockResolvedValue(testMethodology);
    methodologyService.getUnpublishedReleases.mockResolvedValue([]);
    permissionService.canApproveMethodology.mockResolvedValue(true);
    permissionService.canMarkMethodologyAsDraft.mockResolvedValue(false);

    renderPage();

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

  function renderPage() {
    render(
      <MemoryRouter
        initialEntries={[
          generatePath<MethodologyRouteParams>(methodologyStatusRoute.path, {
            methodologyId: testMethodology.id,
          }),
        ]}
      >
        <Route
          path={methodologyStatusRoute.path}
          component={MethodologyStatusPage}
        />
      </MemoryRouter>,
    );
  }
});
