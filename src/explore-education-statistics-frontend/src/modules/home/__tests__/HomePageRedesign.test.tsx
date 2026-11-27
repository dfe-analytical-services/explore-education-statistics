import render from '@common-test/render';
import testTenant, {
  fullTestTenant,
  minimalTestTenant,
} from '@common-test/testTenant';
import HomePageRedesign from '@frontend/modules/home/HomePageRedesign';
import { screen, within } from '@testing-library/react';
import React from 'react';

jest.mock('@azure/search-documents', () => ({
  SearchClient: jest.fn(),
  AzureKeyCredential: jest.fn(),
  odata: jest.fn(),
}));

jest.mock('@frontend/services/azurePublicationService');
jest.mock('@frontend/services/azureDataSetService');

describe('HomePageRedesign', () => {
  test('renders the EES related services and general enquiry section by default', () => {
    render(<HomePageRedesign latestReleases={[]} />);

    const relatedServices = screen.getByRole('heading', {
      name: 'Related services',
      level: 2,
    });
    expect(relatedServices).toHaveAttribute('id', 'related-services');
    expect(
      screen.getByRole('link', {
        name: 'Statistics at DfE (opens in new tab)',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Have a general enquiry?' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'Have a general question about education or the Department for Education?',
      ),
    ).toBeInTheDocument();
  });

  test('renders the related services and public enquiries from the tenant', () => {
    render(<HomePageRedesign latestReleases={[]} />, {
      tenant: fullTestTenant,
    });

    expect(
      screen.getByRole('heading', { name: 'Related services', level: 2 }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Test Related Service (opens in new tab)',
      }),
    ).toHaveAttribute('href', 'https://related-service.example.com');
    expect(
      screen.queryByRole('link', {
        name: 'Statistics at DfE (opens in new tab)',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('Have a general question about testing?'),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'The test statistics service is operated by the Test Organisation (TO).',
      ),
    ).toBeInTheDocument();
  });

  test('hides the related services section for a tenant with no relatedServices', () => {
    render(<HomePageRedesign latestReleases={[]} />, { tenant: testTenant });

    expect(
      screen.queryByRole('heading', { name: 'Related services', level: 2 }),
    ).not.toBeInTheDocument();
    expect(
      within(
        screen.getByRole('navigation', { name: 'On this page' }),
      ).queryByRole('link', { name: 'Related services' }),
    ).not.toBeInTheDocument();
  });

  test('hides the related services section for a tenant with an empty relatedServices list', () => {
    render(<HomePageRedesign latestReleases={[]} />, {
      tenant: { ...fullTestTenant, relatedServices: [] },
    });

    expect(
      screen.queryByRole('heading', { name: 'Related services', level: 2 }),
    ).not.toBeInTheDocument();
    expect(
      within(
        screen.getByRole('navigation', { name: 'On this page' }),
      ).queryByRole('link', { name: 'Related services' }),
    ).not.toBeInTheDocument();
  });

  test('hides the general enquiry section and acronym for a tenant without them', () => {
    render(<HomePageRedesign latestReleases={[]} />, {
      tenant: minimalTestTenant,
    });

    expect(
      screen.queryByRole('heading', { name: 'Have a general enquiry?' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText(
        'The test statistics service is operated by the Test Organisation.',
      ),
    ).toBeInTheDocument();
  });

  test('links to the related services section from the page navigation', () => {
    render(<HomePageRedesign latestReleases={[]} />);

    const nav = within(
      screen.getByRole('navigation', { name: 'On this page' }),
    );
    expect(nav.getByRole('link', { name: 'Related services' })).toHaveAttribute(
      'href',
      '#related-services',
    );
  });
});
