import { ConfirmContextProvider } from '@common/contexts/ConfirmContext';
import FiltersForm, {
  FilterFormSubmitHandler,
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
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import parseYearCodeTuple from '@common/modules/table-tool/utils/parseYearCodeTuple';
import tableBuilderService, {
  ReleaseTableDataQuery,
  Subject,
  SubjectMeta,
  TableHighlight,
  Theme,
} from '@common/services/tableBuilderService';
import React, { ReactElement, ReactNode, useMemo } from 'react';
import { useImmer } from 'use-immer';

interface Publication {
  id: string;
  title: string;
  slug: string;
}

export interface InitialTableToolState {
  initialStep: number;
  subjects?: Subject[];
  highlights?: TableHighlight[];
  subjectMeta?: SubjectMeta;
  query?: ReleaseTableDataQuery;
  response?: {
    table: FullTable;
    tableHeaders: TableHeadersConfig;
  };
}

interface TableToolState extends InitialTableToolState {
  subjects: Subject[];
  highlights: TableHighlight[];
  subjectMeta: SubjectMeta;
  query: ReleaseTableDataQuery;
}

export interface FinalStepRenderProps {
  publication?: Publication;
  query?: ReleaseTableDataQuery;
  response?: {
    table: FullTable;
    tableHeaders: TableHeadersConfig;
  };
}

export interface TableToolWizardProps {
  themeMeta?: Theme[];
  initialState?: Partial<InitialTableToolState>;
  finalStep?: (props: FinalStepRenderProps) => ReactElement;
  renderHighlightLink?: (highlight: TableHighlight) => ReactNode;
  scrollOnMount?: boolean;
  onSubmit?: (table: FullTable) => void;
  loadingFastTrack: boolean;
}

const TableToolWizard = ({
  themeMeta = [],
  initialState = {},
  scrollOnMount,
  renderHighlightLink,
  finalStep,
  onSubmit,
  loadingFastTrack = false,
}: TableToolWizardProps) => {
  const [state, updateState] = useImmer<TableToolState>({
    initialStep: 1,
    subjects: [],
    highlights: [],
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
      locations: {},
    },
    ...initialState,
  });

  const publication = useMemo<Publication | undefined>(() => {
    return themeMeta
      .flatMap(option => option.topics)
      .flatMap(option => option.publications)
      .find(option => option.id === state.query.publicationId);
  }, [state.query.publicationId, themeMeta]);

  const handlePublicationFormSubmit: PublicationFormSubmitHandler = async ({
    publicationId: selectedPublicationId,
  }) => {
    const publicationMeta = await tableBuilderService.getPublication(
      selectedPublicationId,
    );

    updateState(draft => {
      draft.subjects = publicationMeta.subjects;
      draft.highlights = publicationMeta.highlights;

      draft.query.publicationId = selectedPublicationId;
    });
  };

  const handleSubjectFormSubmit: SubjectFormSubmitHandler = async ({
    subjectId: selectedSubjectId,
  }) => {
    const nextSubjectMeta = await tableBuilderService.getSubjectMeta(
      selectedSubjectId,
    );

    updateState(draft => {
      draft.subjectMeta = nextSubjectMeta;

      draft.query.subjectId = selectedSubjectId;
    });
  };

  const handleLocationStepBack = async () => {
    const { subjectId } = state.query;

    const nextSubjectMeta = await tableBuilderService.getSubjectMeta(subjectId);

    updateState(draft => {
      draft.subjectMeta = nextSubjectMeta;
    });
  };

  const handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = async ({
    locations,
  }) => {
    const nextSubjectMeta = await tableBuilderService.filterSubjectMeta({
      locations,
      subjectId: state.query.subjectId,
    });

    updateState(draft => {
      draft.subjectMeta.timePeriod = nextSubjectMeta.timePeriod;

      draft.query.locations = locations;
    });
  };

  const handleTimePeriodStepBack = async () => {
    const { subjectId, locations } = state.query;

    const nextSubjectMeta = await tableBuilderService.filterSubjectMeta({
      subjectId,
      locations,
    });

    updateState(draft => {
      draft.subjectMeta.timePeriod = nextSubjectMeta.timePeriod;
    });
  };

  const handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler = async values => {
    const [startYear, startCode] = parseYearCodeTuple(values.start);
    const [endYear, endCode] = parseYearCodeTuple(values.end);

    const nextSubjectMeta = await tableBuilderService.filterSubjectMeta({
      ...state.query,
      subjectId: state.query.subjectId,
      timePeriod: {
        startYear,
        startCode,
        endYear,
        endCode,
      },
    });

    updateState(draft => {
      draft.subjectMeta.indicators = nextSubjectMeta.indicators;
      draft.subjectMeta.filters = nextSubjectMeta.filters;

      draft.query.timePeriod = {
        startYear,
        startCode,
        endYear,
        endCode,
      };
    });
  };

  const handleFiltersStepBack = async () => {
    const { subjectId, locations, timePeriod } = state.query;

    const nextSubjectMeta = await tableBuilderService.filterSubjectMeta({
      subjectId,
      locations,
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
    const tableHeaders = getDefaultTableHeaderConfig(table.subjectMeta);

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
            {!state.query.releaseId && (
              <WizardStep>
                {stepProps => (
                  <PublicationForm
                    {...stepProps}
                    initialValues={{
                      publicationId: state.query.publicationId ?? '',
                    }}
                    options={themeMeta}
                    onSubmit={handlePublicationFormSubmit}
                  />
                )}
              </WizardStep>
            )}
            <WizardStep>
              {stepProps => (
                <SubjectStep
                  {...stepProps}
                  highlights={state.highlights}
                  subjects={state.subjects}
                  subjectId={state.query.subjectId}
                  renderHighlightLink={renderHighlightLink}
                  onSubmit={handleSubjectFormSubmit}
                  loadingFastTrack={loadingFastTrack}
                />
              )}
            </WizardStep>
            <WizardStep onBack={handleLocationStepBack}>
              {stepProps => (
                <LocationFiltersForm
                  {...stepProps}
                  initialValues={state.query.locations}
                  options={state.subjectMeta.locations}
                  onSubmit={handleLocationFiltersFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep onBack={handleTimePeriodStepBack}>
              {stepProps => (
                <TimePeriodForm
                  {...stepProps}
                  initialValues={{
                    timePeriod: state.query.timePeriod,
                  }}
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
                  subjectMeta={state.subjectMeta}
                  onSubmit={handleFiltersFormSubmit}
                />
              )}
            </WizardStep>
            {finalStep &&
              finalStep({
                query: state.query,
                response: state.response,
                publication,
              })}
          </Wizard>

          <PreviousStepModalConfirm />
        </>
      )}
    </ConfirmContextProvider>
  );
};

export default TableToolWizard;
