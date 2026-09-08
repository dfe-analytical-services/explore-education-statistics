import ScreenerStatus from '@admin/pages/release/data/components/ScreenerStatus';
import { render, screen } from '@testing-library/react';

describe('ScreenerStatus', () => {
  test('renders a terminal status without progress', () => {
    render(
      <ScreenerStatus
        dataSetTitle="Test data set"
        percentageComplete={100}
        status="PendingReview"
      />,
    );

    expect(screen.getByText('Pending review')).toBeInTheDocument();
    expect(screen.queryByRole('progressbar')).not.toBeInTheDocument();
  });

  test('renders progress while screening is still in flight', () => {
    render(
      <ScreenerStatus
        dataSetTitle="Test data set"
        percentageComplete={25}
        status="Screening"
      />,
    );

    expect(screen.getByText('Screening')).toBeInTheDocument();
    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuenow',
      '25',
    );
  });

  test.each([
    ['ScreenerError', 'Screener error'],
    ['PendingImport', 'Pending import'],
    ['FailedScreening', 'Failed screening'],
  ] as const)('renders the %s status as "%s"', (status, label) => {
    render(
      <ScreenerStatus
        dataSetTitle="Test data set"
        percentageComplete={100}
        status={status}
      />,
    );

    expect(screen.getByText(label)).toBeInTheDocument();
  });
});
