/* eslint-disable no-shadow */
import { ConfirmContextProvider } from '@common/context/ConfirmContext';
import tableBuilderService, {
  LocationLevelKeys,
  PublicationSubject,
  PublicationSubjectMeta,
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import { FullTable } from '@common/modules/full-table/types/fullTable';
import { TableHeadersConfig } from '@common/modules/full-table/utils/tableHeaders';
import parseYearCodeTuple from '@common/modules/full-table/utils/TimePeriod';
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
import { PartialRecord } from '@common/types/util';
import React, { ReactElement, useEffect, useState } from 'react';
import { useImmer } from 'use-immer';
import {
  DateRangeState,
  getDefaultSubjectMeta,
  initialiseFromInitialQuery,
  tableGeneration,
} from './utils/tableToolHelpers';

interface Publication {
  id: string;
  title: string;
  slug: string;
}

export interface TableToolState {
  response?: {
    query: TableDataQuery;
    table: FullTable;
    tableHeaders: TableHeadersConfig;
  };
  validInitialQuery?: TableDataQuery;
  dateRange: DateRangeState;
  locations: PartialRecord<LocationLevelKeys, string[]>;
  subjectId: string;
  subjectMeta: PublicationSubjectMeta;
}

export interface FinalStepProps {
  publication?: Publication;
  table?: FullTable;
  query?: TableDataQuery;
  tableHeaders?: TableHeadersConfig;
}

export interface TableToolWizardProps {
  themeMeta: ThemeMeta[];
  publicationId?: string;
  releaseId?: string;
  initialQuery?: TableDataQuery;
  finalStep?: (props: FinalStepProps) => ReactElement;
  onTableCreated?: (response: {
    table: FullTable;
    query: TableDataQuery;
    tableHeaders: TableHeadersConfig;
  }) => void;
  onInitialQueryLoaded?: () => void;
}

const TableToolWizard = ({
  themeMeta,
  publicationId,
  releaseId,
  initialQuery,
  finalStep,
  onTableCreated,
  onInitialQueryLoaded,
}: TableToolWizardProps) => {
  const [publication, setPublication] = useState<Publication>();
  const [subjects, setSubjects] = useState<PublicationSubject[]>([]);

  const [initialStep, setInitialStep] = useState(1);

  const [tableToolState, updateTableToolState] = useImmer<TableToolState>({
    subjectId: '',
    subjectMeta: getDefaultSubjectMeta(),
    locations: {},
    dateRange: {},
  });

  useEffect(() => {
    if (releaseId) {
      tableBuilderService
        .getReleaseMeta(releaseId)
        .then(({ subjects: releaseSubjects }) => {
          setSubjects(releaseSubjects);
        });
    }
  }, [releaseId]);

  useEffect(() => {
    const currentlyLoadingQuery = {
      releaseId,
      initialQuery,
    };
    setInitialStep(1);

    updateTableToolState(() => ({
      subjectMeta: getDefaultSubjectMeta(),
      dateRange: {},
      locations: {},
      subjectId: '',
    }));

    initialiseFromInitialQuery(releaseId, initialQuery).then(state => {
      // make sure nothing changed in the component while we were processing the initialisation
      if (
        currentlyLoadingQuery.releaseId === releaseId &&
        currentlyLoadingQuery.initialQuery === initialQuery
      ) {
        const { initialStep } = state;

        setInitialStep(initialStep);

        updateTableToolState(() => state);

        if (onInitialQueryLoaded) onInitialQueryLoaded();
      }
    });
  }, [initialQuery, onInitialQueryLoaded, releaseId, updateTableToolState]);

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

    setSubjects(publicationSubjects);
    setPublication(selectedPublication);
  };

  const handlePublicationSubjectFormSubmit: PublicationSubjectFormSubmitHandler = async ({
    subjectId: selectedSubjectId,
  }) => {
    const subjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      selectedSubjectId,
    );

    updateTableToolState(draft => {
      draft.subjectId = selectedSubjectId;
      draft.subjectMeta = subjectMeta;
    });
  };

  const handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = async ({
    locations,
  }) => {
    const selectedSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        ...locations,
        subjectId: tableToolState.subjectId,
      },
    );

    updateTableToolState(draft => {
      draft.subjectMeta.timePeriod = selectedSubjectMeta.timePeriod;
      draft.locations = locations;
    });
  };

  const handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler = async values => {
    const [startYear, startCode] = parseYearCodeTuple(values.start);
    const [endYear, endCode] = parseYearCodeTuple(values.end);

    const selectedSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        ...tableToolState.locations,
        subjectId: tableToolState.subjectId,
        timePeriod: {
          startYear,
          startCode,
          endYear,
          endCode,
        },
      },
    );

    updateTableToolState(draft => {
      draft.subjectMeta.filters = selectedSubjectMeta.filters;

      draft.dateRange = {
        startYear,
        startCode,
        endYear,
        endCode,
      };
    });
  };

  const handleFiltersFormSubmit: FilterFormSubmitHandler = async values => {
    const tableQueryResponse = await tableGeneration(
      tableToolState.dateRange,
      tableToolState.subjectMeta,
      values,
      tableToolState.subjectId,
      tableToolState.locations,
      releaseId,
    );

    if (tableQueryResponse) {
      const newState = {
        ...tableToolState,
        response: tableQueryResponse,
      };

      updateTableToolState(() => newState);
      if (onTableCreated) {
        onTableCreated(tableQueryResponse);
      }
    }
  };

  return (
    <ConfirmContextProvider>
      {({ askConfirm }) => (
        <>
          <Wizard
            initialStep={initialStep}
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
                  initialValues={tableToolState.validInitialQuery}
                  onSubmit={handlePublicationSubjectFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <LocationFiltersForm
                  {...stepProps}
                  options={tableToolState.subjectMeta.locations}
                  initialValues={tableToolState.validInitialQuery}
                  onSubmit={handleLocationFiltersFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <TimePeriodForm
                  {...stepProps}
                  initialValues={tableToolState.validInitialQuery}
                  options={tableToolState.subjectMeta.timePeriod.options}
                  onSubmit={handleTimePeriodFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <FiltersForm
                  {...stepProps}
                  onSubmit={handleFiltersFormSubmit}
                  initialValues={tableToolState.validInitialQuery}
                  subjectMeta={tableToolState.subjectMeta}
                />
              )}
            </WizardStep>
            {finalStep &&
              finalStep({
                ...tableToolState.response,
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
