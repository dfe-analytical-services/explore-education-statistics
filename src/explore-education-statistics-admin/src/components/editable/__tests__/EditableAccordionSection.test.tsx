import EditableAccordion from '@admin/components/editable/EditableAccordion';
import EditableAccordionSection from '@admin/components/editable/EditableAccordionSection';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import {
  render as baseRender,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { ReactElement } from 'react';

describe('EditableAccordionSection', () => {
  describe('preview mode', () => {
    test('renders correctly', () => {
      render(
        <EditableAccordionSection
          id="section-1"
          heading="Test section"
          onHeadingChange={noop}
          onRemoveSection={noop}
        >
          <p>Test content</p>
        </EditableAccordionSection>,
      );

      expect(
        screen.getByRole('heading', { name: 'Test section' }),
      ).toBeInTheDocument();
      expect(screen.getByText('Test content')).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Edit section title' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Remove this section' }),
      ).not.toBeInTheDocument();
    });

    function render(element: ReactElement) {
      baseRender(
        <EditingContextProvider editingMode="preview">
          <EditableAccordion
            onAddSection={noop}
            onReorder={noop}
            id="accordion"
          >
            {element}
          </EditableAccordion>
        </EditingContextProvider>,
      );
    }
  });

  describe('edit mode', () => {
    test('renders correctly with main props', () => {
      render(
        <EditableAccordionSection
          id="section-1"
          heading="Test section"
          onHeadingChange={noop}
          onRemoveSection={noop}
        >
          <p>Test content</p>
        </EditableAccordionSection>,
      );

      expect(
        screen.getByRole('heading', { name: 'Test section' }),
      ).toBeInTheDocument();
      expect(screen.getByText('Test content')).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Edit section title' }),
      ).not.toBeAriaDisabled();
      expect(
        screen.getByRole('button', { name: 'Remove this section' }),
      ).not.toBeAriaDisabled();
    });

    test('renders additional header buttons', () => {
      render(
        <EditableAccordionSection
          id="section-1"
          heading="Test section"
          headerButtons={<button type="button">Another header button</button>}
          onHeadingChange={noop}
          onRemoveSection={noop}
        >
          <p>Test content</p>
        </EditableAccordionSection>,
      );

      expect(
        screen.getByRole('button', { name: 'Another header button' }),
      ).toBeInTheDocument();
    });

    test('disables remove button when `disabledRemoveSectionTooltip` is set', () => {
      render(
        <EditableAccordionSection
          id="section-1"
          heading="Test section"
          disabledRemoveSectionTooltip="Cannot remove section"
          onHeadingChange={noop}
          onRemoveSection={noop}
        >
          <p>Test content</p>
        </EditableAccordionSection>,
      );

      expect(
        screen.getByRole('button', { name: 'Remove this section' }),
      ).toBeAriaDisabled();
    });

    test('disables change heading button when `disabledHeadingChangeTooltip` is set', () => {
      render(
        <EditableAccordionSection
          id="section-1"
          heading="Test section"
          disabledHeadingChangeTooltip="Cannot change heading"
          onHeadingChange={noop}
          onRemoveSection={noop}
        />,
      );

      expect(
        screen.getByRole('button', { name: 'Edit section title' }),
      ).toBeAriaDisabled();
    });

    test('clicking remove button shows confirmation modal', async () => {
      render(
        <EditableAccordionSection
          id="section-1"
          heading="Test section"
          onHeadingChange={noop}
          onRemoveSection={noop}
        />,
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Remove this section' }),
      );

      await waitFor(() => {
        expect(screen.getByText('Removing section')).toBeInTheDocument();
      });

      const modal = screen.getByRole('dialog');

      expect(modal).toBeInTheDocument();
      expect(within(modal).getByText('Removing section')).toBeInTheDocument();
      expect(
        within(modal).getByText(
          /Are you sure you want to remove the following section/,
        ),
      ).toBeInTheDocument();
      expect(
        within(modal).getByText(
          /Are you sure you want to remove the following section/,
        ),
      ).toBeInTheDocument();
      expect(
        within(modal).getByText('"Test section"', { collapseWhitespace: true }),
      ).toBeInTheDocument();
    });

    test('confirming remove modal calls `onRemoveSection` handler', async () => {
      const handleRemoveSection = jest.fn();

      render(
        <EditableAccordionSection
          id="section-1"
          heading="Test section"
          onHeadingChange={noop}
          onRemoveSection={handleRemoveSection}
        />,
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Remove this section' }),
      );

      await waitFor(() => {
        expect(screen.getByText('Removing section')).toBeInTheDocument();
      });

      expect(handleRemoveSection).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

      expect(handleRemoveSection).toHaveBeenCalledTimes(1);
    });

    function render(element: ReactElement) {
      baseRender(
        <EditingContextProvider editingMode="edit">
          <EditableAccordion
            onAddSection={noop}
            onReorder={noop}
            id="accordion"
          >
            {element}
          </EditableAccordion>
        </EditingContextProvider>,
      );
    }
  });
});
