import React from 'react';
import { fireEvent, render, wait } from 'react-testing-library';
import Details from '../Details';

describe('Details', () => {
  test('renders correctly', async () => {
    const { container } = render(
      <Details summary="Test summary" id="test-details">
        Test content
      </Details>,
    );

    await wait();

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with `open` prop set to true', async () => {
    const { getByText, container } = render(
      <Details summary="Test summary" id="test-details" open>
        Test content
      </Details>,
    );

    await wait();

    expect(container.querySelector('[open]')).not.toBeNull();

    expect(container.querySelector('summary')).toHaveAttribute(
      'aria-expanded',
      'true',
    );
    expect(getByText('Test content')).toHaveAttribute('aria-hidden', 'false');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('clicking the summary reveals the content', async () => {
    const { container, getByText } = render(
      <Details summary="Test summary" id="test-details">
        Test content
      </Details>,
    );

    await wait();

    const content = getByText('Test content');
    const summary = container.querySelector('summary') as HTMLElement;

    expect(summary).toHaveAttribute('aria-expanded', 'false');
    expect(content).toHaveAttribute('aria-hidden', 'true');

    fireEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(content).toHaveAttribute('aria-hidden', 'false');
  });

  test('onToggle handler returns true when expanded', async () => {
    const handleToggle = jest.fn();

    const { container } = render(
      <Details summary="Test summary" id="test-details" onToggle={handleToggle}>
        Test content
      </Details>,
    );

    await wait();

    const summary = container.querySelector('summary') as HTMLElement;

    expect(summary).toHaveAttribute('aria-expanded', 'false');

    fireEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(handleToggle).toHaveBeenCalledWith(true);
  });

  test('onToggle handler returns false when collapsed', async () => {
    const handleToggle = jest.fn();

    const { container } = render(
      <Details summary="Test summary" id="test-details" onToggle={handleToggle}>
        Test content
      </Details>,
    );

    await wait();

    const summary = container.querySelector('summary') as HTMLElement;

    fireEvent.click(summary);
    fireEvent.click(summary);

    expect(handleToggle).toHaveBeenNthCalledWith(1, true);
    expect(handleToggle).toHaveBeenNthCalledWith(2, false);
  });
});
