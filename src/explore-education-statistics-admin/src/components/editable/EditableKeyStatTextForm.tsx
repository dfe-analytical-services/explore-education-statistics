import { toolbarConfigs } from '@admin/config/ckEditorConfig';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import toHtml from '@admin/utils/markdown/toHtml';
import toMarkdown from '@admin/utils/markdown/toMarkdown';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldTextInput } from '@common/components/form';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import { Formik } from 'formik';
import React from 'react';
import { KeyStatisticText } from '@common/services/publicationService';

interface KeyStatTextFormValues {
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
  toggleShowFormOff: () => void;
  testId?: string;
}

const EditableKeyStatTextForm = ({
  keyStat: {
    id: keyStatId,
    title,
    statistic,
    trend,
    guidanceTitle = 'Help',
    guidanceText,
  },
  isReordering,
  onSubmit,
  toggleShowFormOff,
  testId = 'keyStat',
}: EditableKeyStatTextFormProps) => {
  return (
    <Formik<KeyStatTextFormValues>
      initialValues={{
        title: title ?? '',
        statistic: statistic ?? '',
        trend: trend ?? '',
        guidanceTitle: guidanceTitle ?? 'Help',
        guidanceText: guidanceText ? toHtml(guidanceText) : '',
      }}
      onSubmit={values => {
        onSubmit({
          ...values,
          guidanceTitle: values.guidanceTitle.trim(),
          guidanceText: toMarkdown(values.guidanceText),
        });
        toggleShowFormOff();
      }}
    >
      {form => (
        <Form id={`editableKeyStatForm-${keyStatId}`}>
          <KeyStatTile
            title={title}
            value={statistic}
            titleTag="h4"
            testId={testId}
            isReordering={isReordering}
          >
            {/* TODO: EES-3919 Add inputs for title and statistic */}
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
            <Button
              disabled={!form.isValid}
              type="submit"
              className="govuk-!-margin-right-2"
            >
              Save
            </Button>
            <Button variant="secondary" onClick={toggleShowFormOff}>
              Cancel
            </Button>
          </ButtonGroup>
        </Form>
      )}
    </Formik>
  );
};

export default EditableKeyStatTextForm;
