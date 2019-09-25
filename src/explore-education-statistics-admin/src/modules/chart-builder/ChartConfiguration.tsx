import styles from '@admin/modules/chart-builder/graph-builder.module.scss';
import service from '@admin/services/release/edit-release/data/service';
import {
  FormCheckbox,
  FormGroup,
  FormSelect,
  FormTextInput,
} from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import { ChartDefinition, ChartMetaData } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import {
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React from 'react';
import Button from '@common/components/Button';

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

const InfographicChartOptions = ({
  releaseId,
  fileId,
  onChange,
}: InfographicChartOptionsProps) => {
  const [chartFileOptions, setChartFileOptions] = React.useState<
    SelectOption[]
  >([]);

  const [currentFileId, setCurrentFileId] = React.useState(fileId);

  const [uploadName, setUploadName] = React.useState<string>('');

  const [uploadFile, setUploadFile] = React.useState<File>();

  const [uploading, setUploading] = React.useState<boolean>(false);

  const doUpload = () => {
    if (uploadFile) {
      setUploading(true);
      service
        .uploadChartFile(releaseId, {
          name: uploadName,
          file: uploadFile,
        })
        .then(() =>
          loadChartFilesAndMapToSelectOptionAsync(releaseId).then(
            setChartFileOptions,
          ),
        )
        .then(() => {
          onChange(uploadFile.name);
          setCurrentFileId(uploadFile.name);
          setUploadName('');
          setUploadFile(undefined);
        })
        .finally(() => {
          setUploading(false);
        });
    }
  };

  React.useEffect(() => {
    loadChartFilesAndMapToSelectOptionAsync(releaseId).then(
      setChartFileOptions,
    );
  }, [releaseId]);

  return (
    <FormGroup>
      <FormSelect
        id="infographic-fileid"
        name="infographic-fileid"
        label="Select the file to show"
        order={[]}
        options={chartFileOptions}
        value={fileId}
        onChange={e => {
          setCurrentFileId(e.target.value);
          return onChange(e.target.value);
        }}
      />
      {currentFileId === '' && (
        <FormGroup>
          <FormTextInput
            id="upload-name"
            name="upload-name"
            label="Select a name to give the file"
            defaultValue={uploadName}
            onChange={e => setUploadName(e.target.value)}
            width={10}
          />

          <FormTextInput
            id="upload-chart"
            name="upload-chrt"
            label="Select a file to upload"
            type="file"
            onChange={e => {
              if (e.target.files && e.target.files.length > 0) {
                setUploadFile(e.target.files[0]);
              } else {
                setUploadFile(undefined);
              }
            }}
          />

          <Button
            type="button"
            disabled={
              uploadName.length === 0 || uploadFile === undefined || uploading
            }
            onClick={() => doUpload()}
          >
            Upload
          </Button>
        </FormGroup>
      )}
    </FormGroup>
  );
};
const ChartConfiguration = ({
  chartOptions: initialChartOptions,
  selectedChartType,
  onChange,
  meta,
  data,
  onBoundaryLevelChange,
}: Props) => {
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
          />
          <hr />
        </>
      )}
      <div className={styles.axesOptions}>
        <FormGroup className={styles.formGroup}>
          <FormTextInput
            id="chart-title"
            name="chart-title"
            label="Chart title"
            value={chartOptions.title}
            width={20}
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
          <FormGroup className={styles.formGroup}>
            <FormSelect
              id="legend-position"
              name="legend-position"
              value={chartOptions.legend}
              label="Legend Position"
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
            {chartOptions.legend !== 'none' && (
              <FormTextInput
                id="legend-height"
                name="legend-height"
                label="Legend Height (blank for automatic)"
                value={chartOptions.legendHeight}
                width={5}
                onChange={e => {
                  updateChartOptions({
                    ...chartOptions,
                    legendHeight: e.target.value,
                  });
                }}
              />
            )}
          </FormGroup>
        )}

        {selectedChartType.capabilities.canSize && (
          <FormGroup className={styles.formGroup}>
            <FormTextInput
              id="chart-height"
              name="chart-height"
              label="Chart Height"
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
            <FormTextInput
              id="chart-width"
              name="chart-width"
              label="Chart Width (blank to fill)"
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
