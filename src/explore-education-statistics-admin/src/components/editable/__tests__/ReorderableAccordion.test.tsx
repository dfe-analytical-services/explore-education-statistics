import React from 'react';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import ReorderableAccordion from '../ReorderableAccordion';
import ReorderableAccordionSection from '../ReorderableAccordionSection';

describe('ReorderableAccordion', () => {
  test("doesn't show controls if `canReorder` is false", () => {
    render(
      <ReorderableAccordion
        canReorder={false}
        heading="test accordion"
        reorderHiddenText="files"
        id="1"
        onReorder={noop}
      >
        <ReorderableAccordionSection id="1" heading="test section one">
          test accordion content section one
        </ReorderableAccordionSection>

        <ReorderableAccordionSection id="1" heading="test section two">
          test accordion content section two
        </ReorderableAccordionSection>
      </ReorderableAccordion>,
    );

    expect(
      screen.queryByRole('button', { name: 'Reorder files' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('test accordion content section one'),
    ).toBeInTheDocument();
    expect(
      screen.getByText('test accordion content section two'),
    ).toBeInTheDocument();
  });

  test('shows controls if `canReorder` is true', () => {
    const { container } = render(
      <ReorderableAccordion
        canReorder
        heading="test accordion"
        reorderHiddenText="files"
        id="1"
        onReorder={noop}
      >
        <ReorderableAccordionSection id="1" heading="test section one">
          test accordion content section one
        </ReorderableAccordionSection>

        <ReorderableAccordionSection id="2" heading="test section two">
          test accordion content section two
        </ReorderableAccordionSection>
      </ReorderableAccordion>,
    );

    expect(container.querySelector('.govuk-visually-hidden')).toHaveTextContent(
      'files',
    );

    expect(
      screen.getByRole('button', { name: 'Reorder files' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('test accordion content section one'),
    ).toBeInTheDocument();
    expect(
      screen.getByText('test accordion content section two'),
    ).toBeInTheDocument();
  });
});
