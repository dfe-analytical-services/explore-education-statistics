import FormThemeSelect from '@admin/components/form/FormThemeSelect';
import { Theme } from '@admin/services/themeService';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('FormThemeSelect', () => {
  const testThemes: Theme[] = [
    {
      id: 'theme-1',
      slug: 'theme-1',
      title: 'Theme 1',
      summary: '',
    },
    {
      id: 'theme-2',
      slug: 'theme-2',
      title: 'Theme 2',
      summary: '',
    },
  ];

  test('renders correctly with no theme', () => {
    render(<FormThemeSelect id="theme" legend="Theme" themes={[]} />);

    const themeSelect = screen.getByLabelText('Select theme');
    const themes = within(themeSelect).queryAllByRole('option');

    expect(themes).toHaveLength(0);
  });

  test('renders correctly with single theme', () => {
    render(
      <FormThemeSelect
        id="theme"
        legend="Theme"
        themes={[
          {
            id: 'theme-1',
            slug: 'theme-1',
            title: 'Theme 1',
            summary: '',
          },
        ]}
      />,
    );

    const themeSelect = screen.getByLabelText('Select theme');
    const themes = within(themeSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];

    expect(themes).toHaveLength(1);

    expect(themes[0]).toHaveTextContent('Theme 1');
    expect(themes[0]).toHaveValue('theme-1');
    expect(themes[0].selected).toBe(true);
  });

  test('renders correctly with multiple themes', () => {
    render(<FormThemeSelect id="theme" legend="Theme" themes={testThemes} />);

    const themeSelect = screen.getByLabelText('Select theme');
    const themes = within(themeSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];

    expect(themes).toHaveLength(2);

    expect(themes[0]).toHaveTextContent('Theme 1');
    expect(themes[0]).toHaveValue('theme-1');
    expect(themes[0].selected).toBe(true);

    expect(themes[1]).toHaveTextContent('Theme 2');
    expect(themes[1]).toHaveValue('theme-2');
    expect(themes[1].selected).toBe(false);
  });

  test('changing theme calls `onChange` handler with correct arguments', async () => {
    const handleChange = jest.fn();

    render(
      <FormThemeSelect
        id="theme"
        legend="Theme"
        themes={testThemes}
        onChange={handleChange}
      />,
    );

    expect(handleChange).not.toHaveBeenCalled();

    await userEvent.selectOptions(screen.getByLabelText('Select theme'), [
      'theme-2',
    ]);

    expect(handleChange).toHaveBeenCalledTimes(1);
    expect(handleChange).toHaveBeenCalledWith('theme-2');
  });
});
