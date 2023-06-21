import { ConfirmContextProvider } from '@common/contexts/ConfirmContext';
import FiltersForm, {
  FilterFormSubmitHandler,
  TableQueryErrorCode,
} from '@common/modules/table-tool/components/FiltersForm';
import LocationFiltersForm, {
  LocationFiltersFormSubmitHandler,
} from '@common/modules/table-tool/components/LocationFiltersForm';
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
  ReleaseTableDataQuery,
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import React, { ReactElement, ReactNode, useMemo, useState } from 'react';
import { useImmer } from 'use-immer';

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
  table?: FullTable;
  tableHeaders?: TableHeadersConfig;
  onReorder: (reorderedTableHeaders: TableHeadersConfig) => void;
}

export interface TableToolWizardProps {
  finalStep?: (props: FinalStepRenderProps) => ReactElement;
  hidePublicationStep?: boolean;
  initialState?: Partial<InitialTableToolState>;
  loadingFastTrack?: boolean;
  renderFeaturedTableLink?: (featuredTable: FeaturedTable) => ReactNode;
  scrollOnMount?: boolean;
  showTableQueryErrorDownload?: boolean;
  themeMeta?: Theme[];
  currentStep?: number;
  onPublicationFormSubmit?: (publication: PublicationTreeSummary) => void;
  onPublicationStepBack?: () => void;
  onStepChange?: (nextStep: number, previousStep: number) => void;
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
  finalStep,
  hidePublicationStep,
  initialState = {},
  loadingFastTrack = false,
  renderFeaturedTableLink,
  scrollOnMount,
  showTableQueryErrorDownload = true,
  themeMeta = [],
  currentStep,
  onPublicationFormSubmit,
  onPublicationStepBack,
  onStepChange,
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
  const [reorderedTableHeaders, setReorderedTableHeaders] = useState<
    TableHeadersConfig
  >();

  const handlePublicationFormSubmit: PublicationFormSubmitHandler = async ({
    publication,
  }) => {
    onPublicationFormSubmit?.(publication);

    const [subjects, featuredTables] = await Promise.all([
      tableBuilderService.listLatestReleaseSubjects(publication.id),
      tableBuilderService.listLatestReleaseFeaturedTables(publication.id),
    ]);

    const latestRelease = await publicationService.getLatestPublicationReleaseSummary(
      publication.slug,
    );

    updateState(draft => {
      draft.subjects = subjects;
      draft.featuredTables = featuredTables;
      draft.query.releaseId = undefined;
      draft.query.publicationId = publication.id;
      draft.selectedPublication = {
        id: publication.id,
        slug: publication.slug,
        title: publication.title,
        selectedRelease: {
          id: latestRelease.id,
          latestData: latestRelease.latestRelease,
          slug: latestRelease.slug,
          title: latestRelease.title,
        },
        latestRelease: {
          title: latestRelease.title,
        },
      };
    });
  };

  const handleSubjectStepBack = () => {
    updateState(draft => {
      draft.query.subjectId = '';
    });

    onSubjectStepBack?.(state.selectedPublication);
  };

  const handleSubjectFormSubmit: DataSetFormSubmitHandler = async ({
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
      state.query.releaseId,
    );

    setReorderedTableHeaders(undefined);

    updateState(draft => {
      draft.subjectMeta = nextSubjectMeta;
      draft.query.subjectId = selectedSubjectId;
      draft.query.indicators = [];
      draft.query.filters = [];
      draft.query.locationIds = [];
      draft.query.timePeriod = undefined;
    });
  };

  const handleLocationStepBack = async () => {
    const { releaseId, subjectId } = state.query;

    const nextSubjectMeta = await tableBuilderService.getSubjectMeta(
      subjectId,
      releaseId,
    );

    updateState(draft => {
      draft.subjectMeta = nextSubjectMeta;
    });
  };

  const handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = async ({
    locationIds,
  }) => {
    const nextSubjectMeta = await tableBuilderService.filterSubjectMeta({
      releaseId: state.query.releaseId,
      locationIds,
      subjectId: state.query.subjectId,
    });

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
  };

  const handleTimePeriodStepBack = async () => {
    const { releaseId, subjectId, locationIds } = state.query;

    const nextSubjectMeta = await tableBuilderService.filterSubjectMeta({
      releaseId,
      subjectId,
      locationIds,
    });

    updateState(draft => {
      draft.subjectMeta.timePeriod = nextSubjectMeta.timePeriod;
    });
  };

  const handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler = async values => {
    const { releaseId, subjectId, locationIds } = state.query;
    const [startYear, startCode] = parseYearCodeTuple(values.start);
    const [endYear, endCode] = parseYearCodeTuple(values.end);

    const nextSubjectMeta = await tableBuilderService.filterSubjectMeta({
      releaseId,
      subjectId,
      locationIds,
      timePeriod: {
        startYear,
        startCode,
        endYear,
        endCode,
      },
    });

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
  };

  const handleFiltersStepBack = async () => {
    const { releaseId, subjectId, locationIds, timePeriod } = state.query;

    const nextSubjectMeta = await tableBuilderService.filterSubjectMeta({
      releaseId,
      subjectId,
      locationIds,
      timePeriod,
    });

    updateState(draft => {
      draft.subjectMeta.indicators = nextSubjectMeta.indicators;
      draft.subjectMeta.filters = nextSubjectMeta.filters;
    });
  };

  const handleFiltersFormSubmit: FilterFormSubmitHandler = async ({
    filters,
    indicators,
  }) => {
    updateState(draft => {
      draft.response = undefined;
    });

    const query: ReleaseTableDataQuery = {
      ...state.query,
      indicators,
      filters: Object.values(filters).flat(),
    };

    const tableData = await tableBuilderService.getTableData(query);

    if (!tableData.results.length || !tableData.subjectMeta) {
      throw new Error(
        'No data available for the options selected. Please try again with different options.',
      );
    }

    const table = mapFullTable(tableData);
    const tableHeaders = getDefaultTableHeaderConfig(table);

    if (onSubmit) {
      onSubmit(table);
    }

    updateState(draft => {
      draft.query = query;
      draft.response = {
        table,
        tableHeaders,
      };
    });
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
              <WizardStep size="l" onBack={onPublicationStepBack}>
                {stepProps => (
                  <PublicationForm
                    {...stepProps}
                    initialValues={{
                      publicationId: state.query.publicationId ?? '',
                    }}
                    themes={themeMeta}
                    onSubmit={handlePublicationFormSubmit}
                  />
                )}
              </WizardStep>
            )}
            <WizardStep size="l" onBack={handleSubjectStepBack}>
              {stepProps => (
                <DataSetStep
                  {...stepProps}
                  featuredTables={state.featuredTables}
                  loadingFastTrack={loadingFastTrack}
                  renderFeaturedTableLink={renderFeaturedTableLink}
                  release={state.selectedPublication?.selectedRelease}
                  subjects={state.subjects}
                  subjectId={state.query.subjectId}
                  onSubmit={handleSubjectFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep size="l" onBack={handleLocationStepBack}>
              {stepProps => (
                <LocationFiltersForm
                  {...stepProps}
                  initialValues={state.query.locationIds}
                  options={state.subjectMeta.locations}
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
