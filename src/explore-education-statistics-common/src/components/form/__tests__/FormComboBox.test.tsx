import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import FormComboBox from '../FormComboBox';

describe('FormComboBox', () => {
  test('renders with no options by default', () => {
    const { container } = render(
      <FormComboBox
        id="test-combobox"
        inputLabel="Choose option"
        onInputChange={() => {}}
        onSelect={() => {}}
        options={['Option 1', 'Option 2', 'Option 3']}
      />,
    );

    const options = container.querySelectorAll('[role="option"]');

    expect(options).toHaveLength(0);
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with options in correct order when input changes', () => {
    const { container, getByLabelText } = render(
      <FormComboBox
        id="test-combobox"
        inputLabel="Choose option"
        onInputChange={() => {}}
        onSelect={() => {}}
        options={['Option 1', 'Option 2', 'Option 3']}
      />,
    );

    const input = getByLabelText('Choose option') as HTMLInputElement;

    fireEvent.change(input, {
      target: {
        value: 'Test',
      },
    });

    const options = container.querySelectorAll('[role="option"]');

    expect(options).toHaveLength(3);
    expect(options[0]).toHaveTextContent('Option 1');
    expect(options[1]).toHaveTextContent('Option 2');
    expect(options[2]).toHaveTextContent('Option 3');

    expect(container.innerHTML).toMatchSnapshot();
  });

  describe('input field', () => {
    test('pressing ArrowDown focuses the list box', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
      const { container, getByText, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
      const { container, getByText, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
      const { container, getByText, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
      const { container, getByText, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

      expect(input).not.toHaveFocus();

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
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

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

      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={onSelect}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

      expect(onSelect).not.toHaveBeenCalled();

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });
      fireEvent.keyDown(listBox, { key: 'Enter' });

      expect(onSelect).toHaveBeenCalledWith(2);
    });

    test('clicking option hides all options', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      fireEvent.keyDown(listBox, { key: 'ArrowUp' });
      fireEvent.keyDown(listBox, { key: 'Enter' });

      expect(container.querySelectorAll('[role="option"]')).toHaveLength(0);
    });

    test('pressing Escape on list box options clears them and focuses input field', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

      const listBox = container.querySelector(
        '[role="listbox"]',
      ) as HTMLElement;

      expect(input).toHaveAttribute('value', 'Test');
      expect(input).not.toHaveFocus();
      expect(container.querySelectorAll('[role="option"]')).toHaveLength(3);

      fireEvent.keyDown(listBox, { key: 'Escape' });

      expect(input).toHaveAttribute('value', '');
      expect(input).toHaveFocus();
      expect(container.querySelectorAll('[role="option"]')).toHaveLength(0);
    });

    test('pressing Escape on input field clears it and clears the list box options', () => {
      const { container, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

      expect(input).toHaveAttribute('value', 'Test');
      expect(container.querySelectorAll('[role="option"]')).toHaveLength(3);

      fireEvent.keyDown(input, { key: 'Escape' });

      expect(input).toHaveAttribute('value', '');
      expect(container.querySelectorAll('[role="option"]')).toHaveLength(0);
    });

    test('clicking option calls `onSelect` handler', () => {
      const onSelect = jest.fn();

      const { getByText, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={onSelect}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

      expect(onSelect).not.toHaveBeenCalled();

      fireEvent.click(getByText('Option 2'));

      expect(onSelect).toHaveBeenCalledWith(1);
    });

    test('clicking option hides all options', () => {
      const { container, getByText, getByLabelText } = render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = getByLabelText('Choose option') as HTMLInputElement;

      fireEvent.change(input, {
        target: {
          value: 'Test',
        },
      });

      fireEvent.click(getByText('Option 2'));

      expect(container.querySelectorAll('[role="option"]')).toHaveLength(0);
    });
  });

  test('clicking outside of combobox hides options', () => {
    const { container, getByText, getByLabelText } = render(
      <div>
        <p id="outside">Target</p>

        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />
      </div>,
    );

    const input = getByLabelText('Choose option') as HTMLInputElement;

    fireEvent.change(input, {
      target: {
        value: 'Test',
      },
    });

    expect(container.querySelectorAll('[role="option"]')).toHaveLength(3);

    fireEvent.click(getByText('Target'));

    expect(container.querySelectorAll('[role="option"]')).toHaveLength(0);
  });

  test('re-clicking combobox re-renders options', () => {
    const { container, getByText, getByLabelText } = render(
      <div>
        <p id="outside">Target</p>

        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={() => {}}
          onSelect={() => {}}
          options={['Option 1', 'Option 2', 'Option 3']}
        />
      </div>,
    );

    const input = getByLabelText('Choose option') as HTMLInputElement;

    fireEvent.change(input, {
      target: {
        value: 'Test',
      },
    });

    fireEvent.click(getByText('Target'));

    expect(container.querySelectorAll('[role="option"]')).toHaveLength(0);

    fireEvent.click(getByLabelText('Choose option'));

    expect(container.querySelectorAll('[role="option"]')).toHaveLength(3);
  });
});
