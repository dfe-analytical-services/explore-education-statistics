import ChartExportMenu from '@common/modules/charts/components/ChartExportMenu';
import React, { createRef } from 'react';
import { screen } from '@testing-library/react';
import render from '@common-test/render';

describe('ChartExportMenu', () => {
  test('renders', () => {
    const ref = createRef<HTMLTableElement>();

    render(<ChartExportMenu chartRef={ref} chartTitle="Test title" />);

    expect(
      screen.getByRole('button', { name: 'Export options for Test title' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Download chart as PNG' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Copy chart to clipboard' }),
    ).not.toBeInTheDocument();
  });

  test('renders the options when expanded', async () => {
    const ref = createRef<HTMLTableElement>();

    const { user } = render(
      <ChartExportMenu chartRef={ref} chartTitle="Test title" />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Export options for Test title' }),
    );

    expect(
      screen.getByRole('button', { name: 'Download chart as PNG' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Copy chart to clipboard' }),
    ).toBeInTheDocument();
  });
});
