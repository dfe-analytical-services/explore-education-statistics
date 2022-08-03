import TopicPublications from '@admin/pages/admin-dashboard/components/TopicPublications';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import _permissionService from '@admin/services/permissionService';
import _publicationService, {
  MyPublication,
} from '@admin/services/publicationService';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';

jest.mock('@admin/services/permissionService');
jest.mock('@admin/services/publicationService');
jest.mock('@admin/services/themeService');
jest.mock('@common/services/storageService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;

describe('TopicPublications', () => {
  const testTopic = {
    id: 'topic-1',
    title: 'Topic 1',
    slug: 'topic-1',
    themeId: 'theme-1',
  };
  const testThemeTitle = 'Theme 1';

  const testPublications: MyPublication[] = [
    testPublication,
    { ...testPublication, id: 'publication-2', title: 'Publication 2' },
  ];

  test('renders correctly with list of publications', async () => {
    publicationService.getMyPublicationsByTopic.mockResolvedValue(
      testPublications,
    );
    permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

    render(
      <MemoryRouter>
        <TopicPublications themeTitle={testThemeTitle} topic={testTopic} />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Theme 1 / Topic 1')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Publication 1' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Publication 2' }),
    ).toBeInTheDocument();
  });

  test('renders correctly when no publications are available', async () => {
    publicationService.getMyPublicationsByTopic.mockResolvedValue([]);
    permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

    render(
      <MemoryRouter>
        <TopicPublications themeTitle={testThemeTitle} topic={testTopic} />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Theme 1 / Topic 1')).toBeInTheDocument();
    });

    expect(screen.getByText('No publications available')).toBeInTheDocument();
  });

  test("renders 'Create new publication' button if authorised", async () => {
    publicationService.getMyPublicationsByTopic.mockResolvedValue(
      testPublications,
    );
    permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

    render(
      <MemoryRouter>
        <TopicPublications themeTitle={testThemeTitle} topic={testTopic} />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Theme 1 / Topic 1')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Create new publication' }),
    ).toBeInTheDocument();
  });

  test("does not render 'Create new publication' button if unauthorised", async () => {
    publicationService.getMyPublicationsByTopic.mockResolvedValue(
      testPublications,
    );
    permissionService.canCreatePublicationForTopic.mockResolvedValue(false);

    render(
      <MemoryRouter>
        <TopicPublications themeTitle={testThemeTitle} topic={testTopic} />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Theme 1 / Topic 1')).toBeInTheDocument();
    });

    expect(
      screen.queryByRole('link', { name: 'Create new publication' }),
    ).not.toBeInTheDocument();
  });
});
