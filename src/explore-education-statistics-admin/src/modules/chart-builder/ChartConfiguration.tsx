import styles from '@admin/modules/chart-builder/graph-builder.module.scss';
import React from 'react';
import { ChartDefinition } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import {
  FormCheckbox,
  FormGroup,
  FormSelect,
  FormTextInput,
} from '@common/components/form';
import {
  DataBlockMetadata,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import service from '@admin/services/release/edit-release/data/service';
import { SelectOption } from '@common/components/form/FormSelect';
import Button from '@common/components/Button';

interface Props {
  selectedChartType: ChartDefinition;
  chartOptions: ChartOptions;
  onChange: (chartOptions: ChartOptions) => void;
  meta: DataBlockMetadata;
  data: DataBlockResponse;

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

interface InfographicSelectionProps {
  releaseId: string;
  fileId: string;
  onChange: (fileId: string) => void;
}

const InfographicSelection = ({
  releaseId,
  fileId,
  onChange,
}: InfographicSelectionProps) => {
  const [options, setOptions] = React.useState<SelectOption[]>();
  const [selectedOption, setSelectedOption] = React.useState<string>('');

  const [fileName, setFileName] = React.useState<string>('');
  const [file, setFile] = React.useState<File>();

  const updateOptions = (id: string) => {
    service.getChartFiles(id).then(files => {
      setOptions([
        {
          label: 'Upload a new file...',
          value: '',
        },
        ...files.map(({ title, filename }) => ({
          label: title,
          value: filename,
        })),
      ]);
    });
  };

  const uploadFile = () => {
    if (file && fileName !== '') {
      service
        .uploadChartFile(releaseId, {
          file,
          name: fileName,
        })
        .then(() => updateOptions(releaseId))
        .then(() => {
          setSelectedOption(file.name);
          setFileName('');
          setFile(undefined);
        });
    }
  };

  React.useEffect(() => {
    updateOptions(releaseId);
  }, [releaseId]);

  return (
    <>
      <FormGroup>
        <FormSelect
          id="infographic-fileid"
          name="infographic-fileid"
          label="Select the file to show"
          order={[]}
          value={selectedOption}
          onChange={e => {
            setSelectedOption(e.target.value);
            onChange(e.target.value);
          }}
          options={options}
        />

        {selectedOption === '' && (
          <FormGroup>
            <FormTextInput
              id="chart-file-name"
              name="chart-file-name"
              label="Name of Infographic file"
              value={fileName}
              width={10}
              onChange={e => setFileName(e.target.value)}
            />
            <FormTextInput
              id="chart-file"
              label="Select file to upload"
              name="chart-file"
              type="file"
              width={5}
              onChange={e => {
                if (e.target.files && e.target.files.length > 0)
                  setFile(e.target.files[0]);
              }}
            />

            <Button
              type="button"
              disabled={fileName === '' || file === undefined}
              onClick={() => uploadFile()}
            >
              Upload
            </Button>
          </FormGroup>
        )}
      </FormGroup>
      <hr />
    </>
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
          <InfographicSelection
            releaseId={data.releaseId}
            fileId={chartOptions.fileId || ''}
            onChange={fileId => {
              updateChartOptions({
                ...chartOptions,
                fileId,
              });
            }}
          />
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
