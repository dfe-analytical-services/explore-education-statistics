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
import { KeyStatsFormValues } from '@admin/components/editable/EditableKeyStat';

export interface EditableKeyStatDataBlockFormProps {
  keyStatId: string;
  title: string;
  statistic: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
  isReordering?: boolean;
  onSubmit: (values: KeyStatsFormValues) => void;
  toggleShowFormOff: () => void;
  testId?: string;
}

const EditableKeyStatDataBlockForm = ({
  keyStatId,
  title,
  statistic,
  trend,
  guidanceTitle,
  guidanceText,
  isReordering,
  onSubmit,
  toggleShowFormOff,
  testId = 'keyStat',
}: EditableKeyStatDataBlockFormProps) => {
  return (
    <Formik<KeyStatsFormValues>
      initialValues={{
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
          {/* // @MarkFix? <h3 className="govuk-heading-s">{data_block_name_went_here}</h3>*/}

          <KeyStatTile
            title={title}
            titleTag="h4"
            testId={testId}
            value={statistic}
            isReordering={isReordering}
          >
            <FormFieldTextInput<KeyStatsFormValues>
              name="trend"
              label={<span className={styles.trendText}>Trend</span>}
            />
          </KeyStatTile>

          <FormFieldTextInput<KeyStatsFormValues>
            formGroupClass="govuk-!-margin-top-2"
            name="guidanceTitle"
            label="Guidance title"
          />

          <FormFieldEditor<KeyStatsFormValues>
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

export default EditableKeyStatDataBlockForm;
