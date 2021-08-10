import MethodologySummary from '@admin/pages/admin-dashboard/components/MethodologySummary';
import _methodologyService, {
  BasicMethodology,
  MyMethodology,
} from '@admin/services/methodologyService';
import {
  ExternalMethodology,
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import { MemoryRouter, Router } from 'react-router';
import userEvent from '@testing-library/user-event';
import createMemoryHistoryWithMockedPush from '@admin-test/createMemoryHistoryWithMockedPush';

jest.mock('@admin/services/methodologyService');

const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;

const testContact: PublicationContactDetails = {
  id: 'contact-1',
  contactName: 'John Smith',
  contactTelNo: '0777777777',
  teamEmail: 'john.smith@test.com',
  teamName: 'Team Smith',
};

const testMethodology: MyMethodology = {
  amendment: false,
  id: '1234',
  internalReleaseNote: 'this is the release note',
  previousVersionId: 'lfkjdlfj',
  published: '2021-06-08T09:04:17.9805585',
  slug: 'meth-1',
  status: 'Approved',
  title: 'I am a methodology',
  owningPublication: {
    id: 'p1',
    title: 'Publication title',
  },
  permissions: {
    canApproveMethodology: false,
    canUpdateMethodology: false,
    canDeleteMethodology: false,
    canMakeAmendmentOfMethodology: false,
    canMarkMethodologyAsDraft: false,
  },
};
const testDraftMethodology: MyMethodology = {
  ...testMethodology,
  status: 'Draft',
};
const testAmendmentMethodology: MyMethodology = {
  ...testMethodology,
  amendment: true,
};
const testMethodologyCanAmend: MyMethodology = {
  ...testMethodology,
  permissions: {
    ...testMethodology.permissions,
    canMakeAmendmentOfMethodology: true,
  },
};
const testMethodologyCanRemove: MyMethodology = {
  ...testMethodology,
  amendment: false,
  permissions: {
    ...testMethodology.permissions,
    canDeleteMethodology: true,
  },
};
const testMethodologyCanRemoveAmendment: MyMethodology = {
  ...testMethodology,
  amendment: true,
  permissions: {
    ...testMethodology.permissions,
    canDeleteMethodology: true,
  },
};

const externalMethodology: ExternalMethodology = {
  title: 'Ext methodolology title',
  url: 'http:///test.com',
};

const testPublicationNoMethodology: MyPublication = {
  id: 'publication-1',
  title: 'Publication 1',
  contact: testContact,
  releases: [],
  methodologies: [],
  permissions: {
    canCreateReleases: true,
    canUpdatePublication: true,
    canCreateMethodologies: true,
    canManageExternalMethodology: true,
  },
};

const testPublicationWithMethodology = {
  ...testPublicationNoMethodology,
  methodologies: [testMethodology],
};

const testPublicationWithDraftMethodology = {
  ...testPublicationWithMethodology,
  methodologies: [testDraftMethodology],
};

const testPublicationWithAmendmentMethodology = {
  ...testPublicationWithMethodology,
  methodologies: [testAmendmentMethodology],
};

const testPublicationWithExternalMethodology = {
  ...testPublicationNoMethodology,
  externalMethodology,
};
const testPublicationWithMethodologyCanAmend = {
  ...testPublicationWithMethodology,
  methodologies: [testMethodologyCanAmend],
};
const testPublicationWithMethodologyCanCancelAmend = {
  ...testPublicationWithMethodology,
  methodologies: [testMethodologyCanRemoveAmendment],
};
const testPublicationWithMethodologyCanRemove = {
  ...testPublicationWithMethodology,
  methodologies: [testMethodologyCanRemove],
};

const testTopicId = 'topic-id';

describe('MethodologySummary', () => {
  describe('Create Methodology', () => {
    test('clicking Create Methodology creates the Methodology and takes the user to the Methodology summary', async () => {
      methodologyService.createMethodology.mockResolvedValue(testMethodology);

      const history = createMemoryHistoryWithMockedPush();

      render(
        <Router history={history}>
          <MethodologySummary
            publication={testPublicationNoMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </Router>,
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Create methodology' }),
      );

      await waitFor(() => {
        expect(methodologyService.createMethodology).toHaveBeenCalledWith(
          testPublicationNoMethodology.id,
        );
        expect(history.push).toBeCalledWith(
          `/methodology/${testMethodology.id}/summary`,
        );
      });
    });

    test('does not render the Create Methodology button if the user does not have permission to create one', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              permissions: {
                ...testPublicationNoMethodology.permissions,
                canCreateMethodologies: false,
              },
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.queryByRole('button', { name: 'Create methodology' }),
      ).not.toBeInTheDocument();

      expect(
        screen.getByRole('button', {
          name: 'Link to an externally hosted methodology',
        }),
      ).toBeInTheDocument();
    });
  });

  describe('renders correctly when no Methodology is supplied', () => {
    test('the create and link methodology buttons are shown if the user has permission to use them', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationNoMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.queryByTestId('methodology-summary-link'),
      ).not.toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Create methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', {
          name: 'Link to an externally hosted methodology',
        }),
      ).toBeInTheDocument();

      expect(
        screen.queryByText('No methodologies added.'),
      ).not.toBeInTheDocument();
    });

    test('the create and link methodology buttons are not shown if the user does not have permission to use them', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              permissions: {
                ...testPublicationNoMethodology.permissions,
                canCreateMethodologies: false,
                canManageExternalMethodology: false,
              },
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.queryByTestId('methodology-summary-link'),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Create methodology' }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', {
          name: 'Link to an externally hosted methodology',
        }),
      ).not.toBeInTheDocument();

      expect(screen.getByText('No methodologies added.')).toBeInTheDocument();
    });

    test('clicking the link external methodology button shows the form', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationNoMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByRole('button', {
          name: 'Link to an externally hosted methodology',
        }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Link to an externally hosted methodology', {
            selector: 'legend',
          }),
        ).toBeInTheDocument();

        expect(
          screen.queryByText('Create methodology', { selector: 'a' }),
        ).not.toBeInTheDocument();
      });
    });
  });

  describe('renders correctly with a Methodology', () => {
    test('the methodology is shown', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(screen.getByText(testMethodology.title)).toBeInTheDocument();

      expect(screen.getByText('8 June 2021')).toBeInTheDocument();

      expect(screen.getByText('this is the release note')).toBeInTheDocument();

      expect(
        screen.getByText('View this methodology', { selector: 'a' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByText('Edit this methodology'),
      ).not.toBeInTheDocument();
    });

    test('the approved tag is shown', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.getByText('Approved', { selector: 'span' }),
      ).toBeInTheDocument();
    });

    test('the draft tag is shown', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithDraftMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.getByText('Draft', { selector: 'span' }),
      ).toBeInTheDocument();
    });

    test('the edit button is shown when a user can approve the methodology', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  ...testMethodology,
                  permissions: {
                    ...testMethodology.permissions,
                    canApproveMethodology: true,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.getByText('Edit this methodology', { selector: 'a' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByText('View this methodology'),
      ).not.toBeInTheDocument();
    });

    test('the edit button is shown when a user can mark the methodology as draft', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  ...testMethodology,
                  permissions: {
                    ...testMethodology.permissions,
                    canMarkMethodologyAsDraft: true,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.getByText('Edit this methodology', { selector: 'a' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByText('View this methodology'),
      ).not.toBeInTheDocument();
    });

    test('the edit button is shown when a user can update the methodology', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  ...testMethodology,
                  permissions: {
                    ...testMethodology.permissions,
                    canUpdateMethodology: true,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.getByText('Edit this methodology', { selector: 'a' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByText('View this methodology'),
      ).not.toBeInTheDocument();
    });
  });

  describe('renders correctly with an amended Methodology', () => {
    test('the methodology is shown', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithAmendmentMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(screen.getByText(testMethodology.title)).toBeInTheDocument();

      expect(screen.getByText('8 June 2021')).toBeInTheDocument();

      expect(screen.getByText('this is the release note')).toBeInTheDocument();

      expect(
        screen.getByText('View this amendment', { selector: 'a' }),
      ).toBeInTheDocument();

      expect(screen.queryByText('Edit this amendment')).not.toBeInTheDocument();
    });

    test('the amendment tag is shown', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithAmendmentMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.getByText('Amendment', { selector: 'span' }),
      ).toBeInTheDocument();
    });

    test('the edit button is shown when a user can approve the amendment', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  ...testMethodology,
                  amendment: true,
                  permissions: {
                    ...testMethodology.permissions,
                    canApproveMethodology: true,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.getByText('Edit this amendment', { selector: 'a' }),
      ).toBeInTheDocument();

      expect(screen.queryByText('View this amendment')).not.toBeInTheDocument();
    });

    test('the edit button is shown when a user can mark the amendment as draft', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  ...testMethodology,
                  amendment: true,
                  permissions: {
                    ...testMethodology.permissions,
                    canMarkMethodologyAsDraft: true,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.getByText('Edit this amendment', { selector: 'a' }),
      ).toBeInTheDocument();

      expect(screen.queryByText('View this amendment')).not.toBeInTheDocument();
    });

    test('the edit button is shown when a user can update the amendment', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  ...testMethodology,
                  amendment: true,
                  permissions: {
                    ...testMethodology.permissions,
                    canUpdateMethodology: true,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.getByText('Edit this amendment', { selector: 'a' }),
      ).toBeInTheDocument();

      expect(screen.queryByText('View this amendment')).not.toBeInTheDocument();
    });
  });

  describe('Has an external methodology', () => {
    test(
      'renders the external methodology link, and renders the Edit and Remove buttons if the user has ' +
        'permission',
      () => {
        render(
          <MemoryRouter>
            <MethodologySummary
              publication={testPublicationWithExternalMethodology}
              topicId={testTopicId}
              onChangePublication={noop}
            />
          </MemoryRouter>,
        );

        expect(
          screen.getByText('Ext methodolology title (external methodology)', {
            selector: 'a',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', {
            name: 'Edit externally hosted methodology',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', { name: 'Remove' }),
        ).toBeInTheDocument();
      },
    );

    test(
      'renders the external methodology link, but not the Edit or Remove buttons if the user does not have ' +
        'permission',
      () => {
        render(
          <MemoryRouter>
            <MethodologySummary
              publication={{
                ...testPublicationWithExternalMethodology,
                permissions: {
                  ...testPublicationWithExternalMethodology.permissions,
                  canManageExternalMethodology: false,
                },
              }}
              topicId={testTopicId}
              onChangePublication={noop}
            />
          </MemoryRouter>,
        );

        expect(
          screen.getByText('Ext methodolology title (external methodology)'),
        ).toBeInTheDocument();

        expect(
          screen.queryByRole('button', {
            name: 'Edit externally hosted methodology',
          }),
        ).not.toBeInTheDocument();

        expect(
          screen.queryByRole('button', { name: 'Remove' }),
        ).not.toBeInTheDocument();
      },
    );
  });

  describe('Removing a non-amendment methodology', () => {
    test('the remove methodology button is shown if user has permission', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodologyCanRemove}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology'),
      );

      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Remove' }),
        ).toBeInTheDocument();

        expect(
          screen.queryByRole('button', { name: 'Amend methodology' }),
        ).not.toBeInTheDocument();
      });
    });

    test('shows the confirm modal when clicking the Remove button', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodologyCanRemove}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology'),
      );

      userEvent.click(screen.getByRole('button', { name: 'Remove' }));

      await waitFor(() => {
        expect(
          screen.getByText('Confirm you want to remove this methodology'),
        ).toBeInTheDocument();

        expect(
          screen.getByText(
            'By removing this methodology you will lose any changes made.',
          ),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();
      });
    });

    test('calls the service to remove the Methodology when the confirm button is clicked', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodologyCanRemove}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology'),
      );

      userEvent.click(screen.getByRole('button', { name: 'Remove' }));

      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();

        userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
      });

      await waitFor(() => {
        expect(methodologyService.deleteMethodology).toHaveBeenCalledWith(
          testPublicationWithMethodologyCanAmend.methodologies[0].id,
        );
      });
    });
  });

  describe('Amending a methodology', () => {
    test('the amend methodology button is shown if user has permission', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodologyCanAmend}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology'),
      );

      expect(
        screen.getByRole('button', { name: 'Amend methodology' }),
      ).toBeInTheDocument();
    });

    test('the amend methodology button is not shown if user does not have permission', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.queryByRole('button', { name: 'Amend methodology' }),
      ).not.toBeInTheDocument();
    });

    test('shows the confirm modal when clicking the amend button', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodologyCanAmend}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology'),
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Amend methodology' }),
      );
      await waitFor(() => {
        expect(
          screen.getByText('Confirm you want to amend this live methodology'),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();
      });
    });

    test('calls the service to amend the methodology when the confirm button is clicked', async () => {
      const history = createMemoryHistoryWithMockedPush();
      const mockMethodology: BasicMethodology = {
        amendment: true,
        id: '12345',
        internalReleaseNote: 'this is the release note',
        previousVersionId: 'lfkjdlfj',
        owningPublication: {
          id: 'p1',
          title: 'Publication title',
        },
        published: '2021-06-08T09:04:17.9805585',
        slug: 'meth-1',
        status: 'Approved',
        title: 'I am a methodology amendment',
      };
      methodologyService.createMethodologyAmendment.mockImplementation(() =>
        Promise.resolve(mockMethodology),
      );
      render(
        <Router history={history}>
          <MethodologySummary
            publication={testPublicationWithMethodologyCanAmend}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </Router>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology'),
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Amend methodology' }),
      );
      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();
      });

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(
          methodologyService.createMethodologyAmendment,
        ).toHaveBeenCalledWith(
          testPublicationWithMethodologyCanAmend.methodologies[0].id,
        );
        expect(history.push).toBeCalledWith(
          `/methodology/${mockMethodology.id}/summary`,
        );
      });
    });
  });

  describe('Cancelling an amendment', () => {
    test('the cancel amendment button is shown if user has permission', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodologyCanCancelAmend}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology'),
      );

      expect(
        screen.getByRole('button', { name: 'Cancel amendment' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Remove' }),
      ).not.toBeInTheDocument();
    });

    test('shows the confirm modal when clicking the cancel amendment  button', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodologyCanCancelAmend}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology'),
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel amendment' }));
      await waitFor(() => {
        expect(
          screen.getByText(
            'Confirm you want to cancel this amended methodology',
          ),
        ).toBeInTheDocument();

        expect(
          screen.getByText(
            'By cancelling the amendments you will lose any changes made, and the original methodology will remain unchanged.',
          ),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();
      });
    });

    test('calls the service to cancel the amendment when the confirm button is clicked', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithMethodologyCanCancelAmend}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology'),
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel amendment' }));
      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Confirm' }),
        ).toBeInTheDocument();

        userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
      });

      await waitFor(() => {
        expect(methodologyService.deleteMethodology).toHaveBeenCalledWith(
          testPublicationWithMethodologyCanAmend.methodologies[0].id,
        );
      });
    });
  });
});
