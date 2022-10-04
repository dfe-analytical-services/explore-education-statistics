import PublicationContactPage from '@admin/pages/publication/PublicationContactPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  testContact,
  testPublication,
} from '@admin/pages/publication/__data__/testPublication';
import _publicationService, {
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

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

    renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    expect(screen.getByLabelText('Team name')).toHaveValue('Team Smith');
    expect(screen.getByLabelText('Team email')).toHaveValue(
      'john.smith@test.com',
    );
    expect(screen.getByLabelText('Contact name')).toHaveValue('John Smith');
    expect(screen.getByLabelText('Contact telephone')).toHaveValue(
      '0777777777',
    );
    expect(
      screen.getByRole('button', { name: 'Update contact details' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('clicking the cancel button switches back to readOnly view', async () => {
    publicationService.getContact.mockResolvedValue(testContact);

    renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    expect(screen.getByLabelText('Team name')).toHaveValue('Team Smith');

    userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

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

    renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    userEvent.clear(screen.getByLabelText('Team name'));
    userEvent.tab();

    userEvent.clear(screen.getByLabelText('Team email'));
    userEvent.tab();

    userEvent.clear(screen.getByLabelText('Contact name'));
    userEvent.tab();

    userEvent.clear(screen.getByLabelText('Contact telephone'));
    userEvent.tab();

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

      expect(
        screen.getByText('Enter a contact telephone', {
          selector: '#publicationContactForm-contactTelNo-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('show validation error when contact email is not valid', async () => {
    publicationService.getContact.mockResolvedValue(testContact);

    renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    userEvent.clear(screen.getByLabelText('Team email'));

    userEvent.type(screen.getByLabelText('Team email'), 'not a valid email');

    userEvent.tab();

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

    renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    expect(publicationService.updatePublication).not.toHaveBeenCalled();

    userEvent.click(
      screen.getByRole('button', { name: 'Update contact details' }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(modal.getByRole('heading')).toHaveTextContent(
      'Confirm contact changes',
    );
    userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
  });

  test('clicking confirm calls the publication service', async () => {
    publicationService.getContact.mockResolvedValue(testContact);
    renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    fireEvent.change(screen.getByLabelText('Team name'), {
      target: { value: 'new team name' },
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Update contact details' }),
    );

    userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

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

  test('clicking confirm switches page to readOnly', async () => {
    publicationService.getContact.mockResolvedValue(testContact);
    publicationService.updateContact.mockResolvedValue({
      ...testContact,
      teamName: 'updated team name',
    });

    renderPage(testPublication);

    await waitFor(() => {
      expect(
        screen.getByText('Contact for this publication'),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Edit contact details' }),
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Update contact details' }),
    );

    userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

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
  render(
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
