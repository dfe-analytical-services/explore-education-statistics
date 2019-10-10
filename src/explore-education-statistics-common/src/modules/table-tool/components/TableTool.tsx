import {ConfirmContextProvider} from '@common/context/ConfirmContext';
import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  FilterOption,
  IndicatorOption,
  LocationLevelKeysEnum,
  PublicationSubject,
  PublicationSubjectMeta,
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import {Dictionary} from '@common/types/util';
import PreviousStepModalConfirm from '@common/modules/table-tool/components/PreviousStepModalConfirm';
import {CategoryFilter, Indicator, LocationFilter,} from '@common/modules/full-table/types/filters';
import parseYearCodeTuple from '@common/modules/full-table/utils/TimePeriod';
import mapValues from 'lodash/mapValues';
import React, {createRef, ReactNode} from 'react';
import getDefaultTableHeaderConfig, {TableHeadersConfig} from '@common/modules/full-table/utils/tableHeaders';
import {FullTable} from '@common/modules/full-table/types/fullTable';
import {mapFullTable} from '@common/modules/full-table/utils/mapPermalinks';
import FiltersForm, {FilterFormSubmitHandler, FormValues,} from '@common/modules/table-tool/components/FiltersForm';
import LocationFiltersForm, {
  LocationFiltersFormSubmitHandler,
  LocationsFormValues,
} from '@common/modules/table-tool/components/LocationFiltersForm';
import PublicationForm, {PublicationFormSubmitHandler,} from '@common/modules/table-tool/components/PublicationForm';
import PublicationSubjectForm, {PublicationSubjectFormSubmitHandler,} from '@common/modules/table-tool/components/PublicationSubjectForm';
import TableHeadersForm, {TableHeadersFormValues,} from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import TimePeriodForm, {TimePeriodFormSubmitHandler,} from '@common/modules/table-tool/components/TimePeriodForm';
import mapOptionValues from '@common/modules/table-tool/components/utils/mapOptionValues';
import Wizard from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';

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

interface DateRangeState {
  startYear?: number;
  startCode?: string;
  endYear?: number;
  endCode?: string;
}

const createQuery = (
  filters: Dictionary<CategoryFilter[]>,
  indicators: Indicator[],
  {
    subjectId,
    startYear,
    startCode,
    endYear,
    endCode,
    locations,
  }: {
    subjectId: string;
    locations: Dictionary<LocationFilter[]>;
  } & DateRangeState,
): TableDataQuery => {
  if (!startYear || !startCode || !endYear || !endCode) {
    throw new Error('Missing required timePeriod parameters');
  }

  return {
    ...mapValues(locations, locationLevel =>
      locationLevel.map(location => location.value),
    ),
    subjectId,
    indicators: indicators.map(indicator => indicator.value),
    filters: Object.values(filters).flatMap(categoryFilters =>
      categoryFilters.flatMap(filter => filter.value),
    ),
    timePeriod: {
      startYear,
      startCode,
      endYear,
      endCode,
    },
  };
};

const mapLocations = (
  selectedLocations: LocationsFormValues,
  locationsMeta: PublicationSubjectMeta['locations'],
) =>
  mapValuesWithKeys(selectedLocations, (locationLevel, locationOptions) =>
    locationOptions
      .map(location =>
        locationsMeta[locationLevel].options.find(
          option => option.value === location,
        ),
      )
      .filter(option => typeof option !== 'undefined')
      .map(option => new LocationFilter(option as FilterOption, locationLevel)),
  );

const getSelectedLocationsForQuery = (locationQuery: Dictionary<string[] | undefined>) =>
  mapValuesWithKeys(
    LocationLevelKeysEnum,
    (key, _) => locationQuery[key] || [],
  );

const getFiltersForTableGeneration = ({filters: metaFilters}: PublicationSubjectMeta, {filters}: FormValues) => {
  const filtersByValue = mapValues(metaFilters, value =>
    mapOptionValues(value.options),
  );

  return mapValuesWithKeys(
    filters,
    (filterGroup, selectedFilters) =>
      selectedFilters.map(
        filter =>
          new CategoryFilter(
            filtersByValue[filterGroup][filter],
            filter === metaFilters[filterGroup].totalValue,
          ),
      ),
  );
};

const getIndicatorsForTableGeneration = ({indicators: indicatorsMeta}: PublicationSubjectMeta, {indicators}: FormValues) => {

  const indicatorsByValue = mapOptionValues<IndicatorOption>(
    indicatorsMeta,
  );

  return indicators.map(
    indicator => new Indicator(indicatorsByValue[indicator]),
  );

};

const queryForTable = async (query: TableDataQuery, releaseId?: string): Promise<FullTable> => {
  if (releaseId) {
    return tableBuilderService.getTableDataForRelease(
      query,
      releaseId,
    );
  }
  return tableBuilderService.getTableData(
    query,
  );

};

