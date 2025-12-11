import { PublicationMethodologiesList } from '@common/services/publicationService';
import { render, screen, within } from '@testing-library/react';
import React from 'react';
import ReleaseMethodologyPage from '../ReleaseMethodologyPage';
import {
  testPublicationSummary,
  testReleaseVersionSummary,
} from './__data__/testReleaseData';

describe('ReleaseMethodologyPage', () => {
  const testMethodologies: PublicationMethodologiesList = {
    methodologies: [
      {
        methodologyId: 'test-methodology-id-1',
        slug: 'publication-1',
        title: 'Publication 1',
      },
      {
        methodologyId: 'test-methodology-id-2',
        slug: 'publication-2',
        title: 'Publication 2',
      },
    ],
    externalMethodology: {
      title: 'External methodology',
      url: 'https://test.com/external-methodology',
    },
  };

  test('renders correctly with methodologies', () => {
    render(
      <ReleaseMethodologyPage
        methodologiesSummary={testMethodologies}
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Contact us',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Methodology',
      }),
    ).toBeInTheDocument();

    const list = screen.getByTestId('methodologies-list');
    const listItems = within(list).getAllByRole('listitem');
    expect(listItems).toHaveLength(3);
    expect(
      within(listItems[0]).getByRole('link', {
        name: 'Publication 1',
      }),
    ).toHaveAttribute('href', '/methodology/publication-1');
    expect(
      within(listItems[1]).getByRole('link', {
        name: 'Publication 2',
      }),
    ).toHaveAttribute('href', '/methodology/publication-2');
    expect(
      within(listItems[2]).getByRole('link', {
        name: 'External methodology (opens in new tab)',
      }),
    ).toHaveAttribute('href', 'https://test.com/external-methodology');
  });

  test('renders correctly with only internal methodologies', () => {
    render(
      <ReleaseMethodologyPage
        methodologiesSummary={{
          ...testMethodologies,
          externalMethodology: undefined,
        }}
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );
    expect(
      screen.getByRole('heading', {
        name: 'Methodology',
      }),
    ).toBeInTheDocument();

    const list = screen.getByTestId('methodologies-list');
    const listItems = within(list).getAllByRole('listitem');
    expect(listItems).toHaveLength(2);
    expect(
      within(listItems[0]).getByRole('link', {
        name: 'Publication 1',
      }),
    ).toHaveAttribute('href', '/methodology/publication-1');
    expect(
      within(listItems[1]).getByRole('link', {
        name: 'Publication 2',
      }),
    ).toHaveAttribute('href', '/methodology/publication-2');
  });

  test('renders correctly with only an external methodology', () => {
    render(
      <ReleaseMethodologyPage
        methodologiesSummary={{
          ...testMethodologies,
          methodologies: [],
        }}
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );
    expect(
      screen.getByRole('heading', {
        name: 'Methodology',
      }),
    ).toBeInTheDocument();

    const list = screen.getByTestId('methodologies-list');
    const listItems = within(list).getAllByRole('listitem');
    expect(listItems).toHaveLength(1);
    expect(
      within(listItems[0]).getByRole('link', {
        name: 'External methodology (opens in new tab)',
      }),
    ).toHaveAttribute('href', 'https://test.com/external-methodology');
  });

  test('renders correctly when no methodologies', () => {
    render(
      <ReleaseMethodologyPage
        methodologiesSummary={{
          methodologies: [],
        }}
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(
      screen.queryByRole('heading', {
        name: 'Methodology',
      }),
    ).not.toBeInTheDocument();
  });
});
