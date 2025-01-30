import SubmitError from '@common/components/form/util/SubmitError';
import WarningMessage from '@common/components/WarningMessage';
import locationLevelsMap, {
  LocationLevelKey,
} from '@common/utils/locationLevelsMap';
import { ConfirmContextProvider } from '@common/contexts/ConfirmContext';
import FiltersForm, {
  FilterFormSubmitHandler,
  TableQueryErrorCode,
} from '@common/modules/table-tool/components/FiltersForm';
import { LocationFiltersFormSubmitHandler } from '@common/modules/table-tool/components/LocationFiltersForm';
import LocationStep from '@common/modules/table-tool/components/LocationStep';
import PreviousStepModalConfirm from '@common/modules/table-tool/components/PreviousStepModalConfirm';
import PublicationForm, {
  PublicationFormSubmitHandler,
} from '@common/modules/table-tool/components/PublicationForm';
import DataSetStep, {
  DataSetFormSubmitHandler,
} from '@common/modules/table-tool/components/DataSetStep';
import TimePeriodForm, {
  TimePeriodFormSubmitHandler,
} from '@common/modules/table-tool/components/TimePeriodForm';
import Wizard from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { SelectedPublication } from '@common/modules/table-tool/types/selectedPublication';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import applyTableHeadersOrder from '@common/modules/table-tool/utils/applyTableHeadersOrder';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import parseYearCodeTuple from '@common/modules/table-tool/utils/parseYearCodeTuple';
import publicationService, {
  PublicationTreeSummary,
  Theme,
} from '@common/services/publicationService';
import tableBuilderService, {
  FeaturedTable,
  FullTableQuery,
  LocationOption,
  ReleaseTableDataQuery,
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import React, { ReactElement, ReactNode, useMemo, useState } from 'react';
import { useImmer } from 'use-immer';
import { Dictionary } from 'lodash';

const defaultLocationStepTitle = 'Choose locations';
const defaultDataSetStepTitle = 'Select a data set';

export interface InitialTableToolState {
  initialStep: number;
  selectedPublication?: SelectedPublication;
  subjects?: Subject[];
  featuredTables?: FeaturedTable[];
  subjectMeta?: SubjectMeta;
  query?: ReleaseTableDataQuery;
  response?: {
    table: FullTable;
    tableHeaders: TableHeadersConfig;
  };
}

interface TableToolState extends InitialTableToolState {
  subjects: Subject[];
  featuredTables: FeaturedTable[];
  subjectMeta: SubjectMeta;
  query: ReleaseTableDataQuery;
}

export interface FinalStepRenderProps {
  query?: ReleaseTableDataQuery;
  selectedPublication?: SelectedPublication;
  stepTitle: string;
  table?: FullTable;
  tableHeaders?: TableHeadersConfig;
  onReorder: (reorderedTableHeaders: TableHeadersConfig) => void;
}

export interface TableToolWizardProps {
  currentStep?: number;
  finalStep?: (props: FinalStepRenderProps) => ReactElement;
  hidePublicationStep?: boolean;
  initialState?: Partial<InitialTableToolState>;
  loadingFastTrack?: boolean;
  renderFeaturedTableLink?: (featuredTable: FeaturedTable) => ReactNode;
  scrollOnMount?: boolean;
  showTableQueryErrorDownload?: boolean;
  themeMeta?: Theme[];
  onPublicationFormSubmit?: (publication: PublicationTreeSummary) => void;
  onPublicationStepBack?: () => void;
  onStepChange?: (nextStep: number, previousStep: number) => void;
  onStepSubmit?: ({
    nextStepNumber,
    nextStepTitle,
  }: {
    nextStepNumber: number;
    nextStepTitle: string;
  }) => void;
  onSubjectFormSubmit?(params: {
    publication: SelectedPublication;
    release: SelectedPublication['selectedRelease'];
    subjectId: string;
  }): void;
  onSubjectStepBack?: (publication?: SelectedPublication) => void;
  onSubmit?: (table: FullTable) => void;
  onTableQueryError?: (
    errorCode: TableQueryErrorCode,
    publicationTitle: string,
    subjectName: string,
  ) => void;
}

export default function TableToolWizard({
  currentStep,
  finalStep,
  hidePublicationStep,
  initialState = {},
  loadingFastTrack = false,
  renderFeaturedTableLink,
  scrollOnMount,
  showTableQueryErrorDownload = true,
  themeMeta = [],
  onPublicationFormSubmit,
  onPublicationStepBack,
  onStepChange,
  onStepSubmit,
  onSubjectFormSubmit,
  onSubjectStepBack,
  onSubmit,
  onTableQueryError,
}: TableToolWizardProps) {
  const [state, updateState] = useImmer<TableToolState>({
    initialStep: 1,
    subjects: [],
    featuredTables: [],
    subjectMeta: {
      timePeriod: {
        hint: '',
        legend: '',
        options: [],
      },
      locations: {},
      indicators: {},
      filters: {},
    },
    query: {
      subjectId: '',
      indicators: [],
      filters: [],
      locationIds: [],
    },
    ...initialState,
  });
  const [reorderedTableHeaders, setReorderedTableHeaders] =
    useState<TableHeadersConfig>();
  const locationStepNumber = hidePublicationStep ? 2 : 3;
  const [locationStepTitle, setLocationStepTitle] = useState<string>(
    initialState.initialStep === locationStepNumber && initialState.subjectMeta
      ? getLocationsStepTitle(initialState.subjectMeta?.locations)
      : defaultLocationStepTitle,
  );
  const [dataSetStepTitle, setDataSetStepTitle] = useState<string>(
    defaultDataSetStepTitle,
  );

  const stepTitles = {
    publication: 'Choose a publication',
    dataSet: dataSetStepTitle,
    location: locationStepTitle,
    timePeriod: 'Choose time period',
    filter: 'Choose your filters',
    final: 'Explore data',
  };

  const handlePublicationFormSubmit: PublicationFormSubmitHandler = async ({
    publication,
  }) => {
    onPublicationFormSubmit?.(publication);

    const [subjects, featuredTables] = await Promise.all([
      tableBuilderService.listLatestReleaseSubjects(publication.id),
      tableBuilderService.listLatestReleaseFeaturedTables(publication.id),
    ]);

    const latestRelease =
      await publicationService.getLatestPublicationReleaseSummary(
        publication.slug,
      );

    const updatedDataSetTitle =
      featuredTables.length > 0
        ? 'Select a data set or featured table'
        : defaultDataSetStepTitle;

    updateState(draft => {
      draft.subjects = subjects;
      draft.featuredTables = featuredTables;
      draft.query.releaseVersionId = undefined;
      draft.query.publicationId = publication.id;
      draft.selectedPublication = {
        ...publication,
        selectedRelease: {
          id: latestRelease.id,
          latestData: latestRelease.latestRelease,
          slug: latestRelease.slug,
          title: latestRelease.title,
          type: latestRelease.type,
        },
        latestRelease: {
          title: latestRelease.title,
        },
      };
    });

    setDataSetStepTitle(updatedDataSetTitle);
    onStepSubmit?.({ nextStepNumber: 2, nextStepTitle: updatedDataSetTitle });
  };

  const handlePublicationStepBack = () => {
    onPublicationStepBack?.();
    onStepSubmit?.({
      nextStepNumber: 1,
      nextStepTitle: stepTitles.publication,
    });
  };

  const handleDataSetStepBack = () => {
    updateState(draft => {
      draft.query.subjectId = '';
    });

    onSubjectStepBack?.(state.selectedPublication);
    onStepSubmit?.({ nextStepNumber: 2, nextStepTitle: dataSetStepTitle });
  };

  const handleDataSetFormSubmit: DataSetFormSubmitHandler = async ({
    subjectId: selectedSubjectId,
  }) => {
    if (state.selectedPublication) {
      onSubjectFormSubmit?.({
        publication: state.selectedPublication,
        release: state.selectedPublication.selectedRelease,
        subjectId: selectedSubjectId,
      });
    }

    const nextSubjectMeta = await tableBuilderService.getSubjectMeta(
      selectedSubjectId,
      state.query.releaseVersionId,
    );

    setReorderedTableHeaders(undefined);

    const updatedLocationsTitle = getLocationsStepTitle(
      nextSubjectMeta.locations,
    );
    setLocationStepTitle(updatedLocationsTitle);

    updateState(draft => {
      draft.subjectMeta = nextSubjectMeta;
      draft.query.subjectId = selectedSubjectId;
      draft.query.indicators = [];
      draft.query.filters = [];
      draft.query.locationIds = [];
      draft.query.timePeriod = undefined;
    });

    onStepSubmit?.({ nextStepNumber: 3, nextStepTitle: updatedLocationsTitle });
  };

  const handleLocationStepBack = async () => {
    const { releaseVersionId, subjectId } = state.query;

    const nextSubjectMeta = await tableBuilderService.getSubjectMeta(
      subjectId,
      releaseVersionId,
    );

    updateState(draft => {
      draft.subjectMeta = nextSubjectMeta;
    });

    onStepSubmit?.({ nextStepNumber: 3, nextStepTitle: locationStepTitle });
  };

  const handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler =
    async ({ locationIds }) => {
      const nextSubjectMeta = await tableBuilderService.filterSubjectMeta(
        {
          locationIds,
          subjectId: state.query.subjectId,
        },
        state.query.releaseVersionId,
      );

      const { timePeriod } = state.query;

      // Check if selected time period is in the time period options so can reset it if not.
      const hasStartTimePeriod = nextSubjectMeta.timePeriod.options.some(
        option =>
          option.code === timePeriod?.startCode &&
          option.year === timePeriod.startYear,
      );
      const hasEndTimePeriod = nextSubjectMeta.timePeriod.options.some(
        option =>
          option.code === timePeriod?.endCode &&
          option.year === timePeriod.endYear,
      );

      updateState(draft => {
        draft.subjectMeta.timePeriod = nextSubjectMeta.timePeriod;

        draft.query.locationIds = locationIds;

        if (timePeriod && hasStartTimePeriod && hasEndTimePeriod) {
          draft.query.timePeriod = {
            startYear: hasStartTimePeriod ? timePeriod.startYear : 0,
            startCode: hasStartTimePeriod ? timePeriod.startCode : '',
            endYear: hasEndTimePeriod ? timePeriod.endYear : 0,
            endCode: hasEndTimePeriod ? timePeriod.endCode : '',
          };
        } else {
          draft.query.timePeriod = undefined;
        }
      });

      onStepSubmit?.({
        nextStepNumber: 4,
        nextStepTitle: stepTitles.timePeriod,
      });
    };

  const handleTimePeriodStepBack = async () => {
    const { releaseVersionId, subjectId, locationIds } = state.query;

    const nextSubjectMeta = await tableBuilderService.filterSubjectMeta(
      {
        subjectId,
        locationIds,
      },
      releaseVersionId,
    );

    updateState(draft => {
      draft.subjectMeta.timePeriod = nextSubjectMeta.timePeriod;
    });

    onStepSubmit?.({
      nextStepNumber: 4,
      nextStepTitle: stepTitles.timePeriod,
    });
  };

  const handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler =
    async values => {
      const { releaseVersionId, subjectId, locationIds } = state.query;
      const [startYear, startCode] = parseYearCodeTuple(values.start);
      const [endYear, endCode] = parseYearCodeTuple(values.end);

      const nextSubjectMeta = await tableBuilderService.filterSubjectMeta(
        {
          subjectId,
          locationIds,
          timePeriod: {
            startYear,
            startCode,
            endYear,
            endCode,
          },
        },
        releaseVersionId,
      );

      const indicatorValues = new Set(
        Object.values(nextSubjectMeta.indicators).flatMap(indicator =>
          indicator.options.map(option => option.value),
        ),
      );
      const filteredIndicators = state.query.indicators.filter(indicator =>
        indicatorValues.has(indicator),
      );

      const filterValues = new Set(
        Object.values(nextSubjectMeta.filters).flatMap(filterGroup =>
          Object.values(filterGroup.options).flatMap(filter =>
            filter.options.map(option => option.value),
          ),
        ),
      );
      const filteredFilters = state.query.filters.filter(filter =>
        filterValues.has(filter),
      );

      updateState(draft => {
        draft.subjectMeta.indicators = nextSubjectMeta.indicators;
        draft.subjectMeta.filters = nextSubjectMeta.filters;
        draft.query.indicators = filteredIndicators;
        draft.query.filters = filteredFilters;
        draft.query.timePeriod = {
          startYear,
          startCode,
          endYear,
          endCode,
        };
      });

      onStepSubmit?.({ nextStepNumber: 5, nextStepTitle: stepTitles.filter });
    };

  const handleFiltersStepBack = async () => {
    const { releaseVersionId, subjectId, locationIds, timePeriod } =
      state.query;

    const nextSubjectMeta = await tableBuilderService.filterSubjectMeta(
      {
        subjectId,
        locationIds,
        timePeriod,
      },
      releaseVersionId,
    );

    updateState(draft => {
      draft.subjectMeta.indicators = nextSubjectMeta.indicators;
      draft.subjectMeta.filters = nextSubjectMeta.filters;
    });

    onStepSubmit?.({ nextStepNumber: 5, nextStepTitle: stepTitles.filter });
  };

  const handleFiltersFormSubmit: FilterFormSubmitHandler = async ({
    filters,
    indicators,
  }) => {
    updateState(draft => {
      draft.response = undefined;
    });

    const updatedReleaseTableDataQuery: ReleaseTableDataQuery = {
      ...state.query,
      filters: Object.values(filters).flat(),
      indicators,
    };

    const tableData = await tableBuilderService.getTableData(
      {
        subjectId: updatedReleaseTableDataQuery.subjectId,
        locationIds: updatedReleaseTableDataQuery.locationIds,
        timePeriod: updatedReleaseTableDataQuery.timePeriod,
        filters: updatedReleaseTableDataQuery.filters,
        indicators: updatedReleaseTableDataQuery.indicators,
      } as FullTableQuery,
      updatedReleaseTableDataQuery.releaseVersionId,
    );

    if (!tableData.results.length || !tableData.subjectMeta) {
      throw new SubmitError(
        'No data available for the options selected. Please try again with different options.',
      );
    }

    const table = mapFullTable(tableData);
    const tableHeaders = getDefaultTableHeaderConfig(table);

    if (onSubmit) {
      onSubmit(table);
    }

    updateState(draft => {
      draft.query = updatedReleaseTableDataQuery;
      draft.response = {
        table,
        tableHeaders,
      };
    });

    onStepSubmit?.({ nextStepNumber: 6, nextStepTitle: stepTitles.final });
  };

  const orderedTableHeaders: TableHeadersConfig | undefined = useMemo(() => {
    const reorderedOrSavedTableHeaders =
      reorderedTableHeaders ?? initialState.response?.tableHeaders;

    return state.response?.tableHeaders && reorderedOrSavedTableHeaders
      ? applyTableHeadersOrder({
          reorderedTableHeaders: reorderedOrSavedTableHeaders,
          defaultTableHeaders: state.response.tableHeaders,
        })
      : state.response?.tableHeaders;
  }, [
    initialState.response?.tableHeaders,
    reorderedTableHeaders,
    state.response?.tableHeaders,
  ]);

  const showChangeWarningForSteps = hidePublicationStep ? [1] : [1, 2];

  return (
    <ConfirmContextProvider>
      {({ askConfirm }) => (
        <>
          <Wizard
            scrollOnMount={scrollOnMount}
            initialStep={state.initialStep}
            id="tableToolWizard"
            currentStep={currentStep}
            onStepChange={async (nextStep, previousStep) => {
              onStepChange?.(nextStep, previousStep);
              if (
                nextStep < previousStep &&
                showChangeWarningForSteps.includes(nextStep)
              ) {
                const confirmed = await askConfirm();
                return confirmed ? nextStep : previousStep;
              }

              return nextStep;
            }}
          >
            {!hidePublicationStep && (
              <WizardStep size="l" onBack={handlePublicationStepBack}>
                {stepProps => (
                  <PublicationForm
                    {...stepProps}
                    initialValues={{
                      publicationId: state.query.publicationId ?? '',
                      themeId: '',
                    }}
                    stepTitle={stepTitles.publication}
                    themes={themeMeta}
                    renderSummaryAfter={
                      state.selectedPublication?.isSuperseded &&
                      state.selectedPublication.supersededBy ? (
                        <WarningMessage testId="superseded-warning">
                          This publication has been superseded by{' '}
                          <a
                            data-testid="superseded-by-link"
                            href={`/find-statistics/${state.selectedPublication.supersededBy.slug}`}
                          >
                            {state.selectedPublication.supersededBy.title}
                          </a>
                        </WarningMessage>
                      ) : null
                    }
                    onSubmit={handlePublicationFormSubmit}
                  />
                )}
              </WizardStep>
            )}
            <WizardStep size="l" onBack={handleDataSetStepBack}>
              {stepProps => (
                <DataSetStep
                  {...stepProps}
                  featuredTables={state.featuredTables}
                  loadingFastTrack={loadingFastTrack}
                  renderFeaturedTableLink={renderFeaturedTableLink}
                  releaseVersion={state.selectedPublication?.selectedRelease}
                  stepTitle={stepTitles.dataSet}
                  subjects={state.subjects}
                  subjectId={state.query.subjectId}
                  onSubmit={handleDataSetFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep size="l" onBack={handleLocationStepBack}>
              {stepProps => (
                <LocationStep
                  {...stepProps}
                  initialValues={state.query.locationIds}
                  options={state.subjectMeta.locations}
                  stepTitle={stepTitles.location}
                  onSubmit={handleLocationFiltersFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep size="l" onBack={handleTimePeriodStepBack}>
              {stepProps => (
                <TimePeriodForm
                  {...stepProps}
                  initialValues={state.query.timePeriod}
                  options={state.subjectMeta.timePeriod.options}
                  stepTitle={stepTitles.timePeriod}
                  onSubmit={handleTimePeriodFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep size="l" onBack={handleFiltersStepBack}>
              {stepProps => (
                <FiltersForm
                  {...stepProps}
                  initialValues={{
                    indicators: state.query.indicators,
                    filters: state.query.filters,
                  }}
                  selectedPublication={state.selectedPublication}
                  stepTitle={stepTitles.filter}
                  subject={
                    state.subjects.filter(
                      subject => subject.id === state.query.subjectId,
                    )[0]
                  }
                  subjectMeta={state.subjectMeta}
                  showTableQueryErrorDownload={showTableQueryErrorDownload}
                  onTableQueryError={onTableQueryError}
                  onSubmit={handleFiltersFormSubmit}
                />
              )}
            </WizardStep>
            {finalStep &&
              finalStep({
                query: state.query,
                selectedPublication: state.selectedPublication,
                stepTitle: stepTitles.final,
                table: state.response?.table,
                tableHeaders: orderedTableHeaders,
                onReorder: reordered => setReorderedTableHeaders(reordered),
              })}
          </Wizard>

          <PreviousStepModalConfirm />
        </>
      )}
    </ConfirmContextProvider>
  );
}

function getLocationsStepTitle(
  locations: Dictionary<{
    legend: string;
    options: LocationOption[];
  }>,
) {
  const levelKeys = Object.keys(locations) as LocationLevelKey[];
  return levelKeys.length === 1 && locationLevelsMap[levelKeys[0]]
    ? `Choose ${locationLevelsMap[levelKeys[0]].plural}`
    : defaultLocationStepTitle;
}
