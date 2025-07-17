import PublicationForm from '@admin/pages/publication/components/PublicationForm';
import _publicationService from '@admin/services/publicationService';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationForm', () => {
  test('shows validation error when there is no title', async () => {
    const { user } = render(
      <PublicationForm themeId="theme-id" onSubmit={noop} />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(
        screen.getByText('Enter a publication title', {
          selector: '#publicationForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error when the title is too long', async () => {
    const { user } = render(
      <PublicationForm themeId="theme-id" onSubmit={noop} />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    await user.type(
      screen.getByLabelText('Publication title'),
      'a long publication title a long publication title a long publication title',
    );

    expect(
      await screen.findByText('You have 9 characters too many'),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(
        screen.getByText('Title must be 65 characters or fewer', {
          selector: '#publicationForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error when there is no summary', async () => {
    const { user } = render(
      <PublicationForm themeId="theme-id" onSubmit={noop} />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Publication summary')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(
        screen.getByText('Enter a publication summary', {
          selector: '#publicationForm-summary-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error when the summary is too long', async () => {
    const { user } = render(
      <PublicationForm themeId="theme-id" onSubmit={noop} />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    await user.type(
      screen.getByLabelText('Publication summary'),
      'a long publication summary a long publication summary a long publication summary a long publication summary a long publication summary a long publication summary',
    );

    expect(
      await screen.findByText('You have 1 character too many'),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(
        screen.getByText('Summary must be 160 characters or fewer', {
          selector: '#publicationForm-summary-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows validation errors when there are no contact details', async () => {
    const { user } = render(
      <PublicationForm themeId="theme-id" onSubmit={noop} />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Team name')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Save publication' }));

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

      // NOTE: Contact telephone number is optional, so no validation error
    });
  });

  test.each([' 0abcdefg ', '01234 4567a', '_12345678', '01234 5678 !'])(
    'show validation error when contact tel no "%s" contains non-numeric or non-whitespace characters',
    async telNo => {
      const { user } = render(
        <PublicationForm themeId="theme-id" onSubmit={noop} />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Contact telephone (optional)'),
        ).toBeInTheDocument();
      });

      await user.type(
        screen.getByLabelText('Contact telephone (optional)'),
        telNo,
      );
      await user.click(
        screen.getByRole('button', { name: 'Save publication' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText(
            'Contact telephone must start with a "0" and only contain numeric or whitespace characters',
            {
              selector: '#publicationForm-contactTelNo-error',
            },
          ),
        ).toBeInTheDocument();
      });
    },
  );

  test.each([
    ' 03700002288 ',
    '0370 000 2288',
    '037 0000 2288',
    ' 0 3 7 0 0 0 0 2 2 8 8 ',
  ])(
    'show validation error when contact tel no "%s" is DfE enquiries number',
    async telNo => {
      const { user } = render(
        <PublicationForm themeId="theme-id" onSubmit={noop} />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Contact telephone (optional)'),
        ).toBeInTheDocument();
      });

      await user.type(
        screen.getByLabelText('Contact telephone (optional)'),
        telNo,
      );
      await user.click(
        screen.getByRole('button', { name: 'Save publication' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText(
            'Contact telephone cannot be the DfE enquiries number',
            {
              selector: '#publicationForm-contactTelNo-error',
            },
          ),
        ).toBeInTheDocument();
      });
    },
  );

  test.each([' 0123456 ', '0', '012', '0123 56'])(
    'show validation error when contact tel no "%s" is less than 8 characters',
    async telNo => {
      const { user } = render(
        <PublicationForm themeId="theme-id" onSubmit={noop} />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Contact telephone (optional)'),
        ).toBeInTheDocument();
      });

      await user.type(
        screen.getByLabelText('Contact telephone (optional)'),
        telNo,
      );
      await user.click(
        screen.getByRole('button', { name: 'Save publication' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Contact telephone must be 8 characters or more', {
            selector: '#publicationForm-contactTelNo-error',
          }),
        ).toBeInTheDocument();
      });
    },
  );

  test('show validation error when contact email is not valid', async () => {
    const { user } = render(
      <PublicationForm themeId="theme-id" onSubmit={noop} />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Team email address')).toBeInTheDocument();
    });

    await user.type(
      screen.getByLabelText('Team email address'),
      'not a valid email',
    );
    await user.click(screen.getByRole('button', { name: 'Save publication' }));

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

    const { user } = render(
      <PublicationForm themeId="theme-id" onSubmit={handleSubmit} />,
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Save publication' }),
      ).toBeInTheDocument();
    });

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('can submit with valid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <PublicationForm themeId="theme-id" onSubmit={handleSubmit} />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Publication title'), 'Test title');

    await user.type(
      screen.getByLabelText('Publication summary'),
      'Test summary',
    );

    await user.type(screen.getByLabelText('Team name'), 'Test team');
    await user.type(
      screen.getByLabelText('Team email address'),
      'team@test.com',
    );
    await user.type(screen.getByLabelText('Contact name'), 'John Smith');
    await user.type(
      screen.getByLabelText('Contact telephone (optional)'),
      '0123456789',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(publicationService.createPublication).toHaveBeenCalledWith({
        title: 'Test title',
        summary: 'Test summary',
        themeId: 'theme-id',
        contact: {
          teamName: 'Test team',
          teamEmail: 'team@test.com',
          contactName: 'John Smith',
          contactTelNo: '0123456789',
        },
      });
    });
  });

  test('can submit without a contact tel no', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <PublicationForm themeId="theme-id" onSubmit={handleSubmit} />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Publication title')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Publication title'), 'Test title');

    await user.type(
      screen.getByLabelText('Publication summary'),
      'Test summary',
    );

    await user.type(screen.getByLabelText('Team name'), 'Test team');
    await user.type(
      screen.getByLabelText('Team email address'),
      'team@test.com',
    );
    await user.type(screen.getByLabelText('Contact name'), 'John Smith');

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save publication' }));

    await waitFor(() => {
      expect(publicationService.createPublication).toHaveBeenCalledWith({
        title: 'Test title',
        summary: 'Test summary',
        themeId: 'theme-id',
        contact: {
          teamName: 'Test team',
          teamEmail: 'team@test.com',
          contactName: 'John Smith',
          contactTelNo: undefined,
        },
      });
    });
  });
});
