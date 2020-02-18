import styles from '@admin/modules/chart-builder/graph-builder.module.scss';
import service from '@admin/services/release/edit-release/data/service';
import submitWithFormikValidation from '@admin/validation/formikSubmitHandler';
import { ErrorControlProps } from '@admin/validation/withErrorControl';
import Button from '@common/components/Button';
import {
  FormCheckbox,
  FormGroup,
  Formik,
  FormSelect,
  FormTextInput,
} from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldFileSelector from '@common/components/form/FormFieldFileSelector';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { SelectOption } from '@common/components/form/FormSelect';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import Yup from '@common/lib/validation/yup';
import {
  ChartDefinition,
  ChartMetaData,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { DataBlockResponse } from '@common/services/dataBlockService';
import { FormikProps } from 'formik';
import React, { useState } from 'react';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';

interface Props {
  selectedChartType: ChartDefinition;
  chartOptions: ChartOptions;
  onChange: (chartOptions: ChartOptions) => void;
  data: DataBlockResponse;
  meta: ChartMetaData;

  onBoundaryLevelChange?: (boundaryLevel: string) => void;
}

export interface ChartOptions {
  stacked: boolean;
  legend: 'none' | 'top' | 'bottom';
  legendHeight: string;
  height?: number;
  width?: number;
  title?: string;
  fileId?: string;
  geographicId?: string;
}

interface InfographicChartOptionsProps {
  releaseId: string;
  fileId?: string;
  onChange: (fileId: string) => void;
}

const loadChartFilesAndMapToSelectOptionAsync = (
  releaseId: string,
): Promise<SelectOption[]> => {
  return service.getChartFiles(releaseId).then(chartFiles => {
    return [
      {
        label: 'Upload a new file',
        value: '',
      },
      ...chartFiles.map(({ title, filename }) => ({
        label: title,
        value: filename,
      })),
    ];
  });
};

interface FormValues {
  name: string;
  file: File | null;
  fileId: string;
}

const InfographicChartOptions = ({
  releaseId,
  fileId,
  onChange,
  handleApiErrors,
}: InfographicChartOptionsProps & ErrorControlProps) => {
  const [chartFileOptions, setChartFileOptions] = useState<SelectOption[]>([]);

  const [uploading, setUploading] = useState(false);
  const [fileName, setFileName] = useState<string>();
  const [deleteFile, setDeleteFile] = useToggle(false);

  const formId = 'fileUploadForm';

  const errorCodeMappings = [
    errorCodeToFieldError('FILE_TYPE_INVALID', 'file', 'Choose an image file'),
    errorCodeToFieldError(
      'CANNOT_OVERWRITE_FILE',
      'file',
      'Choose a unique file',
    ),
  ];

  const submitFormHandler = submitWithFormikValidation<FormValues>(
    async values => {
      if (values.file) {
        setUploading(true);

        await service
          .uploadChartFile(releaseId, {
            name: values.name,
            file: values.file as File,
          })
          .then(() => loadChartFilesAndMapToSelectOptionAsync(releaseId))
          .then(setChartFileOptions)
          .then(() => onChange((values.file as File).name))
          .finally(() => {
            setUploading(false);
          });
      }
    },
    handleApiErrors,
    ...errorCodeMappings,
  );

  React.useEffect(() => {
    loadChartFilesAndMapToSelectOptionAsync(releaseId)
      .then(setChartFileOptions)
      .catch(handleApiErrors);
  }, [releaseId, handleApiErrors]);

  React.useEffect(() => {
    const selectedFile = chartFileOptions.find(
      fileOption => fileOption.value === fileId,
    );
    if (selectedFile) {
      setFileName(selectedFile.label);
    } else {
      setFileName('');
    }
  }, [fileId, chartFileOptions]);

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        name: '',
        file: null,
        fileId: fileId || '',
      }}
      validationSchema={Yup.object<FormValues>({
        name: Yup.string().required('Enter a name'),
        file: Yup.mixed().required('Choose a file'),
        fileId: Yup.string(),
      })}
      onSubmit={submitFormHandler}
      render={(form: FormikProps<FormValues>) => {
        return (
          <Form id={formId}>
            {fileId && (
              <>
                <div className={styles.deleteInfographicContainer}>
                  <p className="govuk-!-margin-right-2">{`${fileName}, ${fileId}`}</p>
                  <ButtonText
                    variant="warning"
                    onClick={() => {
                      setDeleteFile(true);
                    }}
                  >
                    Delete infographic
                  </ButtonText>
                </div>
                <ModalConfirm
                  mounted={deleteFile}
                  title="Confirm deletion of infographic"
                  onExit={() => setDeleteFile(false)}
                  onCancel={() => setDeleteFile(false)}
                  onConfirm={async () => {
                    // eslint-disable-next-line no-unused-expressions
                    form.values.fileId &&
                      service
                        .deleteChartFile(releaseId, form.values.fileId)
                        .then(() =>
                          loadChartFilesAndMapToSelectOptionAsync(releaseId),
                        )
                        .then(setChartFileOptions)
                        .catch(handleApiErrors);
                    onChange('');
                    setDeleteFile(false);
                  }}
                >
                  <p>
                    This data will no longer be available for use in this chart
                  </p>
                </ModalConfirm>
              </>
            )}

            {!fileId && (
              <>
                <FormFieldTextInput
                  id={`${formId}-name`}
                  name="name"
                  label="Select a name to give the file"
                  width={10}
                />

                <FormFieldFileSelector<FormValues>
                  id={`${formId}-file`}
                  name="file"
                  label="Select a file to upload"
                  form={form}
                />

                <Button
                  type="submit"
                  disabled={!form.values.file || !form.values.name || uploading}
                >
                  Upload
                </Button>
              </>
            )}
          </Form>
        );
      }}
    />
  );
};
const ChartConfiguration = ({
  chartOptions: initialChartOptions,
  selectedChartType,
  onChange,
  meta,
  data,
  onBoundaryLevelChange,
  handleApiErrors,
  handleManualErrors,
}: Props & ErrorControlProps) => {
  const [chartOptions, setChartOptions] = React.useState<ChartOptions>(
    initialChartOptions,
  );
  const updateChartOptions = (options: ChartOptions) => {
    setChartOptions(options);
    if (onChange) onChange(options);
  };
  const [chartWidth, setChartWidth] = React.useState(
    `${initialChartOptions.width || ''}`,
  );
  const [chartHeight, setChartHeight] = React.useState(
    `${initialChartOptions.height || ''}`,
  );
  return (
    <>
      {selectedChartType.type === 'infographic' && (
        <>
          <InfographicChartOptions
            releaseId={data.releaseId}
            fileId={chartOptions.fileId || ''}
            onChange={fileId => {
              updateChartOptions({
                ...chartOptions,
                fileId,
              });
            }}
            handleApiErrors={handleApiErrors}
            handleManualErrors={handleManualErrors}
          />
          <hr />
        </>
      )}
      <div className={styles.axesOptions}>
        <FormGroup className={styles.formGroupWide}>
          <FormTextInput
            id="chart-title"
            name="chart-title"
            label="Chart title"
            value={chartOptions.title}
            percentageWidth="three-quarters"
            onChange={e => {
              updateChartOptions({
                ...chartOptions,
                title: e.target.value,
              });
            }}
          />
          {selectedChartType.capabilities.stackable && (
            <FormCheckbox
              id="stacked"
              name="stacked"
              label="Stacked bars"
              checked={chartOptions.stacked}
              value="stacked"
              className={styles['margin-top-30']}
              onChange={e => {
                updateChartOptions({
                  ...chartOptions,
                  stacked: e.target.checked,
                });
              }}
            />
          )}
        </FormGroup>
        {selectedChartType.capabilities.hasLegend && (
          <div className={styles.formGroup}>
            <FormGroup>
              <FormSelect
                id="legend-position"
                name="legend-position"
                value={chartOptions.legend}
                label="Legend position"
                options={[
                  { label: 'Top', value: 'top' },
                  { label: 'Bottom', value: 'bottom' },
                  { label: 'None', value: 'none' },
                ]}
                order={[]}
                onChange={e => {
                  updateChartOptions({
                    ...chartOptions,
                    // @ts-ignore
                    legend: e.target.value,
                  });
                }}
              />
            </FormGroup>
            {chartOptions.legend !== 'none' && (
              <FormGroup>
                <FormTextInput
                  id="legend-height"
                  name="legend-height"
                  label="Legend height (px)"
                  hint="Leave blank to set automatically"
                  value={chartOptions.legendHeight}
                  width={5}
                  onChange={e => {
                    updateChartOptions({
                      ...chartOptions,
                      legendHeight: e.target.value,
                    });
                  }}
                />
              </FormGroup>
            )}
          </div>
        )}

        {selectedChartType.capabilities.canSize && (
          <div className={styles.formGroup}>
            <FormGroup>
              <FormTextInput
                id="chart-height"
                name="chart-height"
                label="Chart height (px)"
                value={chartHeight}
                width={5}
                onChange={e => {
                  setChartHeight(e.target.value);
                  updateChartOptions({
                    ...chartOptions,
                    height: parseInt(e.target.value, 10) || undefined,
                  });
                }}
              />
            </FormGroup>
            <FormGroup>
              <FormTextInput
                id="chart-width"
                name="chart-width"
                label="Chart width (px)"
                hint="Leave blank to set as full width"
                value={chartWidth}
                width={5}
                onChange={e => {
                  setChartWidth(e.target.value);
                  updateChartOptions({
                    ...chartOptions,
                    width: parseInt(e.target.value, 10) || undefined,
                  });
                }}
              />
            </FormGroup>
          </div>
        )}

        {selectedChartType.type === 'map' && meta.boundaryLevels && (
          <>
            {meta.boundaryLevels.length === 1 && (
              <div>
                Using <em>{meta.boundaryLevels[0].label}</em>
              </div>
            )}
            {meta.boundaryLevels.length > 1 && (
              <FormGroup className={styles.formGroup}>
                <FormSelect
                  id="geographicId"
                  label="Select a version of geographical data to use"
                  name="geographicId"
                  order={[]}
                  options={[
                    { label: 'Latest', value: '' },
                    ...meta.boundaryLevels.map(({ id, label }) => ({
                      value: id,
                      label,
                    })),
                  ]}
                  onChange={e => {
                    if (onBoundaryLevelChange) {
                      onBoundaryLevelChange(e.target.value);
                    }

                    updateChartOptions({
                      ...chartOptions,
                      geographicId: e.target.value,
                    });
                  }}
                  value={chartOptions.geographicId}
                />
              </FormGroup>
            )}
          </>
        )}
      </div>
    </>
  );
};

export default ChartConfiguration;
