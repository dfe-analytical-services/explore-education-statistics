/* eslint-disable no-shadow */
import { ConfirmContextProvider } from '@common/context/ConfirmContext';
import tableBuilderService, {
  PublicationSubject,
  PublicationSubjectMeta,
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import { LocationFilter } from '@common/modules/full-table/types/filters';
import { FullTable } from '@common/modules/full-table/types/fullTable';
import parseYearCodeTuple from '@common/modules/full-table/utils/TimePeriod';
import FiltersForm, { FilterFormSubmitHandler } from '@common/modules/table-tool/components/FiltersForm';
import LocationFiltersForm, { LocationFiltersFormSubmitHandler } from '@common/modules/table-tool/components/LocationFiltersForm';
import PreviousStepModalConfirm from '@common/modules/table-tool/components/PreviousStepModalConfirm';
import PublicationForm, { PublicationFormSubmitHandler } from '@common/modules/table-tool/components/PublicationForm';
import PublicationSubjectForm, { PublicationSubjectFormSubmitHandler } from '@common/modules/table-tool/components/PublicationSubjectForm';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodForm, { TimePeriodFormSubmitHandler } from '@common/modules/table-tool/components/TimePeriodForm';
import Wizard, { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import { Dictionary } from '@common/types/util';
import mapValues from 'lodash/mapValues';
import React, { ReactNode } from 'react';

import {
  DateRangeState,
  getDefaultSubjectMeta,
  initialiseFromInitialQuery,
  mapLocations,
  tableGeneration,
} from './utils/tableToolHelpers';

interface Publication {
  id: string;
  title: string;
  slug: string;
}

interface TableToolState {
  query: TableDataQuery | undefined;
  createdTable: FullTable | undefined;
  tableHeaders: TableHeadersFormValues | undefined;
  validInitialQuery: TableDataQuery | undefined;
  dateRange: DateRangeState;
  locations: Dictionary<LocationFilter[]>;
  subjectId: string;
  subjectMeta: PublicationSubjectMeta;
}

type PartialState = {
  [P in keyof TableToolState]?: TableToolState[P];
};

interface FinalStepProps {
  publication?: Publication;
  createdTable?: FullTable;
  query?: TableDataQuery;
  tableHeaders?: TableHeadersFormValues;
  updateState: (P: PartialState) => void;
}

interface Props {
  themeMeta: ThemeMeta[];
  publicationId?: string;
  releaseId?: string;

  finalStep?: (props: InjectedWizardProps & FinalStepProps) => ReactNode
  finalStepExtra?: (props: FinalStepProps) => ReactNode;
  finalStepHeading?: string;

  initialQuery?: TableDataQuery;
  initialTableHeaders?: TableHeadersFormValues;
  onInitialQueryCompleted?: () => void;
}


const TableToolWizard = (props: Props) => {

  const {
    themeMeta,
      publicationId,
      releaseId,
      finalStep,
      initialQuery,
      initialTableHeaders,
      onInitialQueryCompleted,
  } = props;

  const [publication, setPublication] = React.useState<Publication>();
  const [subjects, setSubjects] = React.useState<PublicationSubject[]>([]);

  const [initialStep, setInitialStep] = React.useState(1);


  const getInitialState = () => {
    return {
      initialStep: 1,
      subjectId: '',
      subjectMeta: getDefaultSubjectMeta(),
      locations: {},
      dateRange: {},
      tableHeaders: {
        columnGroups: [],
        columns: [],
        rowGroups: [],
        rows: [],
      },
      createdTable: undefined,
      query: undefined,
      validInitialQuery: undefined,
    };
  };

  const [tableToolState, setTableToolState] = React.useState<TableToolState>(() => {
    return getInitialState();
  });

  const updateState = (P: PartialState) => setTableToolState({ ...tableToolState, ...P });

  React.useEffect(() => {
    if (releaseId) {
      tableBuilderService
        .getReleaseMeta(releaseId)
        .then(({ subjects: releaseSubjects }) => {
          setSubjects(releaseSubjects);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [releaseId]);


  React.useEffect(() => {

    const currentlyLoadingQuery = { releaseId, initialQuery, initialTableHeaders };
    setInitialStep(1);
    setTableToolState({
      ...tableToolState,
      subjectMeta: getDefaultSubjectMeta(),
      validInitialQuery: undefined,
    });


    initialiseFromInitialQuery(releaseId, initialQuery, initialTableHeaders)
      .then((state) => {

        // make sure nothing changed in the component while we were processing the initialisation
        if (currentlyLoadingQuery.releaseId === releaseId
          && currentlyLoadingQuery.initialQuery === initialQuery
          && currentlyLoadingQuery.initialTableHeaders === initialTableHeaders
        ) {
          const { initialStep } = state;

          setInitialStep(initialStep);

          setTableToolState(
            state,
          );
          if (onInitialQueryCompleted) onInitialQueryCompleted();

        }
      });

  }, [initialQuery, initialTableHeaders, releaseId]);

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
    const selectedSubjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      selectedSubjectId,
    );

    setTableToolState({
      ...tableToolState,
      subjectId: selectedSubjectId,
      subjectMeta: selectedSubjectMeta,
    });
  };


  const handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = async ({
    locations: selectedLocations,
  }) => {
    const selectedSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        ...selectedLocations,
        subjectId: tableToolState.subjectId,
      },
    );

    setTableToolState({
      ...tableToolState,
      subjectMeta: {
        ...tableToolState.subjectMeta,
        timePeriod: selectedSubjectMeta.timePeriod,
      },
      locations: mapLocations(selectedLocations, tableToolState.subjectMeta.locations),
    });

  };

  const handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler = async values => {
    const [startYear, startCode] = parseYearCodeTuple(values.start);
    const [endYear, endCode] = parseYearCodeTuple(values.end);

    const selectedSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        ...mapValues(tableToolState.locations, locationLevel =>
          locationLevel.map(location => location.value),
        ),
        subjectId: tableToolState.subjectId,
        timePeriod: {
          startYear,
          startCode,
          endYear,
          endCode,
        },
      },
    );

    setTableToolState({
      ...tableToolState,
      subjectMeta: {
        ...tableToolState.subjectMeta,
        filters: selectedSubjectMeta.filters,
      },
      dateRange: {
        startYear,
        startCode,
        endYear,
        endCode,
      },
    });

  };

  const handleFiltersFormSubmit: FilterFormSubmitHandler = async values => {
    const {
      table,
      tableHeaders: generatedTableHeaders,
      query: createdQuery,
    } = await tableGeneration(
      tableToolState.dateRange,
      tableToolState.subjectMeta,
      values,
      tableToolState.subjectId,
      tableToolState.locations,
      releaseId,
    );

    if (table && generatedTableHeaders && createdQuery) {
      const newState = {
        ...tableToolState,
        createdTable: table,
        tableHeaders: generatedTableHeaders,
        query: createdQuery,

      };
      setTableToolState(newState);
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
                  initialValues={
                    tableToolState.validInitialQuery
                  }
                  subjectMeta={tableToolState.subjectMeta}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps =>
                finalStep && finalStep({
                  ...stepProps,
                  ...tableToolState,
                  updateState,
                })
              }
            </WizardStep>
          </Wizard>

          <PreviousStepModalConfirm />
        </>
      )}
    </ConfirmContextProvider>
  );
};

export default TableToolWizard;
