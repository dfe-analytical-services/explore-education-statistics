import useGetChartFile from '@admin/hooks/useGetChartFile';
import ChartAxisConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartAxisConfiguration';
import ChartBuilderPreview from '@admin/pages/release/edit-release/manage-datablocks/components/ChartBuilderPreview';
import ChartConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartConfiguration';
import ChartDataSelector from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDataSelector';
import ChartDefinitionSelector from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDefinitionSelector';
import {
  ChartOptions,
  FormState,
  useChartBuilderReducer,
} from '@admin/pages/release/edit-release/manage-datablocks/reducers/chartBuilderReducer';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useToggle from '@common/hooks/useToggle';
import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import {
  horizontalBarBlockDefinition,
  HorizontalBarProps,
} from '@common/modules/charts/components/HorizontalBarBlock';
import {
  lineChartBlockDefinition,
  LineChartProps,
} from '@common/modules/charts/components/LineChartBlock';
import { mapBlockDefinition } from '@common/modules/charts/components/MapBlock';
import { MapBlockInternalProps } from '@common/modules/charts/components/MapBlockInternal';
import {
  verticalBarBlockDefinition,
  VerticalBarProps,
} from '@common/modules/charts/components/VerticalBarBlock';
import {
  AxisConfiguration,
  AxisType,
  ChartDefinition,
  ChartProps,
} from '@common/modules/charts/types/chart';
import { DataSetConfiguration } from '@common/modules/charts/types/dataSet';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import {
  TableDataQuery,
  TableDataResult,
} from '@common/services/tableBuilderService';
import { Chart } from '@common/services/types/blocks';
import { Dictionary } from '@common/types';
import parseNumber from '@common/utils/number/parseNumber';
import mapValues from 'lodash/mapValues';
import omit from 'lodash/omit';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';

const chartDefinitions: ChartDefinition[] = [
  lineChartBlockDefinition,
  verticalBarBlockDefinition,
  horizontalBarBlockDefinition,
  mapBlockDefinition,
];

const filterChartProps = (props: ChartProps): Chart => {
  // We don't want to persist data set labels
  // anymore in the deprecated format.
  return omit(props, ['data', 'meta', 'labels']) as Chart;
};

export interface ChartBuilderForm extends FormState {
  title: string;
  id: string;
}

export type TableQueryUpdateHandler = (
  query: Partial<TableDataQuery>,
) => Promise<void>;

interface Props {
  data: TableDataResult[];
  meta: FullTableMeta;
  releaseId: string;
  initialConfiguration?: Chart;
  onChartSave: (chart: Chart) => void;
  onChartDelete: (chart: Chart) => void;
  onTableQueryUpdate: TableQueryUpdateHandler;
}

