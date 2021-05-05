import React from 'react';
import { render, waitFor } from '@testing-library/react';
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

  test('does not gain focus or scroll into view when there are errors by default', async () => {
    const { container } = render(
      <ErrorSummary
        id="test-errors"
        errors={[
          { id: 'test-error-1', message: 'Something went wrong 1' },
          { id: 'test-error-2', message: 'Something went wrong 2' },
        ]}
      />,
    );

    const summary = container.querySelector(
      '.govuk-error-summary',
    ) as HTMLElement;

    await waitFor(() => {
      expect(summary).not.toHaveFocus();
      expect(summary).not.toHaveScrolledIntoView();
    });
  });

  test('gains focus and scrolls into view when there are errors and `focusOnError` is true', () => {
    const { container } = render(
      <ErrorSummary
        id="test-errors"
        focusOnError
        errors={[
          { id: 'test-error-1', message: 'Something went wrong 1' },
          { id: 'test-error-2', message: 'Something went wrong 2' },
        ]}
      />,
    );

    const summary = container.querySelector(
      '.govuk-error-summary',
    ) as HTMLElement;

    expect(summary).toHaveFocus();
    expect(summary).toHaveScrolledIntoView();
  });

  test('re-rendering with new errors does not gain focus or scroll into view', () => {
    const { container, getByText, rerender } = render(
      <ErrorSummary
        id="test-errors"
        focusOnError
        errors={[
          { id: 'test-error-1', message: 'Something went wrong 1' },
          { id: 'test-error-2', message: 'Something went wrong 2' },
        ]}
      />,
    );

    const summary = container.querySelector(
      '.govuk-error-summary',
    ) as HTMLElement;

    // Focus something else to prove summary does not re-gain focus
    getByText('Something went wrong 1').focus();

    rerender(
      <ErrorSummary
        id="test-errors"
        errors={[
          { id: 'test-error-1', message: 'Something went wrong 1' },
          { id: 'test-error-2', message: 'Something went wrong 2' },
          { id: 'test-error-3', message: 'Something went wrong 3' },
        ]}
      />,
    );

    expect(summary).not.toHaveFocus();
    expect(summary.scrollIntoView).toHaveBeenCalledTimes(1);
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
