import { fireEvent, render, screen } from '@testing-library/react';
import React, { useState } from 'react';
import FormRadioGroup from '../FormRadioGroup';

describe('FormRadioGroup', () => {
  test('renders list of radios in correct order', () => {
    const { container } = render(
      <FormRadioGroup
        value=""
        id="test-radios"
        name="test-radios"
        legend="Test radios"
        options={[
          { id: 'radio-1', label: 'Test radio 1', value: '1' },
          { id: 'radio-2', label: 'Test radio 2', value: '2' },
          { id: 'radio-3', label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    const radios = screen.getAllByLabelText(/Test radio/);

    expect(radios).toHaveLength(3);
    expect(radios[0]).toHaveAttribute('value', '1');
    expect(radios[1]).toHaveAttribute('value', '2');
    expect(radios[2]).toHaveAttribute('value', '3');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders list of radios in reverse order', () => {
    render(
      <FormRadioGroup
        value=""
        id="test-radios"
        name="test-radios"
        legend="Test radios"
        orderDirection={['desc']}
        options={[
          { id: 'radio-1', label: 'Test radio 1', value: '1' },
          { id: 'radio-2', label: 'Test radio 2', value: '2' },
          { id: 'radio-3', label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    const radios = screen.getAllByLabelText(/Test radio/);

    expect(radios).toHaveLength(3);
    expect(radios[0]).toHaveAttribute('value', '3');
    expect(radios[1]).toHaveAttribute('value', '2');
    expect(radios[2]).toHaveAttribute('value', '1');
  });

  test('renders list of radios in custom order', () => {
    render(
      <FormRadioGroup
        value=""
        id="test-radios"
        name="test-radios"
        legend="Test radios"
        order={['value']}
        orderDirection={['desc']}
        options={[
          { id: 'radio-1', label: 'Test radio 1', value: '2' },
          { id: 'radio-2', label: 'Test radio 2', value: '3' },
          { id: 'radio-3', label: 'Test radio 3', value: '1' },
        ]}
      />,
    );

    const radios = screen.getAllByLabelText(/Test radio/);

    expect(radios).toHaveLength(3);
    expect(radios[0]).toHaveAttribute('value', '3');
    expect(radios[1]).toHaveAttribute('value', '2');
    expect(radios[2]).toHaveAttribute('value', '1');
  });

  test('clicking a radio checks it', () => {
    const RadioWrapper = () => {
      const [value, setValue] = useState('');

      return (
        <FormRadioGroup
          value={value}
          onChange={event => {
            setValue(event.target.value);
          }}
          id="test-radios"
          name="test-radios"
          legend="Test radios"
          options={[{ id: 'radio-1', label: 'Test radio', value: '1' }]}
        />
      );
    };

    render(<RadioWrapper />);

    const radio = screen.getByLabelText('Test radio') as HTMLInputElement;

    expect(radio.checked).toBe(false);

    fireEvent.click(radio);

    expect(radio.checked).toBe(true);
  });

  test('clicking radios toggles between them', () => {
    const RadioWrapper = () => {
      const [value, setValue] = useState('');

      return (
        <FormRadioGroup
          value={value}
          onChange={event => {
            setValue(event.target.value);
          }}
          id="test-radios"
          name="test-radios"
          legend="Test radios"
          options={[
            { id: 'radio-1', label: 'Test radio 1', value: '1' },
            { id: 'radio-2', label: 'Test radio 2', value: '2' },
          ]}
        />
      );
    };

    render(<RadioWrapper />);

    const radio1 = screen.getByLabelText('Test radio 1') as HTMLInputElement;
    const radio2 = screen.getByLabelText('Test radio 2') as HTMLInputElement;

    fireEvent.click(radio1);

    expect(radio1.checked).toBe(true);
    expect(radio2.checked).toBe(false);

    fireEvent.click(radio2);

    expect(radio1.checked).toBe(false);
    expect(radio2.checked).toBe(true);
  });

  test('renders correctly with legend', () => {
    const { container } = render(
      <FormRadioGroup
        value=""
        legend="Choose a radio"
        id="test-radios"
        name="test-radios"
        options={[{ id: 'radio-1', label: 'Test radio 1', value: '1' }]}
      />,
    );

    expect(screen.getByText('Choose a radio')).toBeInTheDocument();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with small size variants', () => {
    const { container } = render(
      <FormRadioGroup
        id="test-radios"
        name="test-radios"
        legend="Choose a radio"
        small
        options={[{ id: 'radio-1', label: 'Test radio 1', value: '1' }]}
      />,
    );

    expect(container.querySelector('.govuk-radios--small')).not.toBeNull();
    expect(container).toMatchSnapshot();
  });

  test('renders option with conditional contents', () => {
    const { container } = render(
      <FormRadioGroup
        value="2"
        id="test-radios"
        name="test-radios"
        legend="Test radios"
        options={[
          {
            id: 'radio-1',
            label: 'Test radio 1',
            value: '1',
            conditional: <p>Conditional 1</p>,
          },
          {
            id: 'radio-2',
            label: 'Test radio 2',
            value: '2',
            conditional: <p>Conditional 2</p>,
          },
          {
            id: 'radio-3',
            label: 'Test radio 3',
            value: '3',
            conditional: <p>Conditional 3</p>,
          },
        ]}
      />,
    );

    const hiddenClass = 'govuk-radios__conditional--hidden';

    expect(screen.getByText('Conditional 1').parentElement).toHaveClass(
      hiddenClass,
    );
    expect(screen.getByText('Conditional 2').parentElement).not.toHaveClass(
      hiddenClass,
    );
    expect(screen.getByText('Conditional 3').parentElement).toHaveClass(
      hiddenClass,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('generates option IDs from id and value if none specified', () => {
    render(
      <FormRadioGroup
        id="test-radios"
        name="test-radios"
        legend="Test radios"
        options={[
          { label: 'Test radio 1', value: 'opt1' },
          { label: 'Test radio 2', value: 'opt-2' },
          { label: 'Test radio 3', value: 'opt.3' },
          { label: 'Test radio 4', value: 'opt 4 is \n here' },
        ]}
      />,
    );

    expect(screen.getByLabelText('Test radio 1')).toHaveAttribute(
      'id',
      'test-radios-opt1',
    );
    expect(screen.getByLabelText('Test radio 2')).toHaveAttribute(
      'id',
      'test-radios-opt-2',
    );
    expect(screen.getByLabelText('Test radio 3')).toHaveAttribute(
      'id',
      'test-radios-opt.3',
    );
    expect(screen.getByLabelText('Test radio 4')).toHaveAttribute(
      'id',
      'test-radios-opt-4-is---here',
    );
  });
});
