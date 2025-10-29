import FilterAccordion from '@common/modules/table-tool/components/FilterAccordion';
import render from '@common-test/render';
import { screen } from '@testing-library/react';
import React from 'react';

describe('FilterAccordion', () => {
  test('renders correctly', () => {
    render(
      <FilterAccordion label="Test label" id="test-id">
        content
      </FilterAccordion>,
    );

    expect(
      screen.getByRole('button', { name: 'Test label - show options' }),
    ).toBeInTheDocument();

    expect(screen.getByText('content')).toBeInTheDocument();
  });

  test('is closed by default and expands when clicked', async () => {
    const { user } = render(
      <FilterAccordion label="Test label" id="test-id">
        <h2>content</h2>
      </FilterAccordion>,
    );

    const button = screen.getByRole('button', {
      name: 'Test label - show options',
    });

    expect(button).toHaveAttribute('aria-expanded', 'false');

    await user.click(button);

    expect(button).toHaveAttribute('aria-expanded', 'true');

    expect(
      screen.getByRole('button', {
        name: 'Test label - hide options',
      }),
    ).toBeInTheDocument();
  });

  test('is expanded when open is set to true', async () => {
    const { user } = render(
      <FilterAccordion label="Test label" id="test-id" open>
        <h2>content</h2>
      </FilterAccordion>,
    );

    const button = screen.getByRole('button', {
      name: 'Test label - hide options',
    });

    expect(button).toHaveAttribute('aria-expanded', 'true');

    await user.click(button);

    expect(button).toHaveAttribute('aria-expanded', 'false');
  });

  test('cannot be opened or closed when preventToggle is set', async () => {
    const { user } = render(
      <FilterAccordion label="Test label" id="test-id" open preventToggle>
        <h2>content</h2>
      </FilterAccordion>,
    );

    const button = screen.getByRole('button', {
      name: 'Test label - hide options',
    });

    expect(button).toHaveAttribute('aria-expanded', 'true');

    await user.click(button);

    expect(button).toHaveAttribute('aria-expanded', 'true');
  });
});
