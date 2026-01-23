import React from 'react';
import { act, render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';
import FormComboBox from '../FormComboBox';

describe('FormComboBox', () => {
  test('renders with no options by default', () => {
    const { container } = render(
      <FormComboBox
        id="test-combobox"
        inputLabel="Choose option"
        onInputChange={noop}
        onSelect={noop}
        options={['Option 1', 'Option 2', 'Option 3']}
      />,
    );

    const options = screen.queryAllByRole('option');

    expect(options).toHaveLength(0);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with options in correct order when input changes', async () => {
    const { container } = render(
      <FormComboBox
        id="test-combobox"
        inputLabel="Choose option"
        onInputChange={noop}
        onSelect={noop}
        options={['Option 1', 'Option 2', 'Option 3']}
      />,
    );

    await userEvent.type(await screen.findByLabelText('Choose option'), 'Test');

    const options = screen.queryAllByRole('option');

    expect(options).toHaveLength(3);
    expect(options[0]).toHaveTextContent('Option 1');
    expect(options[1]).toHaveTextContent('Option 2');
    expect(options[2]).toHaveTextContent('Option 3');

    expect(container.innerHTML).toMatchSnapshot();
  });

  describe('input field', () => {
    test('pressing ArrowDown focuses the first option', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = (await screen.findByLabelText(
        'Choose option',
      )) as HTMLInputElement;

      await userEvent.type(input, 'Test');

      const option1 = await screen.findByText('Option 1');
      const option2 = await screen.findByText('Option 2');
      const option3 = await screen.findByText('Option 3');

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();

      input.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'true');
      expect(option1).toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();
    });

    test('pressing ArrowDown selects the first list item', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const option1 = await screen.findByText('Option 1');
      const option2 = await screen.findByText('Option 2');
      const option3 = await screen.findByText('Option 3');

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();

      (await screen.findByLabelText('Choose option')).focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'true');
      expect(option1).toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();
    });

    test('pressing ArrowUp focuses the list box', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const listBox = await screen.findByRole('listbox');

      expect(listBox).not.toHaveFocus();

      (await screen.findByLabelText('Choose option')).focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowUp}');
      });

      const option = await screen.findByText('Option 3');
      expect(option).toHaveFocus();
    });

    test('pressing ArrowUp selects the last list item', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const option1 = await screen.findByText('Option 1');
      const option2 = await screen.findByText('Option 2');
      const option3 = await screen.findByText('Option 3');

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();

      (await screen.findByLabelText('Choose option')).focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowUp}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'true');
      expect(option3).toHaveFocus();
    });

    test('pressing Esc clears the input and closes the list box', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = await screen.findByLabelText('Choose option');

      await userEvent.type(input, 'Test');

      expect(input).toHaveValue('Test');
      expect(await screen.findByRole('listbox')).toBeInTheDocument();
      expect(screen.queryAllByRole('option')).toHaveLength(3);

      (await screen.findByLabelText('Choose option')).focus();
      await act(async () => {
        await userEvent.keyboard('{Escape}');
      });

      expect(input).toHaveValue('');
      expect(screen.queryByRole('listbox')).not.toBeInTheDocument();
      expect(screen.queryAllByRole('option')).toHaveLength(0);
    });

    test('pressing Tab closes the list box', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = await screen.findByLabelText('Choose option');

      await userEvent.type(input, 'Test');

      expect(input).toHaveValue('Test');
      expect(await screen.findByRole('listbox')).toBeInTheDocument();
      expect(screen.queryAllByRole('option')).toHaveLength(3);

      (await screen.findByLabelText('Choose option')).focus();
      await act(async () => {
        await userEvent.keyboard('{Tab}');
      });

      expect(input).toHaveValue('Test');
      expect(screen.queryByRole('listbox')).not.toBeInTheDocument();
      expect(screen.queryAllByRole('option')).toHaveLength(0);
    });

    test('re-clicking input re-renders options', async () => {
      render(
        <div>
          <p id="outside">Target</p>
          <FormComboBox
            id="test-combobox"
            inputLabel="Choose option"
            onInputChange={noop}
            onSelect={noop}
            options={['Option 1', 'Option 2', 'Option 3']}
          />
        </div>,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      await userEvent.click(await screen.findByText('Target'));

      expect(screen.queryAllByRole('option')).toHaveLength(0);

      await userEvent.click(await screen.findByLabelText('Choose option'));

      expect(await screen.findByRole('listbox')).toBeInTheDocument();
      expect(await screen.findAllByRole('option')).toHaveLength(3);
    });
  });

  describe('list box', () => {
    test('pressing ArrowUp will cycle the selected item through the entire list', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const listBox = await screen.findByRole('listbox');

      const option1 = await screen.findByText('Option 1');
      const option2 = await screen.findByText('Option 2');
      const option3 = await screen.findByText('Option 3');

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowUp}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'true');
      expect(option3).toHaveFocus();

      await act(async () => {
        await userEvent.keyboard('{ArrowUp}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'true');
      expect(option2).toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();

      await act(async () => {
        await userEvent.keyboard('{ArrowUp}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'true');
      expect(option1).toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();

      await act(async () => {
        await userEvent.keyboard('{ArrowUp}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'true');
      expect(option3).toHaveFocus();
    });

    test('pressing ArrowUp adjusts scroll correctly', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const listBox = await screen.findByRole('listbox');

      expect(listBox.scrollTop).toBe(0);

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowUp}');
      });

      expect(listBox.scrollTop).toBe(listBox.scrollHeight);

      await act(async () => {
        await userEvent.keyboard('{ArrowUp}');
      });

      expect(listBox.scrollTop).toBe(
        listBox.scrollHeight -
          (await screen.findByText('Option 3')).scrollHeight,
      );

      await act(async () => {
        await userEvent.keyboard('{ArrowUp}');
      });

      expect(listBox.scrollTop).toBe(
        listBox.scrollHeight -
          (await screen.findByText('Option 2')).scrollHeight,
      );

      await act(async () => {
        await userEvent.keyboard('{ArrowUp}');
      });
      expect(listBox.scrollTop).toBe(0);
    });

    test('pressing ArrowDown will cycle the selected item through the entire list', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const listBox = await screen.findByRole('combobox');

      const option1 = await screen.findByText('Option 1');
      const option2 = await screen.findByText('Option 2');
      const option3 = await screen.findByText('Option 3');

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'true');
      expect(option1).toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();

      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'true');
      expect(option2).toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();

      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'false');
      expect(option1).not.toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'true');
      expect(option3).toHaveFocus();

      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });

      expect(option1).toHaveAttribute('aria-selected', 'true');
      expect(option1).toHaveFocus();

      expect(option2).toHaveAttribute('aria-selected', 'false');
      expect(option2).not.toHaveFocus();

      expect(option3).toHaveAttribute('aria-selected', 'false');
      expect(option3).not.toHaveFocus();
    });

    test('pressing ArrowDown adjusts scroll correctly', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const comboBox = await screen.findByRole('combobox');

      expect(comboBox.scrollTop).toBe(0);

      comboBox.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });

      expect(comboBox.scrollTop).toBe(0);

      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });
      expect(comboBox.scrollTop).toBe(
        (await screen.findByText('Option 1')).scrollHeight,
      );

      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });
      expect(comboBox.scrollTop).toBe(
        comboBox.scrollHeight +
          (await screen.findByText('Option 2')).scrollHeight,
      );

      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });
      expect(comboBox.scrollTop).toBe(comboBox.scrollHeight);
    });

    test('pressing ArrowLeft focuses back to input field', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const input = await screen.findByLabelText('Choose option');

      input.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowDown}');
      });
      expect(await screen.findByLabelText('Choose option')).not.toHaveFocus();

      const listBox = await screen.findByRole('listbox');

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowLeft}');
      });

      expect(await screen.findByLabelText('Choose option')).toHaveFocus();
    });

    test('pressing ArrowLeft moves input field selection one character to the left', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = (await screen.findByLabelText(
        'Choose option',
      )) as HTMLInputElement;

      await userEvent.type(input, 'Test');

      input.selectionStart = 2;
      input.selectionEnd = 2;

      const listBox = await screen.findByRole('listbox');

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowLeft}');
      });

      expect(input.selectionStart).toBe(1);
      expect(input.selectionEnd).toBe(1);
    });

    test('pressing ArrowRight focuses back to input field', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const listBox = await screen.findByRole('listbox');

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowRight}');
      });

      expect(await screen.findByLabelText('Choose option')).toHaveFocus();
    });

    test('pressing ArrowRight moves input field selection one character to the left', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = (await screen.findByLabelText(
        'Choose option',
      )) as HTMLInputElement;

      await userEvent.type(input, 'Test');

      input.selectionStart = 2;
      input.selectionEnd = 2;

      const listBox = await screen.findByRole('listbox');

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowRight}');
      });

      expect(input.selectionStart).toBe(3);
      expect(input.selectionEnd).toBe(3);
    });

    test('pressing Home focuses input field and moves selection back to start', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = (await screen.findByLabelText(
        'Choose option',
      )) as HTMLInputElement;

      await userEvent.type(input, 'Test');

      input.value = 'Test';
      input.selectionStart = 2;
      input.selectionEnd = 2;

      const listBox = await screen.findByRole('listbox');

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{Home}');
      });

      expect(input).toHaveFocus();
      expect(input.selectionStart).toBe(0);
      expect(input.selectionEnd).toBe(0);
    });

    test('pressing End focuses input field and moves selection back to start', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = (await screen.findByLabelText(
        'Choose option',
      )) as HTMLInputElement;

      await userEvent.type(input, 'Test');
      input.selectionStart = 2;
      input.selectionEnd = 2;

      const listBox = await screen.findByRole('listbox');

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{End}');
      });

      expect(input).toHaveFocus();
      expect(input.selectionStart).toBe(4);
      expect(input.selectionEnd).toBe(4);
    });

    test('pressing Enter calls `onSelect` handler with selected item index', async () => {
      const onSelect = jest.fn();

      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={onSelect}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      expect(onSelect).not.toHaveBeenCalled();

      const listBox = await screen.findByRole('listbox');

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowUp}{Enter}');
      });

      expect(onSelect).toHaveBeenCalledWith(2);
    });

    test('pressing Tab closes the list box and hides all options', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const listBox = await screen.findByRole('listbox');

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{Tab}');
      });

      expect(listBox).not.toBeInTheDocument();
      expect(screen.queryAllByRole('option')).toHaveLength(0);
    });

    test('clicking option hides all options', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      const listBox = await screen.findByRole('listbox');

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{ArrowUp}{Enter}');
      });

      expect(screen.queryAllByRole('option')).toHaveLength(0);
    });

    test('pressing Escape on list box options clears them and focuses input field', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = (await screen.findByLabelText(
        'Choose option',
      )) as HTMLInputElement;

      await userEvent.type(input, 'Test');

      const listBox = await screen.findByRole('listbox');

      expect(input).toHaveAttribute('value', 'Test');
      expect(input).toHaveFocus();
      expect(screen.queryAllByRole('option')).toHaveLength(3);

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{Escape}');
      });

      expect(input).toHaveAttribute('value', '');
      expect(input).toHaveFocus();
      expect(screen.queryAllByRole('option')).toHaveLength(0);

      listBox.focus();
      await act(async () => {
        await userEvent.keyboard('{Escape}');
      });

      expect(input).toHaveFocus();
    });

    test('pressing Escape on input field clears it and clears the list box options', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      const input = (await screen.findByLabelText(
        'Choose option',
      )) as HTMLInputElement;

      await userEvent.type(input, 'Test');

      expect(input).toHaveAttribute('value', 'Test');
      expect(screen.queryAllByRole('option')).toHaveLength(3);

      input.focus();
      await act(async () => {
        await userEvent.keyboard('{Escape}');
      });

      expect(input).toHaveAttribute('value', '');
      expect(screen.queryAllByRole('option')).toHaveLength(0);
    });

    test('clicking option calls `onSelect` handler', async () => {
      const onSelect = jest.fn();

      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={onSelect}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      expect(onSelect).not.toHaveBeenCalled();

      await userEvent.click(await screen.findByText('Option 2'));

      expect(onSelect).toHaveBeenCalledWith(1);
    });

    test('clicking option hides all options', async () => {
      render(
        <FormComboBox
          id="test-combobox"
          inputLabel="Choose option"
          onInputChange={noop}
          onSelect={noop}
          options={['Option 1', 'Option 2', 'Option 3']}
        />,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      await userEvent.click(await screen.findByText('Option 2'));

      expect(screen.queryAllByRole('option')).toHaveLength(0);
    });

    test('clicking outside of combobox hides options', async () => {
      render(
        <div>
          <p id="outside">Target</p>

          <FormComboBox
            id="test-combobox"
            inputLabel="Choose option"
            onInputChange={noop}
            onSelect={noop}
            options={['Option 1', 'Option 2', 'Option 3']}
          />
        </div>,
      );

      await userEvent.type(
        await screen.findByLabelText('Choose option'),
        'Test',
      );

      expect(screen.queryAllByRole('option')).toHaveLength(3);

      await userEvent.click(await screen.findByText('Target'));

      expect(screen.queryAllByRole('option')).toHaveLength(0);
    });
  });
});
