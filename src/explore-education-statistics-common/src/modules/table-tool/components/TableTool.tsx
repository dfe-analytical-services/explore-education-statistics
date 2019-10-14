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
import TableHeadersForm, { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import TimePeriodForm, { TimePeriodFormSubmitHandler } from '@common/modules/table-tool/components/TimePeriodForm';
import Wizard from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { Dictionary } from '@common/types/util';
import mapValues from 'lodash/mapValues';
import React, { createRef, ReactNode } from 'react';

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

  const [subjects, setSubjects] = React.useState<PublicationSubject[]>([]);

  const [publication, setPublication] = React.useState<Publication>();

  const [subjectId, setSubjectId] = React.useState<string>('');



  const [subjectMeta, setSubjectMeta] = React.useState<PublicationSubjectMeta>(
    getDefaultSubjectMeta(),
  );


  const [locations, setLocations] = React.useState<Dictionary<LocationFilter[]>>({});

  const [dateRange, setDateRange] = React.useState<DateRangeState>({});

  const [tableHeaders, setTableHeaders] = React.useState<TableHeadersFormValues>({
    columnGroups: [],
    columns: [],
    rowGroups: [],
    rows: [],
  });

  const [createdTable, setCreatedTable] = React.useState<FullTable>();

  const [query, setQuery] = React.useState<TableDataQuery>();

  const [initialStep, setInitialStep] = React.useState(1);

  const [validInitialQuery, setValidInitialQuery] = React.useState<TableDataQuery>();

  React.useEffect(() => {

    initialiseFromInitialQuery(releaseId, initialQuery, initialTableHeaders)
      .then(({
        query: newQuery,
        validInitialQuery: newValidInitialQuery,
        subjectId: newSubjectId,
        locations: newLocations,
        dateRange: newDateRange,
        subjectMeta: meta,
        createdTable: newTable,
        tableHeaders: newTableHeaders,
        initialStep: finalValidStepNumber,
      }) => {

        setInitialStep(finalValidStepNumber);
        setQuery(newQuery);
        setValidInitialQuery(newValidInitialQuery);
        setSubjectId(newSubjectId);
        setLocations(newLocations);
        setDateRange(newDateRange);
        setSubjectMeta(meta);
        setCreatedTable(newTable);
        setTableHeaders(newTableHeaders);


        if (onInitialQueryCompleted) onInitialQueryCompleted();
      });

  }, [initialQuery, initialTableHeaders, onInitialQueryCompleted, releaseId]);

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

    setSubjectId(selectedSubjectId);
    setSubjectMeta(selectedSubjectMeta);
  };

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

  const handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = async ({
    locations: selectedLocations,
  }) => {
    const selectedSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        ...selectedLocations,
        subjectId,
      },
    );

    setSubjectMeta({
      ...subjectMeta,
      timePeriod: selectedSubjectMeta.timePeriod,
    });

    setLocations(mapLocations(selectedLocations, subjectMeta.locations));
  };

  const handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler = async values => {
    const [startYear, startCode] = parseYearCodeTuple(values.start);
    const [endYear, endCode] = parseYearCodeTuple(values.end);

    const selectedSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        ...mapValues(locations, locationLevel =>
          locationLevel.map(location => location.value),
        ),
        subjectId,
        timePeriod: {
          startYear,
          startCode,
          endYear,
          endCode,
        },
      },
    );

    setSubjectMeta({
      ...subjectMeta,
      filters: selectedSubjectMeta.filters,
    });

    setDateRange({
      startYear,
      startCode,
      endYear,
      endCode,
    });
  };

  const handleFiltersFormSubmit: FilterFormSubmitHandler = async values => {
    const {
      table,
      tableHeaders: generatedTableHeaders,
      query: createdQuery,
    } = await tableGeneration(
      dateRange,
      subjectMeta,
      values,
      subjectId,
      locations,
      releaseId,
    );

    if (table && generatedTableHeaders && createdQuery) {
      setCreatedTable(table);
      setTableHeaders(generatedTableHeaders);
      setQuery(createdQuery);
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
                  initialSubjectId={
                    validInitialQuery && validInitialQuery.subjectId
                  }
                  onSubmit={handlePublicationSubjectFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <LocationFiltersForm
                  {...stepProps}
                  options={subjectMeta.locations}
                  initialValues={validInitialQuery}
                  onSubmit={handleLocationFiltersFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <TimePeriodForm
                  {...stepProps}
                  initialValues={
                    validInitialQuery && validInitialQuery.timePeriod
                  }
                  options={subjectMeta.timePeriod.options}
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
                    validInitialQuery && {
                      filters: validInitialQuery.filters,
                      indicators: validInitialQuery.indicators,
                    }
                  }
                  subjectMeta={subjectMeta}
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
                      initialValues={tableHeaders}
                      onSubmit={tableHeaderConfig => {
                        setTableHeaders(tableHeaderConfig);

                        if (dataTableRef.current) {
                          dataTableRef.current.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start',
                          });
                        }
                      }}
                    />
                    {createdTable ? (
                      <TimePeriodDataTable
                        ref={dataTableRef}
                        fullTable={createdTable}
                        tableHeadersConfig={tableHeaders}
                      />
                    ) : null}
                  </div>

                  {createdTable &&
                  finalStepExtra &&
                  query &&
                  finalStepExtra({
                    createdTable,
                    publication,
                    tableHeaders,
                    query,
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
