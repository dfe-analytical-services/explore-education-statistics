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
import { SubjectFormSubmitHandler } from '@common/modules/table-tool/components/SubjectForm';
import SubjectStep from '@common/modules/table-tool/components/SubjectStep';
import TimePeriodForm, {
  TimePeriodFormSubmitHandler,
} from '@common/modules/table-tool/components/TimePeriodForm';
import Wizard from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import applyTableHeadersOrder from '@common/modules/table-tool/utils/applyTableHeadersOrder';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import parseYearCodeTuple from '@common/modules/table-tool/utils/parseYearCodeTuple';
import publicationService, { Theme } from '@common/services/publicationService';
import tableBuilderService, {
  FeaturedTable,
  ReleaseTableDataQuery,
  SelectedPublication,
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import React, { ReactElement, ReactNode, useMemo, useState } from 'react';
import { useImmer } from 'use-immer';
import { useRouter } from 'next/router';

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
  themeMeta?: Theme[];
  initialState?: Partial<InitialTableToolState>;
  hidePublicationSelectionStage?: boolean;
  finalStep?: (props: FinalStepRenderProps) => ReactElement;
  loadingFastTrack?: boolean;
  renderFeaturedTable?: (featuredTable: FeaturedTable) => ReactNode;
  scrollOnMount?: boolean;
  onTableQueryError?: (
    errorCode: TableQueryErrorCode,
    publicationTitle: string,
    subjectName: string,
  ) => void;
  showTableQueryErrorDownload?: boolean;
  onSubmit?: (table: FullTable) => void;
  onSubjectStepBack?: () => void;
}

const TableToolWizard = ({
  themeMeta = [],
  initialState = {},
  scrollOnMount,
  hidePublicationSelectionStage,
  renderFeaturedTable,
  finalStep,
  showTableQueryErrorDownload = true,
  onSubmit,
  onSubjectStepBack,
  onTableQueryError,
  loadingFastTrack = false,
}: TableToolWizardProps) => {
  const router = useRouter();
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

  const handlePublicationStepBack = () => {
    router.push('/data-tables', undefined, { shallow: true });
  };

  const handlePublicationFormSubmit: PublicationFormSubmitHandler = async ({
    publication,
  }) => {
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
    if (onSubjectStepBack) {
      onSubjectStepBack();
    }
  };

  const handleSubjectFormSubmit: SubjectFormSubmitHandler = async ({
    subjectId: selectedSubjectId,
  }) => {
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

  return (
    <ConfirmContextProvider>
      {({ askConfirm }) => (
        <>
          <Wizard
            scrollOnMount={scrollOnMount}
            initialStep={state.initialStep}
            id="tableToolWizard"
            onStepChange={async (nextStep, previousStep) => {
              if (nextStep < previousStep) {
                const confirmed = await askConfirm();
                return confirmed ? nextStep : previousStep;
              }

              return nextStep;
            }}
          >
            {!hidePublicationSelectionStage && (
              <WizardStep onBack={handlePublicationStepBack}>
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
            <WizardStep onBack={handleSubjectStepBack}>
              {stepProps => (
                <SubjectStep
                  {...stepProps}
                  featuredTables={state.featuredTables}
                  subjects={state.subjects}
                  subjectId={state.query.subjectId}
                  renderFeaturedTable={renderFeaturedTable}
                  onSubmit={handleSubjectFormSubmit}
                  loadingFastTrack={loadingFastTrack}
                />
              )}
            </WizardStep>
            <WizardStep onBack={handleLocationStepBack}>
              {stepProps => (
                <LocationFiltersForm
                  {...stepProps}
                  initialValues={state.query.locationIds}
                  options={state.subjectMeta.locations}
                  onSubmit={handleLocationFiltersFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep onBack={handleTimePeriodStepBack}>
              {stepProps => (
                <TimePeriodForm
                  {...stepProps}
                  initialValues={state.query.timePeriod}
                  options={state.subjectMeta.timePeriod.options}
                  onSubmit={handleTimePeriodFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep onBack={handleFiltersStepBack}>
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
};

export default TableToolWizard;
