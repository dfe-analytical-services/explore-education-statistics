import PublicationAdoptMethodologyPage from '@admin/pages/publication/PublicationAdoptMethodologyPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import _methodologyService, {
  BasicMethodologyVersion,
} from '@admin/services/methodologyService';
import _publicationService, {
  ExternalMethodology,
  MyPublication,
  MyPublicationMethodology,
  PublicationContactDetails,
  UpdatePublicationRequest,
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

describe('PublicationAdoptMethodologyPage', () => {
  const testMethodology1: BasicMethodologyVersion = {
    amendment: false,
    id: 'methodology-v1',
    latestInternalReleaseNote: 'this is the release note',
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
  const testMethodology2: BasicMethodologyVersion = {
    amendment: false,
    id: 'methodology-v2',
    latestInternalReleaseNote: 'this is the release note',
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
  const testMethodology3: BasicMethodologyVersion = {
    amendment: false,
    id: 'methodology-v3',
    latestInternalReleaseNote: 'this is the release note',
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

  const testContact: PublicationContactDetails = {
    id: 'contact-1',
    contactName: 'John Smith',
    contactTelNo: '0777777777',
    teamEmail: 'john.smith@test.com',
    teamName: 'Team Smith',
  };

  const testPublication: MyPublication = {
    id: 'publication-1',
    title: 'Publication 1',
    contact: testContact,
    releases: [],
    legacyReleases: [],
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

  const testMethodologies: BasicMethodologyVersion[] = [
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
});

function renderPage(publication: MyPublication) {
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
