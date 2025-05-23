import PublicationContactPage from '@admin/pages/publication/PublicationContactPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  testContact,
  testPublication,
} from '@admin/pages/publication/__data__/testPublication';
import _publicationService, {
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import { fireEvent, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';
import React from 'react';
import render from '@common-test/render';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationContactPage', () => {
  test('renders the contact page correctly', async () => {
    publicationService.getContact.mockResolvedValue(testContact);

    renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    expect(screen.getByTestId('Team name')).toHaveTextContent('Team Smith');
    expect(screen.getByTestId('Team email')).toHaveTextContent(
      'john.smith@test.com',
    );
    expect(screen.getByTestId('Contact name')).toHaveTextContent('John Smith');
    expect(screen.getByTestId('Contact telephone')).toHaveTextContent(
      '0777777777',
    );
    expect(
      screen.getByRole('button', { name: 'Edit contact details' }),
    ).toBeInTheDocument();
  });

  test('does not show the edit button if do not have permission', async () => {
    publicationService.getContact.mockResolvedValue(testContact);

    renderPage({
      ...testPublication,
      permissions: { ...testPublication.permissions, canUpdateContact: false },
    });

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    expect(
      screen.queryByRole('button', { name: 'Edit contact details' }),
    ).not.toBeInTheDocument();
  });

  test('clicking the edit button shows the edit form', async () => {
    publicationService.getContact.mockResolvedValue(testContact);

    const { user } = renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    await user.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    expect(screen.getByLabelText('Team name')).toHaveValue('Team Smith');
    expect(screen.getByLabelText('Team email')).toHaveValue(
      'john.smith@test.com',
    );
    expect(screen.getByLabelText('Contact name')).toHaveValue('John Smith');
    expect(screen.getByLabelText('Contact telephone (optional)')).toHaveValue(
      '0777777777',
    );
    expect(
      screen.getByRole('button', { name: 'Update contact details' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('clicking the cancel button switches back to readOnly view', async () => {
    publicationService.getContact.mockResolvedValue(testContact);

    const { user } = renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    await user.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    expect(screen.getByLabelText('Team name')).toHaveValue('Team Smith');

    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(screen.getByTestId('Team name')).toHaveTextContent('Team Smith');

    expect(screen.queryByLabelText('Team name')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Team email')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Contact name')).not.toBeInTheDocument();
    expect(
      screen.queryByLabelText('Contact telephone'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Update contact details' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Cancel' }),
    ).not.toBeInTheDocument();
  });

  test('shows validation errors when there are no contact details', async () => {
    publicationService.getContact.mockResolvedValue(testContact);

    const { user } = renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    await user.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    await user.clear(screen.getByLabelText('Team name'));
    await user.clear(screen.getByLabelText('Team email'));
    await user.clear(screen.getByLabelText('Contact name'));
    await user.clear(screen.getByLabelText('Contact telephone (optional)'));

    await user.click(
      screen.getByRole('button', { name: 'Update contact details' }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Enter a team name', {
          selector: '#publicationContactForm-teamName-error',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByText('Enter a team email', {
          selector: '#publicationContactForm-teamEmail-error',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByText('Enter a contact name', {
          selector: '#publicationContactForm-contactName-error',
        }),
      ).toBeInTheDocument();

      // NOTE: Contact telephone is optional, so no validation
    });
  });

  test.each([' 0abcdefg ', '01234 4567a', '_12345678', '01234 5678 !'])(
    'show validation error when contact tel no "%s" contains non-numeric or non-whitespace characters',
    async telNo => {
      publicationService.getContact.mockResolvedValue(testContact);

      const { user } = renderPage(testPublication);

      await waitFor(() => {
        expect(
          screen.getByText('Contact for this publication'),
        ).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit contact details' }),
      );

      await user.clear(screen.getByLabelText('Contact telephone (optional)'));

      await user.type(
        screen.getByLabelText('Contact telephone (optional)'),
        telNo,
      );

      await user.click(
        screen.getByRole('button', { name: 'Update contact details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText(
            'Contact telephone must start with a "0" and only contain numeric or whitespace characters',
            {
              selector: '#publicationContactForm-contactTelNo-error',
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
      publicationService.getContact.mockResolvedValue(testContact);

      const { user } = renderPage(testPublication);

      await waitFor(() => {
        expect(
          screen.getByText('Contact for this publication'),
        ).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit contact details' }),
      );

      await user.clear(screen.getByLabelText('Contact telephone (optional)'));

      await user.type(
        screen.getByLabelText('Contact telephone (optional)'),
        telNo,
      );

      await user.click(
        screen.getByRole('button', { name: 'Update contact details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText(
            'Contact telephone cannot be the DfE enquiries number',
            {
              selector: '#publicationContactForm-contactTelNo-error',
            },
          ),
        ).toBeInTheDocument();
      });
    },
  );

  test.each([' 0123456 ', '0', '012', '0123 56'])(
    'show validation error when contact tel no "%s" is less than 8 characters',
    async telNo => {
      publicationService.getContact.mockResolvedValue(testContact);

      const { user } = renderPage(testPublication);

      await waitFor(() => {
        expect(
          screen.getByText('Contact for this publication'),
        ).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', { name: 'Edit contact details' }),
      );

      await user.clear(screen.getByLabelText('Contact telephone (optional)'));

      await user.type(
        screen.getByLabelText('Contact telephone (optional)'),
        telNo,
      );

      await user.click(
        screen.getByRole('button', { name: 'Update contact details' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Contact telephone must be 8 characters or more', {
            selector: '#publicationContactForm-contactTelNo-error',
          }),
        ).toBeInTheDocument();
      });
    },
  );

  test('show validation error when contact email is not valid', async () => {
    publicationService.getContact.mockResolvedValue(testContact);

    const { user } = renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    await user.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    await user.clear(screen.getByLabelText('Team email'));

    await user.type(screen.getByLabelText('Team email'), 'not a valid email');

    await user.click(
      screen.getByRole('button', { name: 'Update contact details' }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Enter a valid team email', {
          selector: '#publicationContactForm-teamEmail-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows a confirmation modal on submit', async () => {
    publicationService.getContact.mockResolvedValue(testContact);

    const { user } = renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    await user.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    expect(publicationService.updatePublication).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Update contact details' }),
    );

    await waitFor(() => {
      expect(screen.getByText('Confirm contact changes')).toBeInTheDocument();
    });

    const modal = within(screen.getByRole('dialog'));
    expect(modal.getByRole('heading')).toHaveTextContent(
      'Confirm contact changes',
    );
    await user.click(modal.getByRole('button', { name: 'Confirm' }));
  });

  test('clicking confirm calls the publication service', async () => {
    publicationService.getContact.mockResolvedValue(testContact);
    const { user } = renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    await user.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    fireEvent.change(screen.getByLabelText('Team name'), {
      target: { value: 'new team name' },
    });

    await user.click(
      screen.getByRole('button', { name: 'Update contact details' }),
    );

    await waitFor(() => {
      expect(screen.getByText('Confirm contact changes')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(publicationService.updateContact).toHaveBeenCalledWith(
        testPublication.id,
        {
          ...testContact,
          teamName: 'new team name',
        },
      );
    });
  });

  test('can submit without a contact tel no', async () => {
    publicationService.getContact.mockResolvedValue({
      contactName: testContact.contactName,
      contactTelNo: '',
      teamEmail: testContact.teamEmail,
      teamName: testContact.teamName,
    });
    const { user } = renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    await user.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    await user.click(
      screen.getByRole('button', { name: 'Update contact details' }),
    );

    await waitFor(() => {
      expect(screen.getByText('Confirm contact changes')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(publicationService.updateContact).toHaveBeenCalledWith(
        testPublication.id,
        {
          contactName: testContact.contactName,
          contactTelNo: undefined,
          teamEmail: testContact.teamEmail,
          teamName: testContact.teamName,
        },
      );
    });
  });

  test('clicking confirm switches page to readOnly', async () => {
    publicationService.getContact.mockResolvedValue(testContact);
    publicationService.updateContact.mockResolvedValue({
      ...testContact,
      teamName: 'updated team name',
    });

    const { user } = renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    await user.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    await user.click(
      screen.getByRole('button', { name: 'Update contact details' }),
    );

    await waitFor(() => {
      expect(screen.getByText('Confirm contact changes')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Edit contact details' }),
      ).toBeInTheDocument();
    });

    expect(screen.getByTestId('Team name')).toHaveTextContent(
      'updated team name',
    );
    expect(screen.getByTestId('Team email')).toHaveTextContent(
      testContact.teamEmail,
    );
    expect(screen.getByTestId('Contact name')).toHaveTextContent(
      testContact.contactName,
    );
    expect(screen.getByTestId('Contact telephone')).toHaveTextContent(
      testContact.contactTelNo,
    );
  });
});

function renderPage(publication: PublicationWithPermissions) {
  return render(
    <MemoryRouter>
      <PublicationContextProvider
        publication={publication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <PublicationContactPage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
