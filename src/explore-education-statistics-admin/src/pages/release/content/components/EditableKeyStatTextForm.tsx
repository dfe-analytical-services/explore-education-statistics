import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import { KeyStatisticText } from '@common/services/publicationService';
import React from 'react';
import Yup from '@common/validation/yup';

export interface KeyStatTextFormValues {
  title: string;
  statistic: string;
  trend: string;
  guidanceTitle?: string;
  guidanceText?: string;
}

interface EditableKeyStatTextFormProps {
  keyStat?: KeyStatisticText;
  keyStatisticGuidanceTitles?: (string | undefined)[];
  onSubmit: (values: KeyStatTextFormValues) => void;
  onCancel: () => void;
  testId: string;
}

export default function EditableKeyStatTextForm({
  keyStat,
  keyStatisticGuidanceTitles,
  onSubmit,
  onCancel,
  testId,
}: EditableKeyStatTextFormProps) {
  const handleSubmit = async (values: KeyStatTextFormValues) => {
    await onSubmit({
      ...values,
      guidanceTitle: values.guidanceTitle,
      guidanceText: values.guidanceText,
    });
  };

  return (
    <div data-testid={testId}>
      <FormProvider
        initialValues={{
          title: keyStat?.title,
          statistic: keyStat?.statistic,
          trend: keyStat?.trend,
          guidanceTitle: keyStat?.guidanceTitle,
          guidanceText: keyStat?.guidanceText,
        }}
        validationSchema={Yup.object<KeyStatTextFormValues>({
          title: Yup.string().required('Enter a title').max(60),
          statistic: Yup.string().required('Enter a statistic').max(12),
          trend: Yup.string().max(230),
          guidanceTitle: Yup.string()
            .max(65)
            .when('guidanceText', {
              is: (val: string) => val !== '',
              then: s => s.required('Enter a guidance title'),
            })
            .test({
              name: 'duplicateGuidanceTitles',
              message: 'Guidance titles must be unique',
              test: (value?: string) =>
                !(
                  value !== undefined &&
                  value !== '' &&
                  keyStatisticGuidanceTitles?.includes(value?.toLowerCase())
                ),
            }),
          guidanceText: Yup.string(),
        })}
      >
        {({ formState }) => {
          return (
            <Form
              id={
                keyStat
                  ? `editableKeyStatTextForm-${keyStat.id}`
                  : 'editableKeyStatTextForm-create'
              }
              onSubmit={handleSubmit}
            >
              <div className={styles.textTile}>
                <FormFieldTextInput<KeyStatTextFormValues>
                  name="title"
                  label={<span>Title</span>}
                />
                <FormFieldTextInput<KeyStatTextFormValues>
                  name="statistic"
                  label={<span>Statistic</span>}
                />
                <FormFieldTextInput<KeyStatTextFormValues>
                  name="trend"
                  label={<span>Trend</span>}
                />
              </div>

              <FormFieldTextInput<KeyStatTextFormValues>
                formGroupClass="govuk-!-margin-top-2"
                name="guidanceTitle"
                label="Guidance title"
              />

              <FormFieldTextArea<KeyStatTextFormValues>
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
    </div>
  );
}
