import { Theme } from '@admin/services/themeService';
import { FormFieldset } from '@common/components/form';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import FormSelect from '@common/components/form/FormSelect';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';
import styles from './FormThemeTopicSelect.module.scss';

export interface FormThemeTopicSelectProps {
  error?: string;
  id: string;
  hint?: string;
  inline?: boolean;
  legend: string;
  legendHidden?: FormFieldsetProps['legendHidden'];
  legendSize?: FormFieldsetProps['legendSize'];
  themes: Theme[];
  themeInputName?: string;
  topicId?: string;
  topicInputName?: string;
  onChange?: (topicId: string, themeId: string) => void;
}

const FormThemeTopicSelect = ({
  error,
  hint,
  id,
  inline = true,
  legend,
  legendHidden,
  legendSize,
  themes,
  themeInputName = 'theme',
  topicId,
  topicInputName = 'topic',
  onChange,
}: FormThemeTopicSelectProps) => {
  const selectedTheme = useMemo<Theme | undefined>(() => {
    const matchingTheme = themes.find(theme =>
      theme.topics.find(topic => topic.id === topicId),
    );

    return matchingTheme ?? themes[0];
  }, [themes, topicId]);

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

              const nextTopic = orderBy(
                nextTheme.topics,
                topic => topic.title,
              )[0];

              if (!nextTopic) {
                return;
              }

              if (onChange) {
                onChange(nextTopic.id, nextTheme.id);
              }
            }}
          />
        </div>
        <div>
          <FormSelect
            id={`${id}-topicId`}
            label="Select topic"
            name={topicInputName}
            className={inline ? styles.select : 'govuk-!-width-one-half'}
            value={topicId}
            options={
              selectedTheme?.topics.map(topic => ({
                label: topic.title,
                value: topic.id,
              })) ?? []
            }
            onChange={event => {
              if (!selectedTheme) {
                return;
              }

              const nextTopic = selectedTheme.topics.find(
                topic => topic.id === event.target.value,
              );

              if (!nextTopic) {
                return;
              }

              if (onChange) {
                onChange(nextTopic.id, selectedTheme.id);
              }
            }}
          />
        </div>
      </div>
    </FormFieldset>
  );
};

export default FormThemeTopicSelect;
