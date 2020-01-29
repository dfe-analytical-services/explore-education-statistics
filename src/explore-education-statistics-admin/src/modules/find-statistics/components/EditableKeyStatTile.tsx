import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { Form, FormFieldTextInput, Formik } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import LoadingSpinner from '@common/components/LoadingSpinner';
import formatPretty from '@common/lib/utils/number/formatPretty';
import KeyStatTile, {
  KeyStatConfig,
  KeyStatProps,
} from '@common/modules/find-statistics/components/KeyStatTile';
import styles from '@common/modules/find-statistics/components/SummaryRenderer.module.scss';
import DataBlockService, {
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { Summary } from '@common/services/publicationService';
import { FormikProps } from 'formik';
import React, { useContext, useEffect, useState } from 'react';

type KeyStatsFormValues = Omit<Summary, 'dataKeys'>;

interface EditableKeyStatProps extends KeyStatProps {
  isEditing?: boolean;
  onRemove?: () => void;
  onSubmit: (values: KeyStatsFormValues) => Promise<undefined>;
}

const EditableKeyStatTile = ({
  isEditing = false,
  onRemove,
  onSubmit,
  dataBlockResponse: response,
  dataBlockRequest,
  id,
  name,
  summary,
  ...props
}: EditableKeyStatProps) => {
  const { handleApiErrors } = useContext(ErrorControlContext);
  const [dataBlockResponse, setDataBlockResponse] = useState<
    DataBlockResponse | undefined
  >(response);
  const [config, setConfig] = useState<KeyStatConfig>({
    indicatorLabel: '',
    value: '',
  });
  const [showForm, setShowForm] = useState(false);
  const [removing, setRemoving] = useState(false);

  useEffect(() => {
    if (!dataBlockResponse) {
      DataBlockService.getDataBlockForSubject(dataBlockRequest)
        .then(newResponse => {
          if (newResponse) {
            setDataBlockResponse(newResponse);
            const [indicatorKey, theIndicator] = Object.entries(
              newResponse.metaData.indicators,
            )[0];
            setConfig({
              indicatorLabel: theIndicator.label,
              value: `${formatPretty(
                newResponse.result[0].measures[indicatorKey],
              )}${theIndicator.unit}`,
            });
          }
        })
        .catch(handleApiErrors);
    }
  }, [dataBlockRequest, dataBlockResponse, handleApiErrors]);

  if (!dataBlockResponse) {
    return (
      <div className={styles.keyStatTile}>
        <span className="govuk-heading-s">{name}</span>
        <div className={styles.keyStat}>
          <LoadingSpinner className="govuk-!-margin-bottom-2 govuk-!-margin-top-2" />
        </div>
      </div>
    );
  }

  if (!isEditing) {
    return (
      <KeyStatTile
        dataBlockRequest={dataBlockRequest}
        id={id}
        name={name}
        {...props}
        dataBlockResponse={dataBlockResponse}
        summary={summary}
        handleApiErrors={handleApiErrors}
      />
    );
  }

  return (
    <>
      {config.indicatorLabel && showForm ? (
        <div className={styles.keyStatTile}>
          <Formik<KeyStatsFormValues>
            initialValues={{
              dataSummary: (summary && summary.dataSummary) || '',
              dataDefinitionTitle:
                (summary && summary.dataDefinitionTitle) ||
                `Define '${config.indicatorLabel}'`,
              dataDefinition: (summary && summary.dataDefinition) || '',
            }}
            onSubmit={values => {
              onSubmit(values).then(() => {
                setShowForm(false);
              });
            }}
            render={(form: FormikProps<KeyStatsFormValues>) => {
              return (
                <Form id={`key-stats-form-${id}`}>
                  <span className="govuk-heading-s">{name}</span>
                  <div className={styles.keyStat}>
                    <h3
                      className="govuk-heading-s"
                      data-testid="key-stat-tile-title"
                    >
                      {config.indicatorLabel}
                    </h3>
                    <p
                      className="govuk-heading-xl"
                      data-testid="key-stat-tile-value"
                    >
                      {config.value}
                    </p>
                    <div className="govuk-!-margin-top-1">
                      <FormFieldTextInput<KeyStatsFormValues>
                        id={`key-stat-dataSummary-${id}`}
                        name="dataSummary"
                        label={<span className={styles.trendText}>Trend</span>}
                      />
                    </div>
                  </div>
                  <Details summary="Guidance text" open>
                    <FormFieldTextInput<KeyStatsFormValues>
                      id={`key-stat-dataDefinitionTitle-${id}`}
                      name="dataDefinitionTitle"
                      label="Guidance title"
                    />
                    <FormFieldTextArea<KeyStatsFormValues>
                      id={`key-stat-dataDefinition-${id}`}
                      name="dataDefinition"
                      label="Guidance text"
                    />
                  </Details>
                  <Button
                    disabled={!form.isValid}
                    type="submit"
                    className="govuk-!-margin-right-2"
                  >
                    Save
                  </Button>
                  <Button
                    disabled={removing}
                    variant="secondary"
                    onClick={() => {
                      setShowForm(false);
                    }}
                  >
                    Cancel
                  </Button>
                </Form>
              );
            }}
          />
        </div>
      ) : (
        <KeyStatTile
          dataBlockRequest={dataBlockRequest}
          id={id}
          name={name}
          {...props}
          dataBlockResponse={dataBlockResponse}
          summary={summary}
          handleApiErrors={handleApiErrors}
        >
          <div className="govuk-!-margin-top-2">
            <Button
              onClick={() => {
                setShowForm(true);
              }}
            >
              Edit
            </Button>
            {onRemove && (
              <Button
                disabled={removing}
                variant="secondary"
                onClick={() => {
                  setRemoving(true);
                  onRemove();
                }}
              >
                Remove
              </Button>
            )}
          </div>
        </KeyStatTile>
      )}
    </>
  );
};

export default EditableKeyStatTile;
