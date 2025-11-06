import flushPromises from '@common-test/flushPromises';
import PublicationForm, {
  PublicationFormSubmitHandler,
} from '@common/modules/table-tool/components/PublicationForm';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { Theme } from '@common/services/publicationService';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('PublicationForm', () => {
  const testThemes: Theme[] = [
    {
      id: 'theme-1',
      title: 'Theme 1',
      summary: '',
      publications: [
        {
          id: 'publication-1',
          title: 'Publication 1',
          slug: 'publication-slug-1',
          isSuperseded: false,
        },
        {
          id: 'publication-2',
          title: 'Publication 2 find me',
          slug: 'publication-slug-2',
          isSuperseded: false,
        },
      ],
    },
    {
      id: 'theme-2',
      title: 'Theme 2',
      summary: '',
      publications: [
        {
          id: 'publication-3',
          title: 'Publication 3',
          slug: 'publication-slug-3',
          isSuperseded: false,
        },
      ],
    },
    {
      id: 'theme-3',
      title: 'Theme 3',
      summary: '',
      publications: [
        {
          id: 'publication-4',
          title: 'Publication 4 find me',
          slug: 'publication-slug-4',
          isSuperseded: false,
        },
        {
          id: 'publication-5',
          title: 'Publication 5',
          slug: 'publication-slug-5',
          isSuperseded: false,
        },
        {
          id: 'publication-6',
          title: 'Publication 6',
          slug: 'publication-slug-6',
          isSuperseded: false,
        },
        {
          id: 'publication-7',
          title: 'Publication 7',
          slug: 'publication-slug-7',
          isSuperseded: false,
        },
      ],
    },
    {
      id: 'theme-4',
      title: 'Theme 4',
      summary: '',
      publications: [
        {
          id: 'publication-8',
          title: 'Publication 8',
          slug: 'publication-slug-8',
          isSuperseded: false,
        },
        {
          id: 'publication-9',
          title: 'Publication 9',
          slug: 'publication-slug-9',
          isSuperseded: false,
        },
        {
          id: 'publication-10',
          title: 'Publication 10',
          slug: 'publication-slug-10',
          isSuperseded: false,
        },
      ],
    },
  ];

  const testThemeWithSupercededPublication: Theme = {
    id: 'theme-5',
    title: 'Theme 5',
    summary: '',
    publications: [
      {
        id: 'publication-11',
        title: 'Publication 11',
        slug: 'publication-slug-11',
        isSuperseded: true,
        supersededBy: {
          id: 'superseding-publication',
          slug: 'superseding-publication-slug',
          title: 'Superseding publication',
        },
      },
      {
        id: 'publication-12',
        title: 'Publication 12',
        slug: 'publication-slug-12',
        isSuperseded: false,
      },
    ],
  };

  const wizardProps: InjectedWizardProps = {
    shouldScroll: true,
    stepNumber: 1,
    currentStep: 1,
    isActive: true,
    isEnabled: true,
    isLoading: false,
    setCurrentStep: (step, task) => task?.(),
    goToNextStep: task => task?.(),
    goToPreviousStep: task => task?.(),
  };

  test('renders the form with the search form, themes list and empty publications list', () => {
    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByLabelText('Search publications by title'),
    ).toBeInTheDocument();
    const themeRadios = within(
      screen.getByRole('group', { name: 'Show publications by theme' }),
    ).getAllByRole('radio');
    expect(themeRadios).toHaveLength(4);
    expect(themeRadios[0]).toEqual(
      screen.getByRole('radio', { name: 'Theme 1' }),
    );
    expect(themeRadios[1]).toEqual(
      screen.getByRole('radio', {
        name: 'Theme 2',
      }),
    );
    expect(themeRadios[2]).toEqual(
      screen.getByRole('radio', {
        name: 'Theme 3',
      }),
    );
    expect(themeRadios[3]).toEqual(
      screen.getByRole('radio', {
        name: 'Theme 4',
      }),
    );

    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');
    expect(publicationRadios).toHaveLength(0);

    expect(
      screen.getByText('Search or select a theme to view publications'),
    ).toBeInTheDocument();
  });

  test('renders publication options filtered by title when using search field', async () => {
    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    await userEvent.type(
      screen.getByLabelText('Search publications by title'),
      'find me',
    );

    await waitFor(() =>
      expect(
        screen.queryByText('Search or select a theme to view publications'),
      ).not.toBeInTheDocument(),
    );

    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');

    expect(publicationRadios).toHaveLength(2);

    expect(publicationRadios[0]).toEqual(
      screen.getByRole('radio', {
        name: 'Publication 2 find me',
      }),
    );
    expect(publicationRadios[1]).toEqual(
      screen.getByRole('radio', {
        name: 'Publication 4 find me',
      }),
    );
  });

  test('does not render superseded publications when using search field', async () => {
    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={[...testThemes, testThemeWithSupercededPublication]}
        onSubmit={noop}
      />,
    );

    await userEvent.type(
      screen.getByLabelText('Search publications by title'),
      'Publication 11',
    );

    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');
    expect(publicationRadios).toHaveLength(0);

    expect(screen.queryByLabelText('Publication 11')).not.toBeInTheDocument();
  });

  test('renders the theme as a hint on the publication options when using search field', async () => {
    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    await userEvent.type(
      screen.getByLabelText('Search publications by title'),
      'find me',
    );

    await waitFor(() =>
      expect(
        screen.queryByText('Search or select a theme to view publications'),
      ).not.toBeInTheDocument(),
    );

    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');
    expect(publicationRadios).toHaveLength(2);

    expect(
      within(
        screen.getByTestId('Radio item for Publication 2 find me'),
      ).getByText('Theme 1'),
    ).toBeInTheDocument();

    expect(
      within(
        screen.getByTestId('Radio item for Publication 4 find me'),
      ).getByText('Theme 3'),
    ).toBeInTheDocument();
  });

  test('renders publication options filtered by case-insensitive title', async () => {
    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    await userEvent.type(
      screen.getByLabelText('Search publications by title'),
      'FiND Me',
    );

    await waitFor(() =>
      expect(
        screen.queryByText('Search or select a theme to view publications'),
      ).not.toBeInTheDocument(),
    );

    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');
    expect(publicationRadios).toHaveLength(2);

    expect(publicationRadios[0]).toEqual(
      screen.getByRole('radio', {
        name: 'Publication 2 find me',
      }),
    );
    expect(publicationRadios[1]).toEqual(
      screen.getByRole('radio', {
        name: 'Publication 4 find me',
      }),
    );
  });

  test('renders the `no publications found` message when there are no search results', async () => {
    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    await userEvent.type(
      screen.getByLabelText('Search publications by title'),
      'Nope',
    );

    expect(
      await screen.findByText('No publications found'),
    ).toBeInTheDocument();

    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');
    expect(publicationRadios).toHaveLength(0);
  });

  test('does not throw error if regex sensitive search term is used', async () => {
    jest.useFakeTimers();
    const user = userEvent.setup({ delay: null });

    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    await user.type(
      screen.getByLabelText('Search publications by title'),
      '[[',
    );

    expect(() => {
      jest.runOnlyPendingTimers();
    }).not.toThrow();

    jest.useRealTimers();
  });

  test('renders empty message when there are no themes', () => {
    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        onSubmit={noop}
        themes={[]}
      />,
    );

    expect(
      screen.queryByRole('group', { name: 'Show publications by theme' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('group', { name: /Select a publication/ }),
    ).not.toBeInTheDocument();

    expect(screen.getByText('No publications found')).toBeInTheDocument();
  });

  test('renders the publication options when a theme is selected', async () => {
    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    await userEvent.click(
      screen.getByRole('radio', {
        name: 'Theme 4',
      }),
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Search or select a theme to view publications'),
      ).not.toBeInTheDocument();
    });

    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');
    expect(publicationRadios).toHaveLength(3);
    expect(publicationRadios[0]).toEqual(
      screen.getByRole('radio', {
        name: 'Publication 8',
      }),
    );
    expect(publicationRadios[1]).toEqual(
      screen.getByRole('radio', {
        name: 'Publication 9',
      }),
    );
    expect(publicationRadios[2]).toEqual(
      screen.getByRole('radio', {
        name: 'Publication 10',
      }),
    );
  });

  test('does not render superseded publications when a theme is selected', async () => {
    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={[...testThemes, testThemeWithSupercededPublication]}
        onSubmit={noop}
      />,
    );

    await userEvent.click(
      screen.getByRole('radio', {
        name: 'Theme 5',
      }),
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Search or select a theme to view publications'),
      ).not.toBeInTheDocument();
    });

    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');
    expect(publicationRadios).toHaveLength(1);
    expect(publicationRadios[0]).toEqual(
      screen.getByRole('radio', {
        name: 'Publication 12',
      }),
    );

    expect(screen.queryByLabelText('Publication 11')).not.toBeInTheDocument();
  });

  test('renders read-only view with initial `publicationId` when step is not active', () => {
    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        initialValues={{
          publicationId: 'publication-3',
        }}
        isActive={false}
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    expect(
      screen.queryByRole('group', { name: 'Show publications by theme' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('group', { name: /Select a publication/ }),
    ).not.toBeInTheDocument();

    expect(screen.getByTestId('Publication')).toHaveTextContent(
      'Publication 3',
    );
  });

  test('renders read-only view with selected publication when step is not active', async () => {
    const { rerender } = render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    await userEvent.click(
      screen.getByRole('radio', {
        name: 'Theme 4',
      }),
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Search or select a theme to view publications'),
      ).not.toBeInTheDocument();
    });

    await userEvent.click(
      screen.getByRole('radio', {
        name: 'Publication 9',
      }),
    );

    rerender(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        isActive={false}
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    expect(screen.queryAllByRole('radio')).toHaveLength(0);
    expect(screen.getByTestId('Publication')).toHaveTextContent(
      'Publication 9',
    );
  });

  test('clicking `Next step` calls `onSubmit` with correct values', async () => {
    const handleSubmit = jest.fn();

    render(
      <PublicationForm
        {...wizardProps}
        stepTitle="Choose a publication"
        themes={testThemes}
        onSubmit={handleSubmit}
      />,
    );

    await userEvent.click(
      screen.getByRole('radio', {
        name: 'Theme 4',
      }),
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Search or select a theme to view publications'),
      ).not.toBeInTheDocument();
    });

    await userEvent.click(
      screen.getByRole('radio', {
        name: 'Publication 9',
      }),
    );

    await userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    await flushPromises();

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith<
        Parameters<PublicationFormSubmitHandler>
      >({
        publication: {
          id: 'publication-9',
          slug: 'publication-slug-9',
          title: 'Publication 9',
          isSuperseded: false,
        },
      });
    });
  });
});
