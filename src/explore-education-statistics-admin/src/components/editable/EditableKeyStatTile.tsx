import { toolbarConfigs } from '@admin/components/form/FormEditor';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import toHtml from '@admin/utils/markdown/toHtml';
import toMarkdown from '@admin/utils/markdown/toMarkdown';
import Button from '@common/components/Button';
import { Form, FormFieldTextInput } from '@common/components/form';
import useToggle from '@common/hooks/useToggle';
import KeyStatTile, {
  KeyStatProps,
} from '@common/modules/find-statistics/components/KeyStatTile';
import styles from '@common/modules/find-statistics/components/KeyStatTile.module.scss';
import { Formik } from 'formik';
import React from 'react';

export interface KeyStatsFormValues {
  dataSummary: string;
  dataDefinitionTitle: string;
  dataDefinition: string;
}

interface EditableKeyStatProps extends KeyStatProps {
  id: string;
  isEditing?: boolean;
  name: string;
  onRemove?: () => void;
  onSubmit: (values: KeyStatsFormValues) => void;
}

const EditableKeyStatTile = ({
  id,
  isEditing = false,
  name,
  query,
  summary,
  onRemove,
  onSubmit,
}: EditableKeyStatProps) => {
  const [showForm, toggleShowForm] = useToggle(false);
  const [removing, toggleRemoving] = useToggle(false);

  if (!isEditing) {
    return <KeyStatTile query={query} summary={summary} />;
  }

  return showForm ? (
    <Formik<KeyStatsFormValues>
      initialValues={{
        dataSummary: summary?.dataSummary?.[0] ?? '',
        dataDefinitionTitle: summary?.dataDefinitionTitle?.[0] ?? 'Help',
        dataDefinition: summary?.dataDefinition?.[0]
          ? toHtml(summary?.dataDefinition?.[0])
          : '',
      }}
      onSubmit={values => {
        onSubmit({
          ...values,
          dataDefinition: toMarkdown(values.dataDefinition),
        });
        toggleShowForm.off();
      }}
    >
      {form => {
        return (
          <Form id={`key-stats-form-${id}`}>
            <h3 className="govuk-heading-s">{name}</h3>

            <KeyStatTile
              query={query}
              renderDataSummary={
                <FormFieldTextInput<KeyStatsFormValues>
                  id={`key-stat-dataSummary-${id}`}
                  name="dataSummary"
                  label={<span className={styles.trendText}>Trend</span>}
                />
              }
            >
              <FormFieldTextInput<KeyStatsFormValues>
                formGroupClass="govuk-!-margin-top-2"
                id={`key-stat-dataDefinitionTitle-${id}`}
                name="dataDefinitionTitle"
                label="Guidance title"
              />

              <FormFieldEditor<KeyStatsFormValues>
                name="dataDefinition"
                toolbarConfig={toolbarConfigs.reduced}
                id={`key-stat-dataDefinition-${id}`}
                label="Guidance text"
              />

              <Button
                disabled={!form.isValid}
                type="submit"
                className="govuk-!-margin-right-2"
              >
                Save
              </Button>
              <Button variant="secondary" onClick={toggleShowForm.off}>
                Cancel
              </Button>
            </KeyStatTile>
          </Form>
        );
      }}
    </Formik>
  ) : (
    <KeyStatTile query={query} summary={summary}>
      <div className="govuk-!-margin-top-2">
        <Button onClick={toggleShowForm.on}>Edit</Button>

        {onRemove && (
          <Button
            disabled={removing}
            variant="secondary"
            onClick={() => {
              toggleRemoving.on();
              onRemove();
            }}
          >
            Remove
          </Button>
        )}
      </div>
    </KeyStatTile>
  );
};

export default EditableKeyStatTile;
