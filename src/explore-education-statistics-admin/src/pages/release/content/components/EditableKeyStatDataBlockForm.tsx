import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { KeyStatisticDataBlock } from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import React from 'react';

export interface KeyStatDataBlockFormValues {
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

export interface EditableKeyStatDataBlockFormProps {
  isReordering?: boolean;
  keyStat: KeyStatisticDataBlock;
  statistic: string;
  testId: string;
  title: string;
  onSubmit: (values: KeyStatDataBlockFormValues) => void;
  onCancel: () => void;
}

export default function EditableKeyStatDataBlockForm({
  isReordering = false,
  keyStat,
  statistic,
  testId,
  title,
  onSubmit,
  onCancel,
}: EditableKeyStatDataBlockFormProps) {
  const handleSubmit = async (values: KeyStatDataBlockFormValues) => {
    onSubmit({
      ...values,
      guidanceTitle: values.guidanceTitle,
      guidanceText: values.guidanceText,
    });
  };

  return (
    <FormProvider
      initialValues={{
        trend: keyStat.trend ?? '',
        guidanceTitle: keyStat.guidanceTitle ?? 'Help',
        guidanceText: keyStat.guidanceText,
      }}
      validationSchema={Yup.object<KeyStatDataBlockFormValues>({
        trend: Yup.string().max(230),
        guidanceTitle: Yup.string().max(65),
        guidanceText: Yup.string(),
      })}
    >
      {({ formState }) => {
        return (
          <Form
            id={`editableKeyStatDataBlockForm-${keyStat.id}`}
            onSubmit={handleSubmit}
          >
            <KeyStatTile
              title={title}
              titleTag="h4"
              testId={testId}
              value={statistic}
              isReordering={isReordering}
            >
              <FormFieldTextInput<KeyStatDataBlockFormValues>
                name="trend"
                label={<span className={styles.trendText}>Trend</span>}
              />
            </KeyStatTile>

            <FormFieldTextInput<KeyStatDataBlockFormValues>
              formGroupClass="govuk-!-margin-top-2"
              name="guidanceTitle"
              label="Guidance title"
            />

            <FormFieldTextArea<KeyStatDataBlockFormValues>
              label="Guidance text"
              name="guidanceText"
              rows={3}
            />

            <ButtonGroup>
              <Button disabled={formState.isSubmitting} type="submit">
                Save
              </Button>
              <Button variant="secondary" onClick={onCancel}>
                Cancel
              </Button>
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
}
