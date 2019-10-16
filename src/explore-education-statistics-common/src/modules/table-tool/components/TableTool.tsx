/* eslint-disable no-shadow */
import {ConfirmContextProvider} from '@common/context/ConfirmContext';
import tableBuilderService, {
  PublicationSubject,
  PublicationSubjectMeta,
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import {LocationFilter} from '@common/modules/full-table/types/filters';
import {FullTable} from '@common/modules/full-table/types/fullTable';
import parseYearCodeTuple from '@common/modules/full-table/utils/TimePeriod';
import FiltersForm, {FilterFormSubmitHandler} from '@common/modules/table-tool/components/FiltersForm';
import LocationFiltersForm, {LocationFiltersFormSubmitHandler} from '@common/modules/table-tool/components/LocationFiltersForm';
import PreviousStepModalConfirm from '@common/modules/table-tool/components/PreviousStepModalConfirm';
import PublicationForm, {PublicationFormSubmitHandler} from '@common/modules/table-tool/components/PublicationForm';
import PublicationSubjectForm, {PublicationSubjectFormSubmitHandler} from '@common/modules/table-tool/components/PublicationSubjectForm';
import TableHeadersForm, {TableHeadersFormValues} from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import TimePeriodForm, {TimePeriodFormSubmitHandler} from '@common/modules/table-tool/components/TimePeriodForm';
import Wizard from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import {Dictionary} from '@common/types/util';
import mapValues from 'lodash/mapValues';
import React, {createRef, ReactNode} from 'react';

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

interface FinalStepProps {
  publication?: Publication;
  createdTable: FullTable;
  query: TableDataQuery;
  tableHeaders: TableHeadersFormValues;
}

interface Props {
  themeMeta: ThemeMeta[];
  publicationId?: string;
  releaseId?: string;

  finalStepExtra?: (props: FinalStepProps) => ReactNode;
  finalStepHeading?: string;

  initialQuery?: TableDataQuery;
  initialTableHeaders?: TableHeadersFormValues;
  onInitialQueryCompleted?: () => void;
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

const TableTool = ({
  themeMeta,
  publicationId,
  releaseId,
  finalStepExtra,
  finalStepHeading,
  initialQuery,
  initialTableHeaders,
  onInitialQueryCompleted,
}: Props) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const [publication, setPublication] = React.useState<Publication>();
  const [subjects, setSubjects] = React.useState<PublicationSubject[]>([]);

  const [initialStep, setInitialStep] = React.useState(1);


  const getInitialState = () => ({
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
    validInitialQuery: undefined
  });

  const [tableToolState, setTableToolState] = React.useState<TableToolState>(getInitialState);

  React.useEffect(() => {
    if (releaseId) {
      tableBuilderService
        .getReleaseMeta(releaseId)
        .then(({subjects: releaseSubjects}) => {
          setSubjects(releaseSubjects);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [releaseId]);

  React.useEffect(() => {

    setInitialStep(1);
    setTableToolState({
      ...tableToolState,
      validInitialQuery: undefined,
      createdTable: undefined,
      tableHeaders: undefined
    });

    initialiseFromInitialQuery(releaseId, initialQuery, initialTableHeaders)
      .then(( state ) => {

        const {tableHeaders, subjectMeta, subjectId, createdTable, locations, query, initialStep, dateRange, validInitialQuery} = state;

        setTableToolState(
          {
            tableHeaders,
            subjectMeta,
            subjectId,
            createdTable,
            locations,
            query,
            dateRange,
            validInitialQuery
          }
        );

        setInitialStep(initialStep);

        if (onInitialQueryCompleted) onInitialQueryCompleted();
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
      subjectMeta: selectedSubjectMeta
    })
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
        timePeriod: selectedSubjectMeta.timePeriod
      },
      locations: mapLocations(selectedLocations, tableToolState.subjectMeta.locations)
    })

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
      }
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
        query: createdQuery

      };
      console.log("submit", newState);
      setTableToolState(newState);
    }
  };

  return (
    <ConfirmContextProvider>
      {({askConfirm}) => (
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
              {stepProps => (
                <>
                  <WizardStepHeading {...stepProps}>
                    {finalStepHeading || 'Explore data'}
                  </WizardStepHeading>

                  <div className="govuk-!-margin-bottom-4">
                    <TableHeadersForm
                      initialValues={tableToolState.tableHeaders}
                      onSubmit={tableHeaderConfig => {
                        setTableToolState({
                          ...tableToolState,
                          tableHeaders: tableHeaderConfig
                        });

                        if (dataTableRef.current) {
                          dataTableRef.current.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start',
                          });
                        }
                      }}
                    />
                    {tableToolState.createdTable && tableToolState.tableHeaders && (
                      <TimePeriodDataTable
                        ref={dataTableRef}
                        fullTable={tableToolState.createdTable}
                        tableHeadersConfig={tableToolState.tableHeaders}
                      />
                    )}
                  </div>

                  {tableToolState.createdTable &&
                  tableToolState.tableHeaders &&
                  finalStepExtra &&
                  tableToolState.query &&
                  finalStepExtra({
                    createdTable: tableToolState.createdTable,
                    publication,
                    tableHeaders: tableToolState.tableHeaders,
                    query: tableToolState.query,
                  })}
                </>
              )}
            </WizardStep>
          </Wizard>

          <PreviousStepModalConfirm />
        </>
      )}
    </ConfirmContextProvider>
  );
};

export default TableTool;
