import { fireEvent, render, wait } from '@testing-library/react';
import React from 'react';
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

  test('re-rendering does not change auto-generated `id`', () => {
    const { container, rerender } = render(
      <Details summary="Test summary">Test content</Details>,
    );

    const originalId = container.querySelector('.govuk-details__text')?.id;

    expect(originalId).toBeDefined();

    rerender(<Details summary="Test summary 2">Test content 2</Details>);

    const currentId = container.querySelector('.govuk-details__text')?.id;

    expect(currentId).toBe(originalId);
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

  test('clicking the summary collapses the content when `open` is true', async () => {
    const { container, getByText } = render(
      <Details summary="Test summary" id="test-details" open>
        Test content
      </Details>,
    );

    await wait();

    const content = getByText('Test content');
    const summary = container.querySelector('summary') as HTMLElement;

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(content).toHaveAttribute('aria-hidden', 'false');

    fireEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'false');
    expect(content).toHaveAttribute('aria-hidden', 'true');
  });

  test('changing `open` prop from false to true reveals the content', async () => {
    const { getByText, container, rerender } = render(
      <Details summary="Test summary" id="test-details">
        Test content
      </Details>,
    );

    await wait();

    const content = getByText('Test content');
    const summary = container.querySelector('summary');

    expect(summary).toHaveAttribute('aria-expanded', 'false');
    expect(content).toHaveAttribute('aria-hidden', 'true');

    rerender(
      <Details summary="Test summary" id="test-details" open>
        Test content
      </Details>,
    );

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(content).toHaveAttribute('aria-hidden', 'false');
  });

  test('changing `open` prop from true to false hides the content', async () => {
    const { getByText, container, rerender } = render(
      <Details summary="Test summary" id="test-details" open>
        Test content
      </Details>,
    );

    await wait();

    const content = getByText('Test content');
    const summary = container.querySelector('summary');

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(content).toHaveAttribute('aria-hidden', 'false');

    rerender(
      <Details summary="Test summary" id="test-details">
        Test content
      </Details>,
    );

    expect(summary).toHaveAttribute('aria-expanded', 'false');
    expect(content).toHaveAttribute('aria-hidden', 'true');
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
    expect(handleToggle).toHaveBeenCalledWith(
      true,
      expect.objectContaining({ target: summary }),
    );
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

    expect(handleToggle).toHaveBeenNthCalledWith(
      1,
      true,
      expect.objectContaining({ target: summary }),
    );
    expect(handleToggle).toHaveBeenNthCalledWith(
      2,
      false,
      expect.objectContaining({ target: summary }),
    );
  });
});
