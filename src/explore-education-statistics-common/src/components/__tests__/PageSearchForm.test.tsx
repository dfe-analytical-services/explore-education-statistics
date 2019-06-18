import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React from 'react';
import { fireEvent, render, wait } from 'react-testing-library';
import PageSearchForm from '../PageSearchForm';

describe('PageSearchForm', () => {
  test('renders results found in default elements', async () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <PageSearchForm />

        <div>
          <p>Not me</p>
          <p>Test 1</p>
        </div>

        <div>
          <h2>Not me</h2>
          <h2>Test 2</h2>
        </div>

        <div>
          <h3>Not me</h3>
          <h3>Test 3</h3>
        </div>

        <div>
          <h4>Not me</h4>
          <h4>Test 4</h4>
        </div>

        <ul>
          <li>Not me</li>
          <li>
            <strong>Test 5</strong>
          </li>
        </ul>
      </div>,
    );

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    expect(
      container.querySelector('#pageSearchForm-resultsLabel'),
    ).toHaveTextContent('Found 5 results');

    const options = container.querySelectorAll('[role="option"]');

    expect(options).toHaveLength(5);
    expect(options[0]).toHaveTextContent('Test 1');
    expect(options[1]).toHaveTextContent('Test 2');
    expect(options[2]).toHaveTextContent('Test 3');
    expect(options[3]).toHaveTextContent('Test 4');
    expect(options[4]).toHaveTextContent('Test 5');

    expect(container.querySelector('[role="listbox"]')).toMatchSnapshot();
  });

  test('renders results found in custom selectors', () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <PageSearchForm elementSelectors={['.target-1', '.target-2 p']} />

        <div>
          <p>Test</p>
        </div>
        <div>
          <h2>Test</h2>
        </div>
        <div>
          <h3>Test</h3>
        </div>
        <div>
          <h4>Test</h4>
        </div>
        <ul>
          <li>
            <strong>Test</strong>
          </li>
        </ul>

        <div className="target-1">
          <p>Test 1</p>
        </div>

        <div className="target-2">
          <h2>Section 1</h2>

          <p>Test 2</p>
        </div>
      </div>,
    );

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    expect(
      container.querySelector('#pageSearchForm-resultsLabel'),
    ).toHaveTextContent('Found 2 results');

    const options = container.querySelectorAll('[role="option"]');

    expect(options).toHaveLength(2);
    expect(options[0]).toHaveTextContent('Test 1');
    expect(options[1]).toHaveTextContent('Test 2');

    expect(container.querySelector('[role="listbox"]')).toMatchSnapshot();
  });

  test('result locations uses nearest heading', async () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <PageSearchForm />

        <div>
          <h2>Section 1</h2>
          <p>Test 1</p>
        </div>

        <h2>Not this section</h2>
        <h3>Section 2</h3>

        <ul>
          <li>
            <strong>Test 2</strong>
          </li>
        </ul>
      </div>,
    );

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    expect(
      container.querySelector('#pageSearchForm-resultsLabel'),
    ).toHaveTextContent('Found 2 results');

    const options = container.querySelectorAll('[role="option"]');

    expect(options).toHaveLength(2);
    expect(options[0]).toHaveTextContent('Test 1');
    expect(options[0]).toHaveTextContent('Section 1');
    expect(options[1]).toHaveTextContent('Test 2');
    expect(options[1]).toHaveTextContent('Section 2');

    expect(container.querySelector('[role="listbox"]')).toMatchSnapshot();
  });

  test('result locations include heading hierarchy', () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <PageSearchForm />

        <h2>Section 1</h2>

        <div>
          <h3>Section 2</h3>

          <div>
            <h4>Section 3</h4>
            <p>Test 1</p>
          </div>
        </div>
      </div>,
    );

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    expect(
      container.querySelector('#pageSearchForm-resultsLabel'),
    ).toHaveTextContent('Found 1 result');

    const options = container.querySelectorAll('[role="option"]');

    expect(options).toHaveLength(1);
    expect(options[0]).toHaveTextContent('Test 1');
    expect(options[0]).toHaveTextContent('Section 1 > Section 2 > Section 3');

    expect(container.querySelector('[role="listbox"]')).toMatchSnapshot();
  });

  test('result locations include accordion sections', () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <PageSearchForm />

        <h2>Section 1</h2>

        <div>
          <h3>Section 2</h3>

          <Accordion id="test-accordion">
            <AccordionSection heading="Accordion section 1">
              <h4>Section 3</h4>

              <p>Test 1</p>
            </AccordionSection>
            <AccordionSection heading="Accordion section 2">
              <p>Test 2</p>
            </AccordionSection>
          </Accordion>
        </div>
      </div>,
    );

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    expect(
      container.querySelector('#pageSearchForm-resultsLabel'),
    ).toHaveTextContent('Found 2 results');

    const options = container.querySelectorAll('[role="option"]');

    expect(options).toHaveLength(2);
    expect(options[0]).toHaveTextContent('Test 1');
    expect(options[0]).toHaveTextContent(
      'Section 1 > Section 2 > Accordion section 1 > Section 3',
    );
    expect(options[1]).toHaveTextContent('Test 2');
    expect(options[1]).toHaveTextContent(
      'Section 1 > Section 2 > Accordion section 2',
    );

    expect(container.querySelector('[role="listbox"]')).toMatchSnapshot();
  });

  test('clicking result scrolls and focuses the element', async () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <h2>Section 1</h2>
        <p id="target">Test</p>

        <PageSearchForm />
      </div>,
    );

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    fireEvent.click(container.querySelector('[role="option"]') as HTMLElement);

    jest.runOnlyPendingTimers();

    const target = container.querySelector('#target');

    expect(target).toHaveFocus();
    expect(target).toHaveScrolledIntoView();
  });

  test('pressing Enter on result scrolls and focuses the element', () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <h2>Section 1</h2>
        <p id="target">Test</p>

        <PageSearchForm />
      </div>,
    );

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    const listBox = container.querySelector('[role="listbox"]') as HTMLElement;

    fireEvent.keyDown(listBox, { key: 'ArrowDown' });
    fireEvent.keyDown(listBox, { key: 'Enter' });

    jest.runOnlyPendingTimers();

    const target = container.querySelector('#target');

    expect(target).toHaveFocus();
    expect(target).toHaveScrolledIntoView();
  });

  test('opens parent accordion of selected result', async () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <Accordion id="test-accordion">
          <AccordionSection heading="Section 1">
            <p id="target">Test</p>
          </AccordionSection>
        </Accordion>

        <PageSearchForm />
      </div>,
    );

    await wait();

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    const accordionSection = container.querySelector(
      '#test-accordion-heading-1',
    );

    expect(accordionSection).toHaveAttribute('aria-expanded', 'false');

    fireEvent.click(container.querySelector('[role="option"]') as HTMLElement);

    jest.runOnlyPendingTimers();

    expect(accordionSection).toHaveAttribute('aria-expanded', 'true');
  });

  test('opens parent details of selected result', async () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <Details summary="Details 1">
          <p id="target">Test</p>
        </Details>

        <PageSearchForm />
      </div>,
    );

    await wait();

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    const summary = container.querySelector('summary');

    expect(summary).toHaveAttribute('aria-expanded', 'false');

    fireEvent.click(container.querySelector('[role="option"]') as HTMLElement);

    jest.runOnlyPendingTimers();

    expect(summary).toHaveAttribute('aria-expanded', 'true');
  });

  test('opens parent tab section of selected result', async () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <Tabs>
          <TabsSection title="Tab 1" id="section-1">
            <p>Something</p>
          </TabsSection>
          <TabsSection title="Tab 2" id="section-2">
            <p id="target">Test</p>
          </TabsSection>
        </Tabs>

        <PageSearchForm />
      </div>,
    );

    await wait();

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    const tabs = container.querySelectorAll('[role="tab"]');

    expect(tabs[0]).toHaveTextContent('Tab 1');
    expect(tabs[1]).toHaveTextContent('Tab 2');

    expect(tabs[0]).toHaveAttribute('aria-selected', 'true');
    expect(tabs[1]).toHaveAttribute('aria-selected', 'false');

    fireEvent.click(container.querySelector('[role="option"]') as HTMLElement);

    jest.runOnlyPendingTimers();

    expect(tabs[0]).toHaveAttribute('aria-selected', 'false');
    expect(tabs[1]).toHaveAttribute('aria-selected', 'true');
  });

  test('opens nested sections of selected result', async () => {
    jest.useFakeTimers();

    const { container, getByLabelText } = render(
      <div>
        <Accordion id="test-accordion">
          <AccordionSection heading="Section 1">
            <Tabs>
              <TabsSection title="Tab 1" id="section-1">
                <p>Something</p>
              </TabsSection>
              <TabsSection title="Tab 2" id="section-2">
                <Details summary="Details 1">
                  <p id="target">Test</p>
                </Details>
              </TabsSection>
            </Tabs>
          </AccordionSection>
        </Accordion>

        <PageSearchForm />
      </div>,
    );

    await wait();

    fireEvent.change(getByLabelText('Find on this page'), {
      target: {
        value: 'Test',
      },
    });

    jest.runOnlyPendingTimers();

    const accordionSection = container.querySelector(
      '#test-accordion-heading-1',
    );
    const tabs = container.querySelectorAll('[role="tab"]');
    const summary = container.querySelector('summary');

    fireEvent.click(container.querySelector('[role="option"]') as HTMLElement);

    jest.runOnlyPendingTimers();

    expect(accordionSection).toHaveAttribute('aria-expanded', 'true');
    expect(tabs[1]).toHaveAttribute('aria-selected', 'true');
    expect(summary).toHaveAttribute('aria-expanded', 'true');
  });
});
