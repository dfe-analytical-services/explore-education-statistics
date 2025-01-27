import PublicationDetailsPage from '@admin/pages/publication/PublicationDetailsPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import _publicationService, {
  Publication,
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import _themeService, { Theme } from '@admin/services/themeService';
import { PublicationSummary } from '@common/services/publicationService';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import render from '@common-test/render';

jest.mock('@admin/services/themeService');
const themeService = _themeService as jest.Mocked<typeof _themeService>;

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationDetailsPage', () => {
  const testThemes: Theme[] = [
    {
      id: 'theme-1',
      slug: 'theme-1-slug',
      summary: '',
      title: 'Theme 1',
    },
    {
      id: 'theme-2',
      slug: 'theme-2-slug',
      summary: '',
      title: 'Theme 2',
    },
  ];

  const testPublicationSummaries: PublicationSummary[] = [
    {
      id: 'publication-1',
      slug: 'publication-1-slug',
      latestReleaseSlug: 'latest-release-slug-1',
      title: 'Publication 1',
      owner: false,
      contact: {
        teamName: 'Mock Contact Team Name',
        teamEmail: 'Mock Contact Team Email',
        contactName: 'Mock Contact Name',
      },
    },
    {
      id: 'publication-2',
      slug: 'publication-2-slug',
      latestReleaseSlug: 'latest-release-slug-2',
      title: 'Publication 2',
      owner: false,
      contact: {
        teamName: 'Mock Contact Team Name',
        teamEmail: 'Mock Contact Team Email',
        contactName: 'Mock Contact Name',
      },
    },
    {
      id: 'publication-3',
      slug: 'publication-3-slug',
      latestReleaseSlug: 'latest-release-slug-3',
      title: 'Publication 3',
      owner: false,
      contact: {
        teamName: 'Mock Contact Team Name',
        teamEmail: 'Mock Contact Team Email',
        contactName: 'Mock Contact Name',
      },
    },
  ];

  const testPublicationWithSummaries: Publication = {
    ...testPublicationSummaries[1],
    summary: '',
    theme: {
      id: 'theme-id-1',
      title: 'theme-title-1',
    },
  };

  beforeEach(() => {
    themeService.getTheme.mockResolvedValue(testThemes[0]);
    themeService.getThemes.mockResolvedValue(testThemes);
    publicationService.getPublication.mockResolvedValue(
      testPublicationWithSummaries,
    );
    publicationService.getPublicationSummaries.mockResolvedValue(
      testPublicationSummaries,
    );
  });

  test('renders the read only page correctly', async () => {
    renderPage(testPublication);

    await waitFor(() => {
      expect(screen.getByText('Publication details')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Publication title')).toHaveTextContent(
      'Publication 1',
    );
    expect(screen.getByTestId('Theme')).toHaveTextContent('Theme 1');
    expect(screen.getByTestId('Superseding publication')).toHaveTextContent(
      'This publication is not archived',
    );
    expect(
      screen.getByRole('button', { name: 'Edit publication details' }),
    ).toBeInTheDocument();
  });

  test('shows the superseding publication if the publication is archived', async () => {
    renderPage({
      ...testPublication,
      supersededById: 'publication-2',
    });

    await waitFor(() => {
      expect(screen.getByText('Publication details')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Superseding publication')).toHaveTextContent(
      'Publication 2',
    );
  });

  describe('details form', () => {
    test('shows the form when the edit button is clicked', async () => {
      const { user } = renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Publication title')).toHaveValue(
        'Publication 1',
      );

      expect(screen.getByLabelText('Publication summary')).toHaveValue(
        'Publication 1 summary',
      );

      const themeSelect = screen.getByLabelText('Select theme');
      expect(themeSelect).toHaveValue('theme-1');
      const themes = within(themeSelect).getAllByRole('option');
      expect(themes).toHaveLength(2);
      expect(themes[0]).toHaveTextContent('Theme 1');
      expect(themes[0]).toHaveValue('theme-1');
      expect(themes[1]).toHaveTextContent('Theme 2');
      expect(themes[1]).toHaveValue('theme-2');

      const supersedingPublicationSelect = screen.getByLabelText(
        'Superseding publication',
      );
      expect(supersedingPublicationSelect).toHaveValue('');
      const supersedingPublications = within(
        supersedingPublicationSelect,
      ).getAllByRole('option');
      expect(supersedingPublications).toHaveLength(3);
      expect(supersedingPublications[0]).toHaveTextContent('None selected');
      expect(supersedingPublications[0]).toHaveValue('');
      expect(supersedingPublications[1]).toHaveTextContent('Publication 2');
      expect(supersedingPublications[1]).toHaveValue('publication-2');
      expect(supersedingPublications[2]).toHaveTextContent('Publication 3');
      expect(supersedingPublications[2]).toHaveValue('publication-3');

      expect(
        screen.getByRole('button', { name: 'Update publication details' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();
    });

    test('clicking the cancel button switches back to readOnly view', async () => {
      const { user } = renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Publication title')).toHaveValue(
        'Publication 1',
      );

      await user.click(screen.getByRole('button', { name: 'Cancel' }));

      expect(screen.getByTestId('Publication title')).toHaveTextContent(
        'Publication 1',
      );

      expect(
        screen.queryByLabelText('Publication title'),
      ).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Theme')).not.toBeInTheDocument();
      expect(
        screen.queryByLabelText('Superseding publication'),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Update publication details' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Cancel' }),
      ).not.toBeInTheDocument();
    });

    test('shows validation errors when there is no title', async () => {
      const { user } = renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      await user.clear(screen.getByLabelText('Publication title'));
      await user.click(
        screen.getByRole('button', { name: 'Update publication details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Enter a title', {
            selector: '#publicationDetailsForm-title-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation errors when there is no summary', async () => {
      const { user } = renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Publication summary'),
        ).toBeInTheDocument();
      });

      await user.clear(screen.getByLabelText('Publication summary'));
      await user.click(
        screen.getByRole('button', { name: 'Update publication details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Enter a summary', {
            selector: '#publicationDetailsForm-summary-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows a confirmation modal on submit', async () => {
      const { user } = renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      expect(publicationService.updatePublication).not.toHaveBeenCalled();

      await user.click(
        screen.getByRole('button', { name: 'Update publication details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Confirm publication changes'),
        ).toBeInTheDocument();
      });

      await user.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Confirm',
        }),
      );
    });

    test('confirmation modal renders correctly when title is changed to match the slug', async () => {
      const { user } = renderPage({
        ...testPublication,
        title: 'Publication 1',
        slug: 'publication-1-updated', // to prevent URL change text in modal from displaying
      });

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      await user.clear(screen.getByLabelText('Publication title'));
      await user.type(
        screen.getByLabelText('Publication title'),
        'Publication 1 updated',
      );

      expect(publicationService.updatePublication).not.toHaveBeenCalled();

      await user.click(
        screen.getByRole('button', { name: 'Update publication details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Confirm publication changes'),
        ).toBeInTheDocument();
      });

      expect(
        screen.queryByText('The URL for this publication will change from'),
      ).not.toBeInTheDocument();

      expect(screen.getByRole('dialog')).toHaveTextContent(
        'The publication title will change from Publication 1 to Publication 1 updated',
      );
    });

    test('confirmation modal renders correctly when a publication title remains the same, but the slug is changed to match the title', async () => {
      const { user } = renderPage({
        ...testPublication,
        title: 'Publication 1 updated',
        slug: 'publication-1', // even with no changes by the user, slug will be changed to match title
      });

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      expect(publicationService.updatePublication).not.toHaveBeenCalled();

      await user.click(
        screen.getByRole('button', { name: 'Update publication details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Confirm publication changes'),
        ).toBeInTheDocument();
      });

      expect(
        screen.queryByText('The publication title will change from'),
      ).not.toBeInTheDocument();

      expect(
        screen.getByText('The URL for this publication will change from'),
      ).toBeInTheDocument();

      expect(screen.getByLabelText('Before URL')).toHaveValue(
        'http://localhost/find-statistics/publication-1',
      );
      expect(screen.getByLabelText('After URL')).toHaveValue(
        'http://localhost/find-statistics/publication-1-updated',
      );
    });

    test('confirmation modal renders correctly when title and slug are changed', async () => {
      const { user } = renderPage({
        ...testPublication,
        title: 'Publication 1',
        slug: 'publication-1',
      });

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      await user.clear(screen.getByLabelText('Publication title'));
      await user.type(
        screen.getByLabelText('Publication title'),
        'Publication 1 updated',
      );

      expect(publicationService.updatePublication).not.toHaveBeenCalled();

      await user.click(
        screen.getByRole('button', { name: 'Update publication details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Confirm publication changes'),
        ).toBeInTheDocument();
      });

      expect(screen.getByRole('dialog')).toHaveTextContent(
        'The publication title will change from Publication 1 to Publication 1 updated',
      );

      expect(
        screen.getByText('The URL for this publication will change from'),
      ).toBeInTheDocument();

      expect(screen.getByLabelText('Before URL')).toHaveValue(
        'http://localhost/find-statistics/publication-1',
      );
      expect(screen.getByLabelText('After URL')).toHaveValue(
        'http://localhost/find-statistics/publication-1-updated',
      );
    });

    test('successfully submits with updated values', async () => {
      const { user } = renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      await user.type(screen.getByLabelText('Publication title'), ' updated');

      await user.selectOptions(screen.getByLabelText('Select theme'), [
        'theme-2',
      ]);

      await user.selectOptions(
        screen.getByLabelText('Superseding publication'),
        ['publication-2'],
      );

      await user.click(
        screen.getByRole('button', { name: 'Update publication details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Confirm publication changes'),
        ).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(publicationService.updatePublication).toHaveBeenCalledWith<
          Parameters<typeof publicationService.updatePublication>
        >(testPublication.id, {
          supersededById: 'publication-2',
          title: 'Publication 1 updated',
          summary: 'Publication 1 summary',
          themeId: 'theme-2',
        });
      });
    });
  });
});

function renderPage(publication: PublicationWithPermissions) {
  return render(
    <MemoryRouter>
      <TestConfigContextProvider>
        <PublicationContextProvider
          publication={publication}
          onPublicationChange={noop}
          onReload={noop}
        >
          <PublicationDetailsPage />
        </PublicationContextProvider>
      </TestConfigContextProvider>
    </MemoryRouter>,
  );
}
