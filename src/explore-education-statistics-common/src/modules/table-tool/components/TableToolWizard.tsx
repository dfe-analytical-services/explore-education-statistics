/* eslint-disable no-shadow */
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
import PublicationSubjectForm, {
  PublicationSubjectFormSubmitHandler,
} from '@common/modules/table-tool/components/PublicationSubjectForm';
import TimePeriodForm, {
  TimePeriodFormSubmitHandler,
} from '@common/modules/table-tool/components/TimePeriodForm';
import Wizard from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import tableBuilderService, {
  LocationLevelKeys,
  locationLevelKeys,
  PublicationSubject,
  PublicationSubjectMeta,
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/table-tool/services/tableBuilderService';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import parseYearCodeTuple from '@common/modules/table-tool/utils/parseYearCodeTuple';
import { TableHeadersConfig } from '@common/modules/table-tool/utils/tableHeaders';
import omitBy from 'lodash/omitBy';
import React, { ReactElement, useEffect, useState } from 'react';
import { useImmer } from 'use-immer';
import {
  executeTableQuery,
  getDefaultSubjectMeta,
} from './utils/tableToolHelpers';

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
  initialState?: TableToolState;
  finalStep?: (props: FinalStepProps) => ReactElement;
  onTableCreated?: (response: {
    query: TableDataQuery;
    table: FullTable;
    tableHeaders: TableHeadersConfig;
  }) => void;
}

const TableToolWizard = ({
  themeMeta,
  publicationId,
  releaseId,
  initialState,
  finalStep,
  onTableCreated,
}: TableToolWizardProps) => {
  const [publication, setPublication] = useState<Publication>();
  const [subjects, setSubjects] = useState<PublicationSubject[]>([]);

  const [tableToolState, updateTableToolState] = useImmer<TableToolState>(
    initialState ?? {
      initialStep: 1,
      subjectMeta: getDefaultSubjectMeta(),
      query: {
        subjectId: '',
        indicators: [],
        filters: [],
      },
    },
  );

  useEffect(() => {
    if (releaseId) {
      tableBuilderService
        .getReleaseMeta(releaseId)
        .then(({ subjects: releaseSubjects }) => {
          setSubjects(releaseSubjects);
        });
    }
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

    setSubjects(publicationSubjects);
    setPublication(selectedPublication);
  };

  const handlePublicationSubjectFormSubmit: PublicationSubjectFormSubmitHandler = async ({
    subjectId: selectedSubjectId,
  }) => {
    const nextSubjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      selectedSubjectId,
    );

    updateTableToolState(draft => {
      draft.subjectMeta = nextSubjectMeta;

      draft.query.subjectId = selectedSubjectId;
    });
  };

  const handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = async ({
    locations,
  }) => {
    const nextSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        ...locations,
        subjectId: tableToolState.query.subjectId,
      },
    );

    updateTableToolState(draft => {
      draft.subjectMeta.timePeriod = nextSubjectMeta.timePeriod;

      draft.query = {
        ...omitBy(draft.query, (values, level) =>
          locationLevelKeys.includes(level as LocationLevelKeys),
        ),
        ...locations,
      } as TableDataQuery;
    });
  };

  const handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler = async values => {
    const [startYear, startCode] = parseYearCodeTuple(values.start);
    const [endYear, endCode] = parseYearCodeTuple(values.end);

    const nextSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        ...tableToolState.query,
        subjectId: tableToolState.query.subjectId,
        timePeriod: {
          startYear,
          startCode,
          endYear,
          endCode,
        },
      },
    );

    updateTableToolState(draft => {
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

  const handleFiltersFormSubmit: FilterFormSubmitHandler = async ({
    filters,
    indicators,
  }) => {
    const query: TableDataQuery = {
      ...tableToolState.query,
      indicators,
      filters: Object.values(filters).flat(),
    };

    const response = await executeTableQuery(query);

    updateTableToolState(draft => {
      draft.query = query;
      draft.response = response;
    });

    if (onTableCreated) {
      onTableCreated({
        ...response,
        query,
      });
    }
  };

  return (
    <ConfirmContextProvider>
      {({ askConfirm }) => (
        <>
          <Wizard
            initialStep={tableToolState.initialStep}
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
                  initialValues={tableToolState.query}
                  onSubmit={handlePublicationSubjectFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <LocationFiltersForm
                  {...stepProps}
                  options={tableToolState.subjectMeta.locations}
                  initialValues={tableToolState.query}
                  onSubmit={handleLocationFiltersFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <TimePeriodForm
                  {...stepProps}
                  initialValues={tableToolState.query}
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
                  initialValues={tableToolState.query}
                  subjectMeta={tableToolState.subjectMeta}
                />
              )}
            </WizardStep>
            {finalStep &&
              finalStep({
                ...tableToolState.response,
                query: tableToolState.query,
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
