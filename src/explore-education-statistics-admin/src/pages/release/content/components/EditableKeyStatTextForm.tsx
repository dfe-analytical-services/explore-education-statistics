import RHFFormFieldEditor from '@admin/components/form/RHFFormFieldEditor';
import {
  pluginsConfigSimple,
  toolbarConfigSimple,
} from '@admin/config/ckEditorConfig';
import toHtml from '@admin/utils/markdown/toHtml';
import toMarkdown from '@admin/utils/markdown/toMarkdown';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import { KeyStatisticText } from '@common/services/publicationService';
import React from 'react';
import classNames from 'classnames';
import Yup from '@common/validation/yup';

export interface KeyStatTextFormValues {
  title: string;
  statistic: string;
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

interface EditableKeyStatTextFormProps {
  keyStat?: KeyStatisticText;
  isReordering?: boolean;
  onSubmit: (values: KeyStatTextFormValues) => void;
  onCancel: () => void;
  testId: string;
}

export default function EditableKeyStatTextForm({
  keyStat,
  isReordering = false,
  onSubmit,
  onCancel,
  testId,
}: EditableKeyStatTextFormProps) {
  const handleSubmit = async (values: KeyStatTextFormValues) => {
    await onSubmit({
      ...values,
      guidanceTitle: values.guidanceTitle,
      guidanceText: toMarkdown(values.guidanceText),
    });
  };

  return (
    <div data-testid={testId}>
      <FormProvider
        initialValues={{
          title: keyStat?.title ?? '',
          statistic: keyStat?.statistic ?? '',
          trend: keyStat?.trend ?? '',
          guidanceTitle: keyStat?.guidanceTitle ?? 'Help',
          guidanceText: keyStat?.guidanceText
            ? toHtml(keyStat.guidanceText)
            : '',
        }}
        validationSchema={Yup.object<KeyStatTextFormValues>({
          title: Yup.string().required('Enter a title').max(60),
          statistic: Yup.string().required('Enter a statistic').max(12),
          trend: Yup.string().max(230),
          guidanceTitle: Yup.string().max(65),
          guidanceText: Yup.string(),
        })}
      >
        {({ formState }) => {
          return (
            <RHFForm
              id={
                keyStat
                  ? `editableKeyStatTextForm-${keyStat.id}`
                  : 'editableKeyStatTextForm-create'
              }
              onSubmit={handleSubmit}
            >
              <div className={styles.textTile}>
                <RHFFormFieldTextInput<KeyStatTextFormValues>
                  name="title"
                  className={classNames({
                    'govuk-!-width-one-third': isReordering,
                  })}
                  label={<span>Title</span>}
                />
                <RHFFormFieldTextInput<KeyStatTextFormValues>
                  name="statistic"
                  className={classNames({
                    'govuk-!-width-one-third': isReordering,
                  })}
                  label={<span>Statistic</span>}
                />
                <RHFFormFieldTextInput<KeyStatTextFormValues>
                  name="trend"
                  className={classNames({
                    'govuk-!-width-one-third': isReordering,
                  })}
                  label={<span>Trend</span>}
                />
              </div>

              <RHFFormFieldTextInput<KeyStatTextFormValues>
                formGroupClass="govuk-!-margin-top-2"
                name="guidanceTitle"
                className={classNames({
                  'govuk-!-width-one-third': isReordering,
                })}
                label="Guidance title"
              />

              <RHFFormFieldEditor<KeyStatTextFormValues>
                name="guidanceText"
                includePlugins={pluginsConfigSimple}
                toolbarConfig={toolbarConfigSimple}
                label="Guidance text"
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
    </div>
  );
}
