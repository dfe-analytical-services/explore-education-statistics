import MethodologySummary from '@admin/pages/admin-dashboard/components/MethodologySummary';
import _methodologyService, {
  BasicMethodologyVersion,
  MethodologyVersion,
} from '@admin/services/methodologyService';
import _publicationService, {
  ExternalMethodology,
  MyPublication,
  PublicationContactDetails,
  UpdatePublicationRequest,
} from '@admin/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import { MemoryRouter, Router } from 'react-router';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
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

const testMethodology: MethodologyVersion = {
  amendment: false,
  id: 'methodology-v1',
  internalReleaseNote: 'this is the release note',
  methodologyId: 'methodology-1',
  previousVersionId: 'methodology-previous-version-1',
  owned: true,
  published: '2021-06-08T09:04:17',
  status: 'Approved',
  title: 'I am a methodology',
  permissions: {
    canApproveMethodology: false,
    canUpdateMethodology: false,
    canDeleteMethodology: false,
    canMakeAmendmentOfMethodology: false,
    canMarkMethodologyAsDraft: false,
    canRemoveMethodologyLink: false,
  },
};
const testMethodology2: MethodologyVersion = {
  amendment: false,
  id: 'methodology-v2',
  internalReleaseNote: 'this is another release note',
  methodologyId: 'methodology-2',
  previousVersionId: 'methodology-previous-version-2',
  owned: true,
  published: '2021-06-10T09:04:17',
  status: 'Approved',
  title: 'I am a methodology 2',
  permissions: {
    canApproveMethodology: false,
    canUpdateMethodology: false,
    canDeleteMethodology: false,
    canMakeAmendmentOfMethodology: false,
    canMarkMethodologyAsDraft: false,
    canRemoveMethodologyLink: false,
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
const testAdoptedMethodology = produce(testMethodology, draft => {
  draft.owned = false;
  draft.permissions.canRemoveMethodologyLink = true;
});
const testAdoptedMethodology2 = produce(testMethodology2, draft => {
  draft.owned = false;
});

const externalMethodology: ExternalMethodology = {
  title: 'Ext methodolology title',
  url: 'http:///test.com',
};

const testPublicationNoMethodology: MyPublication = {
  id: 'publication-3',
  title: 'Publication 3',
  summary: 'Publication 3 summary',
  contact: testContact,
  releases: [],
  methodologies: [],
  themeId: 'theme-1',
  topicId: 'topic-1',
  permissions: {
    canAdoptMethodologies: true,
    canCreateReleases: true,
    canUpdatePublication: true,
    canUpdatePublicationTitle: true,
    canUpdatePublicationSupersededBy: true,
    canCreateMethodologies: true,
    canManageExternalMethodology: true,
  },
};
const testPublicationWithMethodology = produce(
  testPublicationNoMethodology,
  draft => {
    draft.methodologies = [testMethodology];
  },
);
const testPublicationWithDraftMethodology = produce(
  testPublicationNoMethodology,
  draft => {
    draft.methodologies = [testDraftMethodology];
  },
);
const testPublicationWithAmendmentMethodology = produce(
  testPublicationNoMethodology,
  draft => {
    draft.methodologies = [testMethodologyCanRemoveAmendment];
  },
);
const testPublicationWithAdoptedMethodologies = produce(
  testPublicationNoMethodology,
  draft => {
    draft.methodologies = [testAdoptedMethodology, testAdoptedMethodology2];
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
    draft.methodologies = [testMethodologyCanAmend];
  },
);
const testPublicationWithMethodologyCanCancelAmend = produce(
  testPublicationWithMethodology,
  draft => {
    draft.methodologies = [testMethodologyCanRemoveAmendment];
  },
);
const testPublicationWithMethodologyCanRemove = produce(
  testPublicationWithMethodology,
  draft => {
    draft.methodologies = [testMethodologyCanRemove];
  },
);

const testTopicId = 'topic-id';

describe('MethodologySummary', () => {
  describe('Create Methodology', () => {
    test('clicking Create Methodology creates the Methodology and takes the user to the Methodology summary', async () => {
      const createdMethodology: BasicMethodologyVersion = {
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
      methodologyService.createMethodology.mockResolvedValue(
        createdMethodology,
      );

      const history = createMemoryHistory();

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
        expect(history.location.pathname).toBe(
          `/methodology/${createdMethodology.id}/summary`,
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
          name: 'Use an external methodology',
        }),
      ).toBeInTheDocument();
    });
  });

  describe('renders correctly when no Methodology is supplied', () => {
    test('the no methodologies warning is shown', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationNoMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(screen.getByText('No methodologies added')).toBeInTheDocument();
    });

    test('the correct buttons are shown if the user has permission to use them', () => {
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
        screen.getByRole('button', { name: 'Create methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'Adopt an existing methodology',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'Use an external methodology',
        }),
      ).toBeInTheDocument();
    });

    test('the methodology buttons are not shown if the user does not have permission to use them', () => {
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
        screen.queryByRole('button', { name: 'Create methodology' }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('link', {
          name: 'Use an external methodology',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('link', {
          name: 'Adopt an existing methodology',
        }),
      ).not.toBeInTheDocument();
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
        screen.getByRole('link', { name: 'View methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'Edit methodology' }),
      ).not.toBeInTheDocument();
    });

    test('the published tag is shown', () => {
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
        screen.getByText('Published', { selector: 'span' }),
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

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View methodology' }),
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

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View methodology' }),
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

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View methodology' }),
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
        screen.getByRole('link', { name: 'View amendment' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'Edit amendment' }),
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

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit amendment' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View amendment' }),
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

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit amendment' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View amendment' }),
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

      userEvent.click(
        screen.getByTestId('Expand Details Section I am a methodology (Owned)'),
      );

      expect(
        screen.getByRole('link', { name: 'Edit amendment' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('link', { name: 'View amendment' }),
      ).not.toBeInTheDocument();
    });
  });

  describe('External methodologies', () => {
    test('clicking the link to external methodology button takes the user to the page', async () => {
      const history = createMemoryHistory();

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
          name: 'Use an external methodology',
        }),
      );

      await waitFor(() => {
        expect(history.location.pathname).toBe(
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
            name: 'Edit external methodology',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', { name: 'Remove external methodology' }),
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
            name: 'Edit external methodology',
          }),
        ).not.toBeInTheDocument();

        expect(
          screen.queryByRole('button', { name: 'Remove external methodology' }),
        ).not.toBeInTheDocument();
      },
    );
  });

  describe('Removing an external methodology', () => {
    test('shows the confirm modal when clicking the Remove button', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithExternalMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId(
          'Expand Details Section Ext methodolology title (External)',
        ),
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Remove external methodology' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Remove external methodology', { selector: 'h2' }),
        ).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));

      expect(
        modal.getByText(
          'Are you sure you want to remove this external methodology?',
        ),
      ).toBeInTheDocument();

      expect(
        modal.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();

      expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
    });

    test('calls the service to remove the Methodology when the confirm button is clicked', async () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            publication={testPublicationWithExternalMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      userEvent.click(
        screen.getByTestId(
          'Expand Details Section Ext methodolology title (External)',
        ),
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Remove external methodology' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Remove external methodology', { selector: 'h2' }),
        ).toBeInTheDocument();
      });

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      const updatedPublication: UpdatePublicationRequest = {
        title: testPublicationWithExternalMethodology.title,
        summary: testPublicationWithExternalMethodology.summary,
        contact: testContact,
        topicId: testTopicId,
      };

      await waitFor(() => {
        expect(publicationService.updatePublication).toHaveBeenCalledWith<
          Parameters<typeof publicationService.updatePublication>
        >(testPublicationWithExternalMethodology.id, updatedPublication);
      });
    });
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
        expect(screen.getByText('Remove')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Amend methodology' }),
      ).not.toBeInTheDocument();
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
      });

      expect(
        screen.getByText(
          'By removing this methodology you will lose any changes made.',
        ),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
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
        expect(screen.getByText('Confirm')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(methodologyService.deleteMethodology).toHaveBeenCalledWith(
          testPublicationWithMethodologyCanRemove.methodologies[0].id,
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
      });

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
    });

    test('calls the service to amend the methodology when the confirm button is clicked', async () => {
      const history = createMemoryHistory();

      const mockMethodology: BasicMethodologyVersion = {
        amendment: true,
        id: 'methodology-v1',
        latestInternalReleaseNote: 'this is the release note',
        methodologyId: 'methodology-1',
        previousVersionId: 'methodology-previous-version-1',
        owningPublication: {
          id: 'owning-publication-1',
          title: 'Owning publication title',
        },
        published: '2021-06-08T09:04:17',
        slug: 'methodology-slug-1',
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
        expect(screen.getByText('Confirm')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(
          methodologyService.createMethodologyAmendment,
        ).toHaveBeenCalledWith(
          testPublicationWithMethodologyCanAmend.methodologies[0].id,
        );
        expect(history.location.pathname).toBe(
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
      });

      expect(
        screen.getByText(
          'By cancelling the amendments you will lose any changes made, and the original methodology will remain unchanged.',
        ),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
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
        expect(screen.getByText('Confirm')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(methodologyService.deleteMethodology).toHaveBeenCalledWith(
          testPublicationWithMethodologyCanCancelAmend.methodologies[0].id,
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
          testPublicationWithAdoptedMethodologies.methodologies[0]
            .methodologyId,
        );
      });
    });
  });
});
