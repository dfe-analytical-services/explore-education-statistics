import { render, screen, waitFor } from '@testing-library/react';
import KeyStat from '@common/modules/find-statistics/components/KeyStat';
import React from 'react';

describe('KeyStat', () => {
  test('renders correctly with all props provided', async () => {
    render(
      <KeyStat
        title="Number of applications received"
        statistic="608180"
        trend="Down from 620,330 in 2017"
        guidanceTitle="What is the number of applications received?"
        guidanceText="Total number of applications received for places at primary and secondary schools."
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
        '608,180',
      );

      expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
        'Down from 620,330 in 2017',
      );

      expect(
        screen.getByRole('button', {
          name: 'What is the number of applications received?',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'Total number of applications received for places at primary and secondary schools.',
      );
    });
  });

  test('renders correctly with a blank guidanceTitle', async () => {
    render(
      <KeyStat
        title="Number of applications received"
        statistic="608180"
        trend="Down from 620,330 in 2017"
        guidanceTitle={undefined}
        guidanceText="Total number of applications received for places at primary and secondary schools."
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
        '608,180',
      );

      expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
        'Down from 620,330 in 2017',
      );

      expect(
        screen.getByRole('button', {
          name: 'Help for Number of applications received',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'Total number of applications received for places at primary and secondary schools.',
      );
    });
  });

  test('renders correctly without trend and guidance text', async () => {
    render(
      <KeyStat
        title="Number of applications received"
        statistic="608180"
        guidanceTitle="This shouldn't appear"
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
        '608,180',
      );

      expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
      expect(screen.queryByRole('button')).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('keyStat-guidanceText'),
      ).not.toBeInTheDocument();
    });
  });
});
