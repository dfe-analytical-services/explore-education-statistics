import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
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
          <RHFForm
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
              <RHFFormFieldTextInput<KeyStatDataBlockFormValues>
                name="trend"
                label={<span className={styles.trendText}>Trend</span>}
              />
            </KeyStatTile>

            <RHFFormFieldTextInput<KeyStatDataBlockFormValues>
              formGroupClass="govuk-!-margin-top-2"
              name="guidanceTitle"
              label="Guidance title"
            />

            <RHFFormFieldTextArea<KeyStatDataBlockFormValues>
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
          </RHFForm>
        );
      }}
    </FormProvider>
  );
}
