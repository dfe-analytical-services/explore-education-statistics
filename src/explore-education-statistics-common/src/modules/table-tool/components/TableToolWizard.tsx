import { ConfirmContextProvider } from '@common/contexts/ConfirmContext';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
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
import PublicationSubjectForm, {
  PublicationSubjectFormSubmitHandler,
} from '@common/modules/table-tool/components/PublicationSubjectForm';
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
  PublicationSubject,
  PublicationSubjectMeta,
  TableDataQuery,
  ThemeMeta,
} from '@common/services/tableBuilderService';
import React, { ReactElement, useState } from 'react';
import { useImmer } from 'use-immer';

const getDefaultSubjectMeta = (): PublicationSubjectMeta => ({
  timePeriod: {
    hint: '',
    legend: '',
    options: [],
  },
  locations: {},
  indicators: {},
  filters: {},
});

interface Publication {
  id: string;
  title: string;
  slug: string;
}

export interface TableToolState {
  initialStep: number;
  subjectMeta: PublicationSubjectMeta;
  query: TableDataQuery;
  response?: {
    table: FullTable;
    tableHeaders: TableHeadersConfig;
  };
}

export interface FinalStepRenderProps {
  publication?: Publication;
  query?: TableDataQuery;
  response?: {
    table: FullTable;
    tableHeaders: TableHeadersConfig;
  };
}

export interface TableToolWizardProps {
  themeMeta: ThemeMeta[];
  publicationId?: string;
  releaseId?: string;
  initialState?: TableToolState;
  finalStep?: (props: FinalStepRenderProps) => ReactElement;
  scrollOnMount?: boolean;
}

const TableToolWizard = ({
  themeMeta,
  publicationId,
  releaseId,
  initialState,
  finalStep,
}: TableToolWizardProps) => {
  const [publication, setPublication] = useState<Publication>();

  const [state, updateState] = useImmer<TableToolState>(
    initialState ?? {
      initialStep: 1,
      subjectMeta: getDefaultSubjectMeta(),
      query: {
        subjectId: '',
        indicators: [],
        filters: [],
        locations: {},
      },
    },
  );

  const { value: subjects = [], setState: setSubjects } = useAsyncRetry<
    PublicationSubject[]
  >(async () => {
    if (releaseId) {
      const meta = await tableBuilderService.getReleaseMeta(releaseId);
      return meta.subjects;
    }

    return [];
  }, [releaseId]);

  const handlePublicationFormSubmit: PublicationFormSubmitHandler = async ({
    publicationId: selectedPublicationId,
  }) => {
    const selectedPublication = themeMeta
      .flatMap(option => option.topics)
      .flatMap(option => option.publications)
      .find(option => option.id === selectedPublicationId);

    if (!selectedPublication) {
      return;
    }

    const {
      subjects: publicationSubjects,
    } = await tableBuilderService.getPublicationMeta(selectedPublicationId);

    setSubjects({
      isLoading: false,
      value: publicationSubjects,
    });
    setPublication(selectedPublication);
  };

  const handlePublicationSubjectFormSubmit: PublicationSubjectFormSubmitHandler = async ({
    subjectId: selectedSubjectId,
  }) => {
    const nextSubjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      selectedSubjectId,
    );

    updateState(draft => {
      draft.subjectMeta = nextSubjectMeta;

      draft.query.subjectId = selectedSubjectId;
    });
  };

  const handleLocationStepBack = async () => {
    const { subjectId } = state.query;

    const nextSubjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      subjectId,
    );

    updateState(draft => {
      draft.subjectMeta = nextSubjectMeta;
    });
  };

  const handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = async ({
    locations,
  }) => {
    const nextSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        locations,
        subjectId: state.query.subjectId,
      },
    );

    updateState(draft => {
      draft.subjectMeta.timePeriod = nextSubjectMeta.timePeriod;

      draft.query.locations = locations;
    });
  };

  const handleTimePeriodStepBack = async () => {
    const { subjectId, locations } = state.query;

    const nextSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        subjectId,
        locations,
      },
    );

    updateState(draft => {
      draft.subjectMeta.timePeriod = nextSubjectMeta.timePeriod;
    });
  };

  const handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler = async values => {
    const [startYear, startCode] = parseYearCodeTuple(values.start);
    const [endYear, endCode] = parseYearCodeTuple(values.end);

    const nextSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        ...state.query,
        subjectId: state.query.subjectId,
        timePeriod: {
          startYear,
          startCode,
          endYear,
          endCode,
        },
      },
    );

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

    const nextSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        subjectId,
        locations,
        timePeriod,
      },
    );

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

    const query: TableDataQuery = {
      ...state.query,
      indicators,
      filters: Object.values(filters).flat(),
    };

    const tableData = await tableBuilderService.getTableData(query, releaseId);

    if (!tableData.results.length || !tableData.subjectMeta) {
      throw new Error(
        'No data available for the options selected. Please try again with different options.',
      );
    }

    const table = mapFullTable(tableData);
    const tableHeaders = getDefaultTableHeaderConfig(table.subjectMeta);

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
            initialStep={state.initialStep}
            id="tableTool-steps"
            onStepChange={async (nextStep, previousStep) => {
              if (nextStep < previousStep) {
                const confirmed = await askConfirm();
                return confirmed ? nextStep : previousStep;
              }

              return nextStep;
            }}
          >
            {releaseId === undefined && (
              <WizardStep>
                {stepProps => (
                  <PublicationForm
                    {...stepProps}
                    publicationId={publicationId}
                    publicationTitle={publication ? publication.title : ''}
                    options={themeMeta}
                    onSubmit={handlePublicationFormSubmit}
                  />
                )}
              </WizardStep>
            )}
            <WizardStep>
              {stepProps => (
                <PublicationSubjectForm
                  {...stepProps}
                  options={subjects}
                  initialValues={state.query}
                  onSubmit={handlePublicationSubjectFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep onBack={handleLocationStepBack}>
              {stepProps => (
                <LocationFiltersForm
                  {...stepProps}
                  options={state.subjectMeta.locations}
                  initialValues={state.query.locations}
                  onSubmit={handleLocationFiltersFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep onBack={handleTimePeriodStepBack}>
              {stepProps => (
                <TimePeriodForm
                  {...stepProps}
                  initialValues={state.query}
                  options={state.subjectMeta.timePeriod.options}
                  onSubmit={handleTimePeriodFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep onBack={handleFiltersStepBack}>
              {stepProps => (
                <FiltersForm
                  {...stepProps}
                  onSubmit={handleFiltersFormSubmit}
                  initialValues={state.query}
                  subjectMeta={state.subjectMeta}
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
