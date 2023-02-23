import { render, waitFor, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import Accordion from '../Accordion';
import AccordionSection from '../AccordionSection';

describe('Accordion', () => {
  test('renders with section content corresponding to heading button', () => {
    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    const button = screen.getByRole('button', { name: 'Test heading' });

    expect(button).toHaveAttribute('aria-expanded', 'false');

    expect(screen.getByText('Test content')).toHaveAttribute(
      'aria-labelledby',
      button.id,
    );
  });

  test('renders with section content not expanded by default', () => {
    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    expect(
      screen.getByRole('button', { name: 'Test heading' }),
    ).toHaveAttribute('aria-expanded', 'false');
  });

  test('renders with section content expanded when `open` is true', () => {
    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading" open>
          Test content
        </AccordionSection>
      </Accordion>,
    );

    expect(
      screen.getByRole('button', { name: 'Test heading' }),
    ).toHaveAttribute('aria-expanded', 'true');
  });

  test('clicking heading makes the section content expanded', () => {
    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    const heading = screen.getByRole('button', { name: 'Test heading' });

    expect(heading).toHaveAttribute('aria-expanded', 'false');

    userEvent.click(heading);

    expect(heading).toHaveAttribute('aria-expanded', 'true');
  });

  test('generates section IDs in correct order', () => {
    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">Test content</AccordionSection>
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    const headings = screen.getAllByRole('button', { name: 'Test heading' });
    const contents = screen.getAllByText('Test content');

    expect(headings[0]).toHaveAttribute('id', 'test-sections-1-heading');
    expect(contents[0]).toHaveAttribute('id', 'test-sections-1-content');

    expect(headings[1]).toHaveAttribute('id', 'test-sections-2-heading');
    expect(contents[1]).toHaveAttribute('id', 'test-sections-2-content');
  });

  test('can use custom heading or content IDs for sections', () => {
    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading" id="custom-1">
          Test content
        </AccordionSection>
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    const headings = screen.getAllByRole('button', { name: 'Test heading' });
    const contents = screen.getAllByText('Test content');

    expect(headings[0]).toHaveAttribute('id', 'custom-1-heading');
    expect(contents[0]).toHaveAttribute('id', 'custom-1-content');

    expect(headings[1]).toHaveAttribute('id', 'test-sections-2-heading');
    expect(contents[1]).toHaveAttribute('id', 'test-sections-2-content');
  });

  test('scrolls to and opens section if location hash matches section heading ID', async () => {
    jest.useFakeTimers();
    window.location.hash = '#test-sections-1-heading';

    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2">
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    const heading = screen.getByRole('button', { name: 'Test heading 1' });

    jest.runOnlyPendingTimers();

    expect(heading).toHaveAttribute('aria-expanded', 'true');
  });

  test('scrolls to and opens section if location hash matches section content ID', async () => {
    jest.useFakeTimers();
    window.location.hash = '#test-sections-1-content';

    const { container } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2">
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    const content = container.querySelector(
      '#test-sections-1-content',
    ) as HTMLElement;

    jest.advanceTimersByTime(300);

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Test heading 1' }),
      ).toHaveAttribute('aria-expanded', 'true');
      expect(content.scrollIntoView).toHaveBeenCalled();
    });
  });

  test('scrolls to and opens section if location hash matches an element in the section content', async () => {
    jest.useFakeTimers();
    window.location.hash = '#test-heading';

    const { container } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2">
          <h2 id="test-heading">Test content heading</h2>
        </AccordionSection>
      </Accordion>,
    );

    jest.advanceTimersByTime(300);

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Test heading 2' }),
      ).toHaveAttribute('aria-expanded', 'true');

      const content = container.querySelector('#test-heading') as HTMLElement;

      expect(content.scrollIntoView).toHaveBeenCalled();
    });
  });

  test('accordion sections that were opened when the location hash matches an element in the section content can be closed', () => {
    jest.useFakeTimers();
    window.location.hash = '#test-sections-1-content';

    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2">
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    jest.runOnlyPendingTimers();

    const heading = screen.getByRole('button', { name: 'Test heading 1' });

    expect(heading).toHaveAttribute('aria-expanded', 'true');

    userEvent.click(heading);

    expect(heading).toHaveAttribute('aria-expanded', 'false');
  });

  test('does not render `Open/close all sections` button if `showOpenAll` is false', () => {
    render(
      <Accordion id="test-sections" showOpenAll={false}>
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2">
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    expect(
      screen.queryByRole('button', { name: /Close all/ }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: /Open all/ }),
    ).not.toBeInTheDocument();
  });

  test('clicking on `Open all sections` reveals all sections', () => {
    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2" open>
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    const button = screen.getByRole('button', { name: 'Open all sections' });
    const sections = screen.getAllByRole('button', { name: /Test heading/ });

    expect(button).toHaveAttribute('aria-expanded', 'false');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'false');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'true');

    userEvent.click(button);

    expect(button).toHaveAttribute('aria-expanded', 'true');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'true');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'true');
  });

  test('clicking on `Open all` causes `onToggleAll` handler to be called with new state', () => {
    const toggleAll = jest.fn();

    render(
      <Accordion id="test-sections" onToggleAll={toggleAll}>
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    expect(toggleAll).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Open all sections' }));

    expect(toggleAll).toHaveBeenCalledWith(true);
  });

  test('clicking on `Close all sections` closes all sections', () => {
    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1" open>
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2" open>
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    const closeAllButton = screen.getByRole('button', {
      name: 'Close all sections',
    });
    const sections = screen.getAllByRole('button', { name: /Test heading/ });

    expect(closeAllButton).toHaveAttribute('aria-expanded', 'true');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'true');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'true');

    userEvent.click(closeAllButton);

    expect(closeAllButton).toHaveAttribute('aria-expanded', 'false');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'false');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'false');
  });

  test('clicking on `Close all sections` causes `onToggleAll` handler to be called with new state', () => {
    const toggleAll = jest.fn();

    render(
      <Accordion id="test-sections" onToggleAll={toggleAll}>
        <AccordionSection heading="Test heading" open>
          Test content
        </AccordionSection>
      </Accordion>,
    );

    expect(toggleAll).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Close all sections' }));

    expect(toggleAll).toHaveBeenCalledWith(false);
  });

  test('it renders visually hidden text', () => {
    const { container } = render(
      <Accordion
        id="test-sections"
        toggleAllHiddenText="Academic year 2016/17 sections"
      >
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    expect(container.querySelectorAll('.govuk-visually-hidden')).toHaveLength(
      1,
    );
    expect(container.querySelector('.govuk-visually-hidden')).toHaveTextContent(
      'Academic year 2016/17 sections',
    );
  });
});
