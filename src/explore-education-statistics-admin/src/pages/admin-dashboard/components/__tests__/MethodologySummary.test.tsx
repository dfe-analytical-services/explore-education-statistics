import MethodologySummary from '@admin/pages/admin-dashboard/components/MethodologySummary';
import _methodologyService, {
  BasicMethodology,
  MyMethodology,
} from '@admin/services/methodologyService';
import _publicationService, {
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
import produce from 'immer';

jest.mock('@admin/services/methodologyService');
jest.mock('@admin/services/publicationService');

const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
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
  latestInternalReleaseNote: 'this is the release note',
  methodologyId: 'm-1',
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
const testMethodology2: MyMethodology = {
  amendment: false,
  id: '4321',
  latestInternalReleaseNote: 'this is another release note',
  methodologyId: 'm-2',
  previousVersionId: '9876',
  published: '2021-06-10T09:04:17.9805585',
  slug: 'meth-2',
  status: 'Approved',
  title: 'I am a methodology 2',
  owningPublication: {
    id: 'p2',
    title: 'Publication title 2',
  },
  permissions: {
    canApproveMethodology: false,
    canUpdateMethodology: false,
    canDeleteMethodology: false,
    canMakeAmendmentOfMethodology: false,
    canMarkMethodologyAsDraft: false,
  },
};
const testDraftMethodology = produce(testMethodology, draft => {
  draft.status = 'Draft';
});
const testAmendmentMethodology = produce(testMethodology, draft => {
  draft.amendment = true;
});
const testMethodologyCanAmend = produce(testMethodology, draft => {
  draft.permissions.canMakeAmendmentOfMethodology = true;
});
const testMethodologyCanRemove = produce(testMethodology, draft => {
  draft.amendment = false;
  draft.permissions.canDeleteMethodology = true;
});
const testMethodologyCanRemoveAmendment = produce(testMethodology, draft => {
  draft.amendment = true;
  draft.permissions.canDeleteMethodology = true;
});

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
    canAdoptMethodologies: true,
    canCreateReleases: true,
    canUpdatePublication: true,
    canCreateMethodologies: true,
    canManageExternalMethodology: true,
  },
};
const testPublicationWithMethodology = produce(
  testPublicationNoMethodology,
  draft => {
    draft.methodologies = [
      {
        owner: true,
        permissions: { canDropMethodology: false },
        methodology: testMethodology,
      },
    ];
  },
);
const testPublicationWithDraftMethodology = produce(
  testPublicationNoMethodology,
  draft => {
    draft.methodologies = [
      {
        owner: true,
        permissions: { canDropMethodology: false },
        methodology: testDraftMethodology,
      },
    ];
  },
);
const testPublicationWithAmendmentMethodology = produce(
  testPublicationNoMethodology,
  draft => {
    draft.methodologies = [
      {
        owner: true,
        permissions: { canDropMethodology: false },
        methodology: testAmendmentMethodology,
      },
    ];
  },
);
const testPublicationWithAdoptedMethodologies = produce(
  testPublicationNoMethodology,
  draft => {
    draft.methodologies = [
      {
        owner: false,
        permissions: { canDropMethodology: true },
        methodology: testMethodology,
      },
      {
        owner: false,
        permissions: { canDropMethodology: false },
        methodology: testMethodology2,
      },
    ];
  },
);
const testPublicationWithExternalMethodology = produce(
  testPublicationNoMethodology,
  draft => {
    draft.externalMethodology = externalMethodology;
  },
);
const testPublicationWithMethodologyCanAmend = produce(
  testPublicationWithMethodology,
  draft => {
    draft.methodologies = [
      {
        owner: true,
        permissions: { canDropMethodology: false },
        methodology: testMethodologyCanAmend,
      },
    ];
  },
);
const testPublicationWithMethodologyCanCancelAmend = produce(
  testPublicationWithMethodology,
  draft => {
    draft.methodologies = [
      {
        owner: true,
        permissions: { canDropMethodology: false },
        methodology: testMethodologyCanRemoveAmendment,
      },
    ];
  },
);
const testPublicationWithMethodologyCanRemove = produce(
  testPublicationWithMethodology,
  draft => {
    draft.methodologies = [
      {
        owner: true,
        permissions: { canDropMethodology: false },
        methodology: testMethodologyCanRemove,
      },
    ];
  },
);

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
        screen.getByRole('link', {
          name: 'Link to an externally hosted methodology',
        }),
      ).toBeInTheDocument();
    });
  });

  describe('renders correctly when no Methodology is supplied', () => {
    test('the create, adopt and link methodology buttons are shown if the user has permission to use them', () => {
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
        screen.getByRole('link', {
          name: 'Adopt a methodology',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'Link to an externally hosted methodology',
        }),
      ).toBeInTheDocument();

      expect(
        screen.queryByText('No methodologies added.'),
      ).not.toBeInTheDocument();
    });

    test('the create, adopt and link methodology buttons are not shown if the user does not have permission to use them', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              permissions: {
                ...testPublicationNoMethodology.permissions,
                canCreateMethodologies: false,
                canManageExternalMethodology: false,
                canAdoptMethodologies: false,
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
        screen.queryByRole('link', {
          name: 'Link to an externally hosted methodology',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('link', {
          name: 'Adopt a methodology',
        }),
      ).not.toBeInTheDocument();

      expect(screen.getByText('No methodologies added.')).toBeInTheDocument();
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

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByText(`${testMethodology.title} (Owned)`),
      ).toBeInTheDocument();

      expect(screen.getByText('8 June 2021')).toBeInTheDocument();

      expect(screen.getByText('this is the release note')).toBeInTheDocument();

      expect(
        screen.getByRole('link', { name: 'View this methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'Edit this methodology' }),
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

    test('the edit link is shown when a user can approve the methodology', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  methodology: {
                    ...testMethodology,
                    permissions: {
                      ...testMethodology.permissions,
                      canApproveMethodology: true,
                    },
                  },
                  owner: true,
                  permissions: {
                    canDropMethodology: false,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit this methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View this methodology' }),
      ).not.toBeInTheDocument();
    });

    test('the edit link is shown when a user can mark the methodology as draft', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  methodology: {
                    ...testMethodology,
                    permissions: {
                      ...testMethodology.permissions,
                      canMarkMethodologyAsDraft: true,
                    },
                  },
                  owner: true,
                  permissions: {
                    canDropMethodology: false,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit this methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View this methodology' }),
      ).not.toBeInTheDocument();
    });

    test('the edit link is shown when a user can update the methodology', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  methodology: {
                    ...testMethodology,
                    permissions: {
                      ...testMethodology.permissions,
                      canUpdateMethodology: true,
                    },
                  },
                  owner: true,
                  permissions: {
                    canDropMethodology: false,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit this methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View this methodology' }),
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

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByText(`${testMethodology.title} (Owned)`),
      ).toBeInTheDocument();

      expect(screen.getByText('8 June 2021')).toBeInTheDocument();

      expect(screen.getByText('this is the release note')).toBeInTheDocument();

      expect(
        screen.getByRole('link', { name: 'View this amendment' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'Edit this amendment' }),
      ).not.toBeInTheDocument();
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

    test('the edit link is shown when a user can approve the amendment', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  methodology: {
                    ...testMethodology,
                    amendment: true,
                    permissions: {
                      ...testMethodology.permissions,
                      canApproveMethodology: true,
                    },
                  },
                  owner: true,
                  permissions: {
                    canDropMethodology: false,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit this amendment' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View this amendment' }),
      ).not.toBeInTheDocument();
    });

    test('the edit link is shown when a user can mark the amendment as draft', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  methodology: {
                    ...testMethodology,
                    amendment: true,
                    permissions: {
                      ...testMethodology.permissions,
                      canMarkMethodologyAsDraft: true,
                    },
                  },
                  owner: true,
                  permissions: {
                    canDropMethodology: false,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit this amendment' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View this amendment' }),
      ).not.toBeInTheDocument();
    });

    test('the edit link is shown when a user can update the amendment', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={{
              ...testPublicationNoMethodology,
              methodologies: [
                {
                  methodology: {
                    ...testMethodology,
                    amendment: true,
                    permissions: {
                      ...testMethodology.permissions,
                      canUpdateMethodology: true,
                    },
                  },
                  owner: true,
                  permissions: {
                    canDropMethodology: false,
                  },
                },
              ],
            }}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit this amendment' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View this amendment' }),
      ).not.toBeInTheDocument();
    });
  });

  describe('External methodologies', () => {
    test('clicking the link to external methodology button takes the user to the page', async () => {
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
        screen.getByRole('link', {
          name: 'Link to an externally hosted methodology',
        }),
      );

      await waitFor(() => {
        expect(history.push).toBeCalledWith(
          `/publication/${testPublicationNoMethodology.id}/external-methodology`,
        );
      });
    });
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
          screen.getByText('Ext methodolology title (External)'),
        ).toBeInTheDocument();

        userEvent.click(
          screen.getByTestId(
            'Expand Details Section Ext methodolology title (External)',
          ),
        );

        expect(
          screen.getByRole('link', {
            name: 'Edit',
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
          screen.getByText('Ext methodolology title (External)'),
        ).toBeInTheDocument();

        userEvent.click(
          screen.getByTestId(
            'Expand Details Section Ext methodolology title (External)',
          ),
        );

        expect(
          screen.queryByRole('link', {
            name: 'Edit',
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
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
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
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
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
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
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
          testPublicationWithMethodologyCanAmend.methodologies[0].methodology
            .id,
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
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
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
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
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
        latestInternalReleaseNote: 'this is the release note',
        methodologyId: 'm-1',
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
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
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
          testPublicationWithMethodologyCanAmend.methodologies[0].methodology
            .id,
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
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
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
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
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
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
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
          testPublicationWithMethodologyCanAmend.methodologies[0].methodology
            .id,
        );
      });
    });
  });

  describe('Adopted methodologies', () => {
    test('renders adopted methodologies correctly', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithAdoptedMethodologies}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.getByText('I am a methodology (Adopted)'),
      ).toBeInTheDocument();
      userEvent.click(
        screen.getByTestId(
          'Expand Details Section I am a methodology (Adopted)',
        ),
      );
      expect(screen.getByText('8 June 2021')).toBeInTheDocument();
      expect(screen.getByText('this is the release note')).toBeInTheDocument();

      expect(
        screen.getByText('I am a methodology 2 (Adopted)'),
      ).toBeInTheDocument();
      userEvent.click(
        screen.getByTestId(
          'Expand Details Section I am a methodology (Adopted)',
        ),
      );
      expect(screen.getByText('10 June 2021')).toBeInTheDocument();
      expect(
        screen.getByText('this is another release note'),
      ).toBeInTheDocument();
    });

    test('the remove methodology button is only displayed if user has permission', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithAdoptedMethodologies}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId(
          'Expand Details Section I am a methodology 2 (Adopted)',
        ),
      );
      userEvent.click(
        screen.getByTestId(
          'Expand Details Section I am a methodology (Adopted)',
        ),
      );

      expect(
        screen.getAllByRole('button', { name: 'Remove methodology' }).length,
      ).toBe(1);
    });

    test('shows the confirm modal when clicking the remove button', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithAdoptedMethodologies}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId(
          'Expand Details Section I am a methodology (Adopted)',
        ),
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Remove methodology' }),
      );

      await waitFor(() => {
        expect(screen.getByText('Confirm')).toBeInTheDocument();
      });
      expect(
        screen.getByText(
          'Are you sure you want to remove this adopted methodology?',
        ),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
    });

    test('calls the service to remove the adopted Methodology when the confirm button is clicked', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithAdoptedMethodologies}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId(
          'Expand Details Section I am a methodology (Adopted)',
        ),
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Remove methodology' }),
      );

      await waitFor(() => {
        expect(screen.getByText('Confirm')).toBeInTheDocument();
      });

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(publicationService.dropMethodology).toHaveBeenCalledWith(
          testPublicationWithAdoptedMethodologies.id,
          testPublicationWithAdoptedMethodologies.methodologies[0].methodology
            .methodologyId,
        );
      });
    });
  });
});
