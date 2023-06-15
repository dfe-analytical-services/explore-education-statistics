import React from 'react';
import PublicationsTab from '@admin/pages/admin-dashboard/components/PublicationsTab';
import _permissionService from '@admin/services/permissionService';
import _publicationService, {
  Publication,
} from '@admin/services/publicationService';
import _themeService, { Theme } from '@admin/services/themeService';
import _storageService from '@common/services/storageService';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
import { MemoryRouter, Router } from 'react-router';

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
const themeService = _themeService as jest.Mocked<typeof _themeService>;
const storageService = _storageService as jest.Mocked<typeof _storageService>;

describe('PublicationsTab', () => {
  const testThemeTopics: Theme[] = [
    {
      id: 'theme-1',
      title: 'Theme 1',
      slug: 'theme-1',
      summary: '',
      topics: [
        {
          id: 'topic-1',
          title: 'Topic 1',
          slug: 'topic-1',
          themeId: 'theme-1',
        },
        {
          id: 'topic-2',
          title: 'Topic 2',
          slug: 'topic-2',
          themeId: 'theme-1',
        },
      ],
    },
    {
      id: 'theme-2',
      title: 'Theme 2',
      slug: 'theme-2',
      summary: '',
      topics: [
        {
          id: 'topic-3',
          title: 'Topic 3',
          slug: 'topic-3',
          themeId: 'theme-1',
        },
        {
          id: 'topic-4',
          title: 'Topic 4',
          slug: 'topic-4',
          themeId: 'theme-1',
        },
      ],
    },
  ];

  const testTopic1Publications: Publication[] = [
    {
      id: 'publication-1',
      slug: 'publication-1-slug',
      title: 'Publication 1',
      summary: 'Publication 1 summary',
      theme: { id: 'theme-1', title: 'theme 1' },
      topic: { id: 'topic-1', title: 'topic 1' },
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationSummary: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canManageLegacyReleases: true,
        canUpdateContact: true,
        canUpdateContributorReleaseRole: true,
        canViewReleaseTeamAccess: true,
      },
    },
    {
      id: 'publication-2',
      slug: 'publication-2-slug',
      title: 'Publication 2',
      summary: 'Publication 2 summary',
      theme: { id: 'theme-1', title: 'theme 1' },
      topic: { id: 'topic-1', title: 'topic 1' },
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationSummary: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canManageLegacyReleases: true,
        canUpdateContact: true,
        canUpdateContributorReleaseRole: true,
        canViewReleaseTeamAccess: true,
      },
    },
  ];

  const testTopic2Publications: Publication[] = [
    {
      id: 'publication-3',
      slug: 'publication-3-slug',
      title: 'Publication 3',
      summary: 'Publication 3 summary',
      theme: { id: 'theme-1', title: 'theme 1' },
      topic: { id: 'topic-2', title: 'topic 2' },
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationSummary: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canManageLegacyReleases: true,
        canUpdateContact: true,
        canUpdateContributorReleaseRole: true,
        canViewReleaseTeamAccess: true,
      },
    },
  ];

  const testTopic3Publications: Publication[] = [
    {
      id: 'publication-4',
      slug: 'publication-4-slug',
      title: 'Publication 4',
      summary: 'Publication 4 summary',
      theme: { id: 'theme-3', title: 'theme 3' },
      topic: { id: 'topic-3', title: 'topic 3' },
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationSummary: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canManageLegacyReleases: true,
        canUpdateContact: true,
        canUpdateContributorReleaseRole: true,
        canViewReleaseTeamAccess: true,
      },
    },
  ];

  const testTopic4Publications: Publication[] = [
    {
      id: 'publication-5',
      slug: 'publication-5-slug',
      title: 'Publication 5',
      summary: 'Publication 5 summary',
      theme: { id: 'theme-3', title: 'theme 3' },
      topic: { id: 'topic-4', title: 'topic 4' },
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationSummary: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canManageLegacyReleases: true,
        canUpdateContact: true,
        canUpdateContributorReleaseRole: true,
        canViewReleaseTeamAccess: true,
      },
    },
  ];

  beforeEach(() => {
    permissionService.canCreatePublicationForTopic.mockResolvedValue(true);
  });

  describe('BAU user', () => {
    test('renders with first theme and topic selected by default', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic1Publications,
      );

      render(
        <MemoryRouter>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-1');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-1');
      expect(screen.getByRole('heading', { name: 'Theme 1 / Topic 1' }));
    });

    test('adds `themeId` and `topicId` query params from initial theme/topic', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic1Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      const history = createMemoryHistory();

      render(
        <Router history={history}>
          <PublicationsTab isBauUser />
        </Router>,
      );

      await waitFor(() => {
        expect(history.location.search).toBe(
          '?themeId=theme-1&topicId=topic-1',
        );
      });
    });

    test('renders with saved theme and topic', async () => {
      storageService.getSync.mockReturnValue({
        themeId: 'theme-2',
        topicId: 'topic-4',
      });

      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic1Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-4');
      expect(screen.getByRole('heading', { name: 'Theme 2 / Topic 4' }));
    });

    test('renders with first theme/topic if saved theme and topic are invalid', async () => {
      storageService.getSync.mockReturnValue({
        themeId: 'not a theme',
        topicId: 'not a topic',
      });

      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic1Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-1');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-1');
      expect(screen.getByRole('heading', { name: 'Theme 1 / Topic 1' }));
    });

    test('adds `themeId` and `topicId` query params from saved theme and topic', async () => {
      storageService.getSync.mockReturnValue({
        themeId: 'theme-2',
        topicId: 'topic-4',
      });

      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic4Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      const history = createMemoryHistory();

      render(
        <Router history={history}>
          <PublicationsTab isBauUser />
        </Router>,
      );

      await waitFor(() => {
        expect(history.location.search).toBe(
          '?themeId=theme-2&topicId=topic-4',
        );
      });
    });

    test('renders theme/topic selected from query params instead of saved theme/topic', async () => {
      storageService.getSync.mockReturnValue({
        themeId: 'theme-1',
        topicId: 'topic-2',
      });

      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic2Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter
          initialEntries={[{ search: '?themeId=theme-2&topicId=topic-4' }]}
        >
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-4');
      expect(screen.getByRole('heading', { name: 'Theme 2 / Topic 4' }));
    });

    test('renders with first theme selected if `themeId` query param does not match any theme', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic1Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter initialEntries={[{ search: '?themeId=not-a-theme' }]}>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-1');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-1');
      expect(screen.getByRole('heading', { name: 'Theme 1 / Topic 1' }));
    });

    test('renders with saved theme selected if `topicId` query param is missing', async () => {
      storageService.getSync.mockReturnValue({
        themeId: 'theme-2',
        topicId: 'topic-3',
      });

      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic3Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter initialEntries={[{ search: '?topicId=topic-4' }]}>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-4');
      expect(screen.getByRole('heading', { name: 'Theme 2 / Topic 4' }));
    });

    test('renders with first topic selected if `topicId` query param is missing', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic3Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter initialEntries={[{ search: '?themeId=theme-2' }]}>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-3');
      expect(screen.getByRole('heading', { name: 'Theme 2 / Topic 3' }));
    });

    test('renders with first topic selected if `topicId` query param does not match any topic', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic3Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter
          initialEntries={[{ search: '?themeId=theme-2&topicId=not-a-topic' }]}
        >
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-3');
      expect(screen.getByRole('heading', { name: 'Theme 2 / Topic 3' }));
    });

    test('renders with saved topic selected if `topicId` query param is missing', async () => {
      storageService.getSync.mockReturnValue({
        themeId: 'theme-2',
        topicId: 'topic-4',
      });

      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic4Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter initialEntries={[{ search: '?themeId=theme-2' }]}>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-4');
      expect(screen.getByRole('heading', { name: 'Theme 2 / Topic 4' }));
    });

    test('selecting new theme selects first topic automatically', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic3Publications,
      );

      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      userEvent.selectOptions(screen.getByLabelText('Select theme'), 'theme-2');

      await waitFor(() => {
        expect(screen.getByText('Theme 2 / Topic 3'));
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-3');

      expect(
        screen.getByRole('link', { name: 'Publication 4' }),
      ).toBeInTheDocument();
    });

    test('selecting new theme and topic renders correctly', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic4Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      userEvent.selectOptions(screen.getByLabelText('Select theme'), 'theme-2');

      await waitFor(() => {
        expect(screen.getByText('Topic 4', { selector: 'option' })).toHaveValue(
          'topic-4',
        );
      });

      userEvent.selectOptions(screen.getByLabelText('Select topic'), 'topic-4');

      await waitFor(() => {
        expect(screen.getByText('Theme 2 / Topic 4'));
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Select topic')).toHaveValue('topic-4');
      expect(
        screen.getByRole('link', { name: 'Publication 5' }),
      ).toBeInTheDocument();
    });

    test('selecting new theme and topic updates query params correctly', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic4Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      const history = createMemoryHistory();

      render(
        <Router history={history}>
          <PublicationsTab isBauUser />
        </Router>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      userEvent.selectOptions(screen.getByLabelText('Select theme'), 'theme-2');

      await waitFor(() => {
        expect(screen.getByText('Topic 4', { selector: 'option' })).toHaveValue(
          'topic-4',
        );
      });

      userEvent.selectOptions(screen.getByLabelText('Select topic'), 'topic-4');

      await waitFor(() => {
        expect(history.location.search).toBe(
          '?themeId=theme-2&topicId=topic-4',
        );
      });
    });

    test('renders correctly when no themes are available', async () => {
      themeService.getThemes.mockResolvedValue([]);
      publicationService.listPublications.mockResolvedValue([]);
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.queryByLabelText('Select theme')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Select ')).not.toBeInTheDocument();
      expect(
        screen.getByText(
          /do not currently have permission to edit any releases/,
        ),
      ).toBeInTheDocument();
    });

    test('renders list of publications for the selected topic', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic1Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter>
          <PublicationsTab isBauUser />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(
        screen.getByRole('heading', { name: 'Theme 1 / Topic 1' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('link', { name: 'Publication 1' }),
      ).toBeInTheDocument();
    });
  });

  describe('non-BAU user', () => {
    test('does not render the theme and topic dropdowns', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic1Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter>
          <PublicationsTab isBauUser={false} />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(screen.queryByLabelText('Select theme')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Select topic')).not.toBeInTheDocument();
    });

    test('does not add `themeId` and `topicId` query params', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications.mockResolvedValue(
        testTopic1Publications,
      );
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      const history = createMemoryHistory();

      render(
        <Router history={history}>
          <PublicationsTab isBauUser={false} />
        </Router>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      expect(history.location.search).toBe('');
    });

    test('renders a list of all publications grouped by theme and topic', async () => {
      themeService.getThemes.mockResolvedValue(testThemeTopics);
      publicationService.listPublications
        .mockResolvedValueOnce(testTopic1Publications)
        .mockResolvedValueOnce(testTopic2Publications)
        .mockResolvedValueOnce(testTopic3Publications)
        .mockResolvedValueOnce(testTopic4Publications);
      permissionService.canCreatePublicationForTopic.mockResolvedValue(true);

      render(
        <MemoryRouter>
          <PublicationsTab isBauUser={false} />
        </MemoryRouter>,
      );

      await waitFor(() => {
        expect(
          screen.getByText('View and manage your publications'),
        ).toBeInTheDocument();
      });

      const groups = screen.getAllByTestId('topic-publications');
      expect(groups).toHaveLength(4);

      expect(
        within(groups[0]).getByRole('heading', { name: 'Theme 1 / Topic 1' }),
      ).toBeInTheDocument();
      expect(
        within(groups[0]).getByRole('link', { name: 'Publication 1' }),
      ).toBeInTheDocument();
      expect(
        within(groups[0]).getByRole('link', { name: 'Publication 2' }),
      ).toBeInTheDocument();

      expect(
        within(groups[1]).getByRole('heading', { name: 'Theme 1 / Topic 2' }),
      ).toBeInTheDocument();
      expect(
        within(groups[1]).getByRole('link', { name: 'Publication 3' }),
      ).toBeInTheDocument();

      expect(
        within(groups[2]).getByRole('heading', { name: 'Theme 2 / Topic 3' }),
      ).toBeInTheDocument();
      expect(
        within(groups[2]).getByRole('link', { name: 'Publication 4' }),
      ).toBeInTheDocument();

      expect(
        within(groups[3]).getByRole('heading', { name: 'Theme 2 / Topic 4' }),
      ).toBeInTheDocument();
      expect(
        within(groups[3]).getByRole('link', { name: 'Publication 5' }),
      ).toBeInTheDocument();
    });
  });
});
