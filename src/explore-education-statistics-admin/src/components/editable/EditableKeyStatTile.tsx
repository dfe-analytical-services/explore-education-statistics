import { toolbarConfigs } from '@admin/components/form/FormEditor';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import toHtml from '@admin/utils/markdown/toHtml';
import toMarkdown from '@admin/utils/markdown/toMarkdown';
import Button from '@common/components/Button';
import { Form, FormFieldTextInput } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import KeyStatTile, {
  KeyStatConfig,
  KeyStatProps,
} from '@common/modules/find-statistics/components/KeyStatTile';
import styles from '@common/modules/find-statistics/components/KeyStatTile.module.scss';
import tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import formatPretty from '@common/utils/number/formatPretty';
import classNames from 'classnames';
import { Formik } from 'formik';
import React, { useEffect, useState } from 'react';

export interface KeyStatsFormValues {
  dataSummary: string;
  dataDefinitionTitle: string;
  dataDefinition: string;
}

interface EditableKeyStatProps extends KeyStatProps {
  isEditing?: boolean;
  onRemove?: () => void;
  onSubmit: (values: KeyStatsFormValues) => void;
  releaseId: string;
}

const EditableKeyStatTile = ({
  isEditing = false,
  onRemove,
  onSubmit,
  releaseId,
  dataBlockRequest,
  id,
  name,
  summary,
  ...props
}: EditableKeyStatProps) => {
  const [dataBlockResponse, setDataBlockResponse] = useState<
    TableDataResponse | undefined
  >();
  const [config, setConfig] = useState<KeyStatConfig>({
    indicatorLabel: '',
    value: '',
  });
  const [showForm, setShowForm] = useState(false);
  const [removing, setRemoving] = useState(false);

  useEffect(() => {
    if (!dataBlockResponse) {
      tableBuilderService
        .getTableData(
          {
            ...dataBlockRequest,
            includeGeoJson: false,
          },
          releaseId,
        )
        .then(newResponse => {
          if (newResponse) {
            setDataBlockResponse(newResponse);
            const [indicator] = newResponse.subjectMeta.indicators;

            setConfig({
              indicatorLabel: indicator.label,
              value: formatPretty(
                newResponse.results[0].measures[indicator.value],
                indicator.unit,
                indicator.decimalPlaces,
              ),
            });
          }
        });
    }
  }, [dataBlockRequest, dataBlockResponse]);

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
        releaseId={releaseId}
        dataBlockRequest={dataBlockRequest}
        id={id}
        name={name}
        {...props}
        summary={summary}
      />
    );
  }

  return (
    <>
      {config.indicatorLabel && showForm ? (
        <div className={styles.keyStatTile}>
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
              setShowForm(false);
            }}
          >
            {form => {
              return (
                <Form id={`key-stats-form-${id}`}>
                  <h3 className="govuk-heading-s">{name}</h3>

                  <div
                    className={classNames(
                      styles.keyStat,
                      'govuk-!-margin-bottom-2',
                    )}
                    data-testid="key-stat-tile"
                  >
                    <h4
                      className="govuk-heading-s"
                      data-testid="key-stat-tile-title"
                    >
                      {config.indicatorLabel}
                    </h4>

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

                  <FormFieldTextInput<KeyStatsFormValues>
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
          </Formik>
        </div>
      ) : (
        <KeyStatTile
          releaseId={releaseId}
          dataBlockRequest={dataBlockRequest}
          id={id}
          name={name}
          {...props}
          summary={summary}
        >
          <div className={styles.keyStatEdit}>
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
