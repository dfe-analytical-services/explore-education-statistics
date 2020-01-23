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
import React, { useEffect, useState } from 'react';

interface EditableKeyStatProps extends KeyStatProps {
  onRemove?: () => void;
  isEditing?: boolean;
}

type KeyStatsFormValues = Omit<Summary, 'dataKeys'>;

const EditableKeyStatTile = ({
  isEditing = false,
  onRemove,
  dataBlockResponse: response,
  dataBlockRequest,
  id,
  name,
  summary,
  ...props
}: EditableKeyStatProps) => {
  const [dataBlockResponse, setDataBlockResponse] = useState<
    DataBlockResponse | undefined
  >(response);
  const [config, setConfig] = useState<KeyStatConfig>({
    indicatorLabel: '',
    value: '',
  });
  const [removing, setRemoving] = useState<boolean>(false);

  useEffect(() => {
    if (!dataBlockResponse) {
      DataBlockService.getDataBlockForSubject(dataBlockRequest).then(
        newResponse => {
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
        },
      );
    }
  }, [dataBlockRequest]);

  return dataBlockResponse ? (
    <>
      {!isEditing ? (
        <KeyStatTile
          dataBlockRequest={dataBlockRequest}
          id={id}
          name={name}
          {...props}
          dataBlockResponse={dataBlockResponse}
        />
      ) : (
        config.indicatorLabel && (
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
                console.log(values);
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
                          id="key-stat-dataSummary"
                          name="dataSummary"
                          label={
                            <span className={styles.trendText}>Trend</span>
                          }
                        />
                      </div>
                    </div>
                    <Details summary="Guidance text" open>
                      <FormFieldTextInput<KeyStatsFormValues>
                        id="key-stat-dataDefinitionTitle"
                        name="dataDefinitionTitle"
                        label="Guidance title"
                      />
                      <FormFieldTextArea<KeyStatsFormValues>
                        id="key-stat-dataDefinition"
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
                    {onRemove && (
                      <Button
                        disabled={removing}
                        variant="secondary"
                        onClick={() => {
                          onRemove();
                        }}
                      >
                        Remove
                      </Button>
                    )}
                  </Form>
                );
              }}
            />
          </div>
        )
      )}
    </>
  ) : (
    <div className={styles.keyStatTile}>
      <span className="govuk-heading-s">{name}</span>
      <div className={styles.keyStat}>
        <LoadingSpinner className="govuk-!-margin-bottom-2 govuk-!-margin-top-2" />
      </div>
    </div>
  );
};

export default EditableKeyStatTile;
