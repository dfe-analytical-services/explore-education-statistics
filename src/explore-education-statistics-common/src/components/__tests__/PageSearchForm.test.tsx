import flushTasks from '@common-test/flushTasks';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import PageSearchForm from '../PageSearchForm';

const labelText = 'Find on this page';

describe('PageSearchForm', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  test('does not render results if input is less than default 3 characters', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <PageSearchForm inputLabel={labelText} />

        <div>
          <p>Not me</p>
          <p>To 1</p>
        </div>

        <div>
          <h2>Not me</h2>
          <h2>To 2</h2>
        </div>

        <div>
          <h3>Not me</h3>
          <h3>To 3</h3>
        </div>

        <div>
          <h4>Not me</h4>
          <h4>To 4</h4>
        </div>

        <ul>
          <li>Not me</li>
          <li>
            <strong>To 5</strong>
          </li>
        </ul>
      </div>,
    );

    await user.type(screen.getByLabelText(labelText), 'To');

    jest.runOnlyPendingTimers();

    await waitFor(() =>
      expect(screen.queryAllByRole('option')).toHaveLength(0),
    );
  });

  test('does not render results if input is less the custom `minInput` prop', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <PageSearchForm inputLabel={labelText} minInput={5} />

        <div>
          <p>Not me</p>
          <p>Testing 1</p>
        </div>

        <div>
          <h2>Not me</h2>
          <h2>Testing 2</h2>
        </div>

        <div>
          <h3>Not me</h3>
          <h3>Testing 3</h3>
        </div>

        <div>
          <h4>Not me</h4>
          <h4>Testing 4</h4>
        </div>

        <ul>
          <li>Testing me</li>
          <li>
            <strong>Testing 5</strong>
          </li>
        </ul>
      </div>,
    );

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(screen.queryAllByRole('option')).toHaveLength(0);
  });

  test('renders correct results when input is an acronym', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <PageSearchForm inputLabel={labelText} />

        <div>
          <p>Not me</p>
          <p>Testing 1</p>
        </div>

        <div>
          <h2>Not me</h2>
          <h2>TESTING 2</h2>
        </div>

        <div>
          <h3>Not me</h3>
          <h3>TEST 3</h3>
        </div>

        <div>
          <h4>Not me</h4>
          <h4>Testing 4</h4>
        </div>

        <ul>
          <li>Testing me</li>
          <li>
            <strong>Testing 5</strong>
          </li>
        </ul>
      </div>,
    );

    await user.type(screen.getByLabelText(labelText), 'TEST');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 2 results',
    );

    const options = screen.getAllByRole('option');

    expect(options).toHaveLength(2);
    expect(options[0]).toHaveTextContent('TESTING 2');
    expect(options[1]).toHaveTextContent('TEST 3');
  });

  test('renders correct results when input is an acronym and is below `minInput`', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <PageSearchForm inputLabel={labelText} minInput={5} />

        <div>
          <p>Not me</p>
          <p>Testing 1</p>
        </div>

        <div>
          <h2>Not me</h2>
          <h2>TESTING 2</h2>
        </div>

        <div>
          <h3>Not me</h3>
          <h3>TEST 3</h3>
        </div>

        <div>
          <h4>Not me</h4>
          <h4>Testing 4</h4>
        </div>

        <ul>
          <li>Testing me</li>
          <li>
            <strong>Testing 5</strong>
          </li>
        </ul>
      </div>,
    );

    await user.type(screen.getByLabelText(labelText), 'TE');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 2 results',
    );

    const options = screen.getAllByRole('option');

    expect(options).toHaveLength(2);
    expect(options[0]).toHaveTextContent('TESTING 2');
    expect(options[1]).toHaveTextContent('TEST 3');
  });

  test('renders results found in default elements', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <PageSearchForm inputLabel={labelText} />

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

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 5 results',
    );

    const options = screen.getAllByRole('option');

    expect(options).toHaveLength(5);
    expect(options[0]).toHaveTextContent('Test 1');
    expect(options[1]).toHaveTextContent('Test 2');
    expect(options[2]).toHaveTextContent('Test 3');
    expect(options[3]).toHaveTextContent('Test 4');
    expect(options[4]).toHaveTextContent('Test 5');

    expect(screen.getByRole('listbox')).toMatchSnapshot();
  });

  test('renders results found in custom selectors', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <PageSearchForm
          inputLabel={labelText}
          elementSelectors={['.target-1', '.target-2 p']}
        />

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

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 2 results',
    );

    const options = screen.getAllByRole('option');

    expect(options).toHaveLength(2);
    expect(options[0]).toHaveTextContent('Test 1');
    expect(options[1]).toHaveTextContent('Test 2');

    expect(screen.getByRole('listbox')).toMatchSnapshot();
  });

  test('result locations uses nearest heading', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <PageSearchForm inputLabel={labelText} />

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

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 2 results',
    );

    const options = screen.getAllByRole('option');

    expect(options).toHaveLength(2);
    expect(options[0]).toHaveTextContent('Test 1');
    expect(options[0]).toHaveTextContent('Section 1');
    expect(options[1]).toHaveTextContent('Test 2');
    expect(options[1]).toHaveTextContent('Section 2');

    expect(screen.getByRole('listbox')).toMatchSnapshot();
  });

  test('result locations include heading hierarchy', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <PageSearchForm inputLabel={labelText} />

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

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 1 result',
    );

    const options = screen.getAllByRole('option');

    expect(options).toHaveLength(1);
    expect(options[0]).toHaveTextContent('Test 1');
    expect(options[0]).toHaveTextContent('Section 1 > Section 2 > Section 3');

    expect(screen.getByRole('listbox')).toMatchSnapshot();
  });

  test('result locations include accordion sections', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <PageSearchForm inputLabel={labelText} />

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

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 2 results',
    );

    const options = screen.getAllByRole('option');

    expect(options).toHaveLength(2);
    expect(options[0]).toHaveTextContent('Test 1');
    expect(options[0]).toHaveTextContent(
      'Section 1 > Section 2 > Accordion section 1 > Section 3',
    );
    expect(options[1]).toHaveTextContent('Test 2');
    expect(options[1]).toHaveTextContent(
      'Section 1 > Section 2 > Accordion section 2',
    );

    expect(screen.getByRole('listbox')).toMatchSnapshot();
  });

  test('clicking result scrolls and focuses the element', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <h2>Section 1</h2>
        <p data-testid="target">Test</p>

        <PageSearchForm inputLabel={labelText} />
      </div>,
    );

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 1 result',
    );

    await user.click(screen.getAllByRole('option')[0]);

    await flushTasks();

    await waitFor(() => {
      const target = screen.getByTestId('target');
      expect(target).toHaveFocus();
      expect(target).toHaveScrolledIntoView();
    });
  });

  test('pressing Enter on result scrolls and focuses the element', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <h2>Section 1</h2>
        <p data-testid="target">Test</p>

        <PageSearchForm inputLabel={labelText} />
      </div>,
    );

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 1 result',
    );

    await user.keyboard('[ArrowDown]');
    await user.keyboard('[Enter]');

    await flushTasks();

    await waitFor(() => {
      const target = screen.getByTestId('target');
      expect(target).toHaveFocus();
      expect(target).toHaveScrolledIntoView();
    });
  });

  test('opens parent accordion of selected result', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <Accordion id="test-accordion">
          <AccordionSection heading="Section 1">
            <p id="target">Test</p>
          </AccordionSection>
        </Accordion>

        <PageSearchForm inputLabel={labelText} />
      </div>,
    );

    await waitFor(() => {
      expect(screen.getByLabelText(labelText)).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 1 result',
    );

    const accordionSection = screen.getByTestId('accordionSection');

    expect(within(accordionSection).getByText('Show')).toBeInTheDocument();

    await user.click(screen.getByRole('option'));

    jest.runOnlyPendingTimers();

    expect(within(accordionSection).getByText('Hide')).toBeInTheDocument();
  });

  test('opens parent details of selected result', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <Details summary="Details 1">
          <p data-testid="target">Test</p>
        </Details>

        <PageSearchForm inputLabel={labelText} />
      </div>,
    );

    await waitFor(() => {
      expect(screen.getByLabelText(labelText)).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 1 result',
    );

    expect(
      screen.getByRole('button', {
        name: 'Details 1',
      }),
    ).toHaveAttribute('aria-expanded', 'false');

    await user.click(screen.getByRole('option'));

    await flushTasks();

    expect(
      screen.getByRole('button', {
        name: 'Details 1',
      }),
    ).toHaveAttribute('aria-expanded', 'true');
  });

  test('opens parent tab section of selected result', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <Tabs id="test-tabs">
          <TabsSection title="Tab 1">
            <p>Something</p>
          </TabsSection>
          <TabsSection title="Tab 2">
            <p id="target">Test</p>
          </TabsSection>
        </Tabs>

        <PageSearchForm inputLabel={labelText} />
      </div>,
    );

    await waitFor(() => {
      expect(screen.getByLabelText(labelText)).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 1 result',
    );

    const tabs = screen.getAllByRole('tab');

    expect(tabs[0]).toHaveTextContent('Tab 1');
    expect(tabs[1]).toHaveTextContent('Tab 2');

    expect(tabs[0]).toHaveAttribute('aria-selected', 'true');
    expect(tabs[1]).toHaveAttribute('aria-selected', 'false');

    await user.click(screen.getByRole('option'));

    await flushTasks();

    expect(tabs[0]).toHaveAttribute('aria-selected', 'false');
    expect(tabs[1]).toHaveAttribute('aria-selected', 'true');
  });

  test('opens nested sections of selected result', async () => {
    const user = userEvent.setup({ delay: null });
    render(
      <div>
        <Accordion id="test-accordion">
          <AccordionSection heading="Section 1">
            <Tabs id="test-tabs">
              <TabsSection title="Tab 1">
                <p>Something</p>
              </TabsSection>
              <TabsSection title="Tab 2">
                <Details summary="Details 1">
                  <p id="target">Test</p>
                </Details>
              </TabsSection>
            </Tabs>
          </AccordionSection>
        </Accordion>

        <PageSearchForm inputLabel={labelText} />
      </div>,
    );

    await waitFor(() => {
      expect(screen.getByLabelText(labelText)).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText(labelText), 'Test');

    jest.runOnlyPendingTimers();

    expect(await screen.findByTestId('results-label')).toHaveTextContent(
      'Found 1 result',
    );

    const accordionSection = screen.getByTestId('accordionSection');
    const tabs = screen.getAllByRole('tab');

    expect(within(accordionSection).getByText('Show')).toBeInTheDocument();

    await user.click(screen.getByRole('option'));

    await flushTasks();

    expect(within(accordionSection).getByText('Hide')).toBeInTheDocument();

    await waitFor(() => {
      expect(tabs[1]).toHaveAttribute('aria-selected', 'true');
      expect(
        screen.getByRole('button', {
          name: 'Details 1',
        }),
      ).toHaveAttribute('aria-expanded', 'true');
    });
  });
});
