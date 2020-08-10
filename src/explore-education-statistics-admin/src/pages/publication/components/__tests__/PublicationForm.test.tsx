import PublicationForm, {
  PublicationFormValues,
} from '@admin/pages/publication/components/PublicationForm';
import _methodologyService, {
  BasicMethodology,
} from '@admin/services/methodologyService';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/methodologyService');

const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;

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

  test('shows validation error when there is no title', async () => {
    render(<PublicationForm onSubmit={noop} />);

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

    userEvent.click(
      screen.getByLabelText('Link to an externally hosted methodology'),
    );

    userEvent.click(screen.getByLabelText('Link title'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter an external methodology link title', {
          selector: '#publicationForm-externalMethodologyTitle-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when no external methodology URL', async () => {
    render(<PublicationForm onSubmit={noop} />);

    userEvent.click(
      screen.getByLabelText('Link to an externally hosted methodology'),
    );

    await userEvent.clear(screen.getByLabelText('URL'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter an external methodology URL', {
          selector: '#publicationForm-externalMethodologyUrl-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when invalid external methodology URL', async () => {
    render(<PublicationForm onSubmit={noop} />);

    userEvent.click(
      screen.getByLabelText('Link to an externally hosted methodology'),
    );

    await userEvent.type(screen.getByLabelText('URL'), 'not a valid url');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a valid external methodology URL', {
          selector: '#publicationForm-externalMethodologyUrl-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values', async () => {
    methodologyService.getMethodologies.mockResolvedValue(testMethodologies);

    const handleSubmit = jest.fn();

    render(<PublicationForm onSubmit={handleSubmit} />);

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
  });
});
