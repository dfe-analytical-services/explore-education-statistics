import render from '@common-test/render';
import { testPublicationSummary } from '@frontend/modules/find-statistics/__tests__/__data__/testReleaseData';
import DataSetFileContact from '@frontend/modules/data-catalogue/components/DataSetFileContact';
import { screen } from '@testing-library/react';
import omit from 'lodash/omit';
import React from 'react';

describe('DataSetFileContact', () => {
  test('renders the data set contact details', async () => {
    render(
      <DataSetFileContact
        contact={testPublicationSummary.contact}
        dataSetTitle="Test title"
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Contact us' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Test team' }),
    ).toBeInTheDocument();

    expect(screen.getByText(/Contact name: Joe Bloggs/)).toBeInTheDocument();

    expect(screen.getByRole('link', { name: 'test@test.com' })).toHaveAttribute(
      'href',
      'mailto:test@test.com',
    );

    expect(screen.getByText(/Telephone: 01234 567890/)).toBeInTheDocument();
  });

  test('does not render the telephone number section if none provided', async () => {
    render(
      <DataSetFileContact
        contact={omit(testPublicationSummary.contact, 'contactTelNo')}
        dataSetTitle="Test title"
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Contact us' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Test team' }),
    ).toBeInTheDocument();

    expect(screen.getByText(/Contact name: Joe Bloggs/)).toBeInTheDocument();

    expect(screen.getByRole('link', { name: 'test@test.com' })).toHaveAttribute(
      'href',
      'mailto:test@test.com',
    );

    expect(screen.queryByText(/Telephone/)).not.toBeInTheDocument();
  });
});
