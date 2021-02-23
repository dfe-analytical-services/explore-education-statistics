import PublicationForm, {
  PublicationFormValues,
} from '@admin/pages/publication/components/PublicationForm';
import _methodologyService, {
  BasicMethodology,
} from '@admin/services/methodologyService';
import _themeService, { Theme } from '@admin/services/themeService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/methodologyService');
jest.mock('@admin/services/themeService');

const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;
const themeService = _themeService as jest.Mocked<typeof _themeService>;

describe('PublicationForm', () => {
  const testMethodologies: BasicMethodology[] = [
    {
      id: 'methodology-1',
      title: 'Methodology 1',
      slug: 'methodology-1',
      status: 'Approved',
    },
    {
      id: 'methodology-2',
      title: 'Methodology 2',
      slug: 'methodology-2',
      status: 'Approved',
    },
  ];

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

  test('shows validation error when no selected methodology', async () => {
    methodologyService.getMethodologies.mockResolvedValue(testMethodologies);

    render(<PublicationForm onSubmit={noop} />);

    await waitFor(() => {
      expect(
        screen.getByText('Methodology 2 [Approved]', {
          selector: 'option',
        }),
      ).toBeInTheDocument();
    });

    userEvent.selectOptions(
      screen.getByLabelText('Select methodology'),
      'methodology-2',
    );
    userEvent.selectOptions(screen.getByLabelText('Select methodology'), '');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Choose a methodology', {
          selector: '#publicationForm-methodologyId-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation errors when no external methodology link title', async () => {
    render(<PublicationForm onSubmit={noop} />);

    await waitFor(() => {
      expect(
        screen.getByLabelText('Link to an externally hosted methodology'),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByLabelText('Link to an externally hosted methodology'),
    );

    userEvent.click(screen.getByLabelText('Link title'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter an external methodology link title', {
          selector: '#publicationForm-externalMethodology-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when no external methodology URL', async () => {
    render(<PublicationForm onSubmit={noop} />);

    await waitFor(() => {
      expect(
        screen.getByLabelText('Link to an externally hosted methodology'),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByLabelText('Link to an externally hosted methodology'),
    );

    await userEvent.clear(screen.getByLabelText('URL'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter an external methodology URL', {
          selector: '#publicationForm-externalMethodology-url-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when invalid external methodology URL', async () => {
    render(<PublicationForm onSubmit={noop} />);

    await waitFor(() => {
      expect(
        screen.getByLabelText('Link to an externally hosted methodology'),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByLabelText('Link to an externally hosted methodology'),
    );

    await userEvent.type(screen.getByLabelText('URL'), 'not a valid url');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a valid external methodology URL', {
          selector: '#publicationForm-externalMethodology-url-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values', async () => {
    methodologyService.getMethodologies.mockResolvedValue(testMethodologies);

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
    methodologyService.getMethodologies.mockResolvedValue(testMethodologies);

    const handleSubmit = jest.fn();

    render(<PublicationForm onSubmit={handleSubmit} />);

    await waitFor(() => {
      expect(
        screen.getByText('Methodology 2 [Approved]', {
          selector: 'option',
        }),
      ).toBeInTheDocument();
    });

    await userEvent.type(
      screen.getByLabelText('Publication title'),
      'Test title',
    );
    userEvent.selectOptions(
      screen.getByLabelText('Select methodology'),
      'methodology-2',
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
        methodologyId: 'methodology-2',
        teamName: 'Test team',
        teamEmail: 'team@test.com',
        contactName: 'John Smith',
        contactTelNo: '0123456789',
      } as PublicationFormValues);
    });
  });

  describe('with `initialValues`', () => {
    test('renders correctly with selected methodology', async () => {
      methodologyService.getMethodologies.mockResolvedValue(testMethodologies);

      render(
        <PublicationForm
          initialValues={{
            title: 'Test title',
            methodologyId: 'methodology-2',
            teamName: 'Test team',
            teamEmail: 'team@test.com',
            contactTelNo: '0123456789',
            contactName: 'John Smith',
          }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Publication title')).toHaveValue(
          'Test title',
        );
        expect(
          screen.getByLabelText('Choose an existing methodology'),
        ).toBeChecked();
        expect(screen.getByLabelText('Select methodology')).toHaveValue(
          'methodology-2',
        );
        expect(screen.getByLabelText('Team name')).toHaveValue('Test team');
        expect(screen.getByLabelText('Team email address')).toHaveValue(
          'team@test.com',
        );
        expect(screen.getByLabelText('Contact name')).toHaveValue('John Smith');
        expect(screen.getByLabelText('Contact telephone number')).toHaveValue(
          '0123456789',
        );
      });
    });

    test('renders correctly with external methodology', async () => {
      methodologyService.getMethodologies.mockResolvedValue(testMethodologies);

      render(
        <PublicationForm
          initialValues={{
            title: 'Test title',
            externalMethodology: {
              title: 'Test external methodology',
              url: 'http://test.com',
            },
            teamName: 'Test team',
            teamEmail: 'team@test.com',
            contactTelNo: '0123456789',
            contactName: 'John Smith',
          }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Link to an externally hosted methodology'),
        ).toBeChecked();

        expect(screen.getByLabelText('Link title')).toHaveValue(
          'Test external methodology',
        );
        expect(screen.getByLabelText('URL')).toHaveValue('http://test.com');
        expect(screen.getByLabelText('Select methodology')).toHaveValue('');
      });
    });

    test('renders correctly with no methodology', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      methodologyService.getMethodologies.mockResolvedValue(testMethodologies);

      render(
        <PublicationForm
          initialValues={{
            title: 'Test title',
            teamName: 'Test team',
            teamEmail: 'team@test.com',
            contactTelNo: '0123456789',
            contactName: 'John Smith',
          }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByLabelText('No methodology')).toBeChecked();

        expect(screen.getByLabelText('Link title')).toHaveValue('');
        expect(screen.getByLabelText('URL')).toHaveValue('');
        expect(screen.getByLabelText('Select methodology')).toHaveValue('');
      });
    });

    test('renders correctly with selected theme and topic', async () => {
      themeService.getThemes.mockResolvedValue(testThemes);
      methodologyService.getMethodologies.mockResolvedValue(testMethodologies);

      render(
        <PublicationForm
          initialValues={{
            title: 'Test title',
            topicId: 'topic-4',
            methodologyId: 'methodology-2',
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
      methodologyService.getMethodologies.mockResolvedValue(testMethodologies);

      const initialValues: PublicationFormValues = {
        title: 'Test title',
        topicId: 'topic-4',
        methodologyId: 'methodology-2',
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
      methodologyService.getMethodologies.mockResolvedValue(testMethodologies);

      const initialValues: PublicationFormValues = {
        title: 'Test title',
        topicId: 'topic-4',
        methodologyId: 'methodology-2',
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
