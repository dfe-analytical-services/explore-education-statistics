import { Theme } from '@admin/services/themeService';
import { FormFieldset } from '@common/components/form';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import FormSelect from '@common/components/form/FormSelect';
import React, { useMemo } from 'react';
import styles from './FormThemeSelect.module.scss';

export interface FormThemeSelectProps {
  error?: string;
  id: string;
  hint?: string;
  inline?: boolean;
  legend: string;
  legendHidden?: FormFieldsetProps['legendHidden'];
  legendSize?: FormFieldsetProps['legendSize'];
  themes: Theme[];
  themeInputName: string; // @MarkFix needs to be nullable?
  onChange?: (themeId: string) => void;
}

const FormThemeSelect = ({
  error,
  hint,
  id,
  inline = true,
  legend,
  legendHidden,
  legendSize,
  themes,
  themeInputName = 'theme',
  themeId,
  onChange,
}: FormThemeSelectProps) => {
  const selectedTheme = useMemo<Theme | undefined>(() => {
    const matchingTheme = themes.find(theme => theme.id === themeId);

    return matchingTheme ?? themes[0];
  }, [themes, themeId]);

  return (
    <FormFieldset
      id={id}
      error={error}
      hint={hint}
      legend={legend}
      legendHidden={legendHidden}
      legendSize={legendSize}
    >
      <div className={inline ? 'dfe-flex dfe-flex-wrap' : ''}>
        <div
          className={
            inline ? 'govuk-!-margin-right-4' : 'govuk-!-margin-bottom-6'
          }
        >
          <FormSelect
            id={`${id}-themeId`}
            label="Select theme"
            name={themeInputName}
            className={inline ? styles.select : 'govuk-!-width-one-half'}
            value={selectedTheme?.id}
            options={themes.map(theme => ({
              label: theme.title,
              value: theme.id,
            }))}
            onChange={event => {
              const nextTheme = themes.find(
                theme => theme.id === event.target.value,
              );

              if (!nextTheme) {
                return;
              }

              if (onChange) {
                onChange(nextTheme.id);
              }
            }}
          />
        </div>
      </div>
    </FormFieldset>
  );
};

export default FormThemeSelect;
