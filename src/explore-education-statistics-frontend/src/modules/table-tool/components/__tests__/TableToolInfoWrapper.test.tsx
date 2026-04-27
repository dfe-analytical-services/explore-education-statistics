import render from '@common-test/render';
import TableToolInfoWrapper from '@frontend/modules/table-tool/components/TableToolInfoWrapper';
import {
  testSelectedPublicationWithLatestRelease,
  testPublicationMethodologyList,
} from '@frontend/modules/table-tool/components/__tests__/__data__/tableData';
import { screen } from '@testing-library/react';
import React from 'react';

jest.mock('@common/services/publicationService');

describe('TableToolInfoWrapper', () => {
  test('renders successfully', async () => {
    render(
      <TableToolInfoWrapper
        publicationMethodologies={{
          ...testPublicationMethodologyList,
          externalMethodology: {
            url: 'http://test.com',
            title: 'An external methodology',
          },
        }}
        selectedPublication={testSelectedPublicationWithLatestRelease}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Related information',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Test publication, Latest Release Title',
      }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/test-publication/latest-release-slug',
    );
    expect(
      screen.getByRole('link', {
        name: 'methodology title',
      }),
    ).toHaveAttribute('href', '/methodology/m1');
    expect(
      screen.getByRole('link', {
        name: 'An external methodology',
      }),
    ).toHaveAttribute('href', 'http://test.com');

    expect(
      screen.getByRole('heading', {
        name: 'Contact us',
      }),
    ).toBeInTheDocument();
    expect(screen.getByText('The team name')).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'team@name.com',
      }),
    ).toBeInTheDocument();
    expect(screen.getByText(/A person/)).toBeInTheDocument();
    expect(screen.getByText(/012345/)).toBeInTheDocument();
  });
});
