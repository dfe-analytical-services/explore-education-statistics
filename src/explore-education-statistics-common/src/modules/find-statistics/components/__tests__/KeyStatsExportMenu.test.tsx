import KeyStatsExportMenu from '@common/modules/find-statistics/components/KeyStatsExportMenu';
import downloadFile from '@common/utils/file/downloadFile';
import React, { createRef } from 'react';
import { screen, waitFor } from '@testing-library/react';
import render from '@common-test/render';

jest.mock('@common/utils/file/downloadFile', () => jest.fn());

jest.mock('@common/components/ChartsToPNG', () => () => [
  jest.fn().mockResolvedValue('data:image/png;base64,test'),
]);

describe('KeyStatsExportMenu', () => {
  test('renders', () => {
    const ref = createRef<HTMLDivElement>();

    render(
      <KeyStatsExportMenu
        containerRef={ref}
        publicationTitle="Publication title"
        releaseTitle="Release title"
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Export options for headlines' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Copy headlines to clipboard' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Download headlines as PNG' }),
    ).not.toBeInTheDocument();
  });

  test('renders the options when expanded', async () => {
    const ref = createRef<HTMLDivElement>();

    const { user } = render(
      <KeyStatsExportMenu
        containerRef={ref}
        publicationTitle="Publication title"
        releaseTitle="Release title"
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Export options for headlines' }),
    );

    expect(
      screen.getByRole('button', { name: 'Copy headlines to clipboard' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Download headlines as PNG' }),
    ).toBeInTheDocument();
  });

  test('downloads the PNG with the publication and release titles in the file name, replacing slashes with hyphens', async () => {
    const ref = createRef<HTMLDivElement>();

    const { user } = render(
      <KeyStatsExportMenu
        containerRef={ref}
        publicationTitle="Publication title"
        releaseTitle="Academic year 2016/17"
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Export options for headlines' }),
    );
    await user.click(
      screen.getByRole('button', { name: 'Download headlines as PNG' }),
    );

    await waitFor(() => {
      expect(downloadFile).toHaveBeenCalledWith({
        file: 'data:image/png;base64,test',
        extension: 'png',
        fileName: 'Headlines - Publication title - Academic year 2016-17',
      });
    });
  });
});
