import render from '@common-test/render';
import { waitFor, screen, within } from '@testing-library/react';
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

    const button = screen.getByRole('button', { name: /Test heading/ });

    expect(button).toHaveAttribute('aria-expanded', 'false');
  });

  test('renders with section content not expanded by default', () => {
    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    expect(
      screen.getByRole('button', { name: /Test heading/ }),
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
      screen.getByRole('button', { name: /Test heading/ }),
    ).toHaveAttribute('aria-expanded', 'true');
  });

  test('clicking heading makes the section content expanded', async () => {
    const { user } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    const heading = screen.getByRole('button', { name: /Test heading/ });

    expect(heading).toHaveAttribute('aria-expanded', 'false');

    await user.click(heading);

    expect(heading).toHaveAttribute('aria-expanded', 'true');
  });

  test('generates section IDs in correct order', () => {
    render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading">Test content</AccordionSection>
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    const headings = screen.getAllByRole('button', { name: /Test heading/ });
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

    const headings = screen.getAllByRole('button', { name: /Test heading/ });
    const contents = screen.getAllByText('Test content');

    expect(headings[0]).toHaveAttribute('id', 'custom-1-heading');
    expect(contents[0]).toHaveAttribute('id', 'custom-1-content');

    expect(headings[1]).toHaveAttribute('id', 'test-sections-2-heading');
    expect(contents[1]).toHaveAttribute('id', 'test-sections-2-content');
  });

  test('scrolls to and opens section if location hash matches section heading ID', async () => {
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

    const accordionSections = screen.getAllByTestId('accordionSection');
    expect(
      await within(accordionSections[0]).findByText('Hide'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: /Test heading 1/ }),
    ).toHaveAttribute('aria-expanded', 'true');
  });

  test('scrolls to and opens section if location hash matches section content ID', async () => {
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

    const accordionSections = screen.getAllByTestId('accordionSection');
    expect(
      await within(accordionSections[0]).findByText('Hide'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: /Test heading 1/ }),
    ).toHaveAttribute('aria-expanded', 'true');

    await waitFor(() => {
      expect(content.scrollIntoView).toHaveBeenCalled();
    });
  });

  test('scrolls to and opens section if location hash matches an element in the section content', async () => {
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

    const accordionSections = screen.getAllByTestId('accordionSection');
    expect(
      await within(accordionSections[1]).findByText('Hide'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: /Test heading 2/ }),
    ).toHaveAttribute('aria-expanded', 'true');

    await waitFor(() => {
      const content = container.querySelector('#test-heading') as HTMLElement;
      expect(content.scrollIntoView).toHaveBeenCalled();
    });
  });

  test('accordion sections that were opened when the location hash matches an element in the section content can be closed', async () => {
    window.location.hash = '#test-sections-1-content';

    const { user } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2">
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    const accordionSections = screen.getAllByTestId('accordionSection');

    expect(
      await within(accordionSections[0]).findByText('Hide'),
    ).toBeInTheDocument();
    const heading = screen.getByRole('button', { name: /Test heading 1/ });

    expect(heading).toHaveAttribute('aria-expanded', 'true');

    await user.click(heading);

    expect(
      await within(accordionSections[0]).findByText('Show'),
    ).toBeInTheDocument();

    expect(heading).toHaveAttribute('aria-expanded', 'false');
  });

  test('does not render `Show/Hide all sections` button if `showOpenAll` is false', () => {
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
      screen.queryByRole('button', { name: /Show all sections/ }),
    ).not.toBeInTheDocument();
  });

  test('clicking on `Show all sections` reveals all sections', async () => {
    const { user } = render(
      <Accordion id="test-sections">
        <AccordionSection heading="Test heading 1">
          Test content 1
        </AccordionSection>
        <AccordionSection heading="Test heading 2" open>
          Test content 2
        </AccordionSection>
      </Accordion>,
    );

    const button = screen.getByRole('button', {
      name: 'Show all sections',
    });
    const sections = screen.getAllByRole('button', { name: /Test heading/ });

    expect(button).toHaveAttribute('aria-expanded', 'false');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'false');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'true');

    await user.click(button);

    expect(button).toHaveAttribute('aria-expanded', 'true');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'true');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'true');
  });

  test('clicking on `Show all sections` causes `onToggleAll` handler to be called with new state', async () => {
    const toggleAll = jest.fn();

    const { user } = render(
      <Accordion id="test-sections" onToggleAll={toggleAll}>
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    expect(toggleAll).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Show all sections' }));

    expect(toggleAll).toHaveBeenCalledWith(true);
  });

  test('clicking on `Hide all sections` closes all sections', async () => {
    const { user } = render(
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
      name: 'Hide all sections',
    });
    const sections = screen.getAllByRole('button', { name: /Test heading/ });

    expect(closeAllButton).toHaveAttribute('aria-expanded', 'true');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'true');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'true');

    await user.click(closeAllButton);

    expect(closeAllButton).toHaveAttribute('aria-expanded', 'false');
    expect(sections[0]).toHaveAttribute('aria-expanded', 'false');
    expect(sections[1]).toHaveAttribute('aria-expanded', 'false');
  });

  test('clicking on `Hide all sections` causes `onToggleAll` handler to be called with new state', async () => {
    const toggleAll = jest.fn();

    const { user } = render(
      <Accordion id="test-sections" onToggleAll={toggleAll}>
        <AccordionSection heading="Test heading" open>
          Test content
        </AccordionSection>
      </Accordion>,
    );

    expect(toggleAll).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Hide all sections' }));

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

    const hiddenText = container.querySelectorAll('.govuk-visually-hidden');

    expect(hiddenText).toHaveLength(2);
    expect(hiddenText[0]).toHaveTextContent('Academic year 2016/17 sections');
    expect(hiddenText[1]).toHaveTextContent(',');
  });

  test('it renders the correct aria-label', async () => {
    const { user } = render(
      <Accordion
        id="test-sections"
        toggleAllHiddenText="Academic year 2016/17 sections"
      >
        <AccordionSection heading="Test heading">Test content</AccordionSection>
      </Accordion>,
    );

    expect(
      screen.getByRole('button', { name: /Test heading/ }),
    ).toHaveAttribute('aria-label', 'Test heading, show this section');

    await user.click(screen.getByRole('button', { name: /Test heading/ }));

    expect(
      screen.getByRole('button', { name: /Test heading/ }),
    ).toHaveAttribute('aria-label', 'Test heading, hide this section');
  });
});
