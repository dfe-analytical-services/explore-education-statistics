import PublicationDetailsForm from '@admin/pages/publication/components/PublicationDetailsForm';
import _publicationService from '@admin/services/publicationService';
import _themeService, { Theme } from '@admin/services/themeService';
import render from '@common-test/render';
import { PublicationSummary } from '@common/services/publicationService';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
jest.mock('@admin/services/themeService');
const themeService = _themeService as jest.Mocked<typeof _themeService>;

describe('PublicationDetailsForm', () => {
  const testFormValues = {
    summary: 'Test summary',
    title: 'Test title',
    themeId: 'theme-1',
  };

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
  ];

  beforeEach(() => {
    themeService.getThemes.mockResolvedValue(testThemes);
    publicationService.getPublicationSummaries.mockResolvedValue(
      testPublicationSummaries,
    );
  });

  test('renders with initial values', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    expect(screen.getByLabelText('Publication title')).toHaveValue(
      'Test title',
    );
    expect(screen.getByLabelText('Publication summary')).toHaveValue(
      'Test summary',
    );
    expect(screen.getByLabelText('Select theme')).toHaveValue('theme-1');
    expect(screen.getByLabelText('Superseding publication')).toHaveValue('');
  });

  test('shows validation error when there is no title', async () => {
    const { user } = renderComponent();

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

  test('shows validation error when the title is too long', async () => {
    const { user } = renderComponent();

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    await user.clear(screen.getByLabelText('Publication title'));

    await user.type(
      screen.getByLabelText('Publication title'),
      'a long publication title a long publication title a long publication title',
    );

    expect(
      await screen.findByText('You have 9 characters too many'),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Update publication details' }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Title must be 65 characters or fewer', {
          selector: '#publicationDetailsForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error when there is no summary', async () => {
    const { user } = renderComponent();

    await waitFor(() => {
      expect(screen.getByLabelText('Publication summary')).toBeInTheDocument();
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

  test('shows validation error when the summary is too long', async () => {
    const { user } = renderComponent();

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    await user.clear(screen.getByLabelText('Publication summary'));

    await user.type(
      screen.getByLabelText('Publication summary'),
      'a long publication summary a long publication summary a long publication summary a long publication summary a long publication summary a long publication summary',
    );

    expect(
      await screen.findByText('You have 1 character too many'),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Update publication details' }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Summary must be 160 characters or fewer', {
          selector: '#publicationDetailsForm-summary-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = renderComponent(handleSubmit);

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Update publication details' }),
      ).toBeInTheDocument();
    });

    await user.clear(screen.getByLabelText('Publication title'));

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Update publication details' }),
    );

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('can submit with updated values', async () => {
    const handleSubmit = jest.fn();

    const { user } = renderComponent(handleSubmit);

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Publication title'), ' edited');
    await user.type(screen.getByLabelText('Publication summary'), ' edited');
    await user.selectOptions(screen.getByLabelText('Select theme'), 'theme-2');

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Update publication details' }),
    );

    expect(
      await screen.getByText('Confirm publication changes'),
    ).toBeInTheDocument();

    expect(screen.getByRole('dialog')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalled();
    });
  });

  function renderComponent(submitHandler = noop) {
    return render(
      <TestConfigContextProvider>
        <PublicationDetailsForm
          canUpdatePublication
          canUpdatePublicationSummary
          initialValues={testFormValues}
          publicationId="publication-id"
          publicationSlug="publication-slug"
          onCancel={noop}
          onSubmit={submitHandler}
        />
      </TestConfigContextProvider>,
    );
  }
});