const ChartBuilder = ({
  data,
  meta,
  releaseId,
  onChartSave,
  onChartDelete,
  initialConfiguration,
  onTableQueryUpdate,
}: Props) => {
  const containerRef = useRef<HTMLDivElement>(null);

  const [showDeleteModal, toggleDeleteModal] = useToggle(false);

  const [isDataLoading, setDataLoading] = useState(false);
  const [shouldSave, setShouldSave] = useState(false);

  const { state: chartBuilderState, actions } = useChartBuilderReducer(
    initialConfiguration,
  );

  const { axes, definition, options, forms: formStates } = chartBuilderState;

  const getChartFile = useGetChartFile(releaseId);

  const forms: {
    options: ChartBuilderForm;
    data: ChartBuilderForm;
    [key: string]: ChartBuilderForm;
  } = useMemo(() => {
    const formTitles: Dictionary<string> = {
      ...mapValues(
        (definition?.axes as Required<ChartDefinition['axes']>) ?? {},
        axis => axis.title,
      ),
      data: 'Data sets',
      options: 'Chart configuration',
    };

    return mapValues(formStates, (form, formKey) => ({
      ...form,
      title: formTitles[formKey],
      id: `chartBuilder-${formKey}`,
    }));
  }, [definition, formStates]);

  const canSaveChart = useMemo(
    () => Object.values(forms).every(form => form.isValid),
    [forms],
  );

  const hasSubmittedChart = useMemo(
    () => Object.values(forms).some(form => form.submitCount > 0),
    [forms],
  );

  const chartProps = useMemo<ChartRendererProps | undefined>(() => {
    if (!definition) {
      return undefined;
    }

    const baseProps: ChartProps = {
      ...options,
      data,
      axes,
      meta,
    };

    switch (definition.type) {
      case 'infographic':
        return {
          ...baseProps,
          labels: [],
          type: 'infographic',
          fileId: options.fileId ?? '',
          getInfographic: getChartFile,
        };
      case 'line':
        return {
          ...(baseProps as LineChartProps),
          type: 'line',
        };
      case 'horizontalbar':
        return {
          ...(baseProps as HorizontalBarProps),
          type: 'horizontalbar',
        };
      case 'verticalbar':
        return {
          ...(baseProps as VerticalBarProps),
          type: 'verticalbar',
        };
      case 'map':
        return {
          ...(baseProps as MapBlockInternalProps),
          type: 'map',
        };
      default:
        return undefined;
    }
  }, [axes, data, definition, getChartFile, meta, options]);

  // Save the chart using an effect as it's easier to
  // ensure that the correct `chartProps` are passed
  // to the `onChartSave` callback.
  useEffect(() => {
    if (!shouldSave) {
      return;
    }

    if (!canSaveChart || !chartProps) {
      return;
    }

    setShouldSave(false);

    if (containerRef.current) {
      containerRef.current.scrollIntoView({
        behavior: 'smooth',
        block: 'start',
      });
    }

    onChartSave(filterChartProps(chartProps));
  }, [canSaveChart, chartProps, onChartSave, shouldSave]);

  const handleChartDelete = useCallback(async () => {
    toggleDeleteModal.off();

    if (!chartProps) {
      return;
    }

    await onChartDelete(filterChartProps(chartProps));

    actions.resetState();
  }, [actions, chartProps, onChartDelete, toggleDeleteModal]);

  const handleChartDefinitionChange = useCallback(
    async (chartDefinition: ChartDefinition) => {
      actions.updateChartDefinition(chartDefinition);

      if (chartDefinition.type === 'map') {
        setDataLoading(true);

        await onTableQueryUpdate({
          includeGeoJson: true,
        });

        setDataLoading(false);
      }
    },
    [actions, onTableQueryUpdate],
  );

  const [handleChartConfigurationChange] = useDebouncedCallback(
    actions.updateChartOptions,
    200,
  );

  const [handleAxisConfigurationChange] = useDebouncedCallback(
    actions.updateChartAxis,
    200,
  );

  const handleBoundaryLevelChange = useCallback(
    async (boundaryLevel: string) => {
      setDataLoading(true);

      await onTableQueryUpdate({
        boundaryLevel: parseNumber(boundaryLevel),
      });

      setDataLoading(false);
    },
    [onTableQueryUpdate],
  );

  const handleChartDataSubmit = useCallback(
    (nextDataSets: DataSetConfiguration[]) => {
      actions.updateDataSets(nextDataSets);
      setShouldSave(true);
    },
    [actions],
  );

  const handleChartConfigurationSubmit = useCallback(
    (nextChartOptions: ChartOptions) => {
      actions.updateChartOptions(nextChartOptions);
      setShouldSave(true);
    },
    [actions],
  );

  const handleAxisConfigurationSubmit = useCallback(
    (nextAxis: AxisConfiguration) => {
      actions.updateChartAxis(nextAxis);
      setShouldSave(true);
    },
    [actions],
  );

  const deleteButton = useMemo(
    () => (
      <Button variant="warning" onClick={toggleDeleteModal.on}>
        Delete chart
      </Button>
    ),
    [toggleDeleteModal.on],
  );

  return (
    <div ref={containerRef}>
      <ChartDefinitionSelector
        chartDefinitions={chartDefinitions}
        selectedChartDefinition={definition}
        geoJsonAvailable={meta.geoJsonAvailable}
        onChange={handleChartDefinitionChange}
      />

      {definition && (
        <ChartBuilderPreview
          axes={axes}
          chart={chartProps}
          definition={definition}
          loading={isDataLoading}
        />
      )}

      {definition && (
        <Tabs id="chartBuilder-tabs" modifyHash={false}>
          <TabsSection
            title="Chart configuration"
            headingTitle="Chart configuration"
            id={forms.options.id}
          >
            <ChartConfiguration
              buttons={deleteButton}
              canSaveChart={canSaveChart}
              hasSubmittedChart={hasSubmittedChart}
              forms={forms}
              definition={definition}
              chartOptions={options}
              meta={meta}
              releaseId={releaseId}
              onBoundaryLevelChange={handleBoundaryLevelChange}
              onChange={handleChartConfigurationChange}
              onFormStateChange={actions.updateFormState}
              onSubmit={handleChartConfigurationSubmit}
            />
          </TabsSection>

          {definition.data.length > 0 && (
            <TabsSection
              title="Data sets"
              headingTitle="Choose data to add to the chart"
              id={forms.data.id}
            >
              <ChartDataSelector
                buttons={deleteButton}
                canSaveChart={canSaveChart}
                forms={forms}
                meta={meta}
                dataSets={axes.major?.dataSets}
                definition={definition}
                onDataAdded={actions.addDataSet}
                onDataRemoved={actions.removeDataSet}
                onDataChanged={actions.updateDataSets}
                onFormStateChange={actions.updateFormState}
                onSubmit={handleChartDataSubmit}
              />
            </TabsSection>
          )}

          {Object.entries(definition.axes as Required<ChartDefinition['axes']>)
            .filter(([, axis]) => !axis.hide)
            .map(([type, axis]) => {
              const axisConfiguration = axes[type as AxisType];

              if (!axisConfiguration) {
                return null;
              }

              return (
                <TabsSection
                  key={type}
                  id={forms[type].id}
                  title={axis.title}
                  headingTitle={axis.title}
                >
                  <ChartAxisConfiguration
                    buttons={deleteButton}
                    canSaveChart={canSaveChart}
                    hasSubmittedChart={hasSubmittedChart}
                    forms={forms}
                    id={`chartAxisConfiguration-${type}`}
                    type={type as AxisType}
                    configuration={axisConfiguration}
                    definition={definition}
                    data={data}
                    meta={meta}
                    onChange={handleAxisConfigurationChange}
                    onFormStateChange={actions.updateFormState}
                    onSubmit={handleAxisConfigurationSubmit}
                  />
                </TabsSection>
              );
            })}
        </Tabs>
      )}

      <ModalConfirm
        title="Delete chart"
        mounted={showDeleteModal}
        onConfirm={handleChartDelete}
        onExit={toggleDeleteModal.off}
      >
        <p>Are you sure you want to delete this chart?</p>
      </ModalConfirm>
    </div>
  );
};

export default ChartBuilder;
