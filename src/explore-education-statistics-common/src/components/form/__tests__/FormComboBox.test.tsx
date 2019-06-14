import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import FormComboBox from '../FormComboBox';

describe('FormComboBox', () => {
  test('renders correctly with options in correct order', () => {
    const { container } = render(
      <FormComboBox
        id="test-combobox"
        inputLabel="Choose option"
        onInputChange={() => {}}
        onSelect={() => {}}
        listBoxItems={['Option 1', 'Option 2', 'Option 3']}
      />,
    );

    const options = container.querySelectorAll('[role="option"]');

    expect(options).toHaveLength(3);
    expect(options[0]).toHaveTextContent('Option 1');
    expect(options[1]).toHaveTextContent('Option 2');
    expect(options[2]).toHaveTextContent('Option 3');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders selected option correctly', () => {
    const { container, getByText } = render(
      <FormComboBox
        id="test-combobox"
        inputLabel="Choose option"
        initialOption={0}
        onInputChange={() => {}}
        onSelect={() => {}}
        listBoxItems={['Option 1', 'Option 2', 'Option 3']}
      />,
    );

    expect(getByText('Option 1')).toHaveClass('selected');
    expect(getByText('Option 1')).toHaveAttribute('aria-selected', 'true');

    const listbox = container.querySelector('[role="listbox"]');

    expect(listbox).toMatchSnapshot();
  });

  describe('input field', () => {
    test('pressing ArrowDown focuses the list box', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const listBox = container.querySelector('[role="listbox"]');

      expect(listBox).not.toHaveFocus();

      fireEvent.keyDown(getByLabelText('Choose option'), { key: 'ArrowDown' });

      expect(listBox).toHaveFocus();
    });

    test('pressing ArrowDown selects the first list item', () => {
      const { getByLabelText, getByText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const option1 = getByText('Option 1');
      const option2 = getByText('Option 2');
      const option3 = getByText('Option 3');

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'false');

      fireEvent.keyDown(getByLabelText('Choose option'), { key: 'ArrowDown' });

      expect(option1).toHaveAttribute('aria-selected', 'true');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'false');
    });

    test('pressing ArrowUp focuses the list box', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const listBox = container.querySelector('[role="listbox"]');

      expect(listBox).not.toHaveFocus();

      fireEvent.keyDown(getByLabelText('Choose option'), { key: 'ArrowUp' });

      expect(listBox).toHaveFocus();
    });

    test('pressing ArrowUp selects the last list item', () => {
      const { getByLabelText, getByText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const option1 = getByText('Option 1');
      const option2 = getByText('Option 2');
      const option3 = getByText('Option 3');

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'false');

      fireEvent.keyDown(getByLabelText('Choose option'), { key: 'ArrowUp' });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'true');
    });
  });

  describe('list box', () => {
    test('pressing ArrowUp will cycle the selected item through the entire list', () => {
      const { container, getByText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      const option1 = getByText('Option 1');
      const option2 = getByText('Option 2');
      const option3 = getByText('Option 3');

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'false');

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'true');

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option2).toHaveAttribute('aria-selected', 'true');
      expect(option3).toHaveAttribute('aria-selected', 'false');

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });

      expect(option1).toHaveAttribute('aria-selected', 'true');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'false');

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'true');
    });

    test('pressing ArrowUp adjusts scroll correctly', () => {
      const { container, getByText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      expect(listBox.scrollTop).toBe(0);

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });
      expect(listBox.scrollTop).toBe(listBox.scrollHeight);

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });
      expect(listBox.scrollTop).toBe(
        listBox.scrollHeight - getByText('Option 3').scrollHeight,
      );

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });
      expect(listBox.scrollTop).toBe(
        listBox.scrollHeight - getByText('Option 2').scrollHeight,
      );

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });
      expect(listBox.scrollTop).toBe(0);
    });

    test('pressing ArrowDown will cycle the selected item through the entire list', () => {
      const { container, getByText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      const option1 = getByText('Option 1');
      const option2 = getByText('Option 2');
      const option3 = getByText('Option 3');

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'false');

      fireEvent.keyDown(listBox, { key: 'ArrowDown' });

      expect(option1).toHaveAttribute('aria-selected', 'true');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'false');

      fireEvent.keyDown(listBox, { key: 'ArrowDown' });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option2).toHaveAttribute('aria-selected', 'true');
      expect(option3).toHaveAttribute('aria-selected', 'false');

      fireEvent.keyDown(listBox, { key: 'ArrowDown' });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'true');

      fireEvent.keyDown(listBox, { key: 'ArrowDown' });

      expect(option1).toHaveAttribute('aria-selected', 'true');
      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option3).toHaveAttribute('aria-selected', 'false');
    });

    test('pressing ArrowDown adjusts scroll correctly', () => {
      const { container, getByText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      expect(listBox.scrollTop).toBe(0);

      fireEvent.keyDown(listBox, { key: 'ArrowDown' });
      expect(listBox.scrollTop).toBe(0);

      fireEvent.keyDown(listBox, { key: 'ArrowDown' });
      expect(listBox.scrollTop).toBe(
        listBox.scrollHeight + getByText('Option 1').scrollHeight,
      );

      fireEvent.keyDown(listBox, { key: 'ArrowDown' });
      expect(listBox.scrollTop).toBe(
        listBox.scrollHeight + getByText('Option 2').scrollHeight,
      );

      fireEvent.keyDown(listBox, { key: 'ArrowDown' });
      expect(listBox.scrollTop).toBe(listBox.scrollHeight);
    });

    test('pressing ArrowLeft focuses back to input field', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      expect(getByLabelText('Choose option')).not.toHaveFocus();

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      fireEvent.keyDown(listBox, { key: 'ArrowLeft' });

      expect(getByLabelText('Choose option')).toHaveFocus();
    });

    test('pressing ArrowLeft moves input field selection one character to the left', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      input.value = 'Test';
      input.selectionStart = 2;
      input.selectionEnd = 2;

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      fireEvent.keyDown(listBox, { key: 'ArrowLeft' });

      expect(input.selectionStart).toBe(1);
      expect(input.selectionEnd).toBe(1);
    });

    test('pressing ArrowRight focuses back to input field', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      expect(getByLabelText('Choose option')).not.toHaveFocus();

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      fireEvent.keyDown(listBox, { key: 'ArrowRight' });

      expect(getByLabelText('Choose option')).toHaveFocus();
    });

    test('pressing ArrowRight moves input field selection one character to the left', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      input.value = 'Test';
      input.selectionStart = 2;
      input.selectionEnd = 2;

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      fireEvent.keyDown(listBox, { key: 'ArrowRight' });

      expect(input.selectionStart).toBe(3);
      expect(input.selectionEnd).toBe(3);
    });

    test('pressing Home focuses input field and moves selection back to start', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      input.value = 'Test';
      input.selectionStart = 2;
      input.selectionEnd = 2;

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      fireEvent.keyDown(listBox, { key: 'Home' });

      expect(input).toHaveFocus();
      expect(input.selectionStart).toBe(0);
      expect(input.selectionEnd).toBe(0);
    });

    test('pressing End focuses input field and moves selection back to start', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      input.value = 'Test';
      input.selectionStart = 2;
      input.selectionEnd = 2;

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      fireEvent.keyDown(listBox, { key: 'End' });

      expect(input).toHaveFocus();
      expect(input.selectionStart).toBe(4);
      expect(input.selectionEnd).toBe(4);
    });

    test('pressing Enter calls `onSelect` handler with selected item index', () => {
      const onSelect = jest.fn();

      const { container } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={onSelect}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      expect(onSelect).not.toHaveBeenCalled();

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });
      fireEvent.keyDown(listBox, { key: 'Enter' });

      expect(onSelect).toHaveBeenCalledWith(2);
    });

    test('clicking option calls `onSelect` handler', () => {
      const onSelect = jest.fn();

      const { getByText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={onSelect}
          listBoxItems={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      expect(onSelect).not.toHaveBeenCalled();

      fireEvent.click(getByText('Option 2'));

      expect(onSelect).toHaveBeenCalledWith(1);
    });
  });
});
