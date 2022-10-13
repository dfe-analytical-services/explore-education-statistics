import { toolbarConfigs } from '@admin/config/ckEditorConfig';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import toHtml from '@admin/utils/markdown/toHtml';
import toMarkdown from '@admin/utils/markdown/toMarkdown';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import { Form, FormFieldTextInput } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { KeyStatProps } from '@common/modules/find-statistics/components/KeyStat';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import useKeyStatQuery from '@common/modules/find-statistics/hooks/useKeyStatQuery';
import { Formik } from 'formik';
import React from 'react';
import ReactMarkdown from 'react-markdown';

export interface KeyStatsFormValues {
  dataSummary: string;
  dataDefinitionTitle: string;
  dataDefinition: string;
}

interface EditableKeyStatProps extends KeyStatProps {
  isEditing?: boolean;
  isReordering?: boolean;
  name: string;
  onRemove?: () => void;
  onSubmit: (values: KeyStatsFormValues) => void;
}

const EditableKeyStat = ({
  isEditing = false,
  isReordering = false,
  name,
  releaseId,
  dataBlockId,
  summary,
  testId = 'keyStat',
  onRemove,
  onSubmit,
}: EditableKeyStatProps) => {
  const [showForm, toggleShowForm] = useToggle(false);
  const [removing, toggleRemoving] = useToggle(false);

  const { value: keyStat, isLoading, error } = useKeyStatQuery(
    releaseId,
    dataBlockId,
  );

  const formId = `editableKeyStatForm-${dataBlockId}`;

  const renderInner = () => {
    if (error || !keyStat) {
      return (
        <>
          <WarningMessage>Could not load key stat</WarningMessage>

          <ButtonGroup>
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
          </ButtonGroup>
        </>
      );
    }

    if (showForm) {
      return (
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
              dataDefinitionTitle: values.dataDefinitionTitle.trim(),
              dataDefinition: toMarkdown(values.dataDefinition),
            });
            toggleShowForm.off();
          }}
        >
          {form => (
            <Form id={formId}>
              <h3 className="govuk-heading-s">{name}</h3>

              <KeyStatTile
                title={keyStat.title}
                titleTag="h4"
                testId={testId}
                value={keyStat.value}
                isReordering={isReordering}
              >
                <FormFieldTextInput<KeyStatsFormValues>
                  name="dataSummary"
                  label={<span className={styles.trendText}>Trend</span>}
                />
              </KeyStatTile>

              <FormFieldTextInput<KeyStatsFormValues>
                formGroupClass="govuk-!-margin-top-2"
                name="dataDefinitionTitle"
                label="Guidance title"
              />

              <FormFieldEditor<KeyStatsFormValues>
                name="dataDefinition"
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
                <Button variant="secondary" onClick={toggleShowForm.off}>
                  Cancel
                </Button>
              </ButtonGroup>
            </Form>
          )}
        </Formik>
      );
    }

    const dataDefinitionTitle = summary?.dataDefinitionTitle[0] || 'Help';

    return (
      <>
        <KeyStatTile
          title={keyStat.title}
          value={keyStat.value}
          testId={testId}
          isReordering={isReordering}
        >
          {summary?.dataSummary[0] && (
            <p className="govuk-body-s" data-testid={`${testId}-summary`}>
              {summary.dataSummary[0]}
            </p>
          )}
        </KeyStatTile>

        {summary?.dataDefinition[0] && !isReordering && (
          <Details
            summary={dataDefinitionTitle}
            className={styles.definition}
            hiddenText={
              dataDefinitionTitle === 'Help'
                ? `for ${keyStat.title}`
                : undefined
            }
          >
            <div data-testid={`${testId}-definition`}>
              {summary.dataDefinition.map(data => (
                <ReactMarkdown key={data}>{data}</ReactMarkdown>
              ))}
            </div>
          </Details>
        )}

        {isEditing && !isReordering && (
          <ButtonGroup className="govuk-!-margin-top-2">
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
          </ButtonGroup>
        )}
      </>
    );
  };

  return <LoadingSpinner loading={isLoading}>{renderInner()}</LoadingSpinner>;
};

export default EditableKeyStat;
