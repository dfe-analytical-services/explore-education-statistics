import { RelatedInformationItem } from '@common/services/publicationService';
import { render, screen, within } from '@testing-library/react';
import React from 'react';
import ReleaseHelpPage from '../ReleaseHelpPage';
import {
  testPublicationSummary,
  testReleaseVersionSummary,
} from './__data__/testReleaseData';

describe('ReleaseHelpPage', () => {
  const testRelatedLinks: RelatedInformationItem[] = [
    {
      id: 'related-1',
      title: 'Related information 1',
      url: 'https://test.com/related-1',
    },
    {
      id: 'related-2',
      title: 'Related information 2',
      url: 'https://test.com/related-2',
    },
  ];

  test('renders correctly with all content sections', () => {
    render(
      <ReleaseHelpPage
        publicationSummary={testPublicationSummary}
        relatedInformationItems={testRelatedLinks}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Get help by contacting us',
        level: 2,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Accredited official statistics',
        level: 2,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Related information',
        level: 2,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Pre-release access list',
        level: 2,
      }),
    ).toBeInTheDocument();

    const relatedInfoList = screen.getByTestId('related-information-list');
    const listItems = within(relatedInfoList).getAllByRole('listitem');
    expect(listItems).toHaveLength(2);
  });

  test('does not render related info section if no links', () => {
    render(
      <ReleaseHelpPage
        publicationSummary={testPublicationSummary}
        relatedInformationItems={[]}
        releaseVersionSummary={testReleaseVersionSummary}
      />,
    );

    expect(
      screen.queryByRole('heading', {
        name: 'Related information',
      }),
    ).not.toBeInTheDocument();
  });

  test('does not render pra section if no data', () => {
    render(
      <ReleaseHelpPage
        publicationSummary={testPublicationSummary}
        relatedInformationItems={testRelatedLinks}
        releaseVersionSummary={{
          ...testReleaseVersionSummary,
          preReleaseAccessList: '',
        }}
      />,
    );

    expect(
      screen.queryByRole('heading', {
        name: 'Pre-release access list',
      }),
    ).not.toBeInTheDocument();
  });
});
