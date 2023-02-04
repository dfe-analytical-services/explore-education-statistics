import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import { toolbarConfigs } from '@admin/config/ckEditorConfig';
import toHtml from '@admin/utils/markdown/toHtml';
import toMarkdown from '@admin/utils/markdown/toMarkdown';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldTextInput } from '@common/components/form';
import useFormSubmit from '@common/hooks/useFormSubmit';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import { KeyStatisticText } from '@common/services/publicationService';
import { Formik } from 'formik';
import React from 'react';

export interface KeyStatTextFormValues {
  title: string;
  statistic: string;
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

interface EditableKeyStatTextFormProps {
  keyStat: KeyStatisticText;
  isReordering?: boolean;
  onSubmit: (values: KeyStatTextFormValues) => void;
  onCancel: () => void;
  testId?: string;
}

export default function EditableKeyStatTextForm({
  keyStat,
  isReordering,
  testId = 'keyStat',
  onSubmit,
  onCancel,
}: EditableKeyStatTextFormProps) {
  const handleSubmit = useFormSubmit<KeyStatTextFormValues>(async values => {
    await onSubmit({
      ...values,
      guidanceTitle: values.guidanceTitle.trim(),
      guidanceText: toMarkdown(values.guidanceText),
    });
  });

  return (
    <Formik<KeyStatTextFormValues>
      initialValues={{
        title: keyStat.title ?? '',
        statistic: keyStat.statistic ?? '',
        trend: keyStat.trend ?? '',
        guidanceTitle: keyStat.guidanceTitle ?? 'Help',
        guidanceText: keyStat.guidanceText ? toHtml(keyStat.guidanceText) : '',
      }}
      onSubmit={handleSubmit}
    >
      {form => (
        <Form id={`editableKeyStatForm-${keyStat.id}`}>
          <KeyStatTile
            title={keyStat.title}
            value={keyStat.statistic}
            titleTag="h4"
            testId={testId}
            isReordering={isReordering}
          >
            {/* TODO: EES-2469 Inputs for title/statistic have just been added with no testing / consideration for styling / user experience etc. */}
            <FormFieldTextInput<KeyStatTextFormValues>
              name="title"
              label={<span className={styles.trendText}>Title</span>}
            />
            <FormFieldTextInput<KeyStatTextFormValues>
              name="statistic"
              label={<span className={styles.trendText}>Statistic</span>}
            />
            <FormFieldTextInput<KeyStatTextFormValues>
              name="trend"
              label={<span className={styles.trendText}>Trend</span>}
            />
          </KeyStatTile>

          <FormFieldTextInput<KeyStatTextFormValues>
            formGroupClass="govuk-!-margin-top-2"
            name="guidanceTitle"
            label="Guidance title"
          />

          <FormFieldEditor<KeyStatTextFormValues>
            name="guidanceText"
            toolbarConfig={toolbarConfigs.simple}
            label="Guidance text"
          />

          <ButtonGroup>
            <Button disabled={form.isSubmitting} type="submit">
              Save
            </Button>
            <Button variant="secondary" onClick={onCancel}>
              Cancel
            </Button>
          </ButtonGroup>
        </Form>
      )}
    </Formik>
  );
}
