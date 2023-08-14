import { screen, waitFor, within } from '@testing-library/react';
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
import render from '@common-test/render';

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
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: false,
    });

    renderPage({
      ...testMethodology,
      status: 'Draft',
      publishingStrategy: 'WithRelease',
    });

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Status')).toHaveTextContent('In Draft');
    expect(screen.queryByTestId('When to publish')).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('Publish with release'),
    ).not.toBeInTheDocument();

    expect(screen.queryByText('Edit status')).not.toBeInTheDocument();
  });

  test('renders Higher review status details', async () => {
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: false,
    });

    renderPage({
      ...testMethodology,
      status: 'HigherLevelReview',
      publishingStrategy: 'WithRelease',
    });

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Status')).toHaveTextContent(
      'Awaiting higher review',
    );
    expect(screen.queryByTestId('When to publish')).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('Publish with release'),
    ).not.toBeInTheDocument();

    expect(screen.queryByText('Edit status')).not.toBeInTheDocument();
  });

  test('renders Approved details for publishing immediately', async () => {
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: false,
    });

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

    expect(screen.getByTestId('When to publish')).toHaveTextContent(
      'Immediately',
    );

    expect(
      screen.queryByTestId('Publish with release'),
    ).not.toBeInTheDocument();

    expect(screen.queryByText('Edit status')).not.toBeInTheDocument();
  });

  test('renders Approved details for publishing with release', async () => {
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: false,
    });

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

    expect(screen.getByTestId('When to publish')).toHaveTextContent(
      'With a specific release',
    );

    expect(screen.getByTestId('Publish with release')).toHaveTextContent(
      'Dependant Release',
    );

    expect(screen.queryByText('Edit status')).not.toBeInTheDocument();
  });

  test('renders Edit status button if user can approve methodology', async () => {
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: true,
    });

    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Edit status' }),
      ).toBeInTheDocument();
    });
  });

  test('renders Edit status button if user can mark methodology as draft', async () => {
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: true,
      canMarkHigherLevelReview: false,
      canMarkApproved: false,
    });

    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Edit status' }),
      ).toBeInTheDocument();
    });
  });

  test('renders Edit status button if user can mark methodology for higher review', async () => {
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: true,
      canMarkApproved: false,
    });

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
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: true,
    });

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
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: true,
    });

    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(screen.getByTestId('Owning publication')).toHaveTextContent(
        'Owning publication title',
      );
    });
  });

  test('renders the other publications', async () => {
    methodologyService.getUnpublishedReleases.mockResolvedValue([]);
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: true,
    });

    renderPage(testMethodologyWithOtherPublications);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(screen.getByTestId('Other publications')).toHaveTextContent(
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
    permissionService.getMethodologyApprovalPermissions.mockResolvedValue({
      canMarkDraft: false,
      canMarkHigherLevelReview: false,
      canMarkApproved: true,
    });

    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();

      expect(
        screen.queryByTestId('Other publications'),
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

  test('does not render the methodology status history if has no statuses', async () => {
    methodologyService.getMethodologyStatuses.mockResolvedValue([]);

    renderPage(testMethodology);

    await waitFor(() => {
      expect(screen.getByText('Sign off')).toBeInTheDocument();
    });

    expect(
      screen.queryByText('Methodology status history'),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('methodology-status-history'),
    ).not.toBeInTheDocument();
  });

  test('renders the methodology status history if has statuses', async () => {
    methodologyService.getMethodologyStatuses.mockResolvedValue([
      {
        methodologyStatusId: 'methodology-status-2',
        internalReleaseNote: 'Internal note 2',
        approvalStatus: 'Approved',
        created: '2000-01-02T00:00:00',
        createdByEmail: 'testuser2@email.com',
        methodologyVersion: 0,
      },
      {
        methodologyStatusId: 'methodology-status-1',
        internalReleaseNote: 'Internal note 1',
        approvalStatus: 'HigherLevelReview',
        created: '2000-01-01T00:00:00',
        createdByEmail: 'testuser1@email.com',
        methodologyVersion: 0,
      },
    ]);

    renderPage(testMethodology);

    await waitFor(() => {
      expect(
        screen.queryByText('Methodology status history'),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByTestId('methodology-status-history'),
    ).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(
      within(row1Cells[0]).getByText('2 January 2000 00:00'),
    ).toBeInTheDocument();
    expect(within(row1Cells[1]).getByText('Approved')).toBeInTheDocument();
    expect(
      within(row1Cells[2]).getByText('Internal note 2'),
    ).toBeInTheDocument();
    expect(within(row1Cells[3]).getByText('1')).toBeInTheDocument();
    expect(
      within(row1Cells[4]).getByText('testuser2@email.com'),
    ).toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(
      within(row2Cells[0]).getByText('1 January 2000 00:00'),
    ).toBeInTheDocument();
    expect(
      within(row2Cells[1]).getByText('HigherLevelReview'),
    ).toBeInTheDocument();
    expect(
      within(row2Cells[2]).getByText('Internal note 1'),
    ).toBeInTheDocument();
    expect(within(row2Cells[3]).getByText('1')).toBeInTheDocument();
    expect(
      within(row2Cells[4]).getByText('testuser1@email.com'),
    ).toBeInTheDocument();
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
