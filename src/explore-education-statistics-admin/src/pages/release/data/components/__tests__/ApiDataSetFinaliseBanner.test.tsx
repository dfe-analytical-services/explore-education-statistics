import ApiDataSetFinaliseBanner from '@admin/pages/release/data/components/ApiDataSetFinaliseBanner';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import { MemoryRouter } from 'react-router';

describe('ApiDataSetFinaliseBanner', () => {
  test('renders the finalising banner when `finalisingStatus` is `finalising', () => {
    render(
      <ApiDataSetFinaliseBanner
        dataSetId="data-set-id"
        dataSetVersionId="data-set-version-id"
        releaseVersionId="release-id"
        publicationId="publication-id"
        finalisingStatus="finalising"
        onFinalise={noop}
      />,
    );
    const banner = within(screen.getByTestId('notificationBanner'));
    expect(
      banner.getByRole('heading', { name: 'Finalising' }),
    ).toBeInTheDocument();
    expect(
      banner.getByText('Finalising draft API data set version'),
    ).toBeInTheDocument();
  });

  test('renders the action required banner when `draftVersionStatus` is `Mapping', () => {
    render(
      <ApiDataSetFinaliseBanner
        dataSetId="data-set-id"
        dataSetVersionId="data-set-version-id"
        releaseVersionId="release-id"
        publicationId="publication-id"
        draftVersionStatus="Mapping"
        onFinalise={noop}
      />,
    );
    const banner = within(screen.getByTestId('notificationBanner'));
    expect(
      banner.getByRole('heading', { name: 'Action required' }),
    ).toBeInTheDocument();
    expect(
      banner.getByText('Draft API data set version is ready to be finalised'),
    ).toBeInTheDocument();
    expect(
      banner.getByRole('button', { name: 'Finalise this data set version' }),
    ).toBeInTheDocument();
  });

  test('renders the success banner when `draftVersionStatus` is `Failed', () => {
    render(
      <MemoryRouter>
        <ApiDataSetFinaliseBanner
          dataSetId="data-set-id"
          dataSetVersionId="data-set-version-id"
          releaseVersionId="release-id"
          publicationId="publication-id"
          draftVersionStatus="Draft"
          onFinalise={noop}
        />
      </MemoryRouter>,
    );
    const banner = within(screen.getByTestId('notificationBanner'));
    expect(
      banner.getByText('Draft API data set version is ready to be published'),
    ).toBeInTheDocument();
    expect(
      banner.getByRole('heading', {
        name: 'Mappings finalised',
      }),
    ).toBeInTheDocument();
  });

  test('renders the error banner when `finalisingStatus` is `finalised` and `draftVersionStatus` is `Draft', () => {
    render(
      <ApiDataSetFinaliseBanner
        dataSetId="data-set-id"
        dataSetVersionId="data-set-version-id"
        releaseVersionId="release-id"
        publicationId="publication-id"
        draftVersionStatus="Failed"
        finalisingStatus="finalised"
        onFinalise={noop}
      />,
    );
    const banner = within(screen.getByTestId('notificationBanner'));
    expect(banner.getByText('There is a problem')).toBeInTheDocument();
    expect(
      banner.getByText('Data set version finalisation failed'),
    ).toBeInTheDocument();
  });
});

test('renders the finalising banner when `draftVersionStatus` is `Finalising`', () => {
  render(
    <ApiDataSetFinaliseBanner
      dataSetId="data-set-id"
      dataSetVersionId="data-set-version-id"
      releaseVersionId="release-id"
      publicationId="publication-id"
      draftVersionStatus="Finalising"
      onFinalise={noop}
    />,
  );
  const banner = within(screen.getByTestId('notificationBanner'));
  expect(
    banner.getByRole('heading', { name: 'Finalising' }),
  ).toBeInTheDocument();
  expect(
    banner.getByText('Finalising draft API data set version'),
  ).toBeInTheDocument();
  expect(
    banner.getByText('This process can take a few minutes.'),
  ).toBeInTheDocument();
});
