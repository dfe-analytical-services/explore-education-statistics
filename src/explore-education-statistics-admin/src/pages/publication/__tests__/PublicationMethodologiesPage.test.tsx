import PublicationMethodologiesPage from '@admin/pages/publication/PublicationMethodologiesPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import _methodologyService, {
  MethodologyVersion,
  MethodologyVersionSummary,
} from '@admin/services/methodologyService';
import _publicationService, {
  ExternalMethodology,
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import { render, screen, within, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter, Router } from 'react-router-dom';
import { createMemoryHistory } from 'history';
import noop from 'lodash/noop';
import produce from 'immer';

jest.mock('@admin/services/methodologyService');
jest.mock('@admin/services/publicationService');

const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationMethodologiesPage', () => {
  const noMethodologyPermissions = {
    canApproveMethodology: false,
    canUpdateMethodology: false,
    canDeleteMethodology: false,
    canMakeAmendmentOfMethodology: false,
    canMarkMethodologyAsDraft: false,
    canRemoveMethodologyLink: false,
  };
  const testDraftMethodologyPermissions = {
    canApproveMethodology: true,
    canUpdateMethodology: true,
    canDeleteMethodology: true,
    canMakeAmendmentOfMethodology: false,
    canMarkMethodologyAsDraft: true,
    canRemoveMethodologyLink: false,
  };
  const testMethodology1: MethodologyVersionSummary = {
    amendment: false,
    id: 'methodology-v1',
    methodologyId: 'methodology-1',
    owned: true,
    published: '2021-06-08T00:00:00',
    status: 'Approved',
    title: 'Methodology 1',
    permissions: noMethodologyPermissions,
  };
  const testMethodology1Draft = produce(testMethodology1, draft => {
    draft.status = 'Draft';
    draft.permissions = testDraftMethodologyPermissions;
    delete draft.published;
  });
  const testMethodology1Amendment = produce(testMethodology1, draft => {
    draft.amendment = true;
    draft.status = 'Draft';
    draft.permissions = testDraftMethodologyPermissions;
    draft.previousVersionId = 'previous-version-id';
    delete draft.published;
  });

  const testMethodology2: MethodologyVersionSummary = {
    amendment: false,
    id: 'methodology-v2',
    methodologyId: 'methodology-2',
    owned: false,
    published: '2021-06-10T00:00:00',
    status: 'Approved',
    title: 'Methodology 2',
    permissions: noMethodologyPermissions,
  };
  const testMethodology2Draft = produce(testMethodology2, draft => {
    draft.status = 'Draft';
    draft.permissions = testDraftMethodologyPermissions;
    delete draft.published;
  });
  const testMethodology2Amendment = produce(testMethodology2, draft => {
    draft.amendment = true;
    draft.status = 'Draft';
    draft.permissions = testDraftMethodologyPermissions;
    draft.previousVersionId = 'previous-version-id';
    delete draft.published;
  });

  test('renders the methodologies page correctly with methodologies', async () => {
    methodologyService.listMethodologyVersions.mockResolvedValue([
      testMethodology1,
      testMethodology2,
    ]);
    publicationService.getExternalMethodology.mockResolvedValue(undefined);
    renderPage();

    await waitFor(() =>
      expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
    );

    expect(
      screen.getByRole('button', { name: 'Create new methodology' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Methodologies associated to this publication'),
    ).toBeInTheDocument();
    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1Cells[0]).getByText('Methodology 1')).toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2Cells[0]).getByText('Methodology 2')).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Add external methodology' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Adopt an existing methodology' }),
    ).toBeInTheDocument();
  });

  test('renders the methodologies page correctly with no methodologies', async () => {
    methodologyService.listMethodologyVersions.mockResolvedValue([]);
    publicationService.getExternalMethodology.mockResolvedValue(undefined);
    renderPage();

    await waitFor(() =>
      expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
    );

    expect(
      screen.getByText('There are no methodologies for this publication yet.'),
    ).toBeInTheDocument();
  });

  describe('create methodology', () => {
    test('clicking the Create new methodology button creates the Methodology and takes the user to the Methodology summary', async () => {
      const createdMethodology: MethodologyVersion = {
        id: 'methodology-v1',
        amendment: false,
        methodologyId: 'methodology-1',
        title: 'Methodology 1',
        slug: 'methodology-slug-1',
        owningPublication: {
          id: 'p1',
          title: 'Publication title',
        },
        status: 'Draft',
      };

      methodologyService.listMethodologyVersions.mockResolvedValue([]);
      publicationService.getExternalMethodology.mockResolvedValue(undefined);
      methodologyService.createMethodology.mockResolvedValue(
        createdMethodology,
      );

      const history = createMemoryHistory();

      render(
        <Router history={history}>
          <PublicationContextProvider
            publication={testPublication}
            onPublicationChange={noop}
            onReload={noop}
          >
            <PublicationMethodologiesPage />
          </PublicationContextProvider>
        </Router>,
      );

      await waitFor(() =>
        expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Create new methodology' }),
      );

      await waitFor(() => {
        expect(methodologyService.createMethodology).toHaveBeenCalledWith(
          testPublication.id,
        );
        expect(history.location.pathname).toBe(
          `/methodology/${createdMethodology.id}/summary`,
        );
      });
    });

    test('does not render the Create Methodology button if the user does not have permission to create one', async () => {
      methodologyService.listMethodologyVersions.mockResolvedValue([]);
      publicationService.getExternalMethodology.mockResolvedValue(undefined);
      renderPage(
        produce(testPublication, draft => {
          draft.permissions.canCreateMethodologies = false;
        }),
      );

      await waitFor(() =>
        expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
      );

      expect(
        screen.queryByRole('button', { name: 'Create new methodology' }),
      ).not.toBeInTheDocument();
    });
  });

  describe('owned methodology', () => {
    describe('draft', () => {
      test('renders a draft owned methodology correctly', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1Draft,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[0]).getByText('Methodology 1'),
        ).toBeInTheDocument();
        expect(within(row1Cells[1]).getByText('Owned')).toBeInTheDocument();
        expect(within(row1Cells[2]).getByText('Draft')).toBeInTheDocument();
        expect(
          within(row1Cells[3]).getByText('Not yet published'),
        ).toBeInTheDocument();
      });

      test('the view link is shown when a user does not have permission to edit the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1Draft, draft => {
            draft.permissions = noMethodologyPermissions;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('link', {
            name: 'View Methodology 1',
          }),
        ).toBeInTheDocument();

        expect(
          within(row1Cells[4]).queryByRole('link', {
            name: 'Edit Methodology 1',
          }),
        ).not.toBeInTheDocument();
      });

      test('the edit link is shown when a user can approve the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1Draft, draft => {
            draft.permissions.canUpdateMethodology = false;
            draft.permissions.canMarkMethodologyAsDraft = false;
            draft.permissions.canApproveMethodology = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('link', {
            name: 'Edit Methodology 1',
          }),
        ).toBeInTheDocument();
        expect(
          within(row1Cells[4]).queryByRole('link', {
            name: 'View Methodology 1',
          }),
        ).not.toBeInTheDocument();
      });

      test('the edit link is shown when a user can mark the methodology as draft', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1Draft, draft => {
            draft.permissions.canUpdateMethodology = false;
            draft.permissions.canMarkMethodologyAsDraft = true;
            draft.permissions.canApproveMethodology = false;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('link', {
            name: 'Edit Methodology 1',
          }),
        ).toBeInTheDocument();
        expect(
          within(row1Cells[4]).queryByRole('link', {
            name: 'View Methodology 1',
          }),
        ).not.toBeInTheDocument();
      });

      test('the edit link is shown when a user can update the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1Draft, draft => {
            draft.permissions.canUpdateMethodology = true;
            draft.permissions.canMarkMethodologyAsDraft = false;
            draft.permissions.canApproveMethodology = false;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('link', {
            name: 'Edit Methodology 1',
          }),
        ).toBeInTheDocument();
        expect(
          within(row1Cells[4]).queryByRole('link', {
            name: 'View Methodology 1',
          }),
        ).not.toBeInTheDocument();
      });

      test('the delete draft button is shown when a user can delete the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1Draft,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('button', {
            name: 'Delete draft Methodology 1',
          }),
        ).toBeInTheDocument();
      });

      test('the delete draft button is not shown when a user does not have permission to delete the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1Draft, draft => {
            draft.permissions.canDeleteMethodology = false;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).queryByRole('button', {
            name: 'Delete draft Methodology 1',
          }),
        ).not.toBeInTheDocument();
      });
    });

    describe('deleting a draft methodology', () => {
      test('shows the confirm modal when clicking the delete button', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1Draft,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );

        userEvent.click(
          within(row1Cells[4]).getByRole('button', {
            name: 'Delete draft Methodology 1',
          }),
        );

        const modal = within(screen.getByRole('dialog'));

        expect(
          modal.getByText('Confirm you want to delete this draft methodology'),
        ).toBeInTheDocument();

        expect(
          modal.getByText(
            'By deleting this draft methodology you will lose any changes made.',
          ),
        ).toBeInTheDocument();

        expect(
          modal.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();
      });

      test('calls the service to delete the Methodology when the confirm button is clicked', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1Draft,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        userEvent.click(
          within(row1Cells[4]).getByRole('button', {
            name: 'Delete draft Methodology 1',
          }),
        );

        const modal = within(screen.getByRole('dialog'));

        userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

        await waitFor(() => {
          expect(methodologyService.deleteMethodology).toHaveBeenCalledWith(
            testMethodology1Draft.id,
          );
        });
      });
    });

    describe('approved', () => {
      test('renders an approved owned methodology correctly', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[0]).getByText('Methodology 1'),
        ).toBeInTheDocument();
        expect(within(row1Cells[1]).getByText('Owned')).toBeInTheDocument();

        expect(
          within(row1Cells[4]).getByRole('link', {
            name: 'View Methodology 1',
          }),
        ).toBeInTheDocument();
      });

      test('shows the published tag and the date if the methodology has been published', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(within(row1Cells[2]).getByText('Published')).toBeInTheDocument();
        expect(
          within(row1Cells[3]).getByText('8 June 2021'),
        ).toBeInTheDocument();
      });

      test('shows the approved tag if the methodology has not been published', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1, draft => {
            delete draft.published;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(within(row1Cells[2]).getByText('Approved')).toBeInTheDocument();
        expect(
          within(row1Cells[3]).getByText('Not yet published'),
        ).toBeInTheDocument();
      });

      test('the amend methodology button is shown if user has permission', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1, draft => {
            draft.permissions.canMakeAmendmentOfMethodology = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );

        expect(
          within(row1Cells[4]).getByRole('button', {
            name: 'Amend Methodology 1',
          }),
        ).toBeInTheDocument();
      });

      test('the amend methodology button is not shown if user does not have permission', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );

        expect(
          within(row1Cells[4]).queryByRole('button', {
            name: 'Amend Methodology 1',
          }),
        ).not.toBeInTheDocument();
      });
    });

    describe('amendments', () => {
      test('renders an amendment correctly', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1Amendment,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[0]).getByText('Methodology 1'),
        ).toBeInTheDocument();
        expect(within(row1Cells[1]).getByText('Owned')).toBeInTheDocument();
        expect(
          within(row1Cells[2]).getByText('Draft Amendment'),
        ).toBeInTheDocument();
        expect(
          within(row1Cells[3]).getByText('Not yet published'),
        ).toBeInTheDocument();

        expect(
          within(row1Cells[4]).getByRole('link', {
            name: 'View existing version for Methodology 1',
          }),
        ).toHaveAttribute('href', '/methodology/previous-version-id/summary');
      });

      test('the cancel amendment button is shown when a user can delete the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1Amendment, draft => {
            draft.permissions.canDeleteMethodology = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('button', {
            name: 'Cancel amendment for Methodology 1',
          }),
        ).toBeInTheDocument();
      });

      test('the cancel amendment button is not shown when a user does not have permission to delete the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1Amendment, draft => {
            draft.permissions.canDeleteMethodology = false;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).queryByRole('button', {
            name: 'Cancel amendment for Methodology 1',
          }),
        ).not.toBeInTheDocument();
      });
    });

    describe('amending a methodology', () => {
      test('shows the confirm modal when clicking the amend button', async () => {
        renderPage();
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1, draft => {
            draft.permissions.canMakeAmendmentOfMethodology = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        userEvent.click(
          screen.getByRole('button', { name: 'Amend Methodology 1' }),
        );

        const modal = within(screen.getByRole('dialog'));
        expect(
          modal.getByText(
            'Confirm you want to amend this published methodology',
          ),
        ).toBeInTheDocument();

        expect(
          modal.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();
      });

      test('calls the service to amend the methodology when the confirm button is clicked', async () => {
        const history = createMemoryHistory();

        const createdAmendment: MethodologyVersion = {
          amendment: true,
          id: 'methodology-v1',
          methodologyId: 'methodology-1',
          title: 'Methodology 1',
          slug: 'methodology-slug-1',
          owningPublication: {
            id: 'p1',
            title: 'Publication title',
          },
          status: 'Draft',
          previousVersionId: 'methodology-previous-version-1',
        };

        methodologyService.createMethodologyAmendment.mockImplementation(() =>
          Promise.resolve(createdAmendment),
        );
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1, draft => {
            draft.permissions.canMakeAmendmentOfMethodology = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        render(
          <Router history={history}>
            <PublicationContextProvider
              publication={testPublication}
              onPublicationChange={noop}
              onReload={noop}
            >
              <PublicationMethodologiesPage />
            </PublicationContextProvider>
          </Router>,
        );

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        userEvent.click(
          screen.getByRole('button', { name: 'Amend Methodology 1' }),
        );
        const modal = within(screen.getByRole('dialog'));
        userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

        await waitFor(() => {
          expect(
            methodologyService.createMethodologyAmendment,
          ).toHaveBeenCalledWith(testMethodology1.id);
          expect(history.location.pathname).toBe(
            `/methodology/${createdAmendment.id}/summary`,
          );
        });
      });
    });

    describe('cancelling an amendment', () => {
      test('shows the confirm modal when clicking the cancel amendment button', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1Amendment, draft => {
            draft.permissions.canDeleteMethodology = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        userEvent.click(
          screen.getByRole('button', {
            name: 'Cancel amendment for Methodology 1',
          }),
        );

        const modal = within(screen.getByRole('dialog'));
        expect(
          modal.getByText(
            'Confirm you want to cancel this amended methodology',
          ),
        ).toBeInTheDocument();

        expect(
          modal.getByText(
            'By cancelling the amendment you will lose any changes made, and the original methodology will remain unchanged.',
          ),
        ).toBeInTheDocument();

        expect(
          modal.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();
      });

      test('calls the service to cancel the amendment when the confirm button is clicked', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology1Amendment, draft => {
            draft.permissions.canDeleteMethodology = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        userEvent.click(
          screen.getByRole('button', {
            name: 'Cancel amendment for Methodology 1',
          }),
        );

        await waitFor(() => {
          expect(screen.getByText('Confirm')).toBeInTheDocument();
        });

        const modal = within(screen.getByRole('dialog'));

        userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

        await waitFor(() => {
          expect(methodologyService.deleteMethodology).toHaveBeenCalledWith(
            testMethodology1Amendment.id,
          );
        });
      });
    });
  });

  describe('adopted methodology', () => {
    describe('draft', () => {
      test('renders a draft adopted methodology correctly', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology2Draft,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[0]).getByText('Methodology 2'),
        ).toBeInTheDocument();
        expect(within(row1Cells[1]).getByText('Adopted')).toBeInTheDocument();
        expect(within(row1Cells[2]).getByText('Draft')).toBeInTheDocument();
        expect(
          within(row1Cells[3]).getByText('Not yet published'),
        ).toBeInTheDocument();

        expect(
          within(row1Cells[4]).queryByRole('button', {
            name: 'Delete draft Methodology 2',
          }),
        ).not.toBeInTheDocument();
      });

      test('the view link is shown when a user does not have permission to edit the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology2Draft, draft => {
            draft.permissions = noMethodologyPermissions;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('link', {
            name: 'View Methodology 2',
          }),
        ).toBeInTheDocument();

        expect(
          within(row1Cells[4]).queryByRole('link', {
            name: 'Edit Methodology 2',
          }),
        ).not.toBeInTheDocument();
      });

      test('the edit link is shown when a user can approve the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology2Draft, draft => {
            draft.permissions.canUpdateMethodology = false;
            draft.permissions.canMarkMethodologyAsDraft = false;
            draft.permissions.canApproveMethodology = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('link', {
            name: 'Edit Methodology 2',
          }),
        ).toBeInTheDocument();
        expect(
          within(row1Cells[4]).queryByRole('link', {
            name: 'View Methodology 2',
          }),
        ).not.toBeInTheDocument();
      });

      test('the edit link is shown when a user can mark the methodology as draft', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology2Draft, draft => {
            draft.permissions.canUpdateMethodology = false;
            draft.permissions.canMarkMethodologyAsDraft = true;
            draft.permissions.canApproveMethodology = false;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('link', {
            name: 'Edit Methodology 2',
          }),
        ).toBeInTheDocument();
        expect(
          within(row1Cells[4]).queryByRole('link', {
            name: 'View Methodology 2',
          }),
        ).not.toBeInTheDocument();
      });

      test('the edit link is shown when a user can update the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology2Draft, draft => {
            draft.permissions.canUpdateMethodology = true;
            draft.permissions.canMarkMethodologyAsDraft = false;
            draft.permissions.canApproveMethodology = false;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('link', {
            name: 'Edit Methodology 2',
          }),
        ).toBeInTheDocument();
        expect(
          within(row1Cells[4]).queryByRole('link', {
            name: 'View Methodology 2',
          }),
        ).not.toBeInTheDocument();
      });

      test('the remove button is shown when a user can drop the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology2Draft, draft => {
            draft.permissions.canRemoveMethodologyLink = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).getByRole('button', {
            name: 'Remove Methodology 2',
          }),
        ).toBeInTheDocument();
      });

      test('the remove button is not shown when a user does not have permission to drop the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology2Draft, draft => {
            draft.permissions.canRemoveMethodologyLink = false;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[4]).queryByRole('button', {
            name: 'Remove Methodology 2',
          }),
        ).not.toBeInTheDocument();
      });
    });

    describe('approved', () => {
      test('renders an approved adopted methodology correctly', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1,
          testMethodology2,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row2Cells = within(screen.getAllByRole('row')[2]).getAllByRole(
          'cell',
        );
        expect(
          within(row2Cells[0]).getByText('Methodology 2'),
        ).toBeInTheDocument();
        expect(within(row2Cells[1]).getByText('Adopted')).toBeInTheDocument();
        expect(within(row2Cells[2]).getByText('Published')).toBeInTheDocument();
        expect(
          within(row2Cells[3]).getByText('10 June 2021'),
        ).toBeInTheDocument();

        expect(
          within(row2Cells[4]).getByRole('link', {
            name: 'View Methodology 2',
          }),
        ).toBeInTheDocument();

        expect(
          within(row2Cells[4]).queryByRole('button', {
            name: 'Amend Methodology 2',
          }),
        ).not.toBeInTheDocument();
      });

      test('the remove button is shown when a user can drop the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1,
          produce(testMethodology2, draft => {
            draft.permissions.canRemoveMethodologyLink = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row2Cells = within(screen.getAllByRole('row')[2]).getAllByRole(
          'cell',
        );
        expect(
          within(row2Cells[4]).getByRole('button', {
            name: 'Remove Methodology 2',
          }),
        ).toBeInTheDocument();
      });

      test('the remove button is not shown when a user does not have permission to drop the methodology', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology1,
          produce(testMethodology2, draft => {
            draft.permissions.canRemoveMethodologyLink = false;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row2Cells = within(screen.getAllByRole('row')[2]).getAllByRole(
          'cell',
        );
        expect(
          within(row2Cells[4]).queryByRole('button', {
            name: 'Remove Methodology 2',
          }),
        ).not.toBeInTheDocument();
      });
    });

    describe('amendments', () => {
      test('renders an amendment correctly', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          testMethodology2Amendment,
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
          'cell',
        );
        expect(
          within(row1Cells[0]).getByText('Methodology 2'),
        ).toBeInTheDocument();
        expect(within(row1Cells[1]).getByText('Adopted')).toBeInTheDocument();
        expect(
          within(row1Cells[2]).getByText('Draft Amendment'),
        ).toBeInTheDocument();
        expect(
          within(row1Cells[3]).getByText('Not yet published'),
        ).toBeInTheDocument();

        expect(
          within(row1Cells[4]).queryByRole('link', {
            name: 'View existing version for Methodology 2',
          }),
        ).not.toBeInTheDocument();

        expect(
          within(row1Cells[4]).queryByRole('button', {
            name: 'Cancel amendment for Methodology 2',
          }),
        ).not.toBeInTheDocument();
      });
    });

    describe('removing an adopted methodology', () => {
      test('shows the confirm modal when clicking the remove button', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology2, draft => {
            draft.permissions.canRemoveMethodologyLink = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        userEvent.click(
          screen.getByRole('button', {
            name: 'Remove Methodology 2',
          }),
        );

        const modal = within(screen.getByRole('dialog'));
        expect(modal.getByText('Confirm')).toBeInTheDocument();

        expect(
          modal.getByText(
            'Are you sure you want to remove this adopted methodology?',
          ),
        ).toBeInTheDocument();

        expect(
          modal.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();
      });

      test('calls the service to remove the adopted Methodology when the confirm button is clicked', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([
          produce(testMethodology2, draft => {
            draft.permissions.canRemoveMethodologyLink = true;
          }),
        ]);
        publicationService.getExternalMethodology.mockResolvedValue(undefined);
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        userEvent.click(
          screen.getByRole('button', {
            name: 'Remove Methodology 2',
          }),
        );

        const modal = within(screen.getByRole('dialog'));

        userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

        await waitFor(() => {
          expect(publicationService.dropMethodology).toHaveBeenCalledWith(
            testPublication.id,
            testMethodology2.methodologyId,
          );
        });
      });
    });
  });

  describe('external methodology', () => {
    const testExternalMethodology: ExternalMethodology = {
      title: 'External methodolology title',
      url: 'http://test.com',
    };
    test('renders an external methodology correctly', async () => {
      methodologyService.listMethodologyVersions.mockResolvedValue([
        testMethodology1,
        testMethodology2,
      ]);
      publicationService.getExternalMethodology.mockResolvedValue(
        testExternalMethodology,
      );
      renderPage();

      await waitFor(() =>
        expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
      );

      const row3Cells = within(screen.getAllByRole('row')[3]).getAllByRole(
        'cell',
      );
      expect(
        within(row3Cells[0]).getByText('External methodolology title'),
      ).toBeInTheDocument();
      expect(within(row3Cells[1]).getByText('External')).toBeInTheDocument();
      expect(
        within(row3Cells[4]).getByRole('link', {
          name: 'View External methodolology title',
        }),
      ).toHaveAttribute('href', 'http://test.com');
    });

    test('renders the table when there is only an external methodology', async () => {
      methodologyService.listMethodologyVersions.mockResolvedValue([]);
      publicationService.getExternalMethodology.mockResolvedValue(
        testExternalMethodology,
      );
      renderPage();

      await waitFor(() =>
        expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
      );

      expect(screen.getByRole('table')).toBeInTheDocument();

      const row1Cells = within(screen.getAllByRole('row')[1]).getAllByRole(
        'cell',
      );
      expect(
        within(row1Cells[0]).getByText('External methodolology title'),
      ).toBeInTheDocument();
      expect(within(row1Cells[1]).getByText('External')).toBeInTheDocument();
      expect(
        within(row1Cells[4]).getByRole('link', {
          name: 'View External methodolology title',
        }),
      ).toHaveAttribute('href', 'http://test.com');
    });

    test('shows the edit and delete buttons if have permission to manage external methodologies', async () => {
      methodologyService.listMethodologyVersions.mockResolvedValue([
        testMethodology1,
        testMethodology2,
      ]);
      publicationService.getExternalMethodology.mockResolvedValue(
        testExternalMethodology,
      );
      renderPage();

      await waitFor(() =>
        expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
      );

      const row3Cells = within(screen.getAllByRole('row')[3]).getAllByRole(
        'cell',
      );

      expect(
        within(row3Cells[4]).getByRole('link', {
          name: 'Edit External methodolology title',
        }),
      ).toBeInTheDocument();

      expect(
        within(row3Cells[4]).getByRole('button', {
          name: 'Remove External methodolology title',
        }),
      ).toBeInTheDocument();
    });

    test('does not show the edit and delete buttons if do not have permission to manage external methodologies', async () => {
      methodologyService.listMethodologyVersions.mockResolvedValue([
        testMethodology1,
        testMethodology2,
      ]);
      publicationService.getExternalMethodology.mockResolvedValue(
        testExternalMethodology,
      );
      renderPage({
        ...testPublication,
        permissions: {
          ...testPublication.permissions,
          canManageExternalMethodology: false,
        },
      });

      await waitFor(() =>
        expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
      );

      const row3Cells = within(screen.getAllByRole('row')[3]).getAllByRole(
        'cell',
      );

      expect(
        within(row3Cells[4]).queryByRole('link', {
          name: 'Edit External methodolology title',
        }),
      ).not.toBeInTheDocument();

      expect(
        within(row3Cells[4]).queryByRole('button', {
          name: 'Remove External methodolology title',
        }),
      ).not.toBeInTheDocument();
    });

    describe('removing an external methodology', () => {
      test('shows the confirm modal when clicking the remove button', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([]);
        publicationService.getExternalMethodology.mockResolvedValue(
          testExternalMethodology,
        );
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        userEvent.click(
          screen.getByRole('button', {
            name: 'Remove External methodolology title',
          }),
        );

        const modal = within(screen.getByRole('dialog'));

        expect(
          modal.getByText('Remove external methodology', {
            selector: 'h2',
          }),
        ).toBeInTheDocument();

        expect(
          modal.getByText(
            'Are you sure you want to remove this external methodology?',
          ),
        ).toBeInTheDocument();

        expect(
          modal.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();

        expect(
          modal.getByRole('button', { name: 'Cancel' }),
        ).toBeInTheDocument();
      });

      test('calls the service to delete the Methodology when the confirm button is clicked', async () => {
        methodologyService.listMethodologyVersions.mockResolvedValue([]);
        publicationService.getExternalMethodology.mockResolvedValue(
          testExternalMethodology,
        );
        renderPage();

        await waitFor(() =>
          expect(screen.getByText('Manage methodologies')).toBeInTheDocument(),
        );

        userEvent.click(
          screen.getByRole('button', {
            name: 'Remove External methodolology title',
          }),
        );

        const modal = within(screen.getByRole('dialog'));

        userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

        await waitFor(() => {
          expect(
            publicationService.removeExternalMethodology,
          ).toHaveBeenCalledWith<
            Parameters<typeof publicationService.removeExternalMethodology>
          >(testPublication.id);
        });
      });
    });
  });
});

function renderPage(publication: PublicationWithPermissions = testPublication) {
  render(
    <MemoryRouter>
      <PublicationContextProvider
        publication={publication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <PublicationMethodologiesPage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
