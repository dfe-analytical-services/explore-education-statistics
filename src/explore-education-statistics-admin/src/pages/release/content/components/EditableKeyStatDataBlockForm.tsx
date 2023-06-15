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
import { KeyStatisticDataBlock } from '@common/services/publicationService';
import { Formik } from 'formik';
import Yup from '@common/validation/yup';
import React from 'react';

export interface KeyStatDataBlockFormValues {
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

export interface EditableKeyStatDataBlockFormProps {
  keyStat: KeyStatisticDataBlock;
  title: string;
  statistic: string;
  isReordering?: boolean;
  testId: string;
  onSubmit: (values: KeyStatDataBlockFormValues) => Promise<void>;
  onCancel: () => void;
}

const EditableKeyStatDataBlockForm = ({
  keyStat,
  title,
  statistic,
  isReordering,
  testId,
  onSubmit,
  onCancel,
}: EditableKeyStatDataBlockFormProps) => {
  const handleSubmit = useFormSubmit<KeyStatDataBlockFormValues>(
    async values => {
      await onSubmit({
        ...values,
        guidanceTitle: values.guidanceTitle.trim(),
        guidanceText: toMarkdown(values.guidanceText),
      });
    },
  );

  return (
    <Formik<KeyStatDataBlockFormValues>
      initialValues={{
        trend: keyStat.trend ?? '',
        guidanceTitle: keyStat.guidanceTitle ?? 'Help',
        guidanceText: keyStat.guidanceText ? toHtml(keyStat.guidanceText) : '',
      }}
      validationSchema={Yup.object<KeyStatDataBlockFormValues>({
        trend: Yup.string().max(230),
        guidanceTitle: Yup.string().max(65),
        guidanceText: Yup.string(),
      })}
      onSubmit={handleSubmit}
    >
      {form => (
        <Form id={`editableKeyStatDataBlockForm-${keyStat.id}`}>
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

          <FormFieldEditor<KeyStatDataBlockFormValues>
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
};

export default EditableKeyStatDataBlockForm;
