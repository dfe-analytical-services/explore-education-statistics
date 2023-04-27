import PublicationDetailsPage from '@admin/pages/publication/PublicationDetailsPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import _publicationService, {
  Publication,
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import _themeService, { Theme } from '@admin/services/themeService';
import { PublicationSummary } from '@common/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';
import React from 'react';

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
      topics: [
        {
          id: 'theme-1-topic-1',
          slug: 'theme-1-topic-1-slug',
          themeId: 'theme-1',
          title: 'Theme 1 Topic 1',
        },
        {
          id: 'theme-1-topic-2',
          slug: 'theme-1-topic-2-slug',
          themeId: 'theme-1',
          title: 'Theme 1 Topic 2',
        },
      ],
    },
    {
      id: 'theme-2',
      slug: 'theme-2-slug',
      summary: '',
      title: 'Theme 2',
      topics: [
        {
          id: 'theme-2-topic-1',
          slug: 'theme-2-topic-1-slug',
          themeId: 'theme-2',
          title: 'Theme 2 Topic 1',
        },
        {
          id: 'theme-2-topic-2',
          slug: 'theme-2-topic-2-slug',
          themeId: 'theme-2',
          title: 'Theme 2 Topic 2',
        },
      ],
    },
  ];

  const testPublicationSummaries: PublicationSummary[] = [
    {
      id: 'publication-1',
      slug: 'publication-1-slug',
      title: 'Publication 1',
    },
    {
      id: 'publication-2',
      slug: 'publication-2-slug',
      title: 'Publication 2',
    },
    {
      id: 'publication-3',
      slug: 'publication-3-slug',
      title: 'Publication 3',
    },
  ];

  beforeEach(() => {
    themeService.getTheme.mockResolvedValue(testThemes[0]);
    themeService.getThemes.mockResolvedValue(testThemes);
    publicationService.getPublication.mockResolvedValue(
      testPublicationSummaries[1] as Publication,
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
    expect(screen.getByTestId('Topic')).toHaveTextContent('Theme 1 Topic 2');
    expect(screen.getByTestId('Superseding publication')).toHaveTextContent(
      'This publication is not archived',
    );
    expect(
      screen.getByRole('button', { name: 'Edit publication details' }),
    ).toBeInTheDocument();
  });

  test('shows the superseding publication if the publication is archived', async () => {
    renderPage({ ...testPublication, supersededById: 'publication-2' });

    await waitFor(() => {
      expect(screen.getByText('Publication details')).toBeInTheDocument();
    });

    expect(screen.getByTestId('Superseding publication')).toHaveTextContent(
      'Publication 2',
    );
  });

  describe('details form', () => {
    test('shows the form when the edit button is clicked', async () => {
      renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      userEvent.click(
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

      const topicSelect = screen.getByLabelText('Select topic');
      expect(topicSelect).toHaveValue('theme-1-topic-2');
      const topics = within(topicSelect).getAllByRole('option');
      expect(topics).toHaveLength(2);
      expect(topics[0]).toHaveTextContent('Theme 1 Topic 1');
      expect(topics[0]).toHaveValue('theme-1-topic-1');
      expect(topics[1]).toHaveTextContent('Theme 1 Topic 2');
      expect(topics[1]).toHaveValue('theme-1-topic-2');

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

    test('updates the topics dropdown when change the theme', async () => {
      renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      const topics = within(screen.getByLabelText('Select topic')).getAllByRole(
        'option',
      );
      expect(topics).toHaveLength(2);

      expect(topics[0]).toHaveTextContent('Theme 1 Topic 1');
      expect(topics[0]).toHaveValue('theme-1-topic-1');
      expect(topics[1]).toHaveTextContent('Theme 1 Topic 2');
      expect(topics[1]).toHaveValue('theme-1-topic-2');

      userEvent.selectOptions(screen.getByLabelText('Select theme'), [
        'theme-2',
      ]);

      const updatedTopics = within(
        screen.getByLabelText('Select topic'),
      ).getAllByRole('option');
      expect(updatedTopics).toHaveLength(2);

      expect(updatedTopics[0]).toHaveTextContent('Theme 2 Topic 1');
      expect(updatedTopics[0]).toHaveValue('theme-2-topic-1');
      expect(updatedTopics[1]).toHaveTextContent('Theme 2 Topic 2');
      expect(updatedTopics[1]).toHaveValue('theme-2-topic-2');
    });

    test('clicking the cancel button switches back to readOnly view', async () => {
      renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Publication title')).toHaveValue(
        'Publication 1',
      );

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      expect(screen.getByTestId('Publication title')).toHaveTextContent(
        'Publication 1',
      );

      expect(
        screen.queryByLabelText('Publication title'),
      ).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Theme')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Topic')).not.toBeInTheDocument();
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
      renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      userEvent.clear(screen.getByLabelText('Publication title'));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter a title', {
            selector: '#publicationDetailsForm-title-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation errors when there is no summary', async () => {
      renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Publication summary'),
        ).toBeInTheDocument();
      });

      userEvent.clear(screen.getByLabelText('Publication summary'));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter a summary', {
            selector: '#publicationDetailsForm-summary-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows a confirmation modal on submit', async () => {
      renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      expect(publicationService.updatePublication).not.toHaveBeenCalled();

      userEvent.click(
        screen.getByRole('button', { name: 'Update publication details' }),
      );

      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByRole('heading')).toHaveTextContent(
        'Confirm publication changes',
      );
      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
    });

    test('successfullly submits with updated values', async () => {
      renderPage(testPublication);

      await waitFor(() => {
        expect(screen.getByText('Publication details')).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', { name: 'Edit publication details' }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
      });

      userEvent.type(screen.getByLabelText('Publication title'), ' updated');

      userEvent.selectOptions(screen.getByLabelText('Select theme'), [
        'theme-2',
      ]);

      userEvent.selectOptions(screen.getByLabelText('Select topic'), [
        'theme-2-topic-2',
      ]);

      userEvent.selectOptions(
        screen.getByLabelText('Superseding publication'),
        ['publication-2'],
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Update publication details' }),
      );

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(publicationService.updatePublication).toHaveBeenCalledWith<
          Parameters<typeof publicationService.updatePublication>
        >(testPublication.id, {
          supersededById: 'publication-2',
          title: 'Publication 1 updated',
          summary: 'Publication 1 summary',
          topicId: 'theme-2-topic-2',
        });
      });
    });
  });
});

function renderPage(publication: PublicationWithPermissions) {
  render(
    <MemoryRouter>
      <PublicationContextProvider
        publication={publication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <PublicationDetailsPage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
