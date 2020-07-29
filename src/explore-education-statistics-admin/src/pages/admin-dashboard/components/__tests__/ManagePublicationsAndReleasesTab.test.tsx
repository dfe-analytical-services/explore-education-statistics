import ManagePublicationsAndReleasesTab from '@admin/pages/admin-dashboard/components/ManagePublicationsAndReleasesTab';
import { ContactDetails } from '@admin/services/contactService';
import _dashboardService, {
  AdminDashboardPublication,
  Theme,
} from '@admin/services/dashboardService';
import _permissionService from '@admin/services/permissionService';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import { createMemoryHistory } from 'history';
import React from 'react';
import { MemoryRouter, Router } from 'react-router';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/dashboardService');
jest.mock('@admin/services/permissionService');

const dashboardService = _dashboardService as jest.Mocked<
  typeof _dashboardService
>;
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;

describe('ManagePublicationsAndReleasesTab', () => {
  const testThemeTopics: Theme[] = [
    {
      id: 'theme-1',
      title: 'Theme 1',
      topics: [
        { id: 'topic-1', title: 'Topic 1' },
        { id: 'topic-2', title: 'Topic 2' },
      ],
    },
    {
      id: 'theme-2',
      title: 'Theme 2',
      topics: [
        { id: 'topic-3', title: 'Topic 3' },
        { id: 'topic-4', title: 'Topic 4' },
      ],
    },
  ];

  const testContact: ContactDetails = {
    id: 'contact-1',
    contactName: 'John Smith',
    contactTelNo: '0777777777',
    teamEmail: 'john.smith@test.com',
    teamName: 'Team Smith',
  };

  const testPublications: AdminDashboardPublication[] = [
    {
      id: 'publication-1',
      title: 'Publication 1',
      contact: testContact,
      releases: [],
      permissions: {
        canCreateReleases: true,
        canUpdatePublication: true,
      },
    },
    {
      id: 'publication-2',
      title: 'Publication 2',
      contact: testContact,
      releases: [],
      permissions: {
        canCreateReleases: true,
        canUpdatePublication: true,
      },
    },
  ];

  test('renders with first theme and topic selected by default', async () => {
    dashboardService.getMyThemesAndTopics.mockImplementation(
      async () => testThemeTopics,
    );
    dashboardService.getMyPublicationsByTopic.mockImplementation(
      async topicId => {
        expect(topicId).toBe('topic-1');
        return testPublications;
      },
    );
    permissionService.canCreatePublicationForTopic.mockImplementation(
      async () => true,
    );

    render(
      <MemoryRouter>
        <ManagePublicationsAndReleasesTab />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-1');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-1');

      expect(screen.getByTestId('selectedThemeTitle')).toHaveTextContent(
        'Theme 1',
      );
      expect(screen.getByTestId('selectedTopicTitle')).toHaveTextContent(
        'Topic 1',
      );

      expect(
        screen.getByRole('button', { name: 'Publication 1' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Publication 2' }),
      ).toBeInTheDocument();
    });
  });

  test('adds `themeId` and `topicId` query params from initial theme/topic', async () => {
    dashboardService.getMyThemesAndTopics.mockImplementation(
      async () => testThemeTopics,
    );
    dashboardService.getMyPublicationsByTopic.mockImplementation(
      async () => testPublications,
    );
    permissionService.canCreatePublicationForTopic.mockImplementation(
      async () => true,
    );

    const history = createMemoryHistory();

    render(
      <Router history={history}>
        <ManagePublicationsAndReleasesTab />
      </Router>,
    );

    await waitFor(() => {
      expect(history.location.search).toBe('?themeId=theme-1&topicId=topic-1');
    });
  });

  test('selecting new theme selects first topic automatically', async () => {
    dashboardService.getMyThemesAndTopics.mockImplementation(
      async () => testThemeTopics,
    );
    dashboardService.getMyPublicationsByTopic.mockImplementation(
      async topicId => {
        expect(topicId).toBe('topic-3');

        return testPublications;
      },
    );
    permissionService.canCreatePublicationForTopic.mockImplementation(
      async () => true,
    );

    render(
      <MemoryRouter>
        <ManagePublicationsAndReleasesTab />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Select theme')).toBeInTheDocument();
    });

    userEvent.selectOptions(screen.getByLabelText('Select theme'), 'theme-2');

    await waitFor(() => {
      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-3');

      expect(screen.getByTestId('selectedThemeTitle')).toHaveTextContent(
        'Theme 2',
      );
      expect(screen.getByTestId('selectedTopicTitle')).toHaveTextContent(
        'Topic 3',
      );

      expect(
        screen.getByRole('button', { name: 'Publication 1' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Publication 2' }),
      ).toBeInTheDocument();
    });
  });

  test('selecting new theme and topic renders correctly', async () => {
    dashboardService.getMyThemesAndTopics.mockImplementation(
      async () => testThemeTopics,
    );
    dashboardService.getMyPublicationsByTopic.mockImplementation(
      async topicId => {
        expect(topicId).toBe('topic-4');

        return testPublications;
      },
    );
    permissionService.canCreatePublicationForTopic.mockImplementation(
      async () => true,
    );

    render(
      <MemoryRouter>
        <ManagePublicationsAndReleasesTab />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Select theme')).toBeInTheDocument();
    });

    userEvent.selectOptions(screen.getByLabelText('Select theme'), 'theme-2');
    userEvent.selectOptions(screen.getByLabelText('Select topic'), 'topic-4');

    await waitFor(() => {
      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-4');

      expect(screen.getByTestId('selectedThemeTitle')).toHaveTextContent(
        'Theme 2',
      );
      expect(screen.getByTestId('selectedTopicTitle')).toHaveTextContent(
        'Topic 4',
      );

      expect(
        screen.getByRole('button', { name: 'Publication 1' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Publication 2' }),
      ).toBeInTheDocument();
    });
  });

  test('selecting new theme and topic updates query params correctly', async () => {
    dashboardService.getMyThemesAndTopics.mockImplementation(
      async () => testThemeTopics,
    );
    dashboardService.getMyPublicationsByTopic.mockImplementation(
      async () => testPublications,
    );
    permissionService.canCreatePublicationForTopic.mockImplementation(
      async () => true,
    );

    const history = createMemoryHistory();

    render(
      <Router history={history}>
        <ManagePublicationsAndReleasesTab />
      </Router>,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Select theme')).toBeInTheDocument();
    });

    userEvent.selectOptions(screen.getByLabelText('Select theme'), 'theme-2');
    userEvent.selectOptions(screen.getByLabelText('Select topic'), 'topic-4');

    await waitFor(() => {
      expect(history.location.search).toBe('?themeId=theme-2&topicId=topic-4');
    });
  });

  test('renders correctly when no themes are available', async () => {
    dashboardService.getMyThemesAndTopics.mockImplementation(async () => []);
    dashboardService.getMyPublicationsByTopic.mockImplementation(
      async () => [],
    );
    permissionService.canCreatePublicationForTopic.mockImplementation(
      async () => true,
    );

    render(
      <MemoryRouter>
        <ManagePublicationsAndReleasesTab />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.queryByLabelText('Select theme')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Select ')).not.toBeInTheDocument();
      expect(
        screen.getByText(
          /do not currently have permission to edit any releases/,
        ),
      ).toBeInTheDocument();
    });
  });

  test('renders correctly when no publications are available', async () => {
    dashboardService.getMyThemesAndTopics.mockImplementation(
      async () => testThemeTopics,
    );
    dashboardService.getMyPublicationsByTopic.mockImplementation(
      async () => [],
    );
    permissionService.canCreatePublicationForTopic.mockImplementation(
      async () => true,
    );

    render(
      <MemoryRouter>
        <ManagePublicationsAndReleasesTab />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('No publications available')).toBeInTheDocument();
    });
  });

  test("renders 'Create new publication' button if authorised", async () => {
    dashboardService.getMyThemesAndTopics.mockImplementation(
      async () => testThemeTopics,
    );
    dashboardService.getMyPublicationsByTopic.mockImplementation(
      async () => testPublications,
    );
    permissionService.canCreatePublicationForTopic.mockImplementation(
      async () => true,
    );

    render(
      <MemoryRouter>
        <ManagePublicationsAndReleasesTab />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Create new publication', { selector: 'a' }),
      ).toBeInTheDocument();
    });
  });

  test("does not render 'Create new publication' button if unauthorised", async () => {
    dashboardService.getMyThemesAndTopics.mockImplementation(
      async () => testThemeTopics,
    );
    dashboardService.getMyPublicationsByTopic.mockImplementation(
      async () => testPublications,
    );
    permissionService.canCreatePublicationForTopic.mockImplementation(
      async () => false,
    );

    render(
      <MemoryRouter>
        <ManagePublicationsAndReleasesTab />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Create new publication', { selector: 'a' }),
      ).not.toBeInTheDocument();
    });
  });
});
