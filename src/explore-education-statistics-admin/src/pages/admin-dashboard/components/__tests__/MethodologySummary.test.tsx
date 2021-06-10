import MethodologySummary from '@admin/pages/admin-dashboard/components/MethodologySummary';
import {
  ExternalMethodology,
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import { MemoryRouter } from 'react-router';
import userEvent from '@testing-library/user-event';
import { BasicMethodology } from 'src/services/methodologyService';

const testContact: PublicationContactDetails = {
  id: 'contact-1',
  contactName: 'John Smith',
  contactTelNo: '0777777777',
  teamEmail: 'john.smith@test.com',
  teamName: 'Team Smith',
};

const testMethodology: BasicMethodology = {
  amendment: false,
  id: '1234',
  internalReleaseNote: 'this is the release note',
  published: '2021-06-08T09:04:17.9805585',
  slug: 'meth-1',
  status: 'Approved',
  title: 'I am a methodology',
};
const testDraftMethodology: BasicMethodology = {
  ...testMethodology,
  status: 'Draft',
};
const testAmendmentMethodology: BasicMethodology = {
  ...testMethodology,
  amendment: true,
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

const testTopicId = 'topic-id';

describe('MethodologySummary', () => {
  describe('Does not have a methodology', () => {
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
        screen.queryByText('Create methodology', { selector: 'a' }),
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

  describe('Has a methodology', () => {
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
        screen.queryByText('View methodology', { selector: 'a' }),
      ).toBeInTheDocument();
    });

    test('the amend methodology link is shown if user has permission', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            canAmendMethodology
            publication={testPublicationWithMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.queryByText('Amend methodology', { selector: 'a' }),
      ).toBeInTheDocument();
    });

    test('the amend methodology link is not shown if user does not have permission', () => {
      render(
        <MemoryRouter>
          <MethodologySummary
            canAmendMethodology={false}
            publication={testPublicationWithMethodology}
            topicId={testTopicId}
            onChangePublication={noop}
          />
        </MemoryRouter>,
      );

      expect(
        screen.queryByText('Amend methodology', { selector: 'a' }),
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
});
