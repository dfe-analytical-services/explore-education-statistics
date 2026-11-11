import KeyStatsExportMenu from '@common/modules/find-statistics/components/KeyStatsExportMenu';
import React, { createRef } from 'react';
import { screen } from '@testing-library/react';
import render from '@common-test/render';

describe('KeyStatsExportMenu', () => {
  test('renders', () => {
    const ref = createRef<HTMLDivElement>();

    render(<KeyStatsExportMenu containerRef={ref} />);

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

    const { user } = render(<KeyStatsExportMenu containerRef={ref} />);

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
});
