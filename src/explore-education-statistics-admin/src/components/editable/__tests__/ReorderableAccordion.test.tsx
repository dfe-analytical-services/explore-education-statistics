import React from 'react';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import ReorderableAccordion from '../ReorderableAccordion';

describe('ReorderableAccordion', () => {
  test("it doesn't show controls if `canUpdateRelease` is false", () => {
    render(
      <ReorderableAccordion
        canUpdateRelease={false}
        id="test"
        onReorder={() => noop}
        heading="test"
        onSectionOpen={() => noop}
        onToggleAll={() => noop}
      >
        reorder secton
      </ReorderableAccordion>,
    );

    expect(
      screen.queryByRole('button', { name: 'Reorder sections' }),
    ).not.toBeInTheDocument();
  });

  test('it shows controls if `canUpdateRelease` is true', () => {
    render(
      <ReorderableAccordion
        canUpdateRelease
        id="test"
        onReorder={() => noop}
        heading="test"
        onSectionOpen={() => noop}
        onToggleAll={() => noop}
      >
        reorder secton
      </ReorderableAccordion>,
    );

    expect(
      screen.getByRole('button', { name: 'Reorder sections' }),
    ).toBeInTheDocument();
  });

  test('it reorders sections', () => {
    const onReorder = jest.fn();
    render(
      <ReorderableAccordion
        canUpdateRelease
        id="test"
        onReorder={onReorder}
        heading="test"
        onSectionOpen={() => noop}
        onToggleAll={() => noop}
      >
        reorder secton
      </ReorderableAccordion>,
    );

    const reorderButton = screen.getByRole('button', { name: 'Reorder' });
    reorderButton.click();

    expect(
      screen.getByRole('button', { name: 'Save order' }),
    ).toBeInTheDocument();

    expect(
      screen.getByTestId('reorderableAccordionSection'),
    ).toBeInTheDocument();
  });
});
