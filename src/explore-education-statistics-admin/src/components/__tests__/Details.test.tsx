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
});