const tableGeneration = async (
  dateRange: DateRangeState,
  subjectMeta: PublicationSubjectMeta,
  values: FormValues,
  subjectId: string,
  locations: Dictionary<LocationFilter[]>,
  releaseId: string | undefined,
): Promise<{
  table?: FullTable,
  tableHeaders?: TableHeadersConfig,
  query?: TableDataQuery
}> => {
  const {startYear, startCode, endYear, endCode} = dateRange;

  if (!startYear || !startCode || !endYear || !endCode) {
    return {};
  }

  const query: TableDataQuery = createQuery(
    getFiltersForTableGeneration(subjectMeta, values),
    getIndicatorsForTableGeneration(subjectMeta, values),
    {
      subjectId,
      locations,
      ...dateRange,
    });

  const unmappedCreatedTable = await queryForTable(query, releaseId);
  const table = mapFullTable(unmappedCreatedTable);
  const tableHeaders = getDefaultTableHeaderConfig(table.subjectMeta);

  return {
    table,
    tableHeaders,
    query
  };

};

const TableTool = ({
  themeMeta,
  publicationId,
  releaseId,
  finalStepExtra,
  finalStepHeading,
  initialQuery,
  initialTableHeaders,
  onInitialQueryCompleted
}: Props) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const [subjects, setSubjects] = React.useState<PublicationSubject[]>([]);

  const [publication, setPublication] = React.useState<Publication>();

  const [subjectId, setSubjectId] = React.useState<string>('');

  const getDefaultSubjectMeta = () => ({
    timePeriod: {
      hint: '',
      legend: '',
      options: [],
    },
    locations: {},
    indicators: {},
    filters: {},
  });

  const [subjectMeta, setSubjectMeta] = React.useState<PublicationSubjectMeta>(getDefaultSubjectMeta());

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
    setValidInitialQuery(undefined);
    setInitialStep(1);

    if (initialQuery) {
      const doit = async () => {

        try {

          let newQuery: TableDataQuery = {
            subjectId: '',
            filters: [],
            indicators: [],
          };

          const meta = await tableBuilderService.filterPublicationSubjectMeta(initialQuery);
          setSubjectMeta(meta);

          if (initialQuery.subjectId === undefined || initialQuery.subjectId === '') {
            setInitialStep(1);
            setQuery(newQuery);
            setValidInitialQuery(newQuery);
          } else {

            newQuery.subjectId = initialQuery.subjectId;
            setSubjectId(initialQuery.subjectId);

            // validate location data
            // parse out all the location query information, filter it to only those that are set
            const initialLocations: Dictionary<string[]> =
              Object.entries(
                mapValuesWithKeys(LocationLevelKeysEnum, keyName => (initialQuery as Dictionary<string[]>)[keyName])
              )
                .reduce((filtered, [, value]) => ({...filtered, ...((value && value.length > 0) ? value : {})}), {});

            // check if any are actually set to validate if it's actually valid
            const allLocations = ([] as string[]).concat(...Object.values(initialLocations));
            if (allLocations.length === 0) {
              setInitialStep(2);
              setQuery(newQuery);
              setValidInitialQuery(newQuery);
            } else {

              // populate location data
              const newLocations = mapLocations(
                getSelectedLocationsForQuery(initialQuery),
                meta.locations,
              );
              newQuery = {...newQuery, ...initialLocations};
              setLocations(newLocations);


              // validate time period
              // generate and populate time period data
              const newDateRange: DateRangeState = {...initialQuery.timePeriod};
              if (newDateRange.endCode === undefined || newDateRange.endYear === undefined || newDateRange.startCode === undefined || newDateRange.startYear === undefined) {
                setInitialStep(3);
                setQuery(newQuery);
                setValidInitialQuery(newQuery);

              } else {

                newQuery = {...newQuery, timePeriod: initialQuery.timePeriod};
                setDateRange(newDateRange);

                if (initialQuery.filters.length === 0 || initialQuery.indicators.length === 0) {
                  setInitialStep(4);
                  setQuery(newQuery);
                  setValidInitialQuery(newQuery);

                } else {

                  newQuery = {...newQuery, filters: initialQuery.filters, indicators: initialQuery.indicators};

                  // const newValues: FormValues = buildInitialFormValue(meta, newQuery);

                  const newTable = await queryForTable(newQuery, releaseId);

                  const newTableHeaders = initialTableHeaders || getDefaultTableHeaderConfig(newTable.subjectMeta);

                  setInitialStep(5);
                  setCreatedTable(newTable);
                  setTableHeaders(newTableHeaders);
                  setQuery(newQuery);
                  setValidInitialQuery(newQuery);


                }

              }

            }


          }


        } catch (_) {
          //
        }

        if (onInitialQueryCompleted) onInitialQueryCompleted();

      };

      doit();


    }

  }, [initialQuery, initialTableHeaders, onInitialQueryCompleted]);

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
        .then(({subjects: releaseSubjects}) => {
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

    const {table, tableHeaders: generatedTableHeaders, query: createdQuery} = await tableGeneration(
      dateRange,
      subjectMeta,
      values,
      subjectId,
      locations,
      releaseId
    );

    if (table && generatedTableHeaders && createdQuery) {
      setCreatedTable(table);
      setTableHeaders(generatedTableHeaders);
      setQuery(createdQuery);
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
                  initialSubjectId={validInitialQuery && validInitialQuery.subjectId}
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
                  initialValues={validInitialQuery && validInitialQuery.timePeriod}
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
                      indicators: validInitialQuery.indicators
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
