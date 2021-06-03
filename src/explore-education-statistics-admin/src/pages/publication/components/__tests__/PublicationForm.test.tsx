import PublicationForm, {
  PublicationFormValues,
} from '@admin/pages/publication/components/PublicationForm';
import _themeService, { Theme } from '@admin/services/themeService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/themeService');

const themeService = _themeService as jest.Mocked<typeof _themeService>;

describe('PublicationForm', () => {
  const testThemes: Theme[] = [
    {
      id: 'theme-1',
      slug: 'theme-1',
      title: 'Theme 1',
      summary: '',
      topics: [
        {
          id: 'topic-1',
          slug: 'topic-1',
          title: 'Topic 1',
          themeId: 'theme-1',
        },
        {
          id: 'topic-2',
          slug: 'topic-2',
          title: 'Topic 2',
          themeId: 'theme-1',
        },
      ],
    },
    {
      id: 'theme-2',
      slug: 'theme-2',
      title: 'Theme 2',
      summary: '',
      topics: [
        {
          id: 'topic-3',
          slug: 'topic-3',
          title: 'Topic 3',
          themeId: 'theme-2',
        },
        {
          id: 'topic-4',
          slug: 'topic-4',
          title: 'Topic 4',
          themeId: 'theme-2',
        },
      ],
    },
  ];

  test('does not render any theme/topic fields when no `initialValues`', async () => {
    render(<PublicationForm onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    expect(screen.queryByLabelText('Select theme')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Select topic')).not.toBeInTheDocument();
  });

  test('shows validation error when there is no title', async () => {
    render(<PublicationForm onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    userEvent.click(screen.getByLabelText('Publication title'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a publication title', {
          selector: '#publicationForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation errors when there are no contact details', async () => {
    render(<PublicationForm onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByLabelText('Team name')).toBeInTheDocument();
    });

    userEvent.click(screen.getByLabelText('Team name'));
    userEvent.tab();

    userEvent.click(screen.getByLabelText('Team email address'));
    userEvent.tab();

    userEvent.click(screen.getByLabelText('Contact name'));
    userEvent.tab();

    userEvent.click(screen.getByLabelText('Contact telephone number'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a team name', {
          selector: '#publicationForm-teamName-error',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByText('Enter a team email address', {
          selector: '#publicationForm-teamEmail-error',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByText('Enter a contact name', {
          selector: '#publicationForm-contactName-error',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByText('Enter a contact telephone number', {
          selector: '#publicationForm-contactTelNo-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when contact email is not valid', async () => {
    render(<PublicationForm onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByLabelText('Team email address')).toBeInTheDocument();
    });

    await userEvent.type(
      screen.getByLabelText('Team email address'),
      'not a valid email',
    );
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a valid team email address', {
          selector: '#publicationForm-teamEmail-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values', async () => {
    const handleSubmit = jest.fn();

    render(<PublicationForm onSubmit={handleSubmit} />);

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Save publication' }),
      ).toBeInTheDocument();
    });

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('can submit with valid values', async () => {
    const handleSubmit = jest.fn();

    render(<PublicationForm onSubmit={handleSubmit} />);

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    await userEvent.type(
      screen.getByLabelText('Publication title'),
      'Test title',
    );

    await userEvent.type(screen.getByLabelText('Team name'), 'Test team');
    await userEvent.type(
      screen.getByLabelText('Team email address'),
      'team@test.com',
    );
    await userEvent.type(screen.getByLabelText('Contact name'), 'John Smith');
    await userEvent.type(
      screen.getByLabelText('Contact telephone number'),
      '0123456789',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'Test title',
        teamName: 'Test team',
        teamEmail: 'team@test.com',
        contactName: 'John Smith',
        contactTelNo: '0123456789',
      } as PublicationFormValues);
    });
  });

  describe('with `initialValues`', () => {
    test('renders correctly with selected theme and topic', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);

      render(
        <PublicationForm
          initialValues={{
            title: 'Test title',
            topicId: 'topic-4',
            teamName: 'Test team',
            teamEmail: 'team@test.com',
            contactTelNo: '0123456789',
            contactName: 'John Smith',
          }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        const themeSelect = screen.getByLabelText('Select theme');
        const themes = within(themeSelect).getAllByRole(
          'option',
        ) as HTMLOptionElement[];

        expect(themes).toHaveLength(2);

        expect(themes[0]).toHaveTextContent('Theme 1');
        expect(themes[0].selected).toBe(false);

        expect(themes[1]).toHaveTextContent('Theme 2');
        expect(themes[1].selected).toBe(true);

        const topicSelect = screen.getByLabelText('Select topic');
        const topics = within(topicSelect).getAllByRole(
          'option',
        ) as HTMLOptionElement[];

        expect(topics).toHaveLength(2);

        expect(topics[0]).toHaveTextContent('Topic 3');
        expect(topics[0].selected).toBe(false);

        expect(topics[1]).toHaveTextContent('Topic 4');
        expect(topics[1].selected).toBe(true);
      });
    });

    test('can successfully submit without any changes', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);

      const initialValues: PublicationFormValues = {
        title: 'Test title',
        topicId: 'topic-4',
        teamName: 'Test team',
        teamEmail: 'team@test.com',
        contactTelNo: '0123456789',
        contactName: 'John Smith',
      };

      const handleSubmit = jest.fn();

      render(
        <PublicationForm
          initialValues={initialValues}
          onSubmit={handleSubmit}
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Save publication' }),
        ).toBeInTheDocument();
      });

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Save publication' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith(initialValues);
      });
    });

    test('can successfully submit with updated topic', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);

      const initialValues: PublicationFormValues = {
        title: 'Test title',
        topicId: 'topic-4',
        teamName: 'Test team',
        teamEmail: 'team@test.com',
        contactTelNo: '0123456789',
        contactName: 'John Smith',
      };

      const handleSubmit = jest.fn();

      render(
        <PublicationForm
          initialValues={initialValues}
          onSubmit={handleSubmit}
        />,
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Select topic')).toBeInTheDocument();
        expect(
          screen.getByRole('button', { name: 'Save publication' }),
        ).toBeInTheDocument();
      });

      userEvent.selectOptions(screen.getByLabelText('Select topic'), [
        'topic-3',
      ]);

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Save publication' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          ...initialValues,
          topicId: 'topic-3',
        });
      });
    });
  });
});
