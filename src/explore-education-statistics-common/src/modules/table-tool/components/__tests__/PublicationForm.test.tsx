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
      topics: [
        {
          id: 'topic-1',
          title: 'Topic 1',
          summary: '',
          publications: [
            {
              id: 'publication-1',
              title: 'Publication 1',
              slug: 'publication-slug-1',
              isSuperseded: false,
            },
          ],
        },
        {
          id: 'topic-2',
          title: 'Topic 2',
          summary: '',
          publications: [
            {
              id: 'publication-2',
              title: 'Publication 2 find me',
              slug: 'publication-slug-2',
              isSuperseded: false,
            },
          ],
        },
      ],
    },
    {
      id: 'theme-2',
      title: 'Theme 2',
      summary: '',
      topics: [
        {
          id: 'topic-3',
          title: 'Topic 3',
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
      ],
    },
    {
      id: 'theme-3',
      title: 'Theme 3',
      summary: '',
      topics: [
        {
          id: 'topic-4',
          title: 'Topic 4',
          summary: '',
          publications: [
            {
              id: 'publication-4',
              title: 'Publication 4 find me',
              slug: 'publication-slug-4',
              isSuperseded: false,
            },
          ],
        },
        {
          id: 'topic-5',
          title: 'Topic 5',
          summary: '',
          publications: [
            {
              id: 'publication-5',
              title: 'Publication 5',
              slug: 'publication-slug-5',
              isSuperseded: false,
            },
          ],
        },
        {
          id: 'topic-6',
          title: 'Topic 6',
          summary: '',
          publications: [
            {
              id: 'publication-6',
              title: 'Publication 6',
              slug: 'publication-slug-6',
              isSuperseded: false,
            },
          ],
        },
        {
          id: 'topic-7',
          title: 'Topic 7',
          summary: '',
          publications: [
            {
              id: 'publication-7',
              title: 'Publication 7',
              slug: 'publication-slug-7',
              isSuperseded: false,
            },
          ],
        },
      ],
    },
    {
      id: 'theme-4',
      title: 'Theme 4',
      summary: '',
      topics: [
        {
          id: 'topic-8',
          title: 'Topic 8',
          summary: '',
          publications: [
            {
              id: 'publication-8',
              title: 'Publication 8',
              slug: 'publication-slug-8',
              isSuperseded: false,
            },
          ],
        },
        {
          id: 'topic-9',
          title: 'Topic 9',
          summary: '',
          publications: [
            {
              id: 'publication-9',
              title: 'Publication 9',
              slug: 'publication-slug-9',
              isSuperseded: false,
            },
          ],
        },
        {
          id: 'topic-10',
          title: 'Topic 10',
          summary: '',
          publications: [
            {
              id: 'publication-10',
              title: 'Publication 10',
              slug: 'publication-slug-10',
              isSuperseded: false,
            },
          ],
        },
      ],
    },
  ];

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
      <PublicationForm {...wizardProps} themes={testThemes} onSubmit={noop} />,
    );

    expect(screen.getByLabelText('Search publications')).toBeInTheDocument();
    const themeRadios = within(
      screen.getByRole('group', { name: 'Select a theme' }),
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
    jest.useFakeTimers({
      legacyFakeTimers: true,
    });

    render(
      <PublicationForm {...wizardProps} themes={testThemes} onSubmit={noop} />,
    );

    userEvent.type(screen.getByLabelText('Search publications'), 'find me');

    jest.runOnlyPendingTimers();

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

  test('renders the theme as a hint on the publication options when using search field', async () => {
    jest.useFakeTimers({
      legacyFakeTimers: true,
    });

    render(
      <PublicationForm {...wizardProps} themes={testThemes} onSubmit={noop} />,
    );

    userEvent.type(screen.getByLabelText('Search publications'), 'find me');

    jest.runOnlyPendingTimers();

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
    jest.useFakeTimers({
      legacyFakeTimers: true,
    });

    render(
      <PublicationForm {...wizardProps} themes={testThemes} onSubmit={noop} />,
    );

    userEvent.type(screen.getByLabelText('Search publications'), 'FiND Me');

    jest.runOnlyPendingTimers();

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
    jest.useFakeTimers({
      legacyFakeTimers: true,
    });

    render(
      <PublicationForm {...wizardProps} themes={testThemes} onSubmit={noop} />,
    );

    userEvent.type(screen.getByLabelText('Search publications'), 'Nope');

    jest.runOnlyPendingTimers();

    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');
    expect(publicationRadios).toHaveLength(0);

    expect(screen.getByText('No publications found')).toBeInTheDocument();
  });

  test('does not throw error if regex sensitive search term is used', async () => {
    jest.useFakeTimers({
      legacyFakeTimers: true,
    });

    render(
      <PublicationForm {...wizardProps} themes={testThemes} onSubmit={noop} />,
    );

    userEvent.type(screen.getByLabelText('Search publications'), '[');

    expect(() => {
      jest.runOnlyPendingTimers();
    }).not.toThrow();
  });

  test('renders empty message when there are no themes', () => {
    render(<PublicationForm {...wizardProps} onSubmit={noop} themes={[]} />);

    expect(
      screen.queryByRole('group', { name: 'Select a theme' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('group', { name: /Select a publication/ }),
    ).not.toBeInTheDocument();

    expect(screen.getByText('No publications found')).toBeInTheDocument();
  });

  test('renders the publication options when a theme is selected', async () => {
    render(
      <PublicationForm {...wizardProps} themes={testThemes} onSubmit={noop} />,
    );

    userEvent.click(
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

  test('renders read-only view with initial `publicationId` when step is not active', () => {
    render(
      <PublicationForm
        {...wizardProps}
        initialValues={{
          publicationId: 'publication-3',
        }}
        isActive={false}
        themes={testThemes}
        onSubmit={noop}
      />,
    );

    expect(
      screen.queryByRole('group', { name: 'Select a theme' }),
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
      <PublicationForm {...wizardProps} themes={testThemes} onSubmit={noop} />,
    );

    userEvent.click(
      screen.getByRole('radio', {
        name: 'Theme 4',
      }),
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Search or select a theme to view publications'),
      ).not.toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('radio', {
        name: 'Publication 9',
      }),
    );

    rerender(
      <PublicationForm
        {...wizardProps}
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
        themes={testThemes}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(
      screen.getByRole('radio', {
        name: 'Theme 4',
      }),
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Search or select a theme to view publications'),
      ).not.toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('radio', {
        name: 'Publication 9',
      }),
    );

    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

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
