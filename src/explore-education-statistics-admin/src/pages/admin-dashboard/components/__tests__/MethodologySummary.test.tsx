import MethodologySummary from '@admin/pages/admin-dashboard/components/MethodologySummary';
import _methodologyService, {
  BasicMethodology,
  MyMethodology
} from '@admin/services/methodologyService';
import {
  ExternalMethodology,
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import { createMemoryHistory } from 'history';
import noop from 'lodash/noop';
import React from 'react';
import { MemoryRouter, Router } from 'react-router';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/methodologyService');

const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;

const createMemoryHistoryWithMockedPush = () => {
  const history = createMemoryHistory();
  return {
    ...history,
    push: jest.fn(),
  };
};

const testContact: PublicationContactDetails = {
  id: 'contact-1',
  contactName: 'John Smith',
  contactTelNo: '0777777777',
  teamEmail: 'john.smith@test.com',
  teamName: 'Team Smith',
};

const testMethodology: MyMethodology = {
  amendment: false,
  live: true,
  id: '1234',
  internalReleaseNote: 'this is the release note',
  previousVersionId: 'lfkjdlfj',
  published: '2021-06-08T09:04:17.9805585',
  slug: 'meth-1',
  status: 'Approved',
  title: 'I am a methodology',
  permissions: {
    canUpdateMethodology: false,
    canDeleteMethodology: false,
    canMakeAmendmentOfMethodology: false,
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
const testMethodologyCanCancelAmend: MyMethodology = {
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
  permissions: {
    canCreateReleases: true,
    canUpdatePublication: true,
  },
};

const testPublicationWithMethodology = {
  ...testPublicationNoMethodology,
  methodology: testMethodology,
};

const testPublicationWithDraftMethodology = {
  ...testPublicationWithMethodology,
  methodology: testDraftMethodology,
};

const testPublicationWithAmendmentMethodology = {
  ...testPublicationWithMethodology,
  methodology: testAmendmentMethodology,
};

const testPublicationWithExternalMethodology = {
  ...testPublicationNoMethodology,
  externalMethodology,
};

const testPublicationWithMethodologyCanAmend = {
  ...testPublicationWithMethodology,
  methodology: testMethodologyCanAmend,
};

const testPublicationWithMethodologyCanCancelAmend = {
  ...testPublicationWithMethodology,
  methodology: testMethodologyCanCancelAmend,
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
          `/publication/${testPublicationNoMethodology.id}/methodology/${testMethodology.id}/summary`,
        );
      });
    });
  });

  describe('renders correctly when no Methodology is supplied', () => {
    test('the create and link methodology buttons are shown', () => {
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
        screen.queryByRole('button', { name: 'Create methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', {
          name: 'Link to an externally hosted methodology',
        }),
      ).toBeInTheDocument();
    });

    test('clicking the link methodology button shows the form', async () => {
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
          screen.queryByText('Link to an externally hosted methodology', {
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

      expect(screen.queryByText(testMethodology.title)).toBeInTheDocument();

      expect(screen.queryByText('8 June 2021')).toBeInTheDocument();

      expect(
        screen.queryByText('this is the release note'),
      ).toBeInTheDocument();

      expect(
        screen.queryByText('View this methodology', { selector: 'a' }),
      ).toBeInTheDocument();
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
        screen.queryByText('Approved', { selector: 'span' }),
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
        screen.queryByText('Draft', { selector: 'span' }),
      ).toBeInTheDocument();
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
        screen.queryByText('Amendment', { selector: 'span' }),
      ).toBeInTheDocument();
    });
  });

  describe('Has an external methodology', () => {
    test('the external methodology link and buttons are shown', () => {
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
        screen.queryByText('Ext methodolology title (external methodology)', {
          selector: 'a',
        }),
      ).toBeInTheDocument();

      expect(screen.getByRole('button', { name: 'Edit' })).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();
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

      expect(
        screen.getByText('Amend methodology', { selector: 'button' }),
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
        screen.queryByText('Amend methodology', { selector: 'button' }),
      ).not.toBeInTheDocument();
    });

    test('shows the confirm modal when click the amend button', async () => {
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
        screen.getByText('Amend methodology', { selector: 'button' }),
      );
      await waitFor(() => {
        expect(
          screen.queryByText('Confirm you want to amend this live methodology'),
        ).toBeInTheDocument();

        expect(
          screen.getByText('Confirm', { selector: 'button' }),
        ).toBeInTheDocument();
      });
    });

    test('calls the service to amend the methodology when the confirm button is clicked', async () => {
      const history = createMemoryHistoryWithMockedPush();
      const mockMethodology: BasicMethodology = {
        amendment: true,
        live: false,
        id: '12345',
        internalReleaseNote: 'this is the release note',
        previousVersionId: 'lfkjdlfj',
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
        screen.getByText('Amend methodology', { selector: 'button' }),
      );
      await waitFor(() => {
        expect(
          screen.getByText('Confirm', { selector: 'button' }),
        ).toBeInTheDocument();
      });

      userEvent.click(screen.getByText('Confirm', { selector: 'button' }));

      await waitFor(() => {
        expect(
          methodologyService.createMethodologyAmendment,
        ).toHaveBeenCalledWith(
          testPublicationWithMethodologyCanAmend.methodology.id,
        );
        expect(history.push).toBeCalledWith(
          `/publication/${testPublicationWithMethodologyCanAmend.id}/methodology/${mockMethodology.id}/summary`,
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

      expect(
        screen.getByText('Cancel amendment', { selector: 'button' }),
      ).toBeInTheDocument();
    });

    test('shows the confirm modal when click the cancel amendment  button', async () => {
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
        screen.getByText('Cancel amendment', { selector: 'button' }),
      );
      await waitFor(() => {
        expect(
          screen.queryByText(
            'Confirm you want to cancel this amended methodology',
          ),
        ).toBeInTheDocument();

        expect(
          screen.getByText('Confirm', { selector: 'button' }),
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
        screen.getByText('Cancel amendment', { selector: 'button' }),
      );
      await waitFor(() => {
        expect(
          screen.getByText('Confirm', { selector: 'button' }),
        ).toBeInTheDocument();

        userEvent.click(screen.getByText('Confirm', { selector: 'button' }));
      });

      await waitFor(() => {
        expect(methodologyService.deleteMethodology).toHaveBeenCalledWith(
          testPublicationWithMethodologyCanAmend.methodology.id,
        );
      });
    });
  });
});
