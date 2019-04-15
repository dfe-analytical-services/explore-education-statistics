import React from 'react';
import { render } from 'react-testing-library';
import ErrorSummary from '../ErrorSummary';

describe('ErrorSummary', () => {
  test('renders correctly with some errors', () => {
    const { container, getByText } = render(
      <ErrorSummary
        id="test-errors"
        errors={[
          { id: 'test-error-1', message: 'Something went wrong 1' },
          { id: 'test-error-2', message: 'Something went wrong 2' },
        ]}
      />,
    );

    expect(getByText('Something went wrong 1')).toBeDefined();
    expect(getByText('Something went wrong 2')).toBeDefined();

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('does not render when there are no errors', () => {
    const { container } = render(<ErrorSummary id="test-errors" errors={[]} />);

    expect(container.innerHTML).toBe('');
  });

  test('aria-labelledby matches the summary title id', () => {
    const { container, getByText } = render(
      <ErrorSummary
        errors={[
          {
            id: 'test-error-1',
            message: 'Something went wrong',
          },
        ]}
        id="test-errors"
        title="The world is ending"
      />,
    );

    expect(container.querySelector('[aria-labelledby]')).toHaveAttribute(
      'aria-labelledby',
      'test-errors-title',
    );
    expect(getByText('The world is ending')).toHaveAttribute(
      'id',
      'test-errors-title',
    );
  });
});
