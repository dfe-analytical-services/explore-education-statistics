import PublicationsTab from '@admin/pages/admin-dashboard/components/PublicationsTab';
import _permissionService from '@admin/services/permissionService';
import _publicationService, {
  Publication,
} from '@admin/services/publicationService';
import _themeService, { Theme } from '@admin/services/themeService';
import _storageService from '@common/services/storageService';
import render from '@common-test/render';
import { waitFor } from '@testing-library/dom';
import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
import React from 'react';
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
  const testThemes: Theme[] = [
    {
      id: 'theme-1',
      title: 'Theme 1',
      slug: 'theme-1',
      summary: '',
    },
    {
      id: 'theme-2',
      title: 'Theme 2',
      slug: 'theme-2',
      summary: '',
    },
  ];

  const testTheme1Publications: Publication[] = [
    {
      id: 'publication-1',
      slug: 'publication-1-slug',
      title: 'Publication 1',
      summary: 'Publication 1 summary',
      theme: { id: 'theme-1', title: 'theme 1' },
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationSummary: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canManageReleaseSeries: true,
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
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationSummary: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canManageReleaseSeries: true,
        canUpdateContact: true,
        canUpdateContributorReleaseRole: true,
        canViewReleaseTeamAccess: true,
      },
    },
    {
      id: 'publication-3',
      slug: 'publication-3-slug',
      title: 'Publication 3',
      summary: 'Publication 3 summary',
      theme: { id: 'theme-1', title: 'theme 1' },
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationSummary: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canManageReleaseSeries: true,
        canUpdateContact: true,
        canUpdateContributorReleaseRole: true,
        canViewReleaseTeamAccess: true,
      },
    },
  ];

  const testTheme2Publications: Publication[] = [
    {
      id: 'publication-4',
      slug: 'publication-4-slug',
      title: 'Publication 4',
      summary: 'Publication 4 summary',
      theme: { id: 'theme-2', title: 'theme 2' },
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationSummary: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canManageReleaseSeries: true,
        canUpdateContact: true,
        canUpdateContributorReleaseRole: true,
        canViewReleaseTeamAccess: true,
      },
    },
    {
      id: 'publication-5',
      slug: 'publication-5-slug',
      title: 'Publication 5',
      summary: 'Publication 5 summary',
      theme: { id: 'theme-2', title: 'theme 2' },
      permissions: {
        canAdoptMethodologies: true,
        canCreateReleases: true,
        canUpdatePublication: true,
        canUpdatePublicationSummary: true,
        canCreateMethodologies: true,
        canManageExternalMethodology: true,
        canManageReleaseSeries: true,
        canUpdateContact: true,
        canUpdateContributorReleaseRole: true,
        canViewReleaseTeamAccess: true,
      },
    },
  ];

  beforeEach(() => {
    permissionService.canCreatePublicationForTheme.mockResolvedValue(true);
  });

  describe('BAU user', () => {
    test('renders with first theme selected by default', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme1Publications,
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
      expect(screen.getByRole('heading', { name: 'Theme 1' }));
    });

    test('adds `themeId` query param from initial theme', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme1Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

      const history = createMemoryHistory();

      render(
        <Router history={history}>
          <PublicationsTab isBauUser />
        </Router>,
      );

      await waitFor(() => {
        expect(history.location.search).toBe('?themeId=theme-1');
      });
    });

    test('renders with saved theme', async () => {
      storageService.getSync.mockReturnValue({
        themeId: 'theme-2',
      });

      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme1Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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
      expect(screen.getByRole('heading', { name: 'Theme 2' }));
    });

    test('renders with first theme if saved theme is invalid', async () => {
      storageService.getSync.mockReturnValue({
        themeId: 'not a theme',
      });

      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme1Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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
      expect(screen.getByRole('heading', { name: 'Theme 1' }));
    });

    test('adds `themeId` query param from saved theme', async () => {
      storageService.getSync.mockReturnValue({
        themeId: 'theme-2',
      });

      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme2Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

      const history = createMemoryHistory();

      render(
        <Router history={history}>
          <PublicationsTab isBauUser />
        </Router>,
      );

      await waitFor(() => {
        expect(history.location.search).toBe('?themeId=theme-2');
      });
    });

    test('renders theme selected from query params instead of saved theme', async () => {
      storageService.getSync.mockReturnValue({
        themeId: 'theme-1',
      });

      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme1Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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
      expect(screen.getByRole('heading', { name: 'Theme 2' }));
    });

    test('renders with first theme selected if `themeId` query param does not match any theme', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme1Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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
      expect(screen.getByRole('heading', { name: 'Theme 1' }));
    });

    test('selecting new theme renders correctly', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme2Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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

      await userEvent.selectOptions(
        screen.getByLabelText('Select theme'),
        'theme-2',
      );

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: 'Theme 2' }));
      });

      expect(screen.getByLabelText('Select theme')).toHaveValue('theme-2');
      expect(
        screen.getByRole('link', { name: 'Publication 5' }),
      ).toBeInTheDocument();
    });

    test('selecting new theme updates query params correctly', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme2Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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

      await userEvent.selectOptions(
        screen.getByLabelText('Select theme'),
        'theme-2',
      );

      await waitFor(() => {
        expect(history.location.search).toBe('?themeId=theme-2');
      });
    });

    test('renders correctly when no themes are available', async () => {
      themeService.getThemes.mockResolvedValue([]);
      publicationService.listPublications.mockResolvedValue([]);
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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

    test('renders list of publications for the selected theme', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme1Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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
        screen.getByRole('heading', { name: 'Theme 1' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('link', { name: 'Publication 1' }),
      ).toBeInTheDocument();
    });
  });

  describe('non-BAU user', () => {
    test('does not render the theme dropdown', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme1Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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
    });

    test('does not add `themeId` query param', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications.mockResolvedValue(
        testTheme1Publications,
      );
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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

    test('renders a list of all publications grouped by theme', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      publicationService.listPublications
        .mockResolvedValueOnce(testTheme1Publications)
        .mockResolvedValueOnce(testTheme2Publications);
      permissionService.canCreatePublicationForTheme.mockResolvedValue(true);

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

      const groups = screen.getAllByTestId('theme-publications');
      expect(groups).toHaveLength(2);

      expect(
        within(groups[0]).getByRole('heading', { name: 'Theme 1' }),
      ).toBeInTheDocument();
      expect(
        within(groups[0]).getByRole('link', { name: 'Publication 1' }),
      ).toBeInTheDocument();
      expect(
        within(groups[0]).getByRole('link', { name: 'Publication 2' }),
      ).toBeInTheDocument();
      expect(
        within(groups[0]).getByRole('link', { name: 'Publication 3' }),
      ).toBeInTheDocument();

      expect(
        within(groups[1]).getByRole('heading', { name: 'Theme 2' }),
      ).toBeInTheDocument();
      expect(
        within(groups[1]).getByRole('link', { name: 'Publication 4' }),
      ).toBeInTheDocument();
      expect(
        within(groups[1]).getByRole('link', { name: 'Publication 5' }),
      ).toBeInTheDocument();
    });
  });
});
